using System.Net;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using VisionaryCoder.Framework.Pipeline.Interceptors;

namespace VisionaryCoder.Framework.Tests.Pipeline;

[TestClass]
public sealed class ResilienceTests
{
    [TestMethod]
    public async Task DefaultsDoNotRetryWithoutReplaySafety()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ResilienceInterceptor((ResiliencePipeline)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ResilienceInterceptor((AsyncPolicy)null!));
        foreach (var interceptor in new[] { new ResilienceInterceptor(ResilienceInterceptor.DefaultPolicy()), new ResilienceInterceptor(ResilienceInterceptor.DefaultPipeline()) })
        {
            int attempts = 0;
            var failure = new HttpRequestException("transient", null, HttpStatusCode.ServiceUnavailable);
            Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<HttpRequestException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), request => { attempts++; throw failure; })));
            Assert.AreEqual(1, attempts);
        }
    }

    [TestMethod]
    public async Task SuppliedPoliciesAreExecutedOnceWithoutExtraRetryLayers()
    {
        var current = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2, Delay = TimeSpan.Zero, ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>() }).Build();
        var legacy = Policy.Handle<InvalidOperationException>().RetryAsync(2);
        using var source = new CancellationTokenSource();
        foreach (var interceptor in new[] { new ResilienceInterceptor(current), new ResilienceInterceptor(legacy) })
        {
            int attempts = 0;
            Assert.AreEqual("ok", await interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) =>
            {
                Assert.AreEqual(source.Token, token);
                if (++attempts < 3) throw new InvalidOperationException();
                return Task.FromResult<string?>("ok");
            }, source.Token));
            Assert.AreEqual(3, attempts);
        }
    }

    [TestMethod]
    public async Task ReplaySafeDefaultsRetryOnlySelectedTransportFailuresWithBoundedAttempts()
    {
        foreach (int? status in new int?[] { null, 408, 429, 502, 503, 504 })
        {
            var clock = new ManualClock();
            var interceptor = new ResilienceInterceptor(ResilienceInterceptor.DefaultPipeline(true, clock));
            var failure = new HttpRequestException("transient", null, status is null ? null : (HttpStatusCode)status);
            int attempts = 0;
            var operation = interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { attempts++; throw failure; }, default);
            for (int retry = 0; retry < 3; retry++)
                Assert.IsTrue(await clock.FireNextAsync().WaitAsync(TimeSpan.FromSeconds(10)) >= TimeSpan.Zero);
            Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<HttpRequestException>(() => operation));
            Assert.AreEqual(4, attempts);
        }
    }

    [TestMethod]
    public async Task ReplaySafeDefaultsDoNotRetryPermanentFailuresSuccessOrCancellation()
    {
        var interceptor = new ResilienceInterceptor(ResilienceInterceptor.DefaultPipeline(true, new ManualClock()));
        foreach (var failure in new Exception[] { new InvalidOperationException(), new OperationCanceledException(),
            new HttpRequestException("auth", null, HttpStatusCode.Unauthorized), new HttpRequestException("server", null, HttpStatusCode.InternalServerError),
            new HttpRequestException("unsupported", null, HttpStatusCode.NotImplemented), new HttpRequestException("unknown", null, (HttpStatusCode)599) })
        {
            int attempts = 0;
            Assert.AreSame(failure, await Assert.ThrowsAsync<Exception>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { attempts++; throw failure; }, default)));
            Assert.AreEqual(1, attempts);
        }
        Assert.AreEqual("ok", await interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => Task.FromResult<string?>("ok"), default));
        using var source = new CancellationTokenSource();
        int canceledAttempts = 0;
        var canceledFailure = new HttpRequestException("stopped");
        Assert.AreSame(canceledFailure, await Assert.ThrowsExactlyAsync<HttpRequestException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { canceledAttempts++; source.Cancel(); throw canceledFailure; }, source.Token)));
        Assert.AreEqual(1, canceledAttempts);
    }

    [TestMethod]
    public async Task CancellationInterruptsRetryDelayWithoutAnotherAttempt()
    {
        using var source = new CancellationTokenSource();
        var interceptor = new ResilienceInterceptor(ResilienceInterceptor.DefaultPipeline(true, new ManualClock()));
        int attempts = 0;
        var operation = interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { attempts++; throw new HttpRequestException(); }, source.Token);
        source.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => operation);
        Assert.AreEqual(1, attempts);
    }

    [TestMethod]
    public async Task ApplicationTimeoutAndCircuitBreakerPoliciesAreReplaceable()
    {
        var clock = new ManualClock();
        var timeout = new ResilienceInterceptor(new ResiliencePipelineBuilder { TimeProvider = clock }.AddTimeout(TimeSpan.FromSeconds(30)).Build());
        var operation = timeout.InvokeAsync<ReadQuery, string?>(new(), async (request, token) => { await Task.Delay(Timeout.InfiniteTimeSpan, token); return "unreachable"; }, default);
        await clock.FireNextAsync().WaitAsync(TimeSpan.FromSeconds(10));
        var timedOut = await Assert.ThrowsAsync<Exception>(() => operation);
        Assert.AreEqual("Polly.Timeout.TimeoutRejectedException", timedOut.GetType().FullName);

        var breaker = new ResilienceInterceptor(new ResiliencePipelineBuilder { TimeProvider = clock }.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 1, MinimumThroughput = 2, SamplingDuration = TimeSpan.FromSeconds(30), BreakDuration = TimeSpan.FromSeconds(5),
            ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
        }).Build());
        int attempts = 0;
        Task<string?> Call(ReadQuery request, CancellationToken token) { attempts++; throw new InvalidOperationException(); }
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => breaker.InvokeAsync<ReadQuery, string?>(new(), Call, default));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => breaker.InvokeAsync<ReadQuery, string?>(new(), Call, default));
        var broken = await Assert.ThrowsAsync<Exception>(() => breaker.InvokeAsync<ReadQuery, string?>(new(), Call, default));
        Assert.AreEqual("Polly.CircuitBreaker.BrokenCircuitException", broken.GetType().FullName);
        Assert.AreEqual(2, attempts);
    }
}
