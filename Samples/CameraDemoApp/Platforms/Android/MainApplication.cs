using Android.App;
using Android.Runtime;
using AndroidX.AppCompat.App;

namespace CameraDemoApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
