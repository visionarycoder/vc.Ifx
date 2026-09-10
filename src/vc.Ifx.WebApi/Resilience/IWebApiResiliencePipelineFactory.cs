using Polly;

namespace VisionaryCoder.Framework.WebApi.Resilience;

public interface IWebApiResiliencePipelineFactory
{
    ResiliencePipeline CreatePipeline();
}
