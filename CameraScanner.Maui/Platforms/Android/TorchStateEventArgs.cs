using AndroidX.Camera.Core;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class TorchStateEventArgs : EventArgs
    {
        public TorchStateEventArgs(int torchState)
        {
            this.TorchOn = torchState == TorchState.On;
        }

        public bool TorchOn { get; }
    }
}