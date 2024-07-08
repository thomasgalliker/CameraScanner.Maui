#if (ANDROID || IOS || MACCATALYST)
using CameraScanner.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            [nameof(CameraView.AimMode)] = (handler, _) => handler.cameraManager?.UpdateAimMode(),
            [nameof(CameraView.TapToFocusEnabled)] = (handler, _) => handler.cameraManager?.UpdateTapToFocusEnabled(),
            [nameof(CameraView.RequestZoomFactor)] = (handler, _) => handler.cameraManager?.UpdateRequestZoomFactor(),
            [nameof(CameraView.BarcodeDetectionFrameRate)] = (handler, _) => handler.cameraManager?.UpdateBarcodeDetectionFrameRate()
        };

        public CameraViewHandler()
            : base(CameraViewMapper)
        {
            var loggerFactory = this.MauiContext?.Services.GetService<ILoggerFactory>() ?? new NullLoggerFactory();
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
            base.ConnectHandler(platformView);
            this.VirtualView.Unloaded += this.OnVirtualViewUnloaded;
        }

        private void OnVirtualViewUnloaded(object sender, EventArgs e)
        {
            this.DisconnectHandler(this.PlatformView);
        }

        protected override void DisconnectHandler(BarcodeView barcodeView)
        {
            this.VirtualView.Unloaded -= this.OnVirtualViewUnloaded;

            this.cameraManager.Dispose();
            base.DisconnectHandler(barcodeView);
        }
    }
}
#endif