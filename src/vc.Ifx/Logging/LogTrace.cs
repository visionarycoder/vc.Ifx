namespace VisionaryCoder.Framework.Logging;

/// <summary>
/// Delegate for logging trace messages.
/// </summary>
/// <param name="message">The log message.</param>
/// <param name="args">The arguments for the log message.</param>
public delegate void LogTrace(string message, params object[] args);
