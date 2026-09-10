using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class ProxyContractTests
{
    [TestMethod]
    public async Task PipelineUsesReplacementContextAndRejectsInvalidInputs()
    {
        var original = new ProxyContext();
        var replacement = new ProxyContext();
        var transport = new Mock<IProxyTransport>();
        transport.Setup(x => x.SendCoreAsync<int>(replacement, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProxyResponse<int>.Success(42));
        var pipeline = new DefaultProxyPipeline([new ReplacingInterceptor(replacement)], transport.Object);
        (await pipeline.SendAsync<int>(original)).Data.Should().Be(42);
        transport.Verify(x => x.SendCoreAsync<int>(replacement, It.IsAny<CancellationToken>()), Times.Once);

        Action missing = () => new DefaultProxyPipeline(null!, transport.Object);
        missing.Should().Throw<ArgumentNullException>();
        Action missingElement = () => new DefaultProxyPipeline([null!], transport.Object);
        missingElement.Should().Throw<ArgumentNullException>();
        Func<Task> canceled = () => pipeline.SendAsync<int>(original, new CancellationToken(true));
        await canceled.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public void CachingRegistrationResolvesAllDefaultsAndPipelineUsesScopes()
    {
        var services = new ServiceCollection();
        services.AddCaching<MemoryProxyCache>(options => options.DefaultDuration = TimeSpan.FromMinutes(2));
        services.AddProxyPipeline();
        services.AddSingleton(Mock.Of<IProxyTransport>());
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using IServiceScope first = provider.CreateScope();
        using IServiceScope second = provider.CreateScope();
        first.ServiceProvider.GetRequiredService<IProxyPipeline>().Should().NotBeSameAs(second.ServiceProvider.GetRequiredService<IProxyPipeline>());
        provider.GetRequiredService<CachingOptions>().DefaultDuration.Should().Be(TimeSpan.FromMinutes(2));
        provider.GetServices<IProxyInterceptor>().Should().ContainSingle(x => x is CachingInterceptor);
    }

    [TestMethod]
    public async Task RetryOptionsAreValidatedAndZeroMeansOneAttempt()
    {
        Action negativeCount = () => Retry(new ProxyOptions { MaxRetries = -1 });
        negativeCount.Should().Throw<ArgumentOutOfRangeException>();
        Action negativeDelay = () => Retry(new ProxyOptions { RetryDelay = TimeSpan.FromTicks(-1) });
        negativeDelay.Should().Throw<ArgumentOutOfRangeException>();
        var policy = Retry(new ProxyOptions { MaxRetries = 0, RetryDelay = TimeSpan.Zero });
        int calls = 0;
        Task<ProxyResponse<int>> Next(ProxyContext context, CancellationToken token)
        {
            calls++;
            throw new RetryableTransportException("temporary");
        }
        Func<Task> run = () => policy.InvokeAsync<int>(new ProxyContext(), Next);
        await run.Should().ThrowAsync<RetryableTransportException>();
        calls.Should().Be(1);
    }

    [TestMethod]
    public async Task RetryHonorsBothCancellationSourcesAndNeverReplaysAfterCancellation()
    {
        var policy = Retry(new ProxyOptions { MaxRetries = 3, RetryDelay = TimeSpan.Zero });
        int calls = 0;
        Task<ProxyResponse<int>> Next(ProxyContext context, CancellationToken token)
        {
            calls++;
            return Task.FromResult(ProxyResponse<int>.Success(1));
        }
        Func<Task> explicitCancellation = () => policy.InvokeAsync<int>(new ProxyContext(), Next, new CancellationToken(true));
        await explicitCancellation.Should().ThrowAsync<OperationCanceledException>();
        Func<Task> legacyCancellation = () => policy.InvokeAsync<int>(new ProxyContext { CancellationToken = new CancellationToken(true) }, Next);
        await legacyCancellation.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
        using var cancellation = new CancellationTokenSource();
        Task<ProxyResponse<int>> CancelThenFail(ProxyContext context, CancellationToken token)
        {
            calls++;
            cancellation.Cancel();
            throw new RetryableTransportException("temporary");
        }
        Func<Task> duringCall = () => policy.InvokeAsync<int>(new ProxyContext(), CancelThenFail, cancellation.Token);
        await duringCall.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);

        Func<Task> nullContext = () => policy.InvokeAsync<int>(null!, Next);
        await nullContext.Should().ThrowAsync<ArgumentNullException>();
        Func<Task> nullNext = () => policy.InvokeAsync<int>(new ProxyContext(), null!);
        await nullNext.Should().ThrowAsync<ArgumentNullException>();
    }

    private static RetryInterceptor Retry(ProxyOptions value)
    {
        var snapshot = new Mock<IOptionsSnapshot<ProxyOptions>>();
        snapshot.Setup(x => x.Value).Returns(value);
        return new RetryInterceptor(NullLogger<RetryInterceptor>.Instance, snapshot.Object);
    }

    private sealed class ReplacingInterceptor(ProxyContext replacement) : IProxyInterceptor
    {
        public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
            => next(replacement, cancellationToken);
    }
}
