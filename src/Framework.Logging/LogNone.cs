namespace VisionaryCoder.Framework.Logging;

/// <summary>
/// Delegate for logging messages with no specific level.
/// </summary>
/// <param name="message">The log message.</param>
/// <param name="args">The arguments for the log message.</param>
public delegate void LogNone(string message, params object[] args);
