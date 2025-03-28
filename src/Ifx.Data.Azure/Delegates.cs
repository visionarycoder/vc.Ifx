namespace vc.Ifx.Data;

internal delegate void LogCritical(string message, params object[] args);
internal delegate void LogDebug(string message, params object[] args);
internal delegate void LogError(string message, params object[] args);
internal delegate void LogInformation(string message, params object[] args);
internal delegate void LogTrace(string message, params object[] args);
