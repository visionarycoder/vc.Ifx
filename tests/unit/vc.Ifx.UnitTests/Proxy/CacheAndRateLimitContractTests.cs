using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Resilience;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class CacheAndRateLimitContractTests
{
    [TestMethod]
    public void CacheKeysSeparatePayloadsTenantsAndDelimiterBoundaries()
    {
        var provider = new DefaultCacheKeyProvider();
        var context = new ProxyContext { Method = "GET", OperationName = "read", Request = new { Id = 1 }, Body = "body" };
        string initial = provider.GenerateKey(context);
        context.Request = new { Id = 2 };
        provider.GenerateKey(context).Should().NotBe(initial);
        context.Metadata["TenantId"] = "tenant-a";
        string tenantA = provider.GenerateKey(context);
        context.Metadata["TenantId"] = "tenant-b";
        provider.GenerateKey(context).Should().NotBe(tenantA);
        context.Headers["authorization"] = "Bearer value";
        string lowercase = provider.GenerateKey(context);
        context.Headers.Clear();
        context.Headers["Authorization"] = "Bearer value";
        provider.GenerateKey(context).Should().Be(lowercase);
        context.ResultType = typeof(int);
        provider.GenerateKey(context).Should().Be(provider.GenerateKey<int>(context));
        var first = new ProxyContext { OperationName = "a|b", Method = "c", Url = "d" };
        var second = new ProxyContext { OperationName = "a", Method = "b", Url = "c|d" };
        provider.GenerateKey(first).Should().NotBe(provider.GenerateKey(second));
        Action missing = () => provider.GenerateKey(null!);
        missing.Should().Throw<ArgumentNullException>();
        Action missingGeneric = () => provider.GenerateKey<int>(null!);
        missingGeneric.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public async Task ClearInvalidatesOnlyItsOwnEntriesAndCopiesResponseEnvelopes()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
        var cache = new MemoryProxyCache(memory);
        var other = new MemoryProxyCache(memory);
        var original = ProxyResponse<string>.Success("value", 200);
        await cache.SetAsync("key", original, TimeSpan.FromMinutes(1));
        await other.SetAsync("key", ProxyResponse<string>.Success("other"), TimeSpan.FromMinutes(1));
        original.Data = "modified";
        (await cache.GetAsync<string>("key"))!.Data.Should().Be("value");
        (await cache.GetAsync<int>("key")).Should().BeNull();
        var copy = (await cache.GetAsync<string>("key"))!;
        copy.Data = "also modified";
        (await cache.GetAsync<string>("key"))!.Data.Should().Be("value");
        (await cache.ExistsAsync("key")).Should().BeTrue();
        await cache.ClearAsync();
        (await cache.ExistsAsync("key")).Should().BeFalse();
        (await cache.GetAsync<string>("key")).Should().BeNull();
        (await other.GetAsync<string>("key"))!.Data.Should().Be("other");
        await other.RemoveAsync("key");
        (await other.ExistsAsync("key")).Should().BeFalse();
        await cache.SetAsync("key", original, TimeSpan.Zero);
        (await cache.ExistsAsync("key")).Should().BeFalse();
    }

    [TestMethod]
    public async Task CacheFailuresAreBestEffortButCancellationAndInvalidArgumentsPropagate()
    {
        Action missingCache = () => new MemoryProxyCache(null!);
        missingCache.Should().Throw<ArgumentNullException>();
        var mock = new Mock<IMemoryCache>();
        object? ignored;
        mock.Setup(x => x.TryGetValue(It.IsAny<object>(), out ignored)).Throws<InvalidOperationException>();
        mock.Setup(x => x.CreateEntry(It.IsAny<object>())).Throws<InvalidOperationException>();
        mock.Setup(x => x.Remove(It.IsAny<object>())).Throws<InvalidOperationException>();
        var cache = new MemoryProxyCache(mock.Object);
        (await cache.GetAsync<string>("key")).Should().BeNull();
        (await cache.ExistsAsync("key")).Should().BeFalse();
        await cache.SetAsync("key", ProxyResponse<string>.Success("value"), TimeSpan.FromMinutes(1));
        await cache.RemoveAsync("key");
        var canceled = new CancellationToken(true);
        foreach (Func<Task> call in new Func<Task>[]
        {
            () => cache.GetAsync<string>("key", canceled), () => cache.ExistsAsync("key", canceled),
            () => cache.SetAsync("key", ProxyResponse<string>.Success("value"), TimeSpan.FromSeconds(1), canceled),
            () => cache.RemoveAsync("key", canceled), () => cache.ClearAsync(canceled)
        })
            await call.Should().ThrowAsync<OperationCanceledException>();
        foreach (Func<Task> call in new Func<Task>[]
        {
            () => cache.GetAsync<string>(""), () => cache.ExistsAsync(""),
            () => cache.SetAsync("", ProxyResponse<string>.Success("value"), TimeSpan.FromSeconds(1)),
            () => cache.RemoveAsync("")
        })
            await call.Should().ThrowAsync<ArgumentException>();
        Func<Task> nullResponse = () => cache.SetAsync<string>("key", null!, TimeSpan.FromSeconds(1));
        await nullResponse.Should().ThrowAsync<ArgumentNullException>();
        mock.Setup(x => x.TryGetValue(It.IsAny<object>(), out ignored)).Throws<OperationCanceledException>();
        mock.Setup(x => x.CreateEntry(It.IsAny<object>())).Throws<OperationCanceledException>();
        mock.Setup(x => x.Remove(It.IsAny<object>())).Throws<OperationCanceledException>();
        foreach (Func<Task> call in new Func<Task>[]
        {
            () => cache.GetAsync<string>("key"), () => cache.ExistsAsync("key"),
            () => cache.SetAsync("key", ProxyResponse<string>.Success("value"), TimeSpan.FromSeconds(1)),
            () => cache.RemoveAsync("key")
        })
            await call.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task RateLimitAtomicallyEnforcesCapacityAndPartitionsIdentities()
    {
        using var limiter = new RateLimitingInterceptor(NullLogger<RateLimitingInterceptor>.Instance,
            new RateLimiterConfig { MaxRequests = 3, TimeWindow = TimeSpan.FromMinutes(10) });
        int admitted = 0;
        Task<ProxyResponse<int>> Next(ProxyContext context, CancellationToken token)
        {
            Interlocked.Increment(ref admitted);
            return Task.FromResult(ProxyResponse<int>.Success(1));
        }
        await Task.WhenAll(Enumerable.Range(0, 50).Select(i => Task.Run(async () =>
        {
            var context = new ProxyContext { OperationName = "read" };
            try { await limiter.InvokeAsync<int>(context, Next); }
            catch (TransientProxyException) { context.Metadata["RateLimited"].Should().Be(true); }
        })));
        admitted.Should().Be(3);
        foreach (string kind in new[] { "UserId", "ClientId" })
        {
            var context = new ProxyContext { OperationName = "read" };
            context.Metadata[kind] = "same-id";
            await limiter.InvokeAsync<int>(context, Next);
            context.Metadata["RateLimited"].Should().Be(false);
            context.Metadata.Should().ContainKey("RateLimitKey");
        }
        admitted.Should().Be(5);
    }

    [TestMethod]
    public async Task RateLimiterValidatesConfigurationAndCancellation()
    {
        Action missingLogger = () => new RateLimitingInterceptor(null!);
        missingLogger.Should().Throw<ArgumentNullException>();
        Action invalidCapacity = () => new RateLimitingInterceptor(NullLogger<RateLimitingInterceptor>.Instance, new RateLimiterConfig { MaxRequests = 0 });
        invalidCapacity.Should().Throw<ArgumentOutOfRangeException>();
        Action invalidWindow = () => new RateLimitingInterceptor(NullLogger<RateLimitingInterceptor>.Instance, new RateLimiterConfig { TimeWindow = TimeSpan.Zero });
        invalidWindow.Should().Throw<ArgumentOutOfRangeException>();
        using var limiter = new RateLimitingInterceptor(NullLogger<RateLimitingInterceptor>.Instance);
        Task<ProxyResponse<int>> Next(ProxyContext context, CancellationToken token) => Task.FromResult(ProxyResponse<int>.Success(1));
        Func<Task> missingContext = () => limiter.InvokeAsync<int>(null!, Next);
        await missingContext.Should().ThrowAsync<ArgumentNullException>();
        Func<Task> missingNext = () => limiter.InvokeAsync<int>(new ProxyContext(), null!);
        await missingNext.Should().ThrowAsync<ArgumentNullException>();
        Func<Task> canceled = () => limiter.InvokeAsync<int>(new ProxyContext(), Next, new CancellationToken(true));
        await canceled.Should().ThrowAsync<OperationCanceledException>();
        var value = new ProxyContext();
        value.Metadata["UserId"] = null;
        value.Metadata["ClientId"] = null;
        await limiter.InvokeAsync<int>(value, Next);
        value.Metadata["UserId"] = new NullText();
        await limiter.InvokeAsync<int>(value, Next);
        value.Metadata.Remove("UserId");
        value.Metadata["ClientId"] = new NullText();
        await limiter.InvokeAsync<int>(value, Next);
    }

    private sealed class NullText
    {
        public override string ToString() => null!;
    }
}
