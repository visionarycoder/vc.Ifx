namespace VisionaryCoder.Framework;

/// <summary>
/// Configuration options for the VisionaryCoder Framework.
/// </summary>
public sealed class Options
{
    /// <summary>
    /// Gets or sets whether correlation ID generation is enabled.
    /// </summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// Gets or sets whether request ID generation is enabled.
    public bool EnableRequestId { get; set; } = true;

    /// Gets or sets whether structured logging is enabled.
    public bool EnableStructuredLogging { get; set; } = true;

    /// Gets or sets the default HTTP timeout in seconds.
    public int DefaultHttpTimeoutSeconds { get; set; } = Constants.Timeouts.DefaultHttpTimeoutSeconds;

    /// Gets or sets the default cache expiration in minutes.
    public int DefaultCacheExpirationMinutes { get; set; } = Constants.Timeouts.DefaultCacheExpirationMinutes;
}
