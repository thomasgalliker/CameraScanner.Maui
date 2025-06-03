using AndroidX.Camera.Core;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class ZoomStateObserver : GenericObserver<IZoomState, ZoomStateChangedEventArgs>
    {
        protected override ZoomStateChangedEventArgs CreateEventArgs(IZoomState zoomState)
        {
            return new ZoomStateChangedEventArgs(zoomState);
        }
    }
}