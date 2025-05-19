using CameraDemoApp.Services.Logging;
using CameraDemoApp.Services.Navigation;
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
                    fonts.AddFont("IBMPlexMono-Bold.ttf", "IBMPlexMonoBold");
                    fonts.AddFont("IBMPlexMono-Regular.ttf", "IBMPlexMonoRegular");
                    fonts.AddFont("IBMPlexSans-Bold.ttf", "IBMPlexSansBold");
                    fonts.AddFont("IBMPlexSans-Regular.ttf", "IBMPlexSansRegular");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddNLog(NLogLoggerConfiguration.GetLoggingConfiguration());
                b.AddSentry(SentryConfiguration.Configure);
            });

            // Register services
            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<ILauncher>(_ => Launcher.Default);
            builder.Services.AddSingleton<IMediaPicker>(_ => MediaPicker.Default);
            builder.Services.AddSingleton<IClipboard>(_ => Clipboard.Default);

            // Register pages and view models
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();

            builder.Services.AddTransient<DefaultCameraViewPage>();

            builder.Services.AddTransient<QRCodeScannerPage>();
            builder.Services.AddTransient<QRCodeScannerViewModel>();

            builder.Services.AddTransient<UniversalScannerPage>();
            builder.Services.AddTransient<UniversalScannerViewModel>();

            builder.Services.AddTransient<CameraPreviewPage>();
            builder.Services.AddTransient<CameraPreviewViewModel>();

            builder.Services.AddTransient<FilePickerPage>();
            builder.Services.AddTransient<FilePickerViewModel>();

            builder.Services.AddTransient<BarcodeResultDetailPage>();
            builder.Services.AddTransient<BarcodeResultDetailViewModel>();

            builder.Services.AddTransientPopup<ScannerConfigPopup, ScannerConfigViewModel>();

            return builder.Build();
        }
    }
}
