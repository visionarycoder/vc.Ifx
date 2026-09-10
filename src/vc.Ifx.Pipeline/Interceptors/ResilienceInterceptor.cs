using Polly;
using Polly.Retry;
using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

/// <summary>Executes exactly one application-supplied resilience policy.</summary>
public sealed class ResilienceInterceptor : IInterceptor
{
    private readonly ResiliencePipeline? pipeline;
    private readonly AsyncPolicy? legacyPolicy;

    /// <summary>Creates an interceptor using current Polly pipeline APIs.</summary>
    public ResilienceInterceptor(ResiliencePipeline pipeline) =>
        this.pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));

    /// <summary>Retains existing policy consumers without wrapping them in another retry strategy.</summary>
    public ResilienceInterceptor(AsyncPolicy policy) =>
        legacyPolicy = policy ?? throw new ArgumentNullException(nameof(policy));
    /// <inheritdoc />
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse> => InvokeAsync(request, PipelineGuard.Adapt(next), CancellationToken.None);

    /// <inheritdoc />
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        PipelineGuard.Call(request, next, cancellationToken);
        if (pipeline is not null)
            return pipeline.ExecuteAsync(token =>
            {
                token.ThrowIfCancellationRequested();
                return new ValueTask<TResponse>(next(request, token));
            }, cancellationToken).AsTask();
        return legacyPolicy!.ExecuteAsync(token =>
        {
            token.ThrowIfCancellationRequested();
            return next(request, token);
        }, cancellationToken);
    }

    /// <summary>Legacy safe default: retries require an explicit application policy.</summary>
    public static AsyncPolicy DefaultPolicy() => Policy.NoOpAsync();

    /// <summary>Creates a safe default; retry is opt-in only for explicitly replay-safe operations.</summary>
    public static ResiliencePipeline DefaultPipeline(bool operationsAreReplaySafe = false, TimeProvider? timeProvider = null)
    {
        var builder = new ResiliencePipelineBuilder { TimeProvider = timeProvider ?? TimeProvider.System };
        if (operationsAreReplaySafe)
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args => ValueTask.FromResult(
                    !args.Context.CancellationToken.IsCancellationRequested &&
                    args.Outcome.Exception is HttpRequestException http &&
                    (http.StatusCode is null || (int)http.StatusCode is 408 or 429 or 502 or 503 or 504))
            });
        return builder.Build();
    }
}
