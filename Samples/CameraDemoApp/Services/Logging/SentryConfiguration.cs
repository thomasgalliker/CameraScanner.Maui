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
            options.Dsn = "https://a22fbaa9dace1459f65db323b4074f4d@o4507458300280832.ingest.de.sentry.io/4510192056270928";
            options.MinimumEventLevel = LogLevel.Warning;
            options.MinimumBreadcrumbLevel = LogLevel.Debug;
        }
    }
}