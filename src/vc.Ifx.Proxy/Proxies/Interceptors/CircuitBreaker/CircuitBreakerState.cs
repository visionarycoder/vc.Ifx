namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.CircuitBreaker;

/// <summary>
/// Internal state holder for circuit breaker logic.
/// Tracks failure count and open state with thread-safe synchronization.
/// </summary>
internal sealed class CircuitBreakerState
{
    /// <summary>
    /// Gets the synchronization root for thread-safe state management.
    /// </summary>
    public object SyncRoot { get; } = new();

    /// <summary>
    /// Gets or sets the current failure count.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp until which the circuit breaker is open.
    /// Null indicates the circuit is not currently open.
    /// </summary>
    public DateTimeOffset? OpenUntilUtc { get; set; }
}
