using Android.Runtime;
using AndroidX.Camera.Core;

namespace CameraScanner.Maui.Platforms.Android
{
    [Preserve(AllMembers = true)]
    internal class CameraStateObserver : GenericObserver<CameraState, CameraStateChangedEventArgs>
    {
        public CameraStateObserver()
        {
        }

        protected CameraStateObserver(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        protected override CameraStateChangedEventArgs CreateEventArgs(CameraState cameraState)
        {
            return new CameraStateChangedEventArgs(cameraState);
        }
    }
}