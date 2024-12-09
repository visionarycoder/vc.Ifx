namespace vc.Ifx.Delegates;

/// <summary>
/// Delegate for logging debug messages.
/// </summary>
/// <param name="message">The log message.</param>
/// <param name="args">The arguments for the log message.</param>
public delegate void LogDebug(string message, params object[] args);