using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class CachingDecisionContractTests
{
    [TestMethod]
    public async Task SuccessfulNullPayloadCanBeCachedWithoutEvaluatingPayloadPredicate()
    {
        var cache = new Mock<IProxyCache>();
        var key = new Mock<ICacheKeyProvider>();
        key.Setup(x => x.GenerateKey(It.IsAny<ProxyContext>())).Returns("key");
        var policy = new Mock<ICachePolicyProvider>();
        policy.Setup(x => x.ShouldCache(It.IsAny<ProxyContext>())).Returns(true);
        policy.Setup(x => x.GetPolicy(It.IsAny<ProxyContext>())).Returns(new CachePolicy { ShouldCache = data => throw new InvalidOperationException("null must bypass predicate") });
        var response = ProxyResponse<string>.Success(null!);
        var interceptor = new CachingInterceptor(NullLogger<CachingInterceptor>.Instance, cache.Object, key.Object, policy.Object);
        Assert.AreSame(response, await interceptor.InvokeAsync<string>(new(), (context, token) => Task.FromResult(response)));
        cache.Verify(x => x.SetAsync("key", response, TimeSpan.FromMinutes(5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task PolicyAndKeyBypassesNeverReadOrWriteCache()
    {
        foreach (var mode in new[] { "disabled", "request", "null", "blank", "response" })
        {
            var cache = new Mock<IProxyCache>(MockBehavior.Strict);
            var key = new Mock<ICacheKeyProvider>();
            key.Setup(x => x.GenerateKey(It.IsAny<ProxyContext>())).Returns(mode == "null" ? null : mode == "blank" ? " " : "key");
            var policy = new Mock<ICachePolicyProvider>();
            policy.Setup(x => x.GetPolicy(It.IsAny<ProxyContext>())).Returns(new CachePolicy { IsCachingEnabled = mode != "disabled", ShouldCache = data => false });
            policy.Setup(x => x.ShouldCache(It.IsAny<ProxyContext>())).Returns(mode != "request");
            if (mode == "response") cache.Setup(x => x.GetAsync<int>("key", It.IsAny<CancellationToken>())).ReturnsAsync((ProxyResponse<int>?)null);
            var interceptor = new CachingInterceptor(NullLogger<CachingInterceptor>.Instance, cache.Object, key.Object, policy.Object);
            int calls = 0;
            foreach (object disable in new object[] { false, "false" })
            {
                var context = new ProxyContext();
                context.Metadata["DisableCache"] = disable;
                var result = await interceptor.InvokeAsync<int>(context, (call, token) => { calls++; return Task.FromResult(ProxyResponse<int>.Success(7)); });
                Assert.AreEqual(7, result.Data);
            }
            Assert.AreEqual(2, calls);
            if (mode == "response") cache.Verify(x => x.GetAsync<int>("key", It.IsAny<CancellationToken>()), Times.Exactly(2));
            else cache.VerifyNoOtherCalls();
            cache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ProxyResponse<int>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [TestMethod]
    public void DependenciesAreRequiredInBothConstructors()
    {
        var logger = NullLogger<CachingInterceptor>.Instance;
        var cache = Mock.Of<IProxyCache>();
        var key = Mock.Of<ICacheKeyProvider>();
        var policy = Mock.Of<ICachePolicyProvider>();
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(null!, cache, key, policy));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(logger, null!, key, policy));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(logger, cache, null!, policy));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(logger, cache, key, null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(logger, null!, new CachingOptions()));
        using var memory = new MemoryCache(new MemoryCacheOptions());
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(logger, memory, null!));
    }
}
