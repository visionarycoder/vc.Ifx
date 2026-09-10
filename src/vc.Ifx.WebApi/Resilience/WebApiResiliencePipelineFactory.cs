using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace VisionaryCoder.Framework.WebApi.Resilience;

public sealed class WebApiResiliencePipelineFactory(IOptions<WebApiResilienceOptions> options) : IWebApiResiliencePipelineFactory
{
    public ResiliencePipeline CreatePipeline()
    {
        WebApiResilienceOptions resilienceOptions = options.Value;
        var builder = new ResiliencePipelineBuilder();

        if (resilienceOptions.EnableRetry)
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = resilienceOptions.RetryAttemptCount,
                Delay = resilienceOptions.RetryDelay,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TimeoutException>()
            });
        }

        if (resilienceOptions.EnableTimeout)
        {
            builder.AddTimeout(resilienceOptions.Timeout);
        }

        return builder.Build();
    }
}
