using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Logging;

public static class LogHelper
{

    // Synchronous Methods
    public static void LogTraceMessage(ILogger logger, string logMessage, Exception? exception = null)
    {
        LogTrace(logger, logMessage, exception);
    }

    public static void LogDebugMessage(ILogger logger, string logMessage, Exception? exception = null)
    {
        LogDebug(logger, logMessage, exception);
    }

    public static void LogInformationMessage(ILogger logger, string logMessage, Exception? exception = null)
    {
        LogInformation(logger, logMessage, exception);
    }

    public static void LogWarningMessage(ILogger logger, string logMessage, Exception? exception = null)
    {
        LogWarning(logger, logMessage, exception);
    }

    public static void LogErrorMessage(ILogger logger, string logMessage, Exception? exception = null)
    {
        LogError(logger, logMessage, exception);
    }

    public static void LogCriticalMessage(ILogger logger, string logMessage, Exception? exception = null)
    {
        LogCritical(logger, logMessage, exception);
    }

    public static void Log(ILogger logger, string logMessage, LogLevel logLevel = LogLevel.Debug, Exception? exception = null)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                LogTrace(logger, logMessage, exception);
                break;
            case LogLevel.Debug:
                LogDebug(logger, logMessage, exception);
                break;
            case LogLevel.Information:
                LogInformation(logger, logMessage, exception);
                break;
            case LogLevel.Warning:
                LogWarning(logger, logMessage, exception);
                break;
            case LogLevel.Error:
                LogError(logger, logMessage, exception);
                break;
            case LogLevel.Critical:
                LogCritical(logger, logMessage, exception);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Invalid log level.");
        }
    }

    // Asynchronous Methods with CancellationToken
    public static async Task LogTraceMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            LogTrace(logger, logMessage, exception);
        }, cancellationToken);
    }

    public static async Task LogDebugMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            LogDebug(logger, logMessage, exception);
        }, cancellationToken);
    }

    public static async Task LogInformationMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            LogInformation(logger, logMessage, exception);
        }, cancellationToken);
    }

    public static async Task LogWarningMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            LogWarning(logger, logMessage, exception);
        }, cancellationToken);
    }

    public static async Task LogErrorMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            LogError(logger, logMessage, exception);
        }, cancellationToken);
    }

    public static async Task LogCriticalMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            LogCritical(logger, logMessage, exception);
        }, cancellationToken);
    }

    public static async Task LogAsync(ILogger logger, string logMessage, LogLevel logLevel = LogLevel.Debug, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Log(logger, logMessage, logLevel, exception);
        }, cancellationToken);
    }

    // Private Helper Methods
    private static void LogTrace(ILogger logger, string logMessage, Exception? exception)
    {
        if (exception == null)
            logger.LogTrace(logMessage);
        else
            logger.LogTrace(exception, logMessage);
    }

    private static void LogDebug(ILogger logger, string logMessage, Exception? exception)
    {
        if (exception == null)
            logger.LogDebug(logMessage);
        else
            logger.LogDebug(exception, logMessage);
    }

    private static void LogInformation(ILogger logger, string logMessage, Exception? exception)
    {
        if (exception == null)
            logger.LogInformation(logMessage);
        else
            logger.LogInformation(exception, logMessage);
    }

    private static void LogWarning(ILogger logger, string logMessage, Exception? exception)
    {
        if (exception == null)
            logger.LogWarning(logMessage);
        else
            logger.LogWarning(exception, logMessage);
    }

    private static void LogError(ILogger logger, string logMessage, Exception? exception)
    {
        if (exception == null)
            logger.LogError(logMessage);
        else
            logger.LogError(exception, logMessage);
    }

    private static void LogCritical(ILogger logger, string logMessage, Exception? exception)
    {
        if (exception == null)
            logger.LogCritical(logMessage);
        else
            logger.LogCritical(exception, logMessage);
    }
}
