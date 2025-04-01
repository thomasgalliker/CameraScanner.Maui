using Android.Content;
using Android.Gms.Extensions;
using Android.Graphics;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Camera.View;
using AndroidX.Camera.View.Transform;
using AndroidX.Lifecycle;
using CameraScanner.Maui.Platforms.Android;
using Java.Util.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics.Platform;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;
using static Android.Views.ViewGroup;
using Color = Android.Graphics.Color;
using IMLKitBarcodeScanner = Xamarin.Google.MLKit.Vision.BarCode.IBarcodeScanner;
using MLKitBarcodeScanning = Xamarin.Google.MLKit.Vision.BarCode.BarcodeScanning;
using Paint = Android.Graphics.Paint;
using Point = Microsoft.Maui.Graphics.Point;
using RectF = Microsoft.Maui.Graphics.RectF;
using Size = Android.Util.Size;

namespace CameraScanner.Maui
{
    [Preserve(AllMembers = true)]
    internal class CameraManager : IDisposable
    {
        private const int AimRadius = 25;

        private readonly CameraView cameraView;
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly ICameraPermissions cameraPermissions;
        private readonly IDeviceDisplay deviceDisplay;
        private readonly Context context;
        private readonly IExecutorService cameraExecutor;
        private readonly ImageView imageView;
        private readonly LifecycleCameraController cameraController;
        private readonly PreviewView previewView;
        private readonly RelativeLayout relativeLayout;
        private readonly ZoomStateObserver zoomStateObserver;
        private readonly TorchStateObserver torchStateObserver;

        private BarcodeAnalyzer barcodeAnalyzer;
        private IMLKitBarcodeScanner barcodeScanner;

        internal CameraManager(
            ILogger<CameraManager> logger,
            ILoggerFactory loggerFactory,
            ICameraPermissions cameraPermissions,
            IDeviceDisplay deviceDisplay,
            CameraView cameraView,
            Context context)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.cameraPermissions = cameraPermissions;
            this.deviceDisplay = deviceDisplay;
            this.context = context;
            this.cameraView = cameraView;

            this.cameraExecutor = Executors.NewSingleThreadExecutor();
            this.zoomStateObserver = new ZoomStateObserver();
            this.zoomStateObserver.ValueChanged += this.OnZoomStateChanged;

            this.cameraController = new LifecycleCameraController(this.context)
            {
                TapToFocusEnabled = this.cameraView.TapToFocusEnabled,
                ImageAnalysisBackpressureStrategy = ImageAnalysis.StrategyKeepOnlyLatest
            };
            this.cameraController.SetEnabledUseCases(CameraController.ImageAnalysis);
            this.cameraController.ZoomState.ObserveForever(this.zoomStateObserver);

            this.torchStateObserver = new TorchStateObserver();
            this.torchStateObserver.ValueChanged += this.OnTorchStateChanged;
            this.cameraController.TorchState.ObserveForever(this.torchStateObserver);

            this.previewView = new PreviewView(this.context)
            {
                Controller = this.cameraController,
                LayoutParameters = new RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };
            this.previewView.SetImplementationMode(PreviewView.ImplementationMode.Compatible);
            this.previewView.SetScaleType(PreviewView.ScaleType.FillCenter);

            var layoutParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
            layoutParams.AddRule(LayoutRules.CenterInParent);
            var circleBitmap = Bitmap.CreateBitmap(2 * AimRadius, 2 * AimRadius, Bitmap.Config.Argb8888);
            var canvas = new Canvas(circleBitmap);
            canvas.DrawCircle(AimRadius, AimRadius, AimRadius, new Paint { AntiAlias = true, Color = Color.Red, Alpha = 150 });
            this.imageView = new ImageView(this.context) { LayoutParameters = layoutParams };
            this.imageView.SetImageBitmap(circleBitmap);

            this.relativeLayout = new RelativeLayout(this.context)
            {
                LayoutParameters = new RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };
            this.relativeLayout.AddView(this.previewView);

            this.BarcodeView = new BarcodeView(this.context);
            this.BarcodeView.AddView(this.relativeLayout);

            this.deviceDisplay.MainDisplayInfoChanged += this.OnMainDisplayInfoChanged;
        }

        private void OnTorchStateChanged(object sender, TorchStateEventArgs e)
        {
            if (this.cameraView is not null)
            {
                this.cameraView.TorchOn = e.TorchOn;
            }
        }

        private void OnZoomStateChanged(object sender, ZoomStateChangedEventArgs e)
        {
            if (this.cameraView is not null)
            {
                this.UpdateCurrentZoomFactor(e.ZoomRatio);
                this.UpdateMinMaxZoomFactor(e.MinZoomRatio, e.MaxZoomRatio);

                this.UpdateRequestZoomFactor();
            }
        }

        private void UpdateCurrentZoomFactor(float zoomRatio)
        {
            this.cameraView.CurrentZoomFactor = zoomRatio;
        }

        private void UpdateMinMaxZoomFactor(float minZoomRatio, float maxZoomRatio)
        {
            this.cameraView.MinZoomFactor = minZoomRatio;
            this.cameraView.MaxZoomFactor = maxZoomRatio;
        }

        internal void UpdateRequestZoomFactor()
        {
            if (this.cameraController is { ZoomState.IsInitialized: true } &&
                this.zoomStateObserver.LastValue is IZoomState zoomState &&
                this.cameraView is not null &&
                this.cameraView.RequestZoomFactor is float requestZoomFactor and > 0F)
            {
                var zoomRatio = Math.Max(requestZoomFactor, zoomState.MinZoomRatio);
                zoomRatio = Math.Min(zoomRatio, zoomState.MaxZoomRatio);

                if (Math.Abs(zoomRatio - zoomState.ZoomRatio) > 0.001F)
                {
                    this.cameraController.SetZoomRatio(zoomRatio);
                }
            }
        }

        internal bool IsRunning { get; private set; }

        internal BarcodeView BarcodeView { get; }

        internal bool CaptureNextFrame => this.cameraView.CaptureNextFrame;

        internal async void UpdateCameraFacing()
        {
            this.logger.LogDebug("UpdateCameraFacing");
            await this.StartAsync();
        }

        internal async Task StartAsync()
        {
            this.logger.LogDebug("StartAsync");

            try
            {
                if (!await this.cameraPermissions.CheckPermissionAsync())
                {
                    this.logger.LogInformation("UpdateCameraAsync: Camera permission not granted");
                    return;
                }

                if (this.cameraController is not null)
                {
                    if (this.IsRunning)
                    {
                        this.cameraController.Unbind();
                        this.IsRunning = false;
                    }

                    ILifecycleOwner lifecycleOwner = null;
                    if (this.context is ILifecycleOwner owner)
                    {
                        lifecycleOwner = owner;
                    }
                    else if ((this.context as ContextWrapper)?.BaseContext is ILifecycleOwner)
                    {
                        lifecycleOwner = ((ContextWrapper)this.context)?.BaseContext as ILifecycleOwner;
                    }
                    else if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is ILifecycleOwner l)
                    {
                        lifecycleOwner = l;
                    }

                    if (lifecycleOwner is null)
                    {
                        return;
                    }

                    if (this.cameraController.CameraSelector == null)
                    {
                        this.UpdateCamera();
                    }

                    if (this.cameraController.ImageAnalysisTargetSize == null)
                    {
                        this.UpdateCaptureQuality();
                    }

                    this.UpdateOutput();
                    this.UpdateBarcodeFormats();
                    this.UpdateTorch();

                    this.cameraController.BindToLifecycle(lifecycleOwner);
                    this.IsRunning = true;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "StartAsync failed with exception");
                throw;
            }
        }

        internal void Stop()
        {
            if (this.cameraController is not null)
            {
                if ((int)this.cameraController.TorchState.Value == TorchState.On)
                {
                    this.cameraController.EnableTorch(false);

                    if (this.cameraView is not null)
                    {
                        this.cameraView.TorchOn = false;
                    }
                }

                if (this.IsRunning)
                {
                    this.cameraController.Unbind();
                }

                this.IsRunning = false;
            }
        }

        //TODO Implement camera-mlkit-vision
        //https://developer.android.com/reference/androidx/camera/mlkit/vision/MlKitAnalyzer
        internal void UpdateBarcodeFormats()
        {
            if (this.cameraView.BarcodeFormats is BarcodeFormats barcodeFormats)
            {
                this.barcodeScanner?.Dispose();
                var mlKitBarcodeFormats = barcodeFormats.ToPlatform();
                this.barcodeScanner = MLKitBarcodeScanning.GetClient(new BarcodeScannerOptions.Builder()
                    .SetBarcodeFormats(mlKitBarcodeFormats)
                    .Build());
            }
        }

        internal void UpdateCamera()
        {
            if (this.cameraController is not null)
            {
                if (this.cameraView.CameraFacing == CameraFacing.Front)
                {
                    this.cameraController.CameraSelector = CameraSelector.DefaultFrontCamera;
                }
                else
                {
                    this.cameraController.CameraSelector = CameraSelector.DefaultBackCamera;
                }

                // If camera facing is switched, the torch may be turned off
                //if ((int)this.cameraController.TorchState.Value == TorchState.On && this.cameraView.TorchOn == false)
                //{
                //    this.cameraView.TorchOn = true;
                //}
                //if ((int)this.cameraController.TorchState.Value == TorchState.Off && this.cameraView.TorchOn == true)
                //{
                //    this.cameraView.TorchOn = false;
                //}
            }
        }

        //TODO Implement setImageAnalysisResolutionSelector
        //https://developer.android.com/reference/androidx/camera/view/CameraController#setImageAnalysisResolutionSelector(androidx.camera.core.resolutionselector.ResolutionSelector)
        internal async void UpdateCaptureQuality()
        {
            if (this.cameraController is LifecycleCameraController lifecycleCameraController)
            {
                var resolution = this.GetTargetResolution();

                if (!resolution.Equals(lifecycleCameraController.ImageAnalysisTargetSize?.Resolution))
                {
                    lifecycleCameraController.ImageAnalysisTargetSize = new CameraController.OutputSize(resolution);

                    if (this.IsRunning)
                    {
                        await this.StartAsync();
                    }
                }
                else
                {
                    // Resolution remains unchanged
                }
            }
        }

        internal void UpdateTorch()
        {
            if (this.cameraController != null)
            {
                var hasFlashUnit = this.cameraController.CameraInfo?.HasFlashUnit;
                if (hasFlashUnit == false)
                {
                    this.cameraView.TorchOn = false;
                }
                else
                {
                    var requestedTorchOn = this.cameraView.TorchOn;
                    this.cameraController.EnableTorch(requestedTorchOn);
                }
            }
        }

        internal void UpdateBarcodeDetectionFrameRate()
        {
            this.logger.LogDebug("UpdateBarcodeDetectionFrameRate");

            if (this.barcodeAnalyzer is BarcodeAnalyzer analyzer)
            {
                analyzer.BarcodeDetectionFrameRate = this.cameraView.BarcodeDetectionFrameRate;
            }
        }

        internal async void UpdateCameraEnabled()
        {
            if (this.cameraView.CameraEnabled)
            {
                await this.StartAsync();
            }
            else
            {
                this.Stop();
            }
        }

        public void UpdatePauseScanning()
        {
            this.logger.LogDebug("UpdatePauseScanning");

            if (this.barcodeAnalyzer is BarcodeAnalyzer b)
            {
                b.PauseScanning = this.cameraView.PauseScanning;
            }
        }

        internal void UpdateAimMode()
        {
            if (this.cameraView.AimMode)
            {
                this.relativeLayout?.AddView(this.imageView);
            }
            else
            {
                this.relativeLayout?.RemoveView(this.imageView);
            }
        }

        internal void UpdateTapToFocusEnabled()
        {
            if (this.cameraController is not null)
            {
                this.cameraController.TapToFocusEnabled = this.cameraView.TapToFocusEnabled;
            }
        }

        internal async Task PerformBarcodeDetectionAsync(IImageProxy proxy)
        {
            if (this.cameraView.PauseScanning)
            {
                //this.logger.LogDebug("PerformBarcodeDetectionAsync --> paused");
                return;
            }

            //this.logger.LogDebug("PerformBarcodeDetectionAsync");

            using (var target = await MainThread.InvokeOnMainThreadAsync(() => this.previewView?.OutputTransform).ConfigureAwait(false))
            {
                using (var source = new ImageProxyTransformFactory { UsingRotationDegrees = true }.GetOutputTransform(proxy))
                {
                    using (var coordinateTransform = new CoordinateTransform(source, target))
                    {
                        using (var image = InputImage.FromMediaImage(proxy.Image, proxy.ImageInfo.RotationDegrees))
                        {
                            using (var resultsArray = await this.barcodeScanner.Process(image))
                            {
                                var barcodeResults = Platforms.Services.BarcodeScanner.ProcessBarcodeResult(resultsArray, coordinateTransform);

                                if (this.cameraView.ForceInverted)
                                {
                                    Platforms.Services.BarcodeScanner.InvertLuminance(proxy.Image);
                                    using var imageInverted = InputImage.FromMediaImage(proxy.Image, proxy.ImageInfo.RotationDegrees);
                                    using var resultsArrayInverted = await this.barcodeScanner.Process(imageInverted);

                                    var barcodeResultsInverted = Platforms.Services.BarcodeScanner.ProcessBarcodeResult(resultsArrayInverted, coordinateTransform);
                                    barcodeResults.UnionWith(barcodeResultsInverted);
                                }

                                if (this.cameraView.AimMode)
                                {
                                    var previewCenter = new Point(this.previewView.Width / 2d, this.previewView.Height / 2d);

                                    foreach (var barcode in barcodeResults)
                                    {
                                        if (!barcode.PreviewBoundingBox.Contains(previewCenter))
                                        {
                                            barcodeResults.Remove(barcode);
                                        }
                                    }
                                }

                                if (this.cameraView.ViewfinderMode)
                                {
                                    var previewRect = new RectF(0f, 0f, this.previewView.Width, this.previewView.Height);

                                    foreach (var barcode in barcodeResults)
                                    {
                                        if (!previewRect.Contains(barcode.PreviewBoundingBox))
                                        {
                                            barcodeResults.Remove(barcode);
                                        }
                                    }
                                }

                                this.cameraView.DetectionFinished(barcodeResults.ToArray());
                            }
                        }
                    }
                }
            }
        }

        internal void CaptureImage(IImageProxy proxy)
        {
            this.cameraView.CaptureNextFrame = false;
            var image = new PlatformImage(proxy.ToBitmap());
            this.cameraView.TriggerOnImageCaptured(image);
        }

        private void UpdateOutput()
        {
            if (this.cameraController is not null)
            {
                this.cameraController.ClearImageAnalysisAnalyzer();
                this.barcodeAnalyzer?.Dispose();
                this.barcodeAnalyzer = null;

                var barcodeAnalyzerLogger = this.loggerFactory.CreateLogger<BarcodeAnalyzer>();
                this.barcodeAnalyzer = new BarcodeAnalyzer(barcodeAnalyzerLogger, this);
                this.cameraController.SetImageAnalysisAnalyzer(this.cameraExecutor, this.barcodeAnalyzer);
            }
        }

        private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(200);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (this.IsRunning && this.cameraView.CameraEnabled)
                        {
                            this.UpdateCaptureQuality();
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                });
            });
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.deviceDisplay.MainDisplayInfoChanged -= this.OnMainDisplayInfoChanged;

                this.Stop();

                this.zoomStateObserver.ValueChanged -= this.OnZoomStateChanged;
                this.cameraController?.ZoomState.RemoveObserver(this.zoomStateObserver);
                this.zoomStateObserver?.Dispose();

                this.torchStateObserver.ValueChanged -= this.OnTorchStateChanged;
                this.cameraController?.TorchState.RemoveObserver(this.torchStateObserver);
                this.torchStateObserver?.Dispose();

                this.BarcodeView?.RemoveAllViews();
                this.relativeLayout?.RemoveAllViews();

                this.BarcodeView?.Dispose();
                this.relativeLayout?.Dispose();
                this.imageView?.Dispose();
                this.previewView?.Dispose();
                this.cameraController?.Dispose();
                this.barcodeAnalyzer?.Dispose();
                this.barcodeAnalyzer = null;
                this.barcodeScanner?.Dispose();
                this.cameraExecutor?.Dispose();
            }
        }

        internal Size GetTargetResolution()
        {
            CaptureQuality? captureQuality = this.cameraView.CaptureQuality;

            if (this.deviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            {
                return captureQuality switch
                {
                    CaptureQuality.Low => new Size(480, 640),
                    CaptureQuality.Medium => new Size(720, 1280),
                    CaptureQuality.High => new Size(1080, 1920),
                    CaptureQuality.Highest => new Size(2160, 3840),
                    _ => new Size(720, 1280)
                };
            }
            else
            {
                return captureQuality switch
                {
                    CaptureQuality.Low => new Size(640, 480),
                    CaptureQuality.Medium => new Size(1280, 720),
                    CaptureQuality.High => new Size(1920, 1080),
                    CaptureQuality.Highest => new Size(3840, 2160),
                    _ => new Size(1280, 720)
                };
            }
        }
    }
}