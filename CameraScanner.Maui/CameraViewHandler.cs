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
        private readonly ICameraPermissions cameraPermissions;
        private readonly IDeviceInfo deviceInfo;
        private readonly IDeviceDisplay deviceDisplay;

        private CameraManager cameraManager;

        private static readonly PropertyMapper<CameraView, CameraViewHandler> CameraViewMapper = new()
        {
            [nameof(CameraView.CameraFacing)] = (handler, _) => handler.cameraManager?.UpdateCameraFacing(),
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
            this.cameraPermissions = IPlatformApplication.Current.Services.GetService<ICameraPermissions>();
            this.deviceInfo = IPlatformApplication.Current.Services.GetService<IDeviceInfo>();
            this.deviceDisplay = IPlatformApplication.Current.Services.GetService<IDeviceDisplay>();
        }

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
            this.logger.LogDebug("DisconnectHandler");
            this.cameraManager.Dispose();
            base.DisconnectHandler(barcodeView);
        }
    }
}
#endif