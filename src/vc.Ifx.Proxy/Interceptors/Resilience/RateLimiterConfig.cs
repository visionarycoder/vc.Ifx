namespace VisionaryCoder.Framework.Proxy.Interceptors.Resilience;

/// <summary>
/// Rate limiter configuration for tracking request counts and time windows.
/// </summary>
public sealed class RateLimiterConfig
{
    /// <summary>
    /// Gets or sets the maximum number of requests allowed in the time window.
    /// </summary>
    public int MaxRequests { get; set; } = 100;
    /// Gets or sets the time window for rate limiting.
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1);
}
