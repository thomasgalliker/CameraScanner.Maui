using AndroidX.Camera.Core;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class CameraStateChangedEventArgs : EventArgs
    {
        internal CameraStateChangedEventArgs(CameraState cameraState)
        {
            this.CameraState = cameraState;
        }

        public CameraState CameraState { get; set; }
    }
}