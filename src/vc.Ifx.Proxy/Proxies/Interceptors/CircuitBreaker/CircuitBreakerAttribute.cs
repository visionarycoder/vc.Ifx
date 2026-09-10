namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.CircuitBreaker;

/// <summary>
/// Attribute that specifies circuit breaker configuration for method invocation.
/// Can be applied to interface or method levels.
/// The circuit breaker prevents cascading failures by refusing further invocations after a threshold of transient exceptions.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class CircuitBreakerAttribute(int failureThreshold = 3, int breakSeconds = 30) : Attribute
{
    /// <summary>
    /// Gets the number of consecutive failures before the circuit opens.
    /// </summary>
    public int FailureThreshold { get; } = failureThreshold;

    /// <summary>
    /// Gets the duration for which the circuit remains open.
    /// </summary>
    public TimeSpan BreakDuration { get; } = TimeSpan.FromSeconds(breakSeconds);
}
