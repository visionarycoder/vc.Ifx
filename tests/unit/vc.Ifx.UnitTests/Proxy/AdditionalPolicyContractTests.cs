using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.QueryFiltering;
using VisionaryCoder.Framework.Proxy.Interceptors.Resilience;
using VisionaryCoder.Framework.Querying;
using VisionaryCoder.Framework.Querying.Serialization;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class AdditionalPolicyContractTests
{
    [TestMethod]
    public async Task QueryFilterUsesValidatedAstAndPreservesNonFilterResults()
    {
        var interceptor = new QueryFilterInterceptor();
        string json = QueryFilterSerializer.Serialize(new PropertyFilter("Equals", nameof(Row.Value), "2"));
        var context = new ProxyContext { Body = json };
        var result = await interceptor.InvokeAsync<QueryFilter<Row>>(context, (current, token) => throw new AssertFailedException());
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, new[] { new Row { Value = 1 }, new Row { Value = 2 } }.AsQueryable().Apply(result.Data!).Single().Value);
        foreach (object? body in new object?[] { null, new Row(), json })
        {
            context.Body = body;
            Assert.AreEqual(7, (await interceptor.InvokeAsync<int>(context, (current, token) => Task.FromResult(ProxyResponse<int>.Success(7)))).Data);
            Assert.IsTrue((await interceptor.InvokeAsync<List<int>>(context, (current, token) => Task.FromResult(ProxyResponse<List<int>>.Success([7])))).IsSuccess);
        }
        context.Body = "{}";
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => interceptor.InvokeAsync<int>(context, (current, token) => throw new AssertFailedException()));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(null!, (current, token) => throw new AssertFailedException()));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(new(), null!));
        await Assert.ThrowsAsync<OperationCanceledException>(() => interceptor.InvokeAsync<int>(new(), (current, token) => throw new AssertFailedException(), new(true)));
    }

    [TestMethod]
    public async Task CombinedResiliencePreservesResultsErrorsAndUsesOnlyClassifiedRetries()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ResilienceInterceptor(null!));
        var interceptor = new ResilienceInterceptor(NullLogger<ResilienceInterceptor>.Instance);
        Assert.AreEqual(10, interceptor.Order);
        int attempts = 0;
        var context = new ProxyContext();
        var response = ProxyResponse<int>.Success(9);
        Assert.AreSame(response, await interceptor.InvokeAsync<int>(context, (current, token) =>
        {
            if (++attempts == 1) throw new RetryableTransportException("retry");
            return Task.FromResult(response);
        }));
        Assert.AreEqual(2, attempts);
        Assert.AreEqual("true", context.Metadata["ResilienceApplied"]);
        var failure = new InvalidOperationException("original");
        attempts = 0;
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync<int>(context, (current, token) => { attempts++; throw failure; })));
        Assert.AreEqual(1, attempts);
        Assert.AreEqual(nameof(InvalidOperationException), context.Metadata["ResilienceException"]);
        var custom = new ResilienceInterceptor(NullLogger<ResilienceInterceptor>.Instance, new ResiliencePipelineBuilder().Build());
        var unsuccessful = ProxyResponse<int>.Failure("business outcome");
        Assert.AreSame(unsuccessful, await custom.InvokeAsync<int>(new() { OperationName = "operation", CorrelationId = "correlation" }, (current, token) => Task.FromResult(unsuccessful)));
        await Assert.ThrowsAsync<OperationCanceledException>(() => custom.InvokeAsync<int>(new(), (current, token) => throw new AssertFailedException(), new(true)));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => custom.InvokeAsync<int>(null!, (current, token) => throw new AssertFailedException()));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => custom.InvokeAsync<int>(new(), null!));
    }

    [TestMethod]
    public async Task CacheControlLookupIsCaseInsensitiveAndDoesNotMatchDirectiveSubstrings()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions());
        var interceptor = new CachingInterceptor(NullLogger<CachingInterceptor>.Instance, memory, new CachingOptions { KeyGenerator = context => context.OperationId });
        foreach (string? value in new[] { "NO-CACHE=\"name\"", "public, No-Store", "x-no-cache", "max-age=60", null })
        {
            var context = new ProxyContext { Method = "GET", Headers = new() { ["cAcHe-CoNtRoL"] = value! } };
            int calls = 0;
            Task<ProxyResponse<int>> Next(ProxyContext current, CancellationToken token) => Task.FromResult(ProxyResponse<int>.Success(++calls));
            await interceptor.InvokeAsync<int>(context, Next);
            await interceptor.InvokeAsync<int>(context, Next);
            Assert.AreEqual(value is "NO-CACHE=\"name\"" or "public, No-Store" ? 2 : 1, calls);
        }
        var nullHeaders = new ProxyContext { Method = "GET", Headers = null!, Metadata = null! };
        var keys = new DefaultCacheKeyProvider();
        Assert.AreEqual(keys.GenerateKey(new ProxyContext { Method = "GET" }), keys.GenerateKey(nullHeaders));
        nullHeaders.Metadata = [];
        await interceptor.InvokeAsync<int>(nullHeaders, (context, token) => Task.FromResult(ProxyResponse<int>.Success(1)));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(null!, (context, token) => throw new AssertFailedException()));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(new(), null!));
        await Assert.ThrowsAsync<OperationCanceledException>(() => interceptor.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException(), new(true)));
    }

    public sealed class Row { public int Value { get; set; } }
}
