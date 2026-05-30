using System.Text;
using NLog.Config;
using NLog.Targets;
using Superdev.Maui.Extensions;
using LogLevel = NLog.LogLevel;

namespace CameraDemoApp.Services.Logging
{
    public static class NLogLoggerConfiguration
    {
        private static readonly string LogFilePath;

        static NLogLoggerConfiguration()
        {
            LogFilePath = CreateLogFile();
            LogFolderPath = Path.GetDirectoryName(LogFilePath)!;
        }

        public static string LogFolderPath { get; }

        private static string CreateLogFile()
        {
            var filename = $"{AppInfo.PackageName}.log";

            var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Logs");
            if (!Directory.Exists(logFolder))
            {
                logFolder = Directory.CreateDirectory(logFolder).FullName;
            }

            return Path.Combine(logFolder, filename);
        }

        public static LoggingConfiguration GetLoggingConfiguration()
        {
            var deviceInfo = DeviceInfo.Current;
            var appInfo = AppInfo.Current;

            var config = new LoggingConfiguration();
#if DEBUG
            const bool isDebug = true;
#else
            const bool isDebug = false;
#endif
            var diagnosticsInfo = new StringBuilder()
                .AppendLine($"App: {appInfo.PackageName}")
                .AppendLine($"Version: {appInfo.VersionString} ({appInfo.BuildString})")
                .AppendLine($"OS: {deviceInfo.Platform} {deviceInfo.Version}")
                .AppendLine($"Device: {deviceInfo.Manufacturer} {deviceInfo.Model}")
                .AppendLine($"Debug: {isDebug}")
                .ToString()
                .TrimStartAndEnd()
                ;

            config.Variables.Add("DiagnosticsInfo", diagnosticsInfo);

            const string layout = "${longdate:universalTime=True}|${level}|${logger}|${message}${onexception:inner=${newline}${exception:format=tostring}}[EOL]";

            // Console Target
            {
                var target = new ConsoleTarget("console");
                target.Layout = layout;

                config.AddTarget("console", target);

                var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
                config.LoggingRules.Add(loggingRule);
            }

            // Debug Target
            {
                var target = new DebugTarget("debug");
                target.Layout = layout;

                config.AddTarget("debug", target);

                var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
                config.LoggingRules.Add(loggingRule);
            }

            // File Target
            {
                var target = new FileTarget("file");
                target.Layout = layout;
                target.FileName = LogFilePath;
                target.MaxArchiveFiles = 2;
                target.ArchiveSuffixFormat = ".{0:00}";
                target.ArchiveAboveSize = 102400; // 100KB
                target.KeepFileOpen = true;

                target.Header = $"----------------------------------------${{newline}}" +
                                $"${{var:name=DiagnosticsInfo}}${{newline}}" +
                                $"Date: ${{longdate:universalTime=True}}${{newline}}" +
                                $"----------------------------------------";
                target.WriteHeaderWhenInitialFileNotEmpty = true;

                config.AddTarget("file", target);

                var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
                config.LoggingRules.Add(loggingRule);
            }

            return config;
        }
    }
}
