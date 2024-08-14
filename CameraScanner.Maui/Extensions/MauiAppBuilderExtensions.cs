using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CameraScanner.Maui
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseCameraScanner(this MauiAppBuilder builder)
        {
#if (ANDROID || IOS)
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CameraView, CameraViewHandler>();
            });

            builder.Services.TryAddSingleton<ICameraPermissions>(_ => ICameraPermissions.Current);
            builder.Services.TryAddSingleton<IBarcodeScanner>(_ => IBarcodeScanner.Current);
            builder.Services.TryAddSingleton<IDeviceInfo>(_ => DeviceInfo.Current);
            builder.Services.TryAddSingleton<IDeviceDisplay>(_ => DeviceDisplay.Current);
#endif

            return builder;
        }
    }
}