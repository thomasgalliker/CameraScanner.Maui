using AndroidX.Camera.Core;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class ZoomStateChangedEventArgs : EventArgs
    {
        internal ZoomStateChangedEventArgs(IZoomState zoomState)
        {
            this.ZoomState = zoomState;
        }

        public IZoomState ZoomState { get; }
    }
}