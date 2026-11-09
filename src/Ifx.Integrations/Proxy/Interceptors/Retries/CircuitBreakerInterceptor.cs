using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

namespace Ifx.Proxy.Interceptor.Retries;

/// <summary>Preserves consecutive-failure breaker semantics through Polly's compatibility policy.</summary>
public sealed class CircuitBreakerInterceptor : IProxyInterceptor
{
    private readonly AsyncCircuitBreakerPolicy policy;
    private readonly ILogger<CircuitBreakerInterceptor> logger;

    /// <summary>Creates a breaker shared by invocations through this interceptor instance.</summary>
    public CircuitBreakerInterceptor(ILogger<CircuitBreakerInterceptor> logger, int failureThreshold = 5, TimeSpan? timeout = null)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(failureThreshold);
        TimeSpan duration = timeout ?? TimeSpan.FromMinutes(1);
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));
        policy = Policy.Handle<Exception>(exception => exception is not OperationCanceledException)
            .CircuitBreakerAsync(failureThreshold, duration);
    }

    /// <summary>Gets the current breaker state. The policy cannot be manually isolated.</summary>
    public CircuitBreakerState State => Enum.Parse<CircuitBreakerState>(policy.CircuitState.ToString());

    /// <inheritdoc />
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await policy.ExecuteAsync(token => next(context, token), cancellationToken).ConfigureAwait(false);
        }
        catch (BrokenCircuitException exception)
        {
            logger.LogWarning(exception, "Proxy circuit breaker is open.");
            throw new TransientProxyException("The proxy circuit breaker is open.", exception);
        }
        finally
        {
            context.Metadata["CircuitBreakerState"] = State.ToString();
        }
    }
}
