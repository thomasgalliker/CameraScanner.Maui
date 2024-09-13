using Microsoft.Extensions.Logging;
using Sentry.Extensions.Logging;
using Sentry.Maui;

namespace CameraDemoApp.Services.Logging
{
    public static class SentryConfiguration
    {
        public static void Configure(SentryLoggingOptions options)
        {
            options.InitializeSdk = true;
            options.Debug = false;
            options.Dsn = "https://70fdfbf85093b6e5e6b028dbf4fa9385@o4507458300280832.ingest.de.sentry.io/4507927702929488";
            options.MinimumEventLevel = LogLevel.Warning;
            options.MinimumBreadcrumbLevel = LogLevel.Debug;
        }
    }
}