using Microsoft.Extensions.Logging;

namespace CameraScanner.Maui
{
    public partial class CameraViewHandler
    {
        protected override BarcodeView CreatePlatformView()
        {
            this.cameraManager = new CameraManager(
                this.loggerFactory.CreateLogger<CameraManager>(),
                this.loggerFactory,
                this.cameraPermissions,
                this.deviceDisplay,
                this.VirtualView,
                this.Context);

            return this.cameraManager.BarcodeView;
        }
    }
}