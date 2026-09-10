using System.Collections.Concurrent;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Exceptions;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.CircuitBreaker;

/// <summary>
/// Interceptor that implements circuit breaker pattern to prevent cascading failures.
/// Opens the circuit after a threshold of transient exceptions, temporarily blocking invocations.
/// Uses the CircuitBreakerAttribute to determine configuration per method.
/// </summary>
public sealed class CircuitBreakerInterceptor(ITransientExceptionDetector detector) : IProxyInterceptor
{
    /// <summary>
    /// Maintains circuit breaker state per contract method.
    /// </summary>
    private readonly ConcurrentDictionary<string, CircuitBreakerState> states = new(StringComparer.Ordinal);

    /// <summary>
    /// Executes the interceptor to enforce circuit breaker logic on method invocation.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        var attribute = context.GetAttribute<CircuitBreakerAttribute>();

        if (attribute is null)
            return await next().ConfigureAwait(false);

        // Create a unique key for this method
        var key = $"{context.ContractType.FullName}.{context.MethodInfo.Name}";
        var state = states.GetOrAdd(key, _ => new CircuitBreakerState());

        // Check if circuit is currently open
        lock (state.SyncRoot)
        {
            if (state.OpenUntilUtc > DateTimeOffset.UtcNow)
                throw new CircuitBreakerOpenException($"Circuit breaker is open for {key}.");

            // Reset circuit if break duration has elapsed
            if (state.OpenUntilUtc is not null)
            {
                state.OpenUntilUtc = null;
                state.FailureCount = 0;
            }
        }

        try
        {
            var result = await next().ConfigureAwait(false);

            // Clear failures on success
            lock (state.SyncRoot)
            {
                state.FailureCount = 0;
                state.OpenUntilUtc = null;
            }

            return result;
        }
        catch (Exception exception) when (detector.IsTransient(exception))
        {
            // Increment failure count and open circuit if threshold reached
            lock (state.SyncRoot)
            {
                state.FailureCount++;

                if (state.FailureCount >= attribute.FailureThreshold)
                    state.OpenUntilUtc = DateTimeOffset.UtcNow + attribute.BreakDuration;
            }

            throw;
        }
    }
}
