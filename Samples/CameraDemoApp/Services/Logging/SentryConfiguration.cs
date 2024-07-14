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
#if DEBUG
            options.Debug = false;
#endif
            options.Dsn = "https://be0ae8f6191ed3be7ecc42ea64a435ae@o4507458300280832.ingest.de.sentry.io/4507526266093648";
            options.MinimumEventLevel = LogLevel.Warning;
            options.MinimumBreadcrumbLevel = LogLevel.Debug;
        }
    }
}