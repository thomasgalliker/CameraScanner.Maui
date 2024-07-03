using CameraDemoApp.Services;
using CameraDemoApp.ViewModels;
using CameraDemoApp.Views;
using CameraScanner.Maui;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CameraDemoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseCameraScanner()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            builder.Services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddDebug();
                b.AddNLog();
            });

            // Register services
            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();

            // Register pages and view models
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();

            builder.Services.AddTransient<DefaultCameraViewPage>();
            builder.Services.AddTransient<QRCodeScannerPage>();
            builder.Services.AddTransient<QRCodeScannerViewModel>();

            return builder.Build();
        }
    }
}
