using Ifx.Component;
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

using Ifx;
using Ifx.Pipeline;

namespace Ifx.Proxy.Interceptor;

/// <summary>Caches Query-suffixed requests using application-selected identity-aware keys.</summary>
public sealed class CachingInterceptor : IInterceptor
{


    private readonly ICache cache;
    private readonly Func<object, string> keySelector;
    private readonly TimeSpan ttl;

    public int Order { get; init; }

    /// <summary>Creates a cache interceptor with a positive entry lifetime.</summary>
    public CachingInterceptor(ICache cache, Func<object, string> keySelector, TimeSpan ttl, int order = 0) 
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ttl, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(order, 0);
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        this.ttl = ttl;
        Order = order;
    }
    /// <inheritdoc />
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse> => InvokeAsync(request, PipelineGuard.Adapt(next), CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        PipelineGuard.Call(request, next, cancellationToken);
        if (!typeof(TRequest).Name.EndsWith("Query", StringComparison.Ordinal))
            return await next(request, cancellationToken).ConfigureAwait(false);
        string key = keySelector(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var (hit, value) = await cache.TryGetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (hit) return value;
        TResponse response = await next(request, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        await cache.SetAsync(key, response, ttl, cancellationToken).ConfigureAwait(false);
        return response;
    }
}
