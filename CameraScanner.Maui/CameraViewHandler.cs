#if (ANDROID || IOS || MACCATALYST)
using CameraScanner.Maui;
using CameraScanner.Maui.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace CameraScanner.Maui
{
    public partial class CameraViewHandler : ViewHandler<CameraView, BarcodeView>
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private CameraManager cameraManager;

        private static readonly PropertyMapper<CameraView, CameraViewHandler> CameraViewMapper = new()
        {
            [nameof(CameraView.CameraFacing)] = (handler, _) => handler.cameraManager?.UpdateCamera(),
            [nameof(CameraView.CaptureQuality)] = (handler, _) => handler.cameraManager?.UpdateCaptureQuality(),
            [nameof(CameraView.BarcodeFormats)] = (handler, _) => handler.cameraManager?.UpdateBarcodeFormats(),
            [nameof(CameraView.TorchOn)] = (handler, _) => handler.cameraManager?.UpdateTorch(),
            [nameof(CameraView.CameraEnabled)] = (handler, _) => handler.cameraManager?.UpdateCameraEnabled(),
            [nameof(CameraView.PauseScanning)] = (handler, _) => handler.cameraManager?.UpdatePauseScanning(),
            [nameof(CameraView.AimMode)] = (handler, _) => handler.cameraManager?.UpdateAimMode(),
            [nameof(CameraView.TapToFocusEnabled)] = (handler, _) => handler.cameraManager?.UpdateTapToFocusEnabled(),
            [nameof(CameraView.RequestZoomFactor)] = (handler, _) => handler.cameraManager?.UpdateRequestZoomFactor(),
            [nameof(CameraView.BarcodeDetectionFrameRate)] = (handler, _) => handler.cameraManager?.UpdateBarcodeDetectionFrameRate()
        };

        public CameraViewHandler()
            : base(CameraViewMapper)
        {
            var loggerFactory = IPlatformApplication.Current.Services.GetService<ILoggerFactory>();
            this.logger = loggerFactory.CreateLogger<CameraViewHandler>();
            this.loggerFactory = loggerFactory;
        }

        // TODO: Make sure camera scanner is started by default (ScannerEnabled = true)
        //protected override void ConnectHandler(BarcodeView platformView)
        //{
        //    base.ConnectHandler(platformView);

        //    //if (!this.cameraManager.IsRunning && this.VirtualView.CameraEnabled)
        //    //{
        //    //    this.cameraManager.Start();
        //    //}
        //}

        protected override void ConnectHandler(BarcodeView platformView)
        {
            this.logger.LogDebug("ConnectHandler");
            base.ConnectHandler(platformView);

            if (this.VirtualView.AutoDisconnectHandler)
            {
                this.VirtualView.AddCleanUpEvent();
            }
        }

        protected override void DisconnectHandler(BarcodeView barcodeView)
        {
            try
            {
                this.logger.LogDebug("DisconnectHandler");

                // Attempt 1: Crash!
                this.cameraManager.Dispose();


                //
                // _ = Task.Run(async () =>
                // {
                //     await Task.Delay(1000);
                //     await MainThread.InvokeOnMainThreadAsync(async () =>
                //     {
                //         await Task.Delay(500);
                //         this.cameraManager.Dispose();
                //     });
                //     throw new Exception("TEST");
                // });

                base.DisconnectHandler(barcodeView);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "DisconnectHandler failed with exception");
            }
        }
    }
}
#endif