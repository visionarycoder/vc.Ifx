using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

/// <summary>Caches Query-suffixed requests using application-selected identity-aware keys.</summary>
public sealed class CachingInterceptor : IInterceptor
{
    private readonly ICache cache;
    private readonly Func<object, string> keySelector;
    private readonly TimeSpan ttl;

    /// <summary>Creates a cache interceptor with a positive entry lifetime.</summary>
    public CachingInterceptor(ICache cache, Func<object, string> keySelector, TimeSpan ttl)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ttl, TimeSpan.Zero);
        this.ttl = ttl;
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
