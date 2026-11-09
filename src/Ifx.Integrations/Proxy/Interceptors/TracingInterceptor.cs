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

/// <summary>Records operation status and deterministically ends/disposes spans.</summary>
public sealed class TracingInterceptor(ITracer tracer) : IInterceptor
{
    private readonly ITracer tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse> => InvokeAsync(request, PipelineGuard.Adapt(next), CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        PipelineGuard.Call(request, next, cancellationToken);
        string name = typeof(TRequest).Name;
        using var span = tracer.StartSpan(name);
        try
        {
            span.SetTag("request.type", name);
            span.SetTag("correlation.id", Ifx.Pipeline.Correlation.CurrentId ?? Guid.NewGuid().ToString("N"));
            var response = await next(request, cancellationToken).ConfigureAwait(false);
            span.SetTag("status", "success");
            return response;
        }
        catch (OperationCanceledException)
        {
            span.SetTag("status", "canceled");
            throw;
        }
        catch (Exception exception)
        {
            span.SetTag("status", "error");
            span.SetTag("error.message", exception.Message);
            throw;
        }
        finally
        {
            span.End();
        }
    }
}
