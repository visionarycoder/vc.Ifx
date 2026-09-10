namespace VisionaryCoder.Framework.Proxy;

/// Configuration options for proxy operations.
public class ProxyOptions
{
    /// <summary>
    /// Gets or sets the timeout for proxy operations.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    /// <summary>
    /// Gets or sets the number of failures before circuit breaker opens.
    /// </summary>
    public int CircuitBreakerFailures { get; set; } = 5;
    /// <summary>
    /// Gets or sets the duration the circuit breaker stays open.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromMinutes(1);
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    /// <summary>
    /// Gets or sets the maximum number of retry attempts (alias for MaxRetries).
    /// </summary>
    public int MaxRetryAttempts { get => MaxRetries; set => MaxRetries = value; }
    /// <summary>
    /// Gets or sets the retry delay.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    /// <summary>
    /// Gets or sets whether caching is enabled.
    /// </summary>
    public bool CachingEnabled { get; set; } = true;
    /// <summary>
    /// Gets or sets whether auditing is enabled.
    /// </summary>
    public bool AuditingEnabled { get; set; } = true;
}