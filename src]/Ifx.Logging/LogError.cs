namespace Ifx.Logging;

/// <summary>
/// Delegate for logging error messages.
/// </summary>
/// <param name="message">The log message.</param>
/// <param name="args">The arguments for the log message.</param>
public delegate void LogError(string message, params object[] args);