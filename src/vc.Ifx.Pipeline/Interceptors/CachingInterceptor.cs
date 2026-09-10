using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

public sealed class CachingInterceptor(ICache cache, Func<object, string> keySelector, TimeSpan ttl)
    : IInterceptor
{
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(
        TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse>
    {
        // Skip for commands by convention (queries only)
        bool isQuery = typeof(TRequest).Name.EndsWith("Query", StringComparison.Ordinal);
        if (!isQuery) return await next(request);

        string key = keySelector(request);
        (bool hit, TResponse value) = await cache.TryGetAsync<TResponse>(key);
        if (hit) return value;

        TResponse response = await next(request);
        await cache.SetAsync(key, response, ttl);
        return response;
    }
}
