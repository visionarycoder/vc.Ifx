namespace vc.Ifx.Logging;

// Resharper disable all
public delegate void LogCritical(string message, params object[] args);
public delegate void LogDebug(string message, params object[] args);
public delegate void LogError(string message, params object[] args);
public delegate void LogInformation(string message, params object[] args);
public delegate void LogTrace(string message, params object[] args);
public delegate void LogWarning(string message, params object[] args);