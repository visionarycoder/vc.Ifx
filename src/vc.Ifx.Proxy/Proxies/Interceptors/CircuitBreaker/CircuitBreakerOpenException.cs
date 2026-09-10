namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.CircuitBreaker;

/// <summary>
/// Exception thrown when a circuit breaker is open and prevents invocation.
/// </summary>
public sealed class CircuitBreakerOpenException(string message) : Exception(message);
