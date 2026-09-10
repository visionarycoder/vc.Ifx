using Microsoft.Extensions.Logging;
using Polly;
using VisionaryCoder.Framework.Pipeline;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors;

namespace VisionaryCoder.Framework.Tests.Pipeline;

[TestClass]
public sealed class InterceptorTests
{
    [TestMethod]
    public async Task EveryInterceptorGuardsDelegatesRequestsAndPreCancellation()
    {
        IInterceptor[] interceptors = [new AuthInterceptor(new AuthFake()), new CachingInterceptor(new CacheFake(), r => "key", TimeSpan.FromSeconds(1)),
            new LoggingInterceptor(new LoggerFake()), new MetricsInterceptor(new MetricsFake()), new TracingInterceptor(new TracerFake()),
            new ResilienceInterceptor(ResiliencePipeline.Empty), new LegacyInterceptor()];
        using var source = new CancellationTokenSource();
        source.Cancel();
        foreach (var interceptor in interceptors)
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), null!, default));
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<ReadQuery, string?>(null!, Next, default));
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), Next, source.Token));
            if (interceptor is not LegacyInterceptor)
                await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), null!));
            Assert.AreEqual("ok", await interceptor.InvokeAsync<ReadQuery, string?>(new(), r => Task.FromResult<string?>("ok")));
        }
    }

    [TestMethod]
    public async Task AuthorizationRunsBeforeContinuationAndDenialOrCancellationStopsIt()
    {
        bool authorized = false;
        var auth = new AuthFake { Authorize = request => authorized = true };
        var interceptor = new AuthInterceptor(auth);
        using var source = new CancellationTokenSource();
        await interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { Assert.IsTrue(authorized); Assert.AreEqual(source.Token, token); return Task.FromResult<string?>(null); }, source.Token);
        var failure = new UnauthorizedAccessException();
        auth.Authorize = request => throw failure;
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), Never, source.Token)));
        auth.Authorize = request => source.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), Never, source.Token));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AuthInterceptor(null!));
    }

    [TestMethod]
    public async Task CacheHandlesHitsIncludingNullMissesAndCommandBypass()
    {
        var cache = new CacheFake { Hit = true, Value = "cached" };
        var ttl = TimeSpan.FromMinutes(1);
        var interceptor = new CachingInterceptor(cache, request => ((ReadQuery)request).Key, ttl);
        Assert.AreEqual("cached", await interceptor.InvokeAsync<ReadQuery, string?>(new("tenant:item"), Never, default));
        cache.Value = null;
        Assert.IsNull(await interceptor.InvokeAsync<ReadQuery, string?>(new(), Never, default));
        cache.Hit = false;
        using var source = new CancellationTokenSource();
        Assert.AreEqual("fresh", await interceptor.InvokeAsync<ReadQuery, string?>(new("new:key"), (request, token) => { Assert.AreEqual(source.Token, token); return Task.FromResult<string?>("fresh"); }, source.Token));
        Assert.AreEqual("new:key", cache.Key);
        Assert.AreEqual(ttl, cache.Ttl);
        Assert.AreEqual(1, cache.Writes);
        Assert.AreEqual("command", await interceptor.InvokeAsync<WriteRequest, string?>(new(), (request, token) => Task.FromResult<string?>("command"), default));
        Assert.AreEqual(3, cache.Reads);
    }

    [TestMethod]
    public async Task CacheDoesNotStoreFailuresOrCanceledResultsAndPropagatesCacheFailures()
    {
        var cache = new CacheFake();
        var interceptor = new CachingInterceptor(cache, request => "key", TimeSpan.FromSeconds(1));
        var failure = new InvalidOperationException("failure");
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => throw failure, default)));
        using var source = new CancellationTokenSource();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { source.Cancel(); return Task.FromResult<string?>("ignored"); }, source.Token));
        Assert.AreEqual(0, cache.Writes);
        using var reading = new CancellationTokenSource();
        cache.OnRead = reading.Cancel;
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), Never, reading.Token));
        cache.OnRead = () => throw failure;
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), Never, default)));
        cache.OnRead = null;
        cache.WriteFailure = failure;
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), Next, default)));
        Assert.AreEqual(0, cache.Writes);
    }

    [TestMethod]
    public async Task CacheConfigurationIsValidated()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(null!, r => "key", TimeSpan.FromSeconds(1)));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CachingInterceptor(new CacheFake(), null!, TimeSpan.FromSeconds(1)));
        foreach (var seconds in new[] { 0, -1 })
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CachingInterceptor(new CacheFake(), r => "key", TimeSpan.FromSeconds(seconds)));
        foreach (var key in new[] { "", " " })
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => new CachingInterceptor(new CacheFake(), r => key, TimeSpan.FromSeconds(1)).InvokeAsync<ReadQuery, string?>(new(), Never, default));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => new CachingInterceptor(new CacheFake(), r => null!, TimeSpan.FromSeconds(1)).InvokeAsync<ReadQuery, string?>(new(), Never, default));
    }

    [TestMethod]
    public async Task LoggingScopesAndOutcomesAreDeterministic()
    {
        var clock = new ManualClock();
        var logger = new LoggerFake();
        var interceptor = new LoggingInterceptor(logger, clock);
        Correlation.CurrentId = "correlation";
        Assert.AreEqual("ok", await interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { clock.Ticks = 25; return Next(request, token); }, default));
        Assert.AreEqual("correlation", logger.Scope!["CorrelationId"]);
        Assert.AreEqual("ReadQuery", logger.Scope["RequestType"]);
        Assert.IsTrue(logger.Messages.Last().Text.Contains("25ms", StringComparison.Ordinal));
        Assert.AreEqual(1, logger.Disposals);
        Correlation.CurrentId = null;
        var failure = new InvalidOperationException("failure");
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => throw failure, default)));
        Assert.AreEqual(LogLevel.Error, logger.Messages.Last().Level);
        Assert.AreSame(failure, logger.Messages.Last().Error);
        Assert.IsTrue(Guid.TryParseExact((string)logger.Scope!["CorrelationId"], "N", out var parsed));
        var canceled = new OperationCanceledException();
        Assert.AreSame(canceled, await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => throw canceled, default)));
        Assert.AreEqual(LogLevel.Information, logger.Messages.Last().Level);
        Assert.AreEqual(3, logger.Disposals);
        logger.NullScope = true;
        await interceptor.InvokeAsync<ReadQuery, string?>(new(), Next, default);
        Assert.AreEqual(3, logger.Disposals);
        Assert.ThrowsExactly<ArgumentNullException>(() => new LoggingInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new LoggingInterceptor(logger, null!));
    }

    [TestMethod]
    public async Task MetricsRecordExactlyOneTotalAndDurationForEveryOutcome()
    {
        foreach (Exception? failure in new Exception?[] { null, new InvalidOperationException(), new OperationCanceledException() })
        {
            var metrics = new MetricsFake();
            var clock = new ManualClock { Ticks = 10 };
            var interceptor = new MetricsInterceptor(metrics, clock);
            async Task Run() => await interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => { clock.Ticks = 52; return failure is null ? Next(request, token) : Task.FromException<string?>(failure); }, default);
            if (failure is null) await Run();
            else Assert.AreSame(failure, await Assert.ThrowsAsync<Exception>(Run));
            Assert.AreEqual(1, metrics.Counters.Count(c => c.Metric == "requests_total"));
            Assert.AreEqual(failure is null ? 1 : 2, metrics.Counters.Count);
            if (failure is not null) Assert.AreEqual(failure is OperationCanceledException ? "requests_canceled_total" : "requests_failed_total", metrics.Counters[0].Metric);
            Assert.AreEqual(("request_duration_ms", "ReadQuery", 42L), metrics.Durations.Single());
        }
        Assert.ThrowsExactly<ArgumentNullException>(() => new MetricsInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new MetricsInterceptor(new MetricsFake(), null!));
    }

    [TestMethod]
    public async Task TracingEndsAndDisposesOnceWithCorrectStatusAndCorrelation()
    {
        foreach (Exception? failure in new Exception?[] { null, new InvalidOperationException("failure"), new OperationCanceledException() })
        {
            var tracer = new TracerFake();
            var interceptor = new TracingInterceptor(tracer);
            Correlation.CurrentId = failure is null ? "parent" : null;
            async Task Run() => await interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) => failure is null ? Next(request, token) : Task.FromException<string?>(failure), default);
            if (failure is null) await Run();
            else Assert.AreSame(failure, await Assert.ThrowsAsync<Exception>(Run));
            Assert.AreEqual("ReadQuery", tracer.Name);
            Assert.AreEqual("ReadQuery", tracer.Span.Tags["request.type"]);
            Assert.AreEqual(failure is null ? "success" : failure is OperationCanceledException ? "canceled" : "error", tracer.Span.Tags["status"]);
            Assert.AreEqual(1, tracer.Span.Ends);
            Assert.AreEqual(1, tracer.Span.Disposals);
            if (failure is null) Assert.AreEqual("parent", tracer.Span.Tags["correlation.id"]);
            else Assert.IsTrue(Guid.TryParseExact(tracer.Span.Tags["correlation.id"], "N", out var parsed));
            if (failure is InvalidOperationException) Assert.AreEqual("failure", tracer.Span.Tags["error.message"]);
            else Assert.IsFalse(tracer.Span.Tags.ContainsKey("error.message"));
        }
        Correlation.CurrentId = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => new TracingInterceptor(null!));
    }

    private static Task<string?> Next(ReadQuery request, CancellationToken token) => Task.FromResult<string?>("ok");
    private static Task<string?> Never(ReadQuery request, CancellationToken token) => throw new AssertFailedException("Continuation must not execute.");

    private sealed class LoggerFake : ILogger<LoggingInterceptor>
    {
        public Dictionary<string, object>? Scope { get; private set; }
        public bool NullScope { get; set; }
        public int Disposals { get; private set; }
        public List<(LogLevel Level, string Text, Exception? Error)> Messages { get; } = [];
        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            Scope = (Dictionary<string, object>)(object)state;
            return NullScope ? null : new ScopeLease(this);
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Messages.Add((logLevel, formatter(state, exception), exception));
        private sealed class ScopeLease(LoggerFake owner) : IDisposable { public void Dispose() => owner.Disposals++; }
    }
}
