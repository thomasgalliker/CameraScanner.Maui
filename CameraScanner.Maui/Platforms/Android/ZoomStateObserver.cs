using Android.Runtime;
using AndroidX.Camera.Core;

namespace CameraScanner.Maui.Platforms.Android
{
    [Preserve(AllMembers = true)]
    internal class ZoomStateObserver : GenericObserver<IZoomState, ZoomStateChangedEventArgs>
    {
        public ZoomStateObserver()
        {
        }

        protected ZoomStateObserver(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        protected override ZoomStateChangedEventArgs CreateEventArgs(IZoomState zoomState)
        {
            return new ZoomStateChangedEventArgs(zoomState);
        }
    }
}