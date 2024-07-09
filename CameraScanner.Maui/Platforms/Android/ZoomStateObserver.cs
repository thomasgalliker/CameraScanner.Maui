using AndroidX.Camera.Core;
using AndroidX.Lifecycle;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class ZoomStateObserver : Java.Lang.Object, IObserver
    {
        public event EventHandler<ZoomStateChangedEventArgs> ZoomStateChanged;

        public void OnChanged(Java.Lang.Object value)
        {
            if (value is IZoomState zoomState)
            {
                this.ZoomRatio = zoomState.ZoomRatio;
                this.MinZoomRatio = zoomState.MinZoomRatio;
                this.MaxZoomRatio = zoomState.MaxZoomRatio;

                this.ZoomStateChanged?.Invoke(this, new ZoomStateChangedEventArgs(zoomState.ZoomRatio, zoomState.MinZoomRatio, zoomState.MaxZoomRatio));
            }
        }

        public float ZoomRatio { get; private set; }

        public float MinZoomRatio { get; private set; }

        public float MaxZoomRatio { get; private set; }
    }
}