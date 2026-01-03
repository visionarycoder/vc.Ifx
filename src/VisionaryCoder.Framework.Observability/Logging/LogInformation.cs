namespace VisionaryCoder.Framework.Logging;

/// <summary>
/// Delegate for logging informational messages.
/// </summary>
/// <param name="message">The log message.</param>
/// <param name="args">The arguments for the log message.</param>
public delegate void LogInformation(string message, params object[] args);
