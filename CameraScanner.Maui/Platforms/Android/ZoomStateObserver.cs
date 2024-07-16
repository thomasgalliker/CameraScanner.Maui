using AndroidX.Camera.Core;
using AndroidX.Lifecycle;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class ZoomStateObserver : GenericObserver<IZoomState, ZoomStateChangedEventArgs>
    {
        protected override ZoomStateChangedEventArgs CreateEventArgs(IZoomState zoomState)
        {
            return new ZoomStateChangedEventArgs(zoomState.ZoomRatio, zoomState.MinZoomRatio, zoomState.MaxZoomRatio);
        }

        public float? ZoomRatio => this.LastValue?.ZoomRatio;

        public float? MinZoomRatio { get; private set; }

        public float? MaxZoomRatio { get; private set; }
    }
}