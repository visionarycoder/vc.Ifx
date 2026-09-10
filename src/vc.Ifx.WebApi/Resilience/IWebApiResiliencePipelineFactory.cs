using Polly;

namespace VisionaryCoder.Framework.WebApi.Resilience;

/// <summary>Replaceable policy boundary for resilience around volatile operations.</summary>
public interface IWebApiResiliencePipelineFactory
{
    /// <summary>Creates a pipeline whose operations must observe the supplied execution token.</summary>
    ResiliencePipeline CreatePipeline();
}
