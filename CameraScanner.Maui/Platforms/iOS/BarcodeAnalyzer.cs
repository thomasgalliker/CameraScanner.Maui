using AVFoundation;
using CoreMedia;
using CoreVideo;
using Microsoft.Extensions.Logging;

namespace CameraScanner.Maui
{
    [Preserve(AllMembers = true)]
    internal class BarcodeAnalyzer : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        // private readonly ILogger logger;
        private readonly CameraManager cameraManager;

        private uint? skippedFrames;
        private uint? barcodeDetectionFrameRate;

        internal BarcodeAnalyzer(
            ILogger<BarcodeAnalyzer> logger,
            CameraManager cameraManager)
        {
            // this.logger = logger;
            this.cameraManager = cameraManager;
        }

        public uint? BarcodeDetectionFrameRate
        {
            get => this.barcodeDetectionFrameRate;
            set
            {
                this.barcodeDetectionFrameRate = value;
                this.skippedFrames = null;
            }
        }

        public bool PauseScanning { get; set; }

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            try
            {
                if (this.PauseScanning)
                {
                    return;
                }

                if (this.BarcodeDetectionFrameRate is not uint r || r is 0u or 1u || this.skippedFrames == null || ++this.skippedFrames >= r)
                {
                    // this.logger.LogDebug("DidOutputSampleBuffer");

                    if (this.cameraManager.CaptureNextFrame)
                    {
                        this.cameraManager.CaptureImage(sampleBuffer);
                    }
                    else
                    {
                        using (var pixelBuffer = sampleBuffer.GetImageBuffer())
                        {
                            if (pixelBuffer is CVPixelBuffer cvPixelBuffer)
                            {
                                try
                                {
                                    cvPixelBuffer.Lock(CVPixelBufferLock.ReadOnly); // MAYBE NEEDS READ/WRITE
                                    this.cameraManager.PerformBarcodeDetection(cvPixelBuffer);
                                }
                                finally
                                {
                                    cvPixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
                                }
                            }
                        }

                        // GC.Collect(); // TODO: Really needed?
                    }

                    if (this.BarcodeDetectionFrameRate != null)
                    {
                        this.skippedFrames = 0;
                    }
                }
                else
                {
                    // this.logger.LogDebug("DidOutputSampleBuffer -> frame skipped");
                }
            }
            catch (Exception ex)
            {
                // this.logger.LogError(ex, "DidOutputSampleBuffer failed with exception");
            }
            finally
            {
                try
                {
                    sampleBuffer.Dispose();
                }
                catch (Exception ex)
                {
                    // this.logger.LogError(ex, "DidOutputSampleBuffer -> CMSampleBuffer.Dispose failed with exception");
                    // MainThread.BeginInvokeOnMainThread(() => this.cameraManager.Start());
                }
            }
        }
    }
}