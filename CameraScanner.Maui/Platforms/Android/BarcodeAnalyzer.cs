﻿using AndroidX.Camera.Core;
using Microsoft.Extensions.Logging;
using Size = Android.Util.Size;

namespace CameraScanner.Maui
{
    internal class BarcodeAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
    {
        private readonly ILogger<BarcodeAnalyzer> logger;
        private readonly CameraManager cameraManager;

        private uint? skippedFrames;
        private readonly SyncHelper syncHelper;

        internal BarcodeAnalyzer(
            ILogger<BarcodeAnalyzer> logger,
            CameraManager cameraManager)
        {
            this.logger = logger;
            this.cameraManager = cameraManager;
            this.syncHelper = new SyncHelper();
        }

        public Size DefaultTargetResolution => this.cameraManager.GetTargetResolution();

        public int TargetCoordinateSystem => ImageAnalysis.CoordinateSystemOriginal;

        public uint? BarcodeDetectionFrameRate { get; set; }

        public async void Analyze(IImageProxy proxyImage)
        {
            try
            {
                if (this.BarcodeDetectionFrameRate is not uint r || r is 0u or 1u || this.skippedFrames == null || ++this.skippedFrames >= r)
                {
                    this.logger.LogDebug("Analyze");

                    if (this.cameraManager.CaptureNextFrame)
                    {
                        this.cameraManager.CaptureImage(proxyImage);
                    }
                    else
                    {
                        var run = await this.syncHelper.RunOnceAsync(() => this.cameraManager.PerformBarcodeDetectionAsync(proxyImage));
                        if (run == false)
                        {
                            this.logger.LogDebug("Analyze -> frame skipped (already in progress)");
                        }
                    }

                    if (this.BarcodeDetectionFrameRate != null)
                    {
                        this.skippedFrames = 0;
                    }
                }
                else
                {
                    this.logger.LogDebug("Analyze -> frame skipped (BarcodeDetectionFrameRate)");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Analyze failed with exception");
            }
            finally
            {
                try
                {
                    proxyImage.Close();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Analyze -> IImageProxy.Close failed with exception");
                    MainThread.BeginInvokeOnMainThread(() => this.cameraManager.Start());
                }
            }
        }
    }
}