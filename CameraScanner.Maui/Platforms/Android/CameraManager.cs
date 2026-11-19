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
        private readonly IExecutorService? cameraExecutor;
        private readonly ImageView imageView;
        private readonly LifecycleCameraController cameraController;
        private readonly PreviewView previewView;
        private readonly RelativeLayout relativeLayout;

        private readonly ZoomStateObserver zoomStateObserver;
        private readonly TorchStateObserver torchStateObserver;
        private readonly CameraStateObserver cameraStateObserver;

        private BarcodeAnalyzer? barcodeAnalyzer;
        private IMLKitBarcodeScanner? barcodeScanner;
        private ICameraInfo? cameraInfo;
        private int disposed;

        private bool IsDisposed => Volatile.Read(ref this.disposed) != 0;

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
            this.cameraController.InitializationFuture.AddListener(
                new Java.Lang.Runnable(this.OnCameraControllerInitialized),
                ContextCompat.GetMainExecutor(this.context));

            this.torchStateObserver = new TorchStateObserver();
            this.torchStateObserver.ValueChanged += this.OnTorchStateChanged;
            this.cameraController.TorchState.ObserveForever(this.torchStateObserver);

            this.previewView = new PreviewView(this.context)
            {
                Controller = this.cameraController,
                LayoutParameters = new RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };
            this.previewView.SetImplementationMode(PreviewView.ImplementationMode.Compatible!);
            this.previewView.SetScaleType(PreviewView.ScaleType.FillCenter!);

            var layoutParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
            layoutParams.AddRule(LayoutRules.CenterInParent);
            var circleBitmap = Bitmap.CreateBitmap(2 * AimRadius, 2 * AimRadius, Bitmap.Config.Argb8888!);
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

        private void OnCameraControllerInitialized()
        {
            // This callback is queued on the main executor and may run after Dispose().
            this.UpdateCameraStateObserver();
        }

        /// <summary>
        /// Moves the camera-state observer to the CameraState of the currently bound camera.
        /// CameraX binds a different camera (and returns a different CameraInfo) whenever
        /// the camera is (re)bound, e.g. after switching the CameraSelector.
        /// Must be called on the main thread.
        /// </summary>
        private void UpdateCameraStateObserver()
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                var newCameraInfo = this.cameraController.CameraInfo;
                if (newCameraInfo is null || newCameraInfo.Equals(this.cameraInfo))
                {
                    // Camera is not bound (yet) or has not changed.
                    return;
                }

                this.logger.LogDebug("UpdateCameraStateObserver: CameraInfo changed, moving camera-state observer");

                // CameraInfo is owned by CameraX and may be shared with other CameraManager instances,
                // so we must not dispose the previous one here.
                if (this.cameraInfo is ICameraInfo oldCameraInfo)
                {
                    try
                    {
                        oldCameraInfo.CameraState.RemoveObserver(this.cameraStateObserver);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        // Still attach the observer to the new camera below.
                        this.logger.LogDebug(ex, "UpdateCameraStateObserver: Previous camera resource was already disposed");
                    }
                }

                this.cameraInfo = newCameraInfo;
                this.cameraInfo.CameraState.ObserveForever(this.cameraStateObserver);
            }
            catch (ObjectDisposedException ex)
            {
                this.logger.LogDebug(ex, "UpdateCameraStateObserver: Camera resource was already disposed");
            }
            catch (Java.Lang.Exception ex)
            {
                this.logger.LogError(ex, "UpdateCameraStateObserver failed with exception");
            }
        }

        private void OnCameraStateChanged(object? sender, CameraStateChangedEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.logger.Log(e.CameraState.Error == null ? LogLevel.Debug : LogLevel.Error, $"OnCameraStateChanged: {e.CameraState}");

            if (e.CameraState.GetType() == CameraState.Type.Open)
            {
                if (this.cameraController.ZoomState.Value is IZoomState zoomState)
                {
                    this.UpdateCurrentZoomFactor(zoomState);
                    this.UpdateRequestZoomFactor();
                }
            }
        }

        private void OnTorchStateChanged(object? sender, TorchStateEventArgs e)
        {
            if (this.IsDisposed || this.cameraView == null)
            {
                return;
            }

            this.logger.LogDebug($"OnTorchStateChanged: TorchOn={e.TorchOn}");

            this.cameraView.TorchOn = e.TorchOn;
        }

        private void OnZoomStateChanged(object? sender, ZoomStateChangedEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.logger.LogDebug("OnZoomStateChanged");

            this.UpdateCurrentZoomFactor(e.ZoomState);
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
            if (this.IsDisposed || this.cameraController == null || this.cameraController?.ZoomState.IsInitialized == false)
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

            var zoomRatio = Math.Clamp(requestZoomFactor, zoomState.MinZoomRatio, zoomState.MaxZoomRatio);
            if (Math.Abs(zoomRatio - zoomState.ZoomRatio) > 0.001F)
            {
                this.cameraController.SetZoomRatio(zoomRatio);
            }
        }

        internal bool IsRunning { get; private set; }

        internal BarcodeView BarcodeView { get; }

        internal bool CaptureNextFrame => !this.IsDisposed && this.cameraView.CaptureNextFrame;

        internal void UpdateCameraFacing()
        {
            this.logger.LogDebug("UpdateCameraFacing");

            if (this.IsDisposed)
            {
                return;
            }

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

                // If the controller is already bound, CameraX rebinds to the newly selected camera synchronously.
                this.UpdateCameraStateObserver();

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

            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                if (!await this.cameraPermissions.CheckPermissionAsync())
                {
                    this.logger.LogInformation("UpdateCameraAsync: Camera permission not granted");
                    return;
                }

                // The camera view may have been removed while we were waiting for the permission check.
                if (this.IsDisposed)
                {
                    this.logger.LogDebug("StartAsync: CameraManager was disposed while checking camera permission");
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

                    this.UpdateCameraStateObserver();
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
            if (this.IsDisposed)
            {
                return;
            }

            if (this.cameraController is LifecycleCameraController lifecycleCameraController)
            {
                var resolution = this.GetTargetResolution();

                if (!resolution.Equals(lifecycleCameraController.ImageAnalysisTargetSize?.Resolution))
                {
                    lifecycleCameraController.ImageAnalysisTargetSize = new CameraController.OutputSize(resolution);

                    if (this.IsRunning)
                    {
                        try
                        {
                            await this.StartAsync();
                        }
                        catch (Exception ex)
                        {
                            // Exceptions must not escape from async void
                            this.logger.LogError(ex, "UpdateCaptureQuality failed with exception");
                        }
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

            if (this.IsDisposed || this.cameraController == null)
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
            if (this.IsDisposed)
            {
                return;
            }

            try
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
            catch (Exception ex)
            {
                // Exceptions must not escape from async void
                this.logger.LogError(ex, "UpdateCameraEnabled failed with exception");
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
            if (this.IsDisposed || this.cameraView.PauseScanning)
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
                            if (this.IsDisposed || this.barcodeScanner is not IMLKitBarcodeScanner barcodeScanner)
                            {
                                return;
                            }

                            using (var resultsArray = await barcodeScanner.Process(image))
                            {
                                var barcodeResults = Platforms.Services.BarcodeScanner.ProcessBarcodeResult(resultsArray, coordinateTransform);

                                if (this.cameraView.ForceInverted)
                                {
                                    Platforms.Services.BarcodeScanner.InvertLuminance(proxy.Image);
                                    using var imageInverted = InputImage.FromMediaImage(proxy.Image, proxy.ImageInfo.RotationDegrees);
                                    using var resultsArrayInverted = await barcodeScanner.Process(imageInverted);

                                    var barcodeResultsInverted = Platforms.Services.BarcodeScanner.ProcessBarcodeResult(resultsArrayInverted, coordinateTransform);
                                    barcodeResults.UnionWith(barcodeResultsInverted);
                                }

                                if (this.cameraView.AimMode)
                                {
                                    var previewCenter = new Point(this.previewView.Width / 2d, this.previewView.Height / 2d);
                                    barcodeResults.RemoveWhere(b => !b.PreviewBoundingBox.Contains(previewCenter));
                                }

                                if (this.cameraView.ViewfinderMode)
                                {
                                    var previewRect = new RectF(0f, 0f, this.previewView.Width, this.previewView.Height);
                                    barcodeResults.RemoveWhere(b => !previewRect.Contains(b.PreviewBoundingBox));
                                }

                                if (this.IsDisposed)
                                {
                                    return;
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
            if (this.IsDisposed)
            {
                return;
            }

            this.cameraView.CaptureNextFrame = false;
            var image = new PlatformImage(proxy.ToBitmap());
            this.cameraView.TriggerOnImageCaptured(image);
        }

        private void UpdateOutput()
        {
            if (this.cameraController != null && this.cameraExecutor != null)
            {
                this.cameraController.ClearImageAnalysisAnalyzer();
                this.barcodeAnalyzer?.Dispose();
                this.barcodeAnalyzer = null;

                var barcodeAnalyzerLogger = this.loggerFactory.CreateLogger<BarcodeAnalyzer>();
                this.barcodeAnalyzer = new BarcodeAnalyzer(barcodeAnalyzerLogger, this);
                this.cameraController.SetImageAnalysisAnalyzer(this.cameraExecutor, this.barcodeAnalyzer);
            }
        }

        private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(200);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (!this.IsDisposed && this.IsRunning && this.cameraView.CameraEnabled)
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
            if (!disposing || Interlocked.Exchange(ref this.disposed, 1) != 0)
            {
                return;
            }

            this.deviceDisplay.MainDisplayInfoChanged -= this.OnMainDisplayInfoChanged;

            // Stop delivering frames to the analyzer before the camera is stopped and torn down.
            this.TryCleanup(() => this.cameraController.ClearImageAnalysisAnalyzer(), "clearing the image analysis analyzer");
            this.TryCleanup(this.Stop, "stopping the camera");

            // Observers must be removed from their LiveData before they are disposed.
            // Otherwise LiveData may call back into an observer whose managed peer no longer exists.
            this.cameraStateObserver.ValueChanged -= this.OnCameraStateChanged;
            if (this.cameraInfo is ICameraInfo cameraInfo)
            {
                this.TryCleanup(() => cameraInfo.CameraState.RemoveObserver(this.cameraStateObserver), "removing the camera-state observer");
            }

            // CameraInfo is owned by CameraX and may be shared with other CameraManager instances,
            // so we must not dispose it here.
            this.cameraInfo = null;
            this.TryCleanup(this.cameraStateObserver.Dispose, "disposing the camera-state observer");

            this.zoomStateObserver.ValueChanged -= this.OnZoomStateChanged;
            this.TryCleanup(() => this.cameraController.ZoomState.RemoveObserver(this.zoomStateObserver), "removing the zoom-state observer");
            this.TryCleanup(this.zoomStateObserver.Dispose, "disposing the zoom-state observer");

            this.torchStateObserver.ValueChanged -= this.OnTorchStateChanged;
            this.TryCleanup(() => this.cameraController.TorchState.RemoveObserver(this.torchStateObserver), "removing the torch-state observer");
            this.TryCleanup(this.torchStateObserver.Dispose, "disposing the torch-state observer");

            this.TryCleanup(() => this.BarcodeView?.RemoveAllViews(), "clearing the barcode view");
            this.TryCleanup(() => this.relativeLayout?.RemoveAllViews(), "clearing the camera layout");

            this.TryCleanup(() => this.BarcodeView?.Dispose(), "disposing the barcode view");
            this.TryCleanup(() => this.relativeLayout?.Dispose(), "disposing the camera layout");
            this.TryCleanup(() => this.imageView?.Dispose(), "disposing the aim image");
            this.TryCleanup(() => this.previewView?.Dispose(), "disposing the camera preview");
            this.TryCleanup(() => this.cameraController?.Dispose(), "disposing the camera controller");

            this.TryCleanup(() => this.barcodeAnalyzer?.Dispose(), "disposing the barcode analyzer");
            this.barcodeAnalyzer = null;
            this.TryCleanup(() => this.barcodeScanner?.Dispose(), "disposing the barcode scanner");
            this.barcodeScanner = null;

            this.TryCleanup(() => this.cameraExecutor?.Shutdown(), "shutting down the camera executor");
            this.TryCleanup(() => this.cameraExecutor?.Dispose(), "disposing the camera executor");
        }

        private void TryCleanup(Action cleanup, string operation)
        {
            try
            {
                cleanup();
            }
            catch (ObjectDisposedException ex)
            {
                this.logger.LogDebug(ex, $"Dispose: Camera resource was already disposed while {operation}");
            }
            catch (Java.Lang.Exception ex)
            {
                this.logger.LogDebug(ex, $"Dispose: Failed while {operation}");
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