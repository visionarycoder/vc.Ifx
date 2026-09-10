using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace VisionaryCoder.Framework.WebApi.Resilience;

/// <summary>Creates independent Polly pipelines with retry outside per-attempt timeout.</summary>
/// <param name="options">Validated resilience settings.</param>
/// <param name="classifier">Optional replacement failure classification.</param>
/// <param name="timeProvider">Optional clock for delays and cooperative timeouts.</param>
public sealed class WebApiResiliencePipelineFactory(
    IOptions<WebApiResilienceOptions> options,
    IWebApiTransientFailureClassifier? classifier = null,
    TimeProvider? timeProvider = null) : IWebApiResiliencePipelineFactory
{
    private readonly IOptions<WebApiResilienceOptions> options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public ResiliencePipeline CreatePipeline()
    {
        WebApiResilienceOptions resilienceOptions = options.Value;
        resilienceOptions.Validate();
        var failureClassifier = classifier ?? new WebApiTransientFailureClassifier();
        var builder = new ResiliencePipelineBuilder { TimeProvider = timeProvider ?? TimeProvider.System };

        if (resilienceOptions.EnableRetry)
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = resilienceOptions.RetryAttemptCount,
                Delay = resilienceOptions.RetryDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = resilienceOptions.UseJitter,
                ShouldHandle = args => ValueTask.FromResult(
                    !args.Context.CancellationToken.IsCancellationRequested
                    && args.Outcome.Exception is not null
                    && args.Outcome.Exception is not OperationCanceledException
                    && failureClassifier.IsTransient(args.Outcome.Exception))
            });
        }

        if (resilienceOptions.EnableTimeout)
        {
            builder.AddTimeout(resilienceOptions.Timeout);
        }

        return builder.Build();
    }
}
