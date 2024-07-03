using AVFoundation;
using CoreAnimation;
using CoreFoundation;
using CoreGraphics;
using CoreImage;
using CoreMedia;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using System.Diagnostics;
using CoreVideo;
using Microsoft.Extensions.Logging;
using UIKit;
using Vision;
using CameraScanner.Maui;

namespace CameraScanner.Maui
{
    internal class CameraManager : IDisposable
    {
        internal BarcodeView BarcodeView => this.barcodeView;

        internal bool CaptureNextFrame => this.cameraView.CaptureNextFrame;

        private AVCaptureDevice captureDevice;
        private AVCaptureInput captureInput;
        private BarcodeAnalyzer barcodeAnalyzer;

        private readonly AVCaptureVideoDataOutput videoDataOutput;
        private readonly AVCaptureVideoPreviewLayer previewLayer;
        private readonly AVCaptureSession captureSession;
        private readonly BarcodeView barcodeView;
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly CameraView cameraView;
        private readonly CAShapeLayer shapeLayer;
        private readonly DispatchQueue dispatchQueue;
        private readonly NSObject subjectAreaChangedNotification;
        private readonly VNDetectBarcodesRequest detectBarcodesRequest;
        private readonly VNSequenceRequestHandler sequenceRequestHandler;
        private readonly UITapGestureRecognizer tapGestureRecognizer;

        private readonly HashSet<BarcodeResult> barcodeResults = [];
        private const int AimRadius = 8;

        internal CameraManager(
            ILogger<CameraManager> logger,
            ILoggerFactory loggerFactory,
            CameraView cameraView)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.cameraView = cameraView;

            this.captureSession = new AVCaptureSession();
            this.sequenceRequestHandler = new VNSequenceRequestHandler();
            this.dispatchQueue = new DispatchQueue("com.barcodescanning.maui.sessionQueue",
                new DispatchQueue.Attributes { QualityOfService = DispatchQualityOfService.UserInitiated });
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
                    // TODO: Error handling
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

            this.previewLayer = new AVCaptureVideoPreviewLayer(this.captureSession) { VideoGravity = AVLayerVideoGravity.ResizeAspectFill };
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

        internal void Start()
        {
            if (this.captureSession is not null)
            {
                if (this.captureSession.Running)
                {
                    this.dispatchQueue.DispatchAsync(this.captureSession.StopRunning);
                }

                if (this.captureSession.Inputs.Length == 0)
                {
                    this.UpdateCamera();
                }

                if (this.captureSession.SessionPreset is null)
                {
                    this.UpdateCaptureQuality();
                }

                if (!this.captureSession.Outputs.Contains(this.videoDataOutput) &&
                    this.captureSession.CanAddOutput(this.videoDataOutput))
                {
                    this.dispatchQueue.DispatchAsync(() =>
                    {
                        this.captureSession.BeginConfiguration();
                        this.captureSession.AddOutput(this.videoDataOutput);
                        this.captureSession.CommitConfiguration();
                    });
                }

                this.dispatchQueue.DispatchAsync(() =>
                {
                    this.captureSession.StartRunning();
                    this.UpdateOutput();
                    this.UpdateBarcodeFormats();
                    this.UpdateTorch();
                    this.UpdateZoomFactor();
                    this.UpdateBarcodeDetectionFrameRate();
                });
            }
        }

        internal void Stop()
        {
            if (this.captureSession is not null)
            {
                if (this.captureDevice is not null && this.captureDevice.TorchActive)
                {
                    this.CaptureDeviceLock(this.captureDevice, () => this.captureDevice.TorchMode = AVCaptureTorchMode.Off);

                    if (this.cameraView is not null)
                    {
                        this.cameraView.TorchOn = false;
                    }
                }

                if (this.captureSession.Running)
                {
                    this.dispatchQueue.DispatchAsync(this.captureSession.StopRunning);
                }
            }
        }

        internal void UpdateCaptureQuality()
        {
            if (this.captureSession is not null)
            {
                this.dispatchQueue.DispatchAsync(() =>
                {
                    this.captureSession.BeginConfiguration();
                    this.captureSession.SessionPreset = GetBestSupportedPreset(this.captureSession, this.cameraView.CaptureQuality);
                    this.captureSession.CommitConfiguration();
                });
            }
        }

        private static NSString GetBestSupportedPreset(AVCaptureSession captureSession, CaptureQuality quality)
        {
            ArgumentNullException.ThrowIfNull(captureSession);

            while (!captureSession.CanSetSessionPreset(SessionPresetTranslator(quality)) && quality != CaptureQuality.Low)
            {
                quality -= 1;
            }

            return SessionPresetTranslator(quality);
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
            if (this.detectBarcodesRequest is not null)
            {
                this.detectBarcodesRequest.Symbologies = MapBarcodeFormats(this.cameraView.BarcodeFormats);
            }
        }

        internal static VNBarcodeSymbology[] MapBarcodeFormats(BarcodeFormats barcodeFormats)
        {
            if (barcodeFormats.HasFlag(BarcodeFormats.All))
            {
                return [];
            }

            var symbologiesList = new List<VNBarcodeSymbology>();

            if (barcodeFormats.HasFlag(BarcodeFormats.Code128))
            {
                symbologiesList.Add(VNBarcodeSymbology.Code128);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Code39))
            {
                symbologiesList.Add(VNBarcodeSymbology.Code39);
                symbologiesList.Add(VNBarcodeSymbology.Code39Checksum);
                symbologiesList.Add(VNBarcodeSymbology.Code39FullAscii);
                symbologiesList.Add(VNBarcodeSymbology.Code39FullAsciiChecksum);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Code93))
            {
                symbologiesList.Add(VNBarcodeSymbology.Code93);
                symbologiesList.Add(VNBarcodeSymbology.Code93i);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.CodaBar))
            {
                symbologiesList.Add(VNBarcodeSymbology.Codabar);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.DataMatrix))
            {
                symbologiesList.Add(VNBarcodeSymbology.DataMatrix);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Ean13))
            {
                symbologiesList.Add(VNBarcodeSymbology.Ean13);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Ean8))
            {
                symbologiesList.Add(VNBarcodeSymbology.Ean8);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Itf))
            {
                symbologiesList.Add(VNBarcodeSymbology.Itf14);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.QRCode))
            {
                symbologiesList.Add(VNBarcodeSymbology.QR);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Upca))
            {
                symbologiesList.Add(VNBarcodeSymbology.Ean13);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Upce))
            {
                symbologiesList.Add(VNBarcodeSymbology.Upce);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Pdf417))
            {
                symbologiesList.Add(VNBarcodeSymbology.Pdf417);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Aztec))
            {
                symbologiesList.Add(VNBarcodeSymbology.Aztec);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.MicroQR))
            {
                symbologiesList.Add(VNBarcodeSymbology.MicroQR);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.MicroPdf417))
            {
                symbologiesList.Add(VNBarcodeSymbology.MicroPdf417);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.I2OF5))
            {
                symbologiesList.Add(VNBarcodeSymbology.I2OF5);
                symbologiesList.Add(VNBarcodeSymbology.I2OF5Checksum);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.GS1DataBar))
            {
                symbologiesList.Add(VNBarcodeSymbology.GS1DataBar);
                symbologiesList.Add(VNBarcodeSymbology.GS1DataBarLimited);
                symbologiesList.Add(VNBarcodeSymbology.GS1DataBarExpanded);
            }

            return symbologiesList.ToArray();
        }

        internal void UpdateCamera()
        {
            if (this.captureSession is not null)
            {
                this.dispatchQueue.DispatchAsync(() =>
                {
                    this.captureSession.BeginConfiguration();

                    if (this.captureInput is not null)
                    {
                        if (this.captureSession.Inputs.Contains(this.captureInput))
                        {
                            this.captureSession.RemoveInput(this.captureInput);
                        }

                        this.captureInput.Dispose();
                    }

                    // _captureDevice?.Dispose();

                    // TODO: Optimize camera evaluation process
                    var mediaType = AVMediaTypes.Video;
                    using (var captureDeviceDiscoverySession = AVCaptureDeviceDiscoverySession.Create(
                               new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera }, mediaType,
                               AVCaptureDevicePosition.Unspecified))
                    {
                        var cameraFacing = this.cameraView.CameraFacing == CameraFacing.Back
                            ? AVCaptureDevicePosition.Back
                            : AVCaptureDevicePosition.Front;
                        var captureDevices = captureDeviceDiscoverySession.Devices;
                        this.captureDevice = captureDevices.FirstOrDefault(c => c.Position == cameraFacing) ??
                                             AVCaptureDevice.GetDefaultDevice(mediaType);
                    }

                    if (this.cameraView.CameraFacing == CameraFacing.Front)
                    {
                        this.captureDevice = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera,
                            AVMediaTypes.Video, AVCaptureDevicePosition.Front);
                    }
                    else
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                        {
                            this.captureDevice = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInTripleCamera,
                                AVMediaTypes.Video, AVCaptureDevicePosition.Back);
                            this.captureDevice ??= AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInDualWideCamera,
                                AVMediaTypes.Video, AVCaptureDevicePosition.Back);
                        }

                        this.captureDevice ??= AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInDualCamera, AVMediaTypes.Video,
                            AVCaptureDevicePosition.Back);
                        this.captureDevice ??= AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera,
                            AVMediaTypes.Video, AVCaptureDevicePosition.Back);
                    }

                    this.captureDevice ??= AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);

                    if (this.captureDevice is not null)
                    {
                        this.captureInput = new AVCaptureDeviceInput(this.captureDevice, out _);

                        if (this.captureSession.CanAddInput(this.captureInput))
                        {
                            this.captureSession.AddInput(this.captureInput);
                        }
                    }

                    this.captureSession.SessionPreset = GetBestSupportedPreset(this.captureSession, this.cameraView.CaptureQuality);
                    this.captureSession.CommitConfiguration();

                    this.UpdateZoomFactor();
                    this.ResetFocus();
                });
            }
        }

        internal void UpdateTorch()
        {
            if (this.captureDevice is not null && this.captureDevice.HasTorch && this.captureDevice.TorchAvailable)
            {
                if (this.cameraView.TorchOn)
                {
                    this.CaptureDeviceLock(this.captureDevice, () =>
                    {
                        if (this.captureDevice.IsTorchModeSupported(AVCaptureTorchMode.On))
                        {
                            this.captureDevice.TorchMode = AVCaptureTorchMode.On;
                        }
                    });
                }
                else
                {
                    this.CaptureDeviceLock(this.captureDevice, () =>
                    {
                        if (this.captureDevice.IsTorchModeSupported(AVCaptureTorchMode.Off))
                        {
                            this.captureDevice.TorchMode = AVCaptureTorchMode.Off;
                        }
                    });
                }
            }
        }

        internal void UpdateZoomFactor()
        {
            if (this.cameraView is not null && this.captureDevice is not null)
            {
                this.cameraView.MinZoomFactor = (float)this.captureDevice.MinAvailableVideoZoomFactor;
                this.cameraView.MaxZoomFactor = (float)this.captureDevice.MaxAvailableVideoZoomFactor;

                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    this.cameraView.DeviceSwitchZoomFactor =
                        this.captureDevice.VirtualDeviceSwitchOverVideoZoomFactors?.Select(s => (float)s).ToArray() ?? [];
                }

                var factor = this.cameraView.RequestZoomFactor;

                if (factor > 0)
                {
                    factor = Math.Max(factor, this.cameraView.MinZoomFactor);
                    factor = Math.Min(factor, this.cameraView.MaxZoomFactor);

                    this.CaptureDeviceLock(this.captureDevice, () =>
                    {
                        this.captureDevice.VideoZoomFactor = factor;
                        this.cameraView.CurrentZoomFactor = factor;
                    });
                }
            }
        }

        internal void UpdateBarcodeDetectionFrameRate()
        {
            if (this.barcodeAnalyzer is BarcodeAnalyzer b)
            {
                b.BarcodeDetectionFrameRate = this.cameraView.BarcodeDetectionFrameRate;
            }
        }

        internal void UpdateCameraEnabled()
        {
            if (this.cameraView.CameraEnabled)
            {
                this.Start();
            }
            else
            {
                this.Stop();
            }
        }

        internal void UpdateAimMode()
        {
            if (this.cameraView.AimMode)
            {
                this.barcodeView?.Layer?.AddSublayer(this.shapeLayer);
            }
            else
            {
                this.shapeLayer?.RemoveFromSuperLayer();
            }
        }

        internal void UpdateTapToFocusEnabled() { }

        internal void PerformBarcodeDetection(CVPixelBuffer cvPixelBuffer)
        {
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
            if (this.cameraView.TapToFocusEnabled && this.captureDevice is { FocusPointOfInterestSupported: true } c)
            {
                this.CaptureDeviceLock(this.captureDevice, () =>
                {
                    c.FocusPointOfInterest =
                        this.previewLayer.CaptureDevicePointOfInterestForPoint(this.tapGestureRecognizer.LocationInView(this.barcodeView));
                    c.FocusMode = AVCaptureFocusMode.AutoFocus;
                    c.SubjectAreaChangeMonitoringEnabled = true;
                });
            }
        }

        private void ResetFocus()
        {
            if (this.captureDevice is not null)
            {
                this.CaptureDeviceLock(this.captureDevice, () =>
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

        private void CaptureDeviceLock(AVCaptureDevice captureDevice, Action action)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (captureDevice.LockForConfiguration(out _))
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "CaptureDeviceLock failed with exception");
                    }
                    finally
                    {
                        captureDevice.UnlockForConfiguration();
                    }
                }
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
                this.Stop();

                if (this.subjectAreaChangedNotification is not null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(this.subjectAreaChangedNotification);
                }

                if (this.tapGestureRecognizer is not null)
                {
                    this.barcodeView?.RemoveGestureRecognizer(this.tapGestureRecognizer);
                }

                this.videoDataOutput?.SetSampleBufferDelegate(null, null);

                this.previewLayer?.RemoveFromSuperLayer();
                this.shapeLayer?.RemoveFromSuperLayer();

                this.barcodeView?.Dispose();
                this.previewLayer?.Dispose();
                this.shapeLayer?.Dispose();

                this.captureSession?.Dispose();
                this.videoDataOutput?.Dispose();
                this.captureInput?.Dispose();

                this.barcodeAnalyzer?.Dispose();
                this.captureDevice?.Dispose();
                this.sequenceRequestHandler?.Dispose();
                this.detectBarcodesRequest?.Dispose();
                this.tapGestureRecognizer?.Dispose();
                this.subjectAreaChangedNotification?.Dispose();
                this.dispatchQueue?.Dispose();
            }
        }
    }
}