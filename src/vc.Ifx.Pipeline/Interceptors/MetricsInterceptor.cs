using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

/// <summary>Counts all completions and records one duration per operation.</summary>
public sealed class MetricsInterceptor : IInterceptor
{
    private readonly IMetrics metrics;
    private readonly TimeProvider clock;

    /// <summary>Creates metrics using the system clock.</summary>
    public MetricsInterceptor(IMetrics metrics) : this(metrics, TimeProvider.System) { }
    /// <summary>Creates metrics using an explicit clock.</summary>
    public MetricsInterceptor(IMetrics metrics, TimeProvider timeProvider)
    {
        this.metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        clock = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
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
        string name = typeof(TRequest).Name;
        long start = clock.GetTimestamp();
        try
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            metrics.IncrementCounter("requests_canceled_total", name);
            throw;
        }
        catch (Exception)
        {
            metrics.IncrementCounter("requests_failed_total", name);
            throw;
        }
        finally
        {
            metrics.IncrementCounter("requests_total", name);
            metrics.ObserveHistogram("request_duration_ms", name, (long)clock.GetElapsedTime(start).TotalMilliseconds);
        }
    }
}
