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

namespace Ifx.Proxy.Interceptor;
public sealed class OrderedProxyInterceptor<TInner>(TInner inner, int order) : IProxyInterceptor, IOrderedProxyInterceptor where TInner : IProxyInterceptor
{
    public int Order => order;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default) => inner.InvokeAsync(context, next, cancellationToken);
}
