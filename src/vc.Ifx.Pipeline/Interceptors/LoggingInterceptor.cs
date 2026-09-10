using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : IInterceptor
{

    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next) where TRequest : IRequest<TResponse>
    {

        string name = typeof(TRequest).Name;
        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestType"] = name,
            ["CorrelationId"] = Correlation.CurrentId ?? Guid.NewGuid().ToString("N")
        });

        logger.LogInformation("Handling {RequestType}", name);
        var sw = Stopwatch.StartNew();
        try
        {
            TResponse response = await next(request);
            logger.LogInformation("Handled {RequestType} in {Elapsed}ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling {RequestType}", name);
            throw;
        }
    }
}
