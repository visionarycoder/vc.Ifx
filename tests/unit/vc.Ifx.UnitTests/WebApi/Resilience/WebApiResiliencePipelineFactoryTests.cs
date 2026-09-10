using Polly;
using VisionaryCoder.Framework.WebApi.Resilience;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace VisionaryCoder.Framework.Tests.WebApi.Resilience;

[TestClass]
public sealed class WebApiResiliencePipelineFactoryTests
{
    [TestMethod]
    public void CreatePipeline_WithDefaultOptions_ShouldCreatePipeline()
    {
        var factory = new WebApiResiliencePipelineFactory(OptionsFactory.Create(new WebApiResilienceOptions()));

        ResiliencePipeline pipeline = factory.CreatePipeline();

        pipeline.Should().NotBeNull();
    }

    [TestMethod]
    public void CreatePipeline_WithRetryAndTimeoutDisabled_ShouldCreatePipeline()
    {
        var options = new WebApiResilienceOptions
        {
            EnableRetry = false,
            EnableTimeout = false
        };
        var factory = new WebApiResiliencePipelineFactory(OptionsFactory.Create(options));

        ResiliencePipeline pipeline = factory.CreatePipeline();

        pipeline.Should().NotBeNull();
    }
}
