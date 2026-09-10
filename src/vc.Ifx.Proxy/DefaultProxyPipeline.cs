using System.Reflection;

using VisionaryCoder.Framework.Proxy.Attributes;

namespace VisionaryCoder.Framework.Proxy;
/// <summary>
/// Default implementation of the proxy pipeline that executes interceptors in order.
/// </summary>
/// <param name="interceptors">The collection of interceptors to execute.</param>
/// <param name="transport">The transport implementation for sending requests.</param>
public sealed class DefaultProxyPipeline(IEnumerable<IProxyInterceptor> interceptors, IProxyTransport transport) : IProxyPipeline
{
    private readonly IReadOnlyList<IProxyInterceptor> orderedInterceptors = Order(interceptors);
    private readonly IProxyTransport transport = transport ?? throw new ArgumentNullException(nameof(transport));
    /// <summary>
    /// Sends a request through the pipeline and returns a response.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="context">The proxy context containing request information.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    public Task<ProxyResponse<T>> SendAsync<T>(ProxyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        // Build the pipeline by wrapping interceptors around the transport
        ProxyDelegate<T> terminal = (_, ct) => transport.SendCoreAsync<T>(context, ct);
        // Wrap each interceptor around the previous delegate (reverse order for proper execution)
        foreach (IProxyInterceptor interceptor in orderedInterceptors.Reverse())
        {
            ProxyDelegate<T> next = terminal;
            terminal = (ctx, ct) => interceptor.InvokeAsync<T>(ctx, next, ct);
        }
        return terminal(context, cancellationToken);
    }
    /// Orders the interceptors based on their Order value.
    /// <param name="interceptors">The interceptors to order.</param>
    /// <returns>An ordered list of interceptors.</returns>
    private static IReadOnlyList<IProxyInterceptor> Order(IEnumerable<IProxyInterceptor> interceptors)
    { int index = 0;

        // DI preserves registration order—use index to keep stability for same order values
        return interceptors
            .Select(interceptor => new
            {
                Interceptor = interceptor,
                Order = GetOrder(interceptor),
                Index = index++
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Index)
            .Select(x => x.Interceptor)
            .ToList(); 
            }
    /// Gets the order value for an interceptor.
    /// <param name="interceptor">The interceptor to get the order for.</param>
    /// <returns>The order value.</returns>
    private static int GetOrder(IProxyInterceptor interceptor)
    {   // Interface-based order takes precedence over attribute
        if (interceptor is IOrderedProxyInterceptor orderedInterceptor)
            return orderedInterceptor.Order;
        // Fall back to attribute-based order
        ProxyInterceptorOrderAttribute? attribute = interceptor.GetType().GetCustomAttribute<ProxyInterceptorOrderAttribute>();
        return attribute?.Order ?? 0;
    }
}
