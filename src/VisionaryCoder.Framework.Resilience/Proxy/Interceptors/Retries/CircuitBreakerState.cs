namespace VisionaryCoder.Framework.Proxy.Interceptors.Retries;

/// <summary>
/// Circuit breaker state enumeration.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>Circuit is closed - operations are allowed.</summary>
    Closed,
    /// <summary>Circuit is open - operations are blocked.</summary>
    Open,
    /// <summary>Circuit is half-open - testing if operations can resume.</summary>
    HalfOpen
}
