using Android.Runtime;
using Java.Lang;

namespace CameraScanner.Maui.Platforms.Android
{
    [Preserve(AllMembers = true)]
    internal class TorchStateObserver : GenericObserver<Number, TorchStateEventArgs>
    {
        public TorchStateObserver()
        {
        }

        protected TorchStateObserver(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        protected override TorchStateEventArgs CreateEventArgs(Number value)
        {
            return new TorchStateEventArgs(value.IntValue());
        }
    }
}