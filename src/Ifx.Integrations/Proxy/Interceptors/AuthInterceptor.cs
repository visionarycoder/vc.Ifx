using Ifx;
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

using Ifx.Pipeline;
using Ifx.Proxy;

namespace Ifx.Proxy.Interceptor;

/// <summary>Authorizes before executing the continuation.</summary>
public sealed class AuthInterceptor(IAuthorizationService auth) : IInterceptor
{
    private readonly IAuthorizationService auth = auth ?? throw new ArgumentNullException(nameof(auth));

    /// <inheritdoc />
    public int Order => -200;

    /// <inheritdoc />
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse> => InvokeAsync(request, PipelineGuard.Adapt(next), CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        PipelineGuard.Call(request, next, cancellationToken);
        await auth.AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return await next(request, cancellationToken).ConfigureAwait(false);
    }
}
