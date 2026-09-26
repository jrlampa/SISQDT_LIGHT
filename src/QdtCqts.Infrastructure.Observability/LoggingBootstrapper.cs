using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using QdtCqts.Domain;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace QdtCqts.Infrastructure.Observability;

public enum LoggingProfile
{
    Development,
    Test,
    Production,
    Diagnostic
}

public sealed class LoggingConfiguration
{
    public LoggingProfile Profile { get; set; } = LoggingProfile.Production;
    public string LogDirectory { get; set; } = "logs";
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public long FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public int RetainedFileCountLimit { get; set; } = 30; // 30 dias
    public bool EnableConsole { get; set; } = true;
    public bool EnableFileRolling { get; set; } = true;

    public static LoggingConfiguration ForProfile(LoggingProfile profile, string? logDir = null)
    {
        var config = new LoggingConfiguration
        {
            Profile = profile,
            LogDirectory = logDir ?? "logs"
        };

        switch (profile)
        {
            case LoggingProfile.Diagnostic:
                config.MinimumLevel = LogEventLevel.Verbose;
                break;
            case LoggingProfile.Development:
                config.MinimumLevel = LogEventLevel.Debug;
                break;
            case LoggingProfile.Test:
                config.MinimumLevel = LogEventLevel.Information;
                config.EnableFileRolling = false;
                break;
            case LoggingProfile.Production:
            default:
                config.MinimumLevel = LogEventLevel.Information;
                break;
        }

        return config;
    }
}

public static class LoggingBootstrapper
{
    public static ILoggerFactory CreateLoggerFactory(LoggingConfiguration configuration)
    {
        var serilogConfig = new LoggerConfiguration()
            .MinimumLevel.Is(configuration.MinimumLevel)
            .Enrich.FromLogContext()
            .Enrich.With(new CorrelationLogEnricher())
            .Enrich.WithProperty("Application", "SISQDT_LIGHT")
            .Enrich.WithProperty("CodeVersion", VersioningMetadata.CodeVersion)
            .Enrich.WithProperty("SchemaVersion", VersioningMetadata.SchemaVersion)
            .Enrich.WithProperty("RuleSetVersion", VersioningMetadata.RuleSetVersion);

        if (configuration.EnableConsole)
        {
            serilogConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{EventId}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }

        if (configuration.EnableFileRolling)
        {
            var logPath = Path.Combine(configuration.LogDirectory, "application-.log");
            serilogConfig.WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: configuration.FileSizeLimitBytes,
                retainedFileCountLimit: configuration.RetainedFileCountLimit,
                rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{EventId}] [{CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }

        var serilogLogger = serilogConfig.CreateLogger();

        var factory = new LoggerFactory();
        factory.AddSerilog(serilogLogger, dispose: true);
        return factory;
    }

    private sealed class CorrelationLogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlation = CorrelationContext.Current;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlation.CorrelationId));
            if (!string.IsNullOrEmpty(correlation.CalculationId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CalculationId", correlation.CalculationId));
            }
        }
    }
}

public static class StartupLogger
{
    public static void LogStartup(Microsoft.Extensions.Logging.ILogger logger, LoggingConfiguration config, string? gitCommit = null)
    {
        logger.LogInformation(CalculationEventIds.AppStartup,
            "SISQDT_LIGHT inicializado. CodeVersion={CodeVersion}, Commit={Commit}, OS={OS}, NetRuntime={NetRuntime}, SchemaVersion={SchemaVersion}, RuleSetVersion={RuleSetVersion}, EvidenceVersion={EvidenceVersion}, LoggingProfile={LoggingProfile}",
            VersioningMetadata.CodeVersion,
            gitCommit ?? "dev",
            RuntimeInformation.OSDescription,
            RuntimeInformation.FrameworkDescription,
            VersioningMetadata.SchemaVersion,
            VersioningMetadata.RuleSetVersion,
            VersioningMetadata.EvidenceVersion,
            config.Profile);
    }
}
