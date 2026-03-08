using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.File;
using Serilog.Formatting.Json;
using Serilog.Formatting;
using Serilog.Events;
using System.Text;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Chik.Exams;

public static class LoggingExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        Directory.CreateDirectory(ApplicationConstants.LogsDirectory);

        // Custom formatter that combines timestamp, level, and JSON
        var formatter = new FileTimestampJsonFormatter();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.InMemory", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.SqlServer", LogEventLevel.Warning)
            .MinimumLevel.Override("ZiggyCreatures.Caching.Fusion", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.OpenTelemetry(o => {
                o.Endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";
                o.ResourceAttributes.Add("service.name", ApplicationConstants.Name);
            })
            .WriteTo.File(
                formatter,
                path: GetLogFilePath(),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                retainedFileCountLimit: 10,
                rollOnFileSizeLimit: true
            )
            .CreateLogger();

        services.TrackSingleton(Log.Logger);
        services.TrackSingleton<ILoggerFactory>(new SerilogLoggerFactory());
        services.TrackSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<Auth>>());
        services.AddSerilog();

        return services;
    }

    public static string GetLogFilePath(this ILogger logger)
    {
        return GetLogFilePath();
    }

    private static string GetLogFilePath()
    {
        return Path.Combine(ApplicationConstants.LogsDirectory, $"{ApplicationConstants.Name}.log");
    }

    public static void Debug(this ILogger logger, string message)
    {
        logger.LogDebug(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Message = message
            })
        );
    }

    public static void Debug<TData>(this ILogger logger, string message, TData data)
    {
        logger.LogDebug(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Message = message,
                Data = data
            })
        );
    }

    public static void Info(this ILogger logger, string message)
    {
        logger.LogInformation(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Message = message
            })
        );
    }

    public static void Info<TData>(this ILogger logger, string message, TData data)
    {
        logger.LogInformation(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Message = message,
                Data = data
            })
        );
    }
    
    public static void Warn(this ILogger logger, string message)
    {
        logger.LogWarning(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Message = message
            })
        );
    }

    public static void Warn<TData>(this ILogger logger, string message, TData data)
    {
        logger.LogWarning(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Message = message,
                Data = data
            })
        );
    }

    public static void Error(this ILogger logger, Exception exception)
    {
        logger.LogError(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                exception.Message,
                Exception = exception
            })
        );
    }

    public static void Error<TData>(this ILogger logger, Exception exception, TData data)
    {
        logger.LogError(
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                exception.Message,
                Exception = exception,
                Data = data
            })
        );
    }
}

public class FileTimestampJsonFormatter : ITextFormatter
{
    private readonly JsonFormatter _jsonFormatter = new();

    public void Format(LogEvent logEvent, TextWriter output)
    {
        // Format: "2025-09-26 07:17:28.238 -04:00 [LEVEL] JSON"
        var timestamp = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
        var level = logEvent.Level.ToString().ToUpper();

        // Get JSON part
        var jsonBuilder = new StringBuilder();
        using (var jsonWriter = new StringWriter(jsonBuilder))
        {
            _jsonFormatter.Format(logEvent, jsonWriter);
        }

        output.Write($"{timestamp} [{level}] {jsonBuilder}");
    }
}