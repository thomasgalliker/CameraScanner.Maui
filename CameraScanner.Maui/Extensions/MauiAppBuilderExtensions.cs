namespace CameraScanner.Maui
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseBarcodeScanning(this MauiAppBuilder builder)
        {
#if (ANDROID || IOS)
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CameraView, CameraViewHandler>();
            });

            builder.Services.AddSingleton<IPermissionService>(_ => PermissionService.Current);
            builder.Services.AddSingleton<IBarcodeScanner>(_ => BarcodeScanner.Current);
#endif

            return builder;
        }
    }
}