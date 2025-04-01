using AVFoundation;
using CameraScanner.Maui;
using CameraScanner.Maui.Utils;
using CoreAnimation;
using CoreFoundation;
using CoreGraphics;
using CoreImage;
using CoreMedia;
using CoreVideo;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics.Platform;
using UIKit;
using Vision;

namespace CameraScanner.Maui
{
    [Preserve(AllMembers = true)]
    internal class CameraManager : IDisposable
    {
        private readonly string instance = new Guid().ToString().Substring(0, 5).ToUpperInvariant();
        private static readonly AVCaptureDeviceType[] SupportedCaptureDeviceTypes = InitializeCaptureDevices();

        private readonly AsyncLock updateCameraLock = new AsyncLock();

        internal BarcodeView BarcodeView => this.barcodeView;

        internal bool CaptureNextFrame => this.cameraView.CaptureNextFrame;

        private bool started;
        private bool disposed;
        private AVCaptureDevice captureDevice;
        private AVCaptureInput captureInput;
        private BarcodeAnalyzer barcodeAnalyzer;

        private AVCaptureVideoDataOutput videoDataOutput;
        private readonly AVCaptureVideoPreviewLayer previewLayer;
        private AVCaptureSession captureSession;
        private readonly BarcodeView barcodeView;
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly ICameraPermissions cameraPermissions;
        private readonly IDeviceInfo deviceInfo;
        private readonly CameraView cameraView;
        private readonly CAShapeLayer shapeLayer;
        private readonly NSObject subjectAreaChangedNotification;
        private readonly VNDetectBarcodesRequest detectBarcodesRequest;
        private readonly VNSequenceRequestHandler sequenceRequestHandler;
        private readonly UITapGestureRecognizer tapGestureRecognizer;

        private readonly HashSet<BarcodeResult> barcodeResults = [];
        private const int AimRadius = 8;

        internal CameraManager(
            ILogger<CameraManager> logger,
            ILoggerFactory loggerFactory,
            ICameraPermissions cameraPermissions,
            IDeviceInfo deviceInfo,
            CameraView cameraView)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.cameraPermissions = cameraPermissions;
            this.deviceInfo = deviceInfo;
            this.cameraView = cameraView;

            this.captureSession = new AVCaptureSession();
            this.sequenceRequestHandler = new VNSequenceRequestHandler();
            this.videoDataOutput = new AVCaptureVideoDataOutput { AlwaysDiscardsLateVideoFrames = true };
            this.detectBarcodesRequest = new VNDetectBarcodesRequest((request, error) =>
            {
                if (error is null)
                {
                    var vnBarcodeObservations = request.GetResults<VNBarcodeObservation>();
                    Platforms.Services.BarcodeScanner.ProcessBarcodeResult(vnBarcodeObservations, this.barcodeResults, this.previewLayer);
                }
                else
                {
                    var exception = new NSErrorException(error);
                    logger.LogError(exception, "VNDetectBarcodesRequest failed with error");
                }
            });

            this.tapGestureRecognizer = new UITapGestureRecognizer(this.FocusOnTap);
            this.subjectAreaChangedNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureDevice.SubjectAreaDidChangeNotification, n =>
                {
                    if (n.Name == AVCaptureDevice.SubjectAreaDidChangeNotification)
                    {
                        this.ResetFocus();
                    }
                });

            this.previewLayer = new AVCaptureVideoPreviewLayer(this.captureSession)
            {
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };
            this.shapeLayer = new CAShapeLayer
            {
                Path = UIBezierPath.FromOval(new CGRect(-AimRadius, -AimRadius, 2 * AimRadius, 2 * AimRadius)).CGPath,
                FillColor = UIColor.Red.ColorWithAlpha(0.60f).CGColor,
                StrokeColor = UIColor.Clear.CGColor,
                LineWidth = 0
            };

            this.barcodeView = new BarcodeView(this.previewLayer, this.shapeLayer);
            this.barcodeView.Layer.AddSublayer(this.previewLayer);
            this.barcodeView.AddGestureRecognizer(this.tapGestureRecognizer);
        }

        internal async Task StartAsync()
        {
            this.logger.LogDebug("StartAsync");

            try
            {
                if (this.started)
                {
                    this.Stop();
                }

                if (this.videoDataOutput is not AVCaptureVideoDataOutput videoDataOutput)
                {
                    return;
                }

                if (this.captureSession is not AVCaptureSession captureSession)
                {
                    return;
                }

                if (captureSession.Running)
                {
                    captureSession.StopRunning();
                }

                if (captureSession.Inputs.Length == 0)
                {
                    await this.UpdateCameraAsync();
                }

                if (captureSession.SessionPreset is null)
                {
                    this.UpdateCaptureQuality();
                }

                if (!captureSession.Outputs.Contains(videoDataOutput) &&
                    captureSession.CanAddOutput(videoDataOutput))
                {
                    ConfigureCaptureSession(captureSession, cs => cs.AddOutput(videoDataOutput));
                }

                captureSession.StartRunning();
                this.UpdateOutput();
                this.UpdateBarcodeFormats();
                this.UpdateTorch();
                this.UpdateMinMaxZoomFactor();
                this.UpdateDeviceSwitchZoomFactors();
                this.UpdateBarcodeDetectionFrameRate();

                this.started = true;
                this.logger.LogDebug("StartAsync finished successfully");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "StartAsync failed with exception");
                throw;
            }
        }

        internal void Stop()
        {
            this.logger.LogDebug("Stop");

            try
            {
                if (!this.started)
                {
                    return;
                }

                if (this.captureSession is not AVCaptureSession captureSession)
                {
                    return;
                }

                if (this.captureDevice is not AVCaptureDevice captureDevice)
                {
                    return;
                }

                if (captureDevice.TorchActive)
                {
                    this.SetTorchModeOn(AVCaptureTorchMode.Off);

                    if (this.cameraView is not null)
                    {
                        this.cameraView.TorchOn = false;
                    }
                }

                if (captureSession.Running)
                {
                    captureSession.StopRunning();
                }

                this.started = false;
                this.logger.LogDebug("Stop finished successfully");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Stop failed with exception");
                throw;
            }
        }

        internal void UpdateCaptureQuality()
        {
            this.logger.LogDebug("UpdateCaptureQuality");

            if (this.disposed)
            {
                return;
            }

            if (this.captureSession is not AVCaptureSession captureSession)
            {
                return;
            }

            ConfigureCaptureSession(captureSession, cs =>
            {
                cs.SessionPreset = GetBestSupportedPreset(captureSession, this.cameraView.CaptureQuality);
            });
        }

        private static NSString GetBestSupportedPreset(AVCaptureSession captureSession, CaptureQuality captureQuality)
        {
            ArgumentNullException.ThrowIfNull(captureSession);

            while (!captureSession.CanSetSessionPreset(SessionPresetTranslator(captureQuality)) && captureQuality != CaptureQuality.Low)
            {
                captureQuality -= 1;
            }

            return SessionPresetTranslator(captureQuality);
        }

        private static NSString SessionPresetTranslator(CaptureQuality captureQuality)
        {
            return captureQuality switch
            {
                CaptureQuality.Low => AVCaptureSession.Preset640x480,
                CaptureQuality.Medium => AVCaptureSession.Preset1280x720,
                CaptureQuality.High => AVCaptureSession.Preset1920x1080,
                CaptureQuality.Highest => AVCaptureSession.Preset3840x2160,
                _ => AVCaptureSession.Preset1280x720
            };
        }

        internal void UpdateBarcodeFormats()
        {
            this.logger.LogDebug("UpdateBarcodeFormats");

            if (this.disposed)
            {
                return;
            }

            if (this.detectBarcodesRequest is not null && this.cameraView.BarcodeFormats is BarcodeFormats barcodeFormats)
            {
                var vnBarcodeSymbologies = barcodeFormats.ToPlatform();
                this.detectBarcodesRequest.Symbologies = vnBarcodeSymbologies;
            }
        }

        internal async void UpdateCameraFacing()
        {
            this.logger.LogDebug("UpdateCameraFacing");
            await this.UpdateCameraAsync();
        }

        internal async Task UpdateCameraAsync()
        {
            using (await this.updateCameraLock.LockAsync())
            {
                this.logger.LogDebug("UpdateCameraAsync");

                if (this.deviceInfo.DeviceType == DeviceType.Virtual)
                {
                    this.logger.LogInformation("UpdateCameraAsync: No camera available on iOS simulator.");
                    return;
                }

                if (!await this.cameraPermissions.CheckPermissionAsync())
                {
                    this.logger.LogInformation("UpdateCameraAsync: Camera permission not granted");
                    return;
                }

                if (this.captureSession != null)
                {
                    try
                    {
                        const AVMediaTypes mediaType = AVMediaTypes.Video;

                        var captureDevicePosition = MapCameraFacing(this.cameraView.CameraFacing);
                        float? virtualDeviceSwitchOverVideoZoomFactor;

                        using (var captureDeviceDiscoverySession = AVCaptureDeviceDiscoverySession.Create(
                                   SupportedCaptureDeviceTypes, mediaType, captureDevicePosition))
                        {
                            // Find the camera with the most virtual device switch overs.
                            // This is the virtual camera which allows to zoom through all physical cameras.
                            // More info: https://developer.apple.com/documentation/avfoundation/avcaptureprimaryconstituentdevicerestrictedswitchingbehaviorconditions
                            var selectedCaptureDevice = captureDeviceDiscoverySession.Devices
                                .OrderByDescending(d => d.VirtualDeviceSwitchOverVideoZoomFactors.Length)
                                .FirstOrDefault();

                            virtualDeviceSwitchOverVideoZoomFactor = selectedCaptureDevice.VirtualDeviceSwitchOverVideoZoomFactors
                                .FirstOrDefault()?.FloatValue;

                            this.captureDevice = selectedCaptureDevice;
                        }

                        if (this.captureDevice == null)
                        {
                            throw new Exception("No AVCaptureDevice could be found");
                        }

                        ConfigureCaptureSession(this.captureSession, cs =>
                        {
                            if (this.captureInput != null)
                            {
                                if (this.captureSession.Inputs.Contains(this.captureInput))
                                {
                                    this.captureSession.RemoveInput(this.captureInput);
                                }

                                this.captureInput.Dispose();
                            }

                            this.captureInput = new AVCaptureDeviceInput(this.captureDevice, out _);

                            if (this.captureSession.CanAddInput(this.captureInput))
                            {
                                this.captureSession.AddInput(this.captureInput);
                            }

                            this.captureSession.SessionPreset = GetBestSupportedPreset(this.captureSession, this.cameraView.CaptureQuality);
                        });

                        this.UpdateMinMaxZoomFactor();
                        this.UpdateDeviceSwitchZoomFactors();

                        if (this.cameraView.RequestZoomFactor is float requestZoomFactor and > 1F)
                        {
                            // Set requested zoom
                            this.SetVideoZoomFactor(requestZoomFactor);
                        }
                        else if (virtualDeviceSwitchOverVideoZoomFactor is float defaultCameraZoom
                                 and > 1F)
                        {
                            // Set default zoom
                            this.SetVideoZoomFactor(defaultCameraZoom);
                        }

                        this.UpdateCurrentZoomFactor();

                        this.ResetFocus();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "UpdateCameraAsync failed with exception");
                        throw;
                    }
                }
            }
        }

        private static AVCaptureDeviceType[] InitializeCaptureDevices()
        {
            AVCaptureDeviceType[] deviceTypes =
            [
                AVCaptureDeviceType.BuiltInWideAngleCamera,
                AVCaptureDeviceType.BuiltInTelephotoCamera,
                AVCaptureDeviceType.BuiltInDualCamera
            ];

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 1))
            {
                deviceTypes = [.. deviceTypes, AVCaptureDeviceType.BuiltInTrueDepthCamera];
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                deviceTypes =
                [
                    .. deviceTypes,
                    AVCaptureDeviceType.BuiltInUltraWideCamera,
                    AVCaptureDeviceType.BuiltInTripleCamera,
                    AVCaptureDeviceType.BuiltInDualWideCamera
                ];
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 4))
            {
                deviceTypes = [.. deviceTypes, AVCaptureDeviceType.BuiltInLiDarDepthCamera];
            }

            return deviceTypes;
        }

        private static AVCaptureDevicePosition MapCameraFacing(CameraFacing cameraViewCameraFacing)
        {
            var cameraFacing = cameraViewCameraFacing == CameraFacing.Back
                ? AVCaptureDevicePosition.Back
                : AVCaptureDevicePosition.Front;

            return cameraFacing;
        }

        internal void UpdateTorch()
        {
            this.logger.LogDebug("UpdateTorch");

            if (this.disposed)
            {
                return;
            }

            if (this.captureDevice is not null && this.captureDevice.HasTorch && this.captureDevice.TorchAvailable)
            {
                if (this.cameraView.TorchOn)
                {
                    this.SetTorchModeOn(AVCaptureTorchMode.On);
                }
                else
                {
                    this.SetTorchModeOn(AVCaptureTorchMode.Off);
                }
            }
        }

        private void SetTorchModeOn(AVCaptureTorchMode torchMode)
        {
            CaptureDeviceLock(this.captureDevice, () =>
            {
                if (this.captureDevice.IsTorchModeSupported(torchMode))
                {
                    this.captureDevice.TorchMode = torchMode;
                }
            });
        }

        private void UpdateDeviceSwitchZoomFactors()
        {
            this.logger.LogDebug("UpdateDeviceSwitchZoomFactors");

            if (this.disposed)
            {
                return;
            }

            if (this.cameraView is not CameraView cameraView)
            {
                return;
            }

            if (this.captureDevice is not AVCaptureDevice captureDevice)
            {
                return;
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                if (captureDevice.VirtualDeviceSwitchOverVideoZoomFactors is NSNumber[] zoomFactors)
                {
                    cameraView.DeviceSwitchZoomFactors = zoomFactors
                        .Where(z => z != null)
                        .Select(z => z.FloatValue)
                        .ToArray();
                }
            }
        }

        internal void UpdateRequestZoomFactor()
        {
            this.logger.LogDebug("UpdateRequestZoomFactor");

            if (this.disposed)
            {
                return;
            }

            if (this.cameraView is null || this.captureDevice is null)
            {
                return;
            }

            if (this.cameraView.RequestZoomFactor is float requestZoomFactor and > 0F)
            {
                this.SetVideoZoomFactor(requestZoomFactor);
            }
        }

        private void SetVideoZoomFactor(float requestZoomFactor)
        {
            if (this.disposed)
            {
                return;
            }

            var videoZoomFactor = Math.Max(requestZoomFactor, (float)this.captureDevice.MinAvailableVideoZoomFactor);
            videoZoomFactor = Math.Min(videoZoomFactor, (float)this.captureDevice.MaxAvailableVideoZoomFactor);

            CaptureDeviceLock(this.captureDevice, () =>
            {
                this.captureDevice.VideoZoomFactor = videoZoomFactor;
                this.UpdateCurrentZoomFactor();
            });
        }

        private void UpdateCurrentZoomFactor()
        {
            if (this.disposed)
            {
                return;
            }

            this.cameraView.CurrentZoomFactor = (float)this.captureDevice.VideoZoomFactor;
        }

        private void UpdateMinMaxZoomFactor()
        {
            if (this.disposed)
            {
                return;
            }

            if (this.cameraView is not null && this.captureDevice is not null)
            {
                this.cameraView.MinZoomFactor = (float)this.captureDevice.MinAvailableVideoZoomFactor;
                this.cameraView.MaxZoomFactor = (float)this.captureDevice.MaxAvailableVideoZoomFactor;
            }
        }

        internal void UpdateBarcodeDetectionFrameRate()
        {
            this.logger.LogDebug("UpdateBarcodeDetectionFrameRate");

            if (this.disposed)
            {
                return;
            }

            if (this.barcodeAnalyzer is BarcodeAnalyzer b)
            {
                b.BarcodeDetectionFrameRate = this.cameraView.BarcodeDetectionFrameRate;
            }
        }

        internal async void UpdateCameraEnabled()
        {
            this.logger.LogDebug("UpdateCameraEnabled");

            if (this.disposed)
            {
                return;
            }

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

            if (this.disposed)
            {
                return;
            }

            if (this.barcodeAnalyzer is BarcodeAnalyzer b)
            {
                b.PauseScanning = this.cameraView.PauseScanning;
            }
        }

        internal void UpdateAimMode()
        {
            this.logger.LogDebug("UpdateAimMode");

            if (this.disposed)
            {
                return;
            }

            if (this.cameraView.AimMode)
            {
                this.barcodeView?.Layer?.AddSublayer(this.shapeLayer);
            }
            else
            {
                this.shapeLayer?.RemoveFromSuperLayer();
            }
        }

        internal void UpdateTapToFocusEnabled()
        {
            this.logger.LogDebug("UpdateTapToFocusEnabled");
        }

        internal void PerformBarcodeDetection(CVPixelBuffer cvPixelBuffer)
        {
            // this.logger.LogDebug("PerformBarcodeDetection");

            try
            {
                if (this.disposed)
                {
                    return;
                }

                if (this.cameraView.PauseScanning)
                {
                    return;
                }

                this.barcodeResults.Clear();

                this.sequenceRequestHandler?.Perform([this.detectBarcodesRequest], cvPixelBuffer, out _);

                if (this.cameraView.AimMode)
                {
                    var previewCenter = new Point(this.previewLayer.Bounds.Width / 2, this.previewLayer.Bounds.Height / 2);

                    foreach (var barcode in this.barcodeResults)
                    {
                        if (!barcode.PreviewBoundingBox.Contains(previewCenter))
                        {
                            this.barcodeResults.Remove(barcode);
                        }
                    }
                }

                if (this.cameraView.ViewfinderMode)
                {
                    var previewRect = new RectF(0, 0, (float)this.previewLayer.Bounds.Width, (float)this.previewLayer.Bounds.Height);

                    foreach (var barcode in this.barcodeResults)
                    {
                        if (!previewRect.Contains(barcode.PreviewBoundingBox))
                        {
                            this.barcodeResults.Remove(barcode);
                        }
                    }
                }

                this.cameraView.DetectionFinished(this.barcodeResults);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PerformBarcodeDetection failed with exception");
            }
        }

        internal void CaptureImage(CMSampleBuffer sampleBuffer)
        {
            this.cameraView.CaptureNextFrame = false;
            using var imageBuffer = sampleBuffer.GetImageBuffer();
            using var cIImage = new CIImage(imageBuffer);
            using var cIContext = new CIContext();
            using var cGImage = cIContext.CreateCGImage(cIImage, cIImage.Extent);
            var image = new PlatformImage(new UIImage(cGImage));
            this.cameraView.TriggerOnImageCaptured(image);
        }

        private void FocusOnTap()
        {
            if (this.disposed)
            {
                return;
            }

            if (this.cameraView.TapToFocusEnabled && this.captureDevice is { FocusPointOfInterestSupported: true } c)
            {
                CaptureDeviceLock(this.captureDevice, () =>
                {
                    var locationInView = this.tapGestureRecognizer.LocationInView(this.barcodeView);
                    c.FocusPointOfInterest = this.previewLayer.CaptureDevicePointOfInterestForPoint(locationInView);
                    c.FocusMode = AVCaptureFocusMode.AutoFocus;
                    c.SubjectAreaChangeMonitoringEnabled = true;
                });
            }
        }

        private void ResetFocus()
        {
            if (this.disposed)
            {
                return;
            }

            if (this.captureDevice is not null)
            {
                CaptureDeviceLock(this.captureDevice, () =>
                {
                    if (this.captureDevice.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                    {
                        this.captureDevice.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                    }
                    else if (this.captureDevice.IsFocusModeSupported(AVCaptureFocusMode.AutoFocus))
                    {
                        this.captureDevice.FocusMode = AVCaptureFocusMode.AutoFocus;
                    }

                    this.captureDevice.SubjectAreaChangeMonitoringEnabled = false;
                });
            }
        }

        private void UpdateOutput()
        {
            this.logger.LogDebug("UpdateOutput");

            if (this.disposed)
            {
                return;
            }

            if (this.videoDataOutput is not null)
            {
                this.videoDataOutput.SetSampleBufferDelegate(null, null);

                this.barcodeAnalyzer?.Dispose();
                this.barcodeAnalyzer = null;

                var barcodeAnalyzerLogger = this.loggerFactory.CreateLogger<BarcodeAnalyzer>();
                this.barcodeAnalyzer = new BarcodeAnalyzer(barcodeAnalyzerLogger, this);

                this.videoDataOutput.SetSampleBufferDelegate(this.barcodeAnalyzer, DispatchQueue.DefaultGlobalQueue);
            }
        }

        private static void CaptureDeviceLock(AVCaptureDevice captureDevice, Action action)
        {
            // DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (captureDevice.LockForConfiguration(out _))
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        captureDevice.UnlockForConfiguration();
                    }
                }
            } //);
        }

        private static void ConfigureCaptureSession(AVCaptureSession captureSession, Action<AVCaptureSession> action)
        {
            // DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                captureSession.BeginConfiguration();

                try
                {
                    action(captureSession);
                }
                finally
                {
                    captureSession.CommitConfiguration();
                }
            } //);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            if (disposing)
            {
                try
                {
                    this.logger.LogDebug("Dispose");

                    this.Stop();

                    //await Task.Delay(200);

                    // this.logger.LogDebug("RemoveObserver");
                    NSNotificationCenter.DefaultCenter.RemoveObserver(this.subjectAreaChangedNotification);

                    if (this.tapGestureRecognizer is not null)
                    {
                        // this.logger.LogDebug("RemoveGestureRecognizer");
                        this.barcodeView?.RemoveGestureRecognizer(this.tapGestureRecognizer);
                    }

                    // this.logger.LogDebug("SetSampleBufferDelegate");
                    this.videoDataOutput?.SetSampleBufferDelegate(null, null);

                    // this.logger.LogDebug("barcodeAnalyzer.Dispose");
                    this.barcodeAnalyzer?.Dispose();
                    this.barcodeAnalyzer = null;

                    // this.logger.LogDebug("previewLayer.RemoveFromSuperLayer");
                    this.previewLayer?.RemoveFromSuperLayer();
                    // this.logger.LogDebug("shapeLayer.RemoveFromSuperLayer");
                    this.shapeLayer?.RemoveFromSuperLayer();

                    // this.logger.LogDebug("barcodeView.Dispose");
                    this.barcodeView?.Dispose();
                    // this.logger.LogDebug("previewLayer.Dispose");
                    this.previewLayer?.Dispose();
                    // this.logger.LogDebug("shapeLayer.Dispose");
                    this.shapeLayer?.Dispose();

                    // this.logger.LogDebug("captureSession.Dispose");
                    this.captureSession?.Dispose();
                    this.captureSession = null;

                    // this.logger.LogDebug("videoDataOutput.Dispose");
                    this.videoDataOutput?.Dispose();
                    this.videoDataOutput = null;

                    // this.logger.LogDebug("captureInput.Dispose");
                    this.captureInput?.Dispose();
                    this.captureInput = null;

                    // this.logger.LogDebug("captureDevice.Dispose");
                    this.captureDevice?.Dispose();
                    this.captureDevice = null;

                    // this.logger.LogDebug("sequenceRequestHandler.Dispose");
                    this.sequenceRequestHandler?.Dispose();

                    // this.logger.LogDebug("detectBarcodesRequest.Dispose");
                    this.detectBarcodesRequest?.Dispose();

                    // this.logger.LogDebug("tapGestureRecognizer.Dispose");
                    this.tapGestureRecognizer?.Dispose();

                    // this.logger.LogDebug("subjectAreaChangedNotification.Dispose");
                    this.subjectAreaChangedNotification?.Dispose();

                    this.logger.LogDebug("Dispose finished");
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Dispose failed with exception");
                }
            }
        }
    }
}