using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

/// <summary>Records operation status and deterministically ends/disposes spans.</summary>
public sealed class TracingInterceptor(ITracer tracer) : IInterceptor
{
    private readonly ITracer tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
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
            span.SetTag("correlation.id", Correlation.CurrentId ?? Guid.NewGuid().ToString("N"));
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
