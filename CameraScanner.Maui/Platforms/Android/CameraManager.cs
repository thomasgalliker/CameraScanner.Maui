using Android.Content;
using Android.Gms.Extensions;
using Android.Graphics;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Core.Content;
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

        private ZoomStateObserver zoomStateObserver;
        private TorchStateObserver torchStateObserver;
        private CameraStateObserver cameraStateObserver;

        private BarcodeAnalyzer barcodeAnalyzer;
        private IMLKitBarcodeScanner barcodeScanner;
        private ICameraInfo cameraInfo;

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

            this.cameraStateObserver = new CameraStateObserver();
            this.cameraStateObserver.ValueChanged += this.OnCameraStateChanged;

            this.cameraController = new LifecycleCameraController(this.context)
            {
                PinchToZoomEnabled = true,
                TapToFocusEnabled = this.cameraView.TapToFocusEnabled,
                ImageAnalysisBackpressureStrategy = ImageAnalysis.StrategyKeepOnlyLatest
            };
            this.cameraController.SetEnabledUseCases(CameraController.ImageAnalysis);
            this.cameraController.ZoomState.ObserveForever(this.zoomStateObserver);
            this.cameraController.InitializationFuture.AddListener(new Java.Lang.Runnable(() =>
            {
                this.cameraInfo?.CameraState.RemoveObserver(this.cameraStateObserver);
                this.cameraInfo = this.cameraController.CameraInfo;
                this.cameraInfo?.CameraState.ObserveForever(this.cameraStateObserver);
            }), ContextCompat.GetMainExecutor(this.context));

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

        private void OnCameraStateChanged(object sender, CameraStateChangedEventArgs e)
        {
            this.logger.Log(e.CameraState.Error == null ? LogLevel.Debug : LogLevel.Error, $"OnCameraStateChanged: {e.CameraState}");

            if (e.CameraState?.GetType() == CameraState.Type.Open)
            {
                if (this.cameraController?.ZoomState.Value is IZoomState zoomState)
                {
                    this.UpdateCurrentZoomFactor(zoomState);
                    this.UpdateRequestZoomFactor();
                }
            }
        }

        private void OnTorchStateChanged(object sender, TorchStateEventArgs e)
        {
            if (this.cameraView == null)
            {
                return;
            }

            this.logger.LogDebug($"OnTorchStateChanged: TorchOn={e.TorchOn}");

            this.cameraView.TorchOn = e.TorchOn;
        }

        private void OnZoomStateChanged(object sender, ZoomStateChangedEventArgs e)
        {
            this.logger.LogDebug("OnZoomStateChanged");

            this.UpdateCurrentZoomFactor(e.ZoomState);
            this.UpdateRequestZoomFactor();

        }

        private void UpdateCurrentZoomFactor(IZoomState zoomState)
        {
            if (this.cameraView == null)
            {
                return;
            }

            this.logger.LogDebug($"UpdateCurrentZoomFactor: CurrentZoomFactor={zoomState.ZoomRatio}, " +
                                 $"MinZoomRatio={zoomState.MinZoomRatio}, MaxZoomRatio={zoomState.MaxZoomRatio}");

            this.cameraView.CurrentZoomFactor = zoomState.ZoomRatio;
            this.cameraView.MinZoomFactor = zoomState.MinZoomRatio;
            this.cameraView.MaxZoomFactor = zoomState.MaxZoomRatio;
        }

        internal void UpdateRequestZoomFactor()
        {
            if (this.cameraController == null || this.cameraController?.ZoomState.IsInitialized == false)
            {
                return;
            }

            if (this.cameraController.ZoomState.Value is not IZoomState zoomState)
            {
                return;
            }

            if (this.cameraView?.RequestZoomFactor is not (float requestZoomFactor and > 0F))
            {
                return;
            }

            this.logger.LogDebug("UpdateRequestZoomFactor");

            var zoomRatio = Math.Max(requestZoomFactor, zoomState.MinZoomRatio);
            zoomRatio = Math.Min(zoomRatio, zoomState.MaxZoomRatio);

            if (Math.Abs(zoomRatio - zoomState.ZoomRatio) > 0.001F)
            {
                this.cameraController.SetZoomRatio(zoomRatio);
            }
        }

        internal bool IsRunning { get; private set; }

        internal BarcodeView BarcodeView { get; }

        internal bool CaptureNextFrame => this.cameraView.CaptureNextFrame;

        internal void UpdateCameraFacing()
        {
            this.logger.LogDebug("UpdateCameraFacing");

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

                    if (this.cameraController.CameraSelector != CameraSelector.DefaultBackCamera &&
                        this.cameraController.CameraSelector != CameraSelector.DefaultFrontCamera )
                    {
                        this.UpdateCameraFacing();
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
            this.logger.LogDebug("UpdateTorch");

            if (this.cameraController == null)
            {
                return;
            }

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

                if (this.cameraStateObserver != null)
                {
                    this.cameraStateObserver.ValueChanged -= this.OnCameraStateChanged;
                    this.cameraInfo?.CameraState.RemoveObserver(this.cameraStateObserver);
                    this.cameraStateObserver.Dispose();
                    this.cameraStateObserver = null;
                }

                if (this.zoomStateObserver != null)
                {
                    this.zoomStateObserver.ValueChanged -= this.OnZoomStateChanged;
                    this.cameraController?.ZoomState.RemoveObserver(this.zoomStateObserver);
                    this.zoomStateObserver.Dispose();
                    this.zoomStateObserver = null;
                }

                if (this.torchStateObserver != null)
                {
                    this.torchStateObserver.ValueChanged -= this.OnTorchStateChanged;
                    this.cameraController?.TorchState.RemoveObserver(this.torchStateObserver);
                    this.torchStateObserver.Dispose();
                    this.torchStateObserver = null;
                }

                this.BarcodeView?.RemoveAllViews();
                this.relativeLayout?.RemoveAllViews();

                this.BarcodeView?.Dispose();
                this.relativeLayout?.Dispose();
                this.imageView?.Dispose();
                this.previewView?.Dispose();
                this.cameraController?.Dispose();
                this.cameraInfo?.Dispose();
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