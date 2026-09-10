namespace VisionaryCoder.Framework.Logging;

/// <summary>
/// Configuration options for logging interceptors.
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// Gets or sets whether to enable standard logging interceptor.
    /// </summary>
    public bool EnableStandardLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable timing measurements.
    /// </summary>
    public bool EnableTiming { get; set; } = true;

    /// <summary>
    /// Gets or sets the threshold for slow operation warnings in milliseconds.
    /// </summary>
    public long SlowOperationThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the threshold for critical operation errors in milliseconds.
    /// </summary>
    public long CriticalOperationThresholdMs { get; set; } = 5000;
}