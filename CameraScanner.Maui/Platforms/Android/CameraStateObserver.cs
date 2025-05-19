using AndroidX.Camera.Core;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class CameraStateObserver : GenericObserver<CameraState, CameraStateChangedEventArgs>
    {
        protected override CameraStateChangedEventArgs CreateEventArgs(CameraState cameraState)
        {
            return new CameraStateChangedEventArgs(cameraState);
        }
    }
}