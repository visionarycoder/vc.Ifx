using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Idempotency;

/// <summary>
/// Interceptor that implements idempotency for marked methods.
/// Caches results by idempotency key to prevent duplicate execution and ensure consistent results.
/// Uses the IdempotentAttribute to identify eligible methods.
/// </summary>
public sealed class IdempotencyInterceptor(IIdempotencyKeyResolver keyResolver, IIdempotencyStore store)
    : IProxyInterceptor
{
    /// <summary>
    /// Executes the interceptor to implement idempotency on method invocation.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        // Check if the method is marked as idempotent
        if (!context.HasAttribute<IdempotentAttribute>())
            return await next().ConfigureAwait(false);

        // Resolve the idempotency key
        var key = keyResolver.GetKey(context);

        // Return cached result if it exists
        if (await store.ExistsAsync(key).ConfigureAwait(false))
            return await store.GetAsync(key).ConfigureAwait(false);

        // Execute the invocation and cache the result
        var result = await next().ConfigureAwait(false);
        await store.SaveAsync(key, result).ConfigureAwait(false);

        return result;
    }
}
