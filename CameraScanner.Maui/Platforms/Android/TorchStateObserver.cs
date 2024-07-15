using Java.Lang;

namespace CameraScanner.Maui.Platforms.Android
{
    internal class TorchStateObserver : GenericObserver<Number, TorchStateEventArgs>
    {
        protected override TorchStateEventArgs CreateEventArgs(Number value)
        {
            return new TorchStateEventArgs(value.IntValue());
        }
    }
}