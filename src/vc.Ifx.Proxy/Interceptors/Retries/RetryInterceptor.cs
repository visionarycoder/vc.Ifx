using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Retries;

/// <summary>Retries explicitly classified transport failures using bounded Polly exponential backoff.</summary>
public sealed class RetryInterceptor : IOrderedProxyInterceptor
{
    private readonly ILogger<RetryInterceptor> logger;
    private readonly ResiliencePipeline pipeline;

    /// <inheritdoc />
    public int Order => 200;

    /// <summary>Creates a retry policy from a validated snapshot of the supplied options.</summary>
    public RetryInterceptor(ILogger<RetryInterceptor> logger, IOptionsSnapshot<ProxyOptions> options)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ProxyOptions configured = options?.Value ?? throw new ArgumentNullException(nameof(options));
        ArgumentOutOfRangeException.ThrowIfNegative(configured.MaxRetryAttempts);
        if (configured.RetryDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "Retry delay cannot be negative.");

        pipeline = configured.MaxRetryAttempts == 0
            ? ResiliencePipeline.Empty
            : new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RetryableTransportException>(),
                MaxRetryAttempts = configured.MaxRetryAttempts,
                Delay = configured.RetryDelay,
                MaxDelay = TimeSpan.FromSeconds(30),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = arguments =>
                {
                    this.logger.LogWarning(arguments.Outcome.Exception,
                        "Retryable exception on attempt {Attempt}, retrying in {Delay}ms",
                        arguments.AttemptNumber + 1, arguments.RetryDelay.TotalMilliseconds);
                    return default;
                }
            }).Build();
    }

    /// <inheritdoc />
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.CancellationToken);
        linked.Token.ThrowIfCancellationRequested();
        int attempts = 0;
        try
        {
            ProxyResponse<T> response = await pipeline.ExecuteAsync(async token =>
            {
                attempts++;
                return await next(context, token).ConfigureAwait(false);
            }, linked.Token).ConfigureAwait(false);
            if (attempts > 1)
                logger.LogInformation("Operation succeeded after {Attempt} retries", attempts - 1);
            return response;
        }
        catch (RetryableTransportException exception)
        {
            logger.LogError(exception, "Operation failed after {MaxAttempts} attempts, giving up", attempts);
            throw;
        }
    }
}
