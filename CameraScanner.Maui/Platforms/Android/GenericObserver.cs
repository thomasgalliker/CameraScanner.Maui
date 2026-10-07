using Android.Runtime;
using AndroidX.Lifecycle;

namespace CameraScanner.Maui.Platforms.Android
{
    internal abstract class GenericObserver<TValue, TEventArgs> : Java.Lang.Object, IObserver where TEventArgs : EventArgs
    {
        protected GenericObserver()
        {
        }

        protected GenericObserver(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        public TValue? LastValue { get; private set; }

        public event EventHandler<TEventArgs>? ValueChanged;

        public void OnChanged(Java.Lang.Object? value)
        {
            if (value is TValue genericValue)
            {
                this.LastValue = genericValue;
                this.ValueChanged?.Invoke(this, this.CreateEventArgs(genericValue));
            }
        }

        protected abstract TEventArgs CreateEventArgs(TValue value);
    }
}