using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

public sealed class TracingInterceptor(ITracer tracer) : IInterceptor
{

    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse>
    {
        string spanName = typeof(TRequest).Name;
        using ISpan span = tracer.StartSpan(spanName);
        try
        {
            span.SetTag("request.type", spanName);
            span.SetTag("correlation.id", Correlation.CurrentId ?? Guid.NewGuid().ToString());

            TResponse response = await next(request);

            span.SetTag("status", "success");
            return response;
        }
        catch (Exception ex)
        {
            span.SetTag("status", "error");
            span.SetTag("error.message", ex.Message);
            throw;
        }
        finally
        {
            span.End();
        }

    }

}
