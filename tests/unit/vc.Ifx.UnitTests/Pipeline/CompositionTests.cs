using Polly;
using Polly.Retry;
using VisionaryCoder.Framework.Pipeline;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors;
using VisionaryCoder.Framework.Pipeline.Interceptors.Abstractions;

namespace VisionaryCoder.Framework.Tests.Pipeline;

[TestClass]
public sealed class CompositionTests
{
    [TestMethod]
    public async Task AuthorizationStillRunsForCacheHitsAndDenialNeverReachesCache()
    {
        using var source = new CancellationTokenSource();
        var cache = new TokenCache(source.Token);
        var auth = new TokenAuth(source.Token);
        var resolver = new ResolverFake();
        int dispatches = 0;
        var local = new LocalFake { Call = (request, token) => { dispatches++; Assert.AreEqual(source.Token, token); return Task.FromResult<object?>(null); } };
        var invoker = new PipelineInvoker([new AuthInterceptor(auth), new CachingInterceptor(cache, request => "tenant:read", TimeSpan.FromMinutes(1))], resolver, local, new LegacyRemote());
        Assert.IsNull(await invoker.InvokeAsync<ReadQuery, string?>(new(), source.Token));
        Assert.IsNull(await invoker.InvokeAsync<ReadQuery, string?>(new(), source.Token));
        Assert.AreEqual(2, auth.Calls);
        Assert.AreEqual(2, cache.Reads);
        Assert.AreEqual(1, cache.Writes);
        Assert.AreEqual(1, dispatches);
        Assert.AreEqual(1, resolver.Calls);
        auth.Deny = true;
        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => invoker.InvokeAsync<ReadQuery, string?>(new(), source.Token));
        Assert.AreEqual(3, auth.Calls);
        Assert.AreEqual(2, cache.Reads);
        Assert.AreEqual(1, dispatches);
    }

    [TestMethod]
    public async Task DiagnosticsOutsideResilienceMeasureOneLogicalOperation()
    {
        var metrics = new MetricsFake();
        var tracer = new TracerFake();
        var clock = new ManualClock();
        int attempts = 0;
        var local = new LocalFake { Call = (request, token) =>
        {
            clock.Ticks += 10;
            if (++attempts < 4) throw new HttpRequestException();
            return Task.FromResult<object?>("ok");
        } };
        var policy = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3, Delay = TimeSpan.Zero, ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        }).Build();
        var invoker = new PipelineInvoker([new TracingInterceptor(tracer), new MetricsInterceptor(metrics, clock), new ResilienceInterceptor(policy)], new ResolverFake(), local, new LegacyRemote());
        Assert.AreEqual("ok", await invoker.InvokeAsync<ReadQuery, string?>(new()));
        Assert.AreEqual(4, attempts);
        Assert.AreEqual(("requests_total", "ReadQuery"), metrics.Counters.Single());
        Assert.AreEqual(40L, metrics.Durations.Single().Value);
        Assert.AreEqual("success", tracer.Span.Tags["status"]);
        Assert.AreEqual(1, tracer.Span.Ends);
        Assert.AreEqual(1, tracer.Span.Disposals);
    }

    [TestMethod]
    public async Task NestedInvocationsReuseAndRestoreParentCorrelation()
    {
        Correlation.CurrentId = null;
        string? outerId = null;
        var inner = new PipelineInvoker([], new ResolverFake(), new LocalFake { Call = (request, token) => Task.FromResult<object?>(Correlation.CurrentId) }, new LegacyRemote());
        var outer = new PipelineInvoker([], new ResolverFake(), new LocalFake { Call = async (request, token) =>
        {
            outerId = Correlation.CurrentId;
            Assert.AreEqual(outerId, await inner.InvokeAsync<ReadQuery, string?>(new(), token));
            Assert.AreEqual(outerId, Correlation.CurrentId);
            return "ok";
        } }, new LegacyRemote());
        await outer.InvokeAsync<ReadQuery, string?>(new());
        Assert.IsNotNull(outerId);
        Assert.IsNull(Correlation.CurrentId);
    }

    private sealed class TokenAuth(CancellationToken expected) : IAuthorizationService
    {
        public int Calls { get; private set; }
        public bool Deny { get; set; }
        public Task AuthorizeAsync(object request) => throw new AssertFailedException("The token overload is required.");
        public Task AuthorizeAsync(object request, CancellationToken cancellationToken)
        {
            Assert.AreEqual(expected, cancellationToken);
            Calls++;
            if (Deny) throw new UnauthorizedAccessException();
            return Task.CompletedTask;
        }
    }

    private sealed class TokenCache(CancellationToken expected) : ICache
    {
        public int Reads { get; private set; }
        public int Writes { get; private set; }
        public Task<(bool Hit, T value)> TryGetAsync<T>(string key) => throw new AssertFailedException("The token overload is required.");
        public Task SetAsync<T>(string key, T value, TimeSpan ttl) => throw new AssertFailedException("The token overload is required.");
        public Task<(bool Hit, T value)> TryGetAsync<T>(string key, CancellationToken cancellationToken)
        {
            Assert.AreEqual(expected, cancellationToken);
            Assert.AreEqual("tenant:read", key);
            Reads++;
            return Task.FromResult((Writes != 0, default(T)!));
        }
        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
        {
            Assert.AreEqual(expected, cancellationToken);
            Assert.AreEqual("tenant:read", key);
            Assert.AreEqual(TimeSpan.FromMinutes(1), ttl);
            Assert.IsNull(value);
            Writes++;
            return Task.CompletedTask;
        }
    }
}
