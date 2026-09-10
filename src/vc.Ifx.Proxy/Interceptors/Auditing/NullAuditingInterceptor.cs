namespace VisionaryCoder.Framework.Proxy.Interceptors.Auditing;
/// <summary>
/// Null object pattern implementation of auditing interceptor that performs no operations.
/// </summary>
public sealed class NullAuditingInterceptor(int order = 300) : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => order;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any auditing
        return next(context, cancellationToken);
    }
}
