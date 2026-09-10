using System.Diagnostics;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

public sealed class MetricsInterceptor(IMetrics metrics)
    : IInterceptor
{
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse>
    {
        string name = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        try
        {
            TResponse response = await next(request);
            sw.Stop();

            metrics.IncrementCounter("requests_total", name);
            metrics.ObserveHistogram("request_duration_ms", name, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception)
        {
            sw.Stop();
            metrics.IncrementCounter("requests_failed_total", name);
            metrics.ObserveHistogram("request_duration_ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
