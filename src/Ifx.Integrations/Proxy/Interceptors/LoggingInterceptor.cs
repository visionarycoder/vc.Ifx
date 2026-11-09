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

/// <summary>Records scoped lifecycle diagnostics without logging request payloads.</summary>
public sealed class LoggingInterceptor : IInterceptor
{
    private readonly ILogger<LoggingInterceptor> logger;
    private readonly TimeProvider clock;

    /// <inheritdoc />
    public int Order => 100;

    /// <summary>Creates a logger using the system clock.</summary>
    public LoggingInterceptor(ILogger<LoggingInterceptor> logger) : this(logger, TimeProvider.System) { }
    /// <summary>Creates a logger using an explicit clock.</summary>
    public LoggingInterceptor(ILogger<LoggingInterceptor> logger, TimeProvider timeProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestType"] = name,
            ["CorrelationId"] = Ifx.Pipeline.Correlation.CurrentId ?? Guid.NewGuid().ToString("N")
        });
        logger.LogInformation("Handling {RequestType}", name);
        long start = clock.GetTimestamp();
        try
        {
            var response = await next(request, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Handled {RequestType} in {Elapsed}ms", name, clock.GetElapsedTime(start).TotalMilliseconds);
            return response;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Canceled {RequestType}", name);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error handling {RequestType}", name);
            throw;
        }
    }
}
