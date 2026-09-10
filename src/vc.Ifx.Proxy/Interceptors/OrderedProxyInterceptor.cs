namespace VisionaryCoder.Framework.Proxy.Interceptors;
public sealed class OrderedProxyInterceptor<TInner>(TInner inner, int order) : IProxyInterceptor, IOrderedProxyInterceptor where TInner : IProxyInterceptor
{
    public int Order => order;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default) => inner.InvokeAsync(context, next, cancellationToken);
}
