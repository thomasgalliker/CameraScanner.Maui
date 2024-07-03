using AndroidX.Camera.Core;
using AndroidX.Lifecycle;
using CameraScanner.Maui;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class ZoomStateObserver : Java.Lang.Object, IObserver
    {
        private readonly CameraManager cameraManager;
        private readonly CameraView cameraView;

        internal ZoomStateObserver(CameraManager cameraManager, CameraView cameraView)
        {
            this.cameraManager = cameraManager;
            this.cameraView = cameraView;
        }

        public void OnChanged(Java.Lang.Object value)
        {
            if (value is not null && this.cameraView is not null && value is IZoomState state)
            {
                this.cameraView.CurrentZoomFactor = state.ZoomRatio;
                this.cameraView.MinZoomFactor = state.MinZoomRatio;
                this.cameraView.MaxZoomFactor = state.MaxZoomRatio;

                this.cameraManager?.UpdateZoomFactor();
            }
        }
    }
}