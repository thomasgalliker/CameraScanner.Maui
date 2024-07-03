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

            builder.Services.AddSingleton<ICameraPermissions>(_ => CameraPermissions.Current);
            builder.Services.AddSingleton<IBarcodeScanner>(_ => BarcodeScanner.Current);
#endif

            return builder;
        }
    }
}