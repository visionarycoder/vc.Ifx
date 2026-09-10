using System.Net;
using System.Threading.Channels;
using Polly;
using VisionaryCoder.Framework.WebApi.Resilience;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace VisionaryCoder.Framework.Tests.WebApi.Resilience;

[TestClass]
public sealed class ResilienceContractTests
{
    private static WebApiResilienceOptions RetryOptions() => new()
    {
        EnableRetry = true, OperationsAreReplaySafe = true, EnableTimeout = false,
        RetryDelay = TimeSpan.Zero, UseJitter = false
    };

    private static ResiliencePipeline Pipeline(WebApiResilienceOptions options,
        IWebApiTransientFailureClassifier? classifier = null, TimeProvider? clock = null) =>
        new WebApiResiliencePipelineFactory(OptionsFactory.Create(options), classifier, clock).CreatePipeline();

    [TestMethod]
    public void DefaultsAreConservativeAndValidatedEvenWhenStrategiesAreDisabled()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new WebApiResiliencePipelineFactory(null!));
        var defaults = new WebApiResilienceOptions();
        Assert.IsFalse(defaults.EnableRetry);
        Assert.IsFalse(defaults.OperationsAreReplaySafe);
        Assert.IsTrue(defaults.EnableTimeout);
        Assert.IsTrue(defaults.UseJitter);
        Assert.AreEqual(3, defaults.RetryAttemptCount);
        Assert.AreEqual(TimeSpan.FromMilliseconds(200), defaults.RetryDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), defaults.Timeout);
        Action<WebApiResilienceOptions>[] invalid =
        [
            x => x.EnableRetry = true, x => x.RetryAttemptCount = 0, x => x.RetryAttemptCount = 101,
            x => x.RetryDelay = TimeSpan.FromTicks(-1), x => x.RetryDelay = TimeSpan.FromDays(1) + TimeSpan.FromTicks(1),
            x => x.Timeout = TimeSpan.Zero, x => x.Timeout = TimeSpan.FromMilliseconds(10) - TimeSpan.FromTicks(1),
            x => x.Timeout = TimeSpan.FromDays(1) + TimeSpan.FromTicks(1)
        ];
        foreach (var mutate in invalid)
        {
            var options = new WebApiResilienceOptions {EnableTimeout = false};
            mutate(options);
            Assert.Throws<ArgumentException>(() => Pipeline(options));
        }
        Assert.IsNotNull(Pipeline(new() {RetryAttemptCount = 100, RetryDelay = TimeSpan.FromDays(1), Timeout = TimeSpan.FromDays(1)}));
        Assert.IsNotNull(Pipeline(new() {Timeout = TimeSpan.FromMilliseconds(10)}));
    }

    [TestMethod]
    public void ClassifierExhaustivelyRejectsUnapprovedHttpStatuses()
    {
        var classifier = new WebApiTransientFailureClassifier();
        for (int code = 99; code <= 600; code++)
            Assert.AreEqual(code is 408 or 429 or 502 or 503 or 504,
                classifier.IsTransient(new HttpRequestException("error", null, (HttpStatusCode)code)), $"Status {code}");
        Assert.IsTrue(classifier.IsTransient(new HttpRequestException()));
        Assert.IsTrue(classifier.IsTransient(new TimeoutException()));
        Assert.IsFalse(classifier.IsTransient(new Exception()));
        Assert.IsFalse(classifier.IsTransient(new OperationCanceledException()));
        Assert.IsFalse(classifier.IsTransient(new TaskCanceledException()));
        Assert.ThrowsExactly<ArgumentNullException>(() => classifier.IsTransient(null!));
    }

    [TestMethod]
    public async Task RetryAttemptsAreBoundedAndSuccessfulResultsAreNotRetried()
    {
        var pipeline = Pipeline(RetryOptions());
        int calls = 0;
        await pipeline.ExecuteAsync(token =>
        {
            calls++;
            if (calls < 3) throw new HttpRequestException();
            return ValueTask.CompletedTask;
        });
        Assert.AreEqual(3, calls);
        calls = 0;
        var failure = new TimeoutException();
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<TimeoutException>(() => pipeline.ExecuteAsync(token =>
        {
            calls++;
            throw failure;
        }).AsTask()));
        Assert.AreEqual(4, calls);
        calls = 0;
        int result = await pipeline.ExecuteAsync(token =>
        {
            calls++;
            return ValueTask.FromResult(42);
        });
        Assert.AreEqual(42, result);
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public async Task DefaultsAndNonTransientFailuresNeverRetry()
    {
        foreach (var pipeline in new[] {Pipeline(new()), Pipeline(RetryOptions())})
        {
            int calls = 0;
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => pipeline.ExecuteAsync(token =>
            {
                calls++;
                throw new InvalidOperationException();
            }).AsTask());
            Assert.AreEqual(1, calls);
        }
        int transientCalls = 0;
        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => Pipeline(new()).ExecuteAsync(token =>
        {
            transientCalls++;
            throw new HttpRequestException();
        }).AsTask());
        Assert.AreEqual(1, transientCalls);
    }

    [TestMethod]
    public async Task ReplaceableClassifierCannotOverrideCancellation()
    {
        var classifier = new Classifier {Transient = true};
        var pipeline = Pipeline(RetryOptions(), classifier);
        int calls = 0;
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => pipeline.ExecuteAsync(token =>
        {
            calls++;
            throw new InvalidOperationException();
        }).AsTask());
        Assert.AreEqual(4, calls);
        Assert.IsTrue(classifier.Calls > 0);
        classifier.Calls = 0;
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => pipeline.ExecuteAsync(token =>
            throw new OperationCanceledException()).AsTask());
        Assert.AreEqual(0, classifier.Calls);
        using var source = new CancellationTokenSource();
        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => pipeline.ExecuteAsync(token =>
        {
            source.Cancel();
            throw new HttpRequestException();
        }, source.Token).AsTask());
        Assert.AreEqual(0, classifier.Calls);
        await Assert.ThrowsAsync<OperationCanceledException>(() => pipeline.ExecuteAsync(token =>
            throw new AssertFailedException("Canceled operation ran."), source.Token).AsTask());
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task TimeoutUsesInjectedClockAndCancelsTheOperation()
    {
        var clock = new ManualClock();
        var pipeline = Pipeline(new(), clock: clock);
        CancellationToken observed = default;
        Task operation = pipeline.ExecuteAsync(async token =>
        {
            observed = token;
            await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token);
        }).AsTask();
        Assert.AreEqual(TimeSpan.FromSeconds(30), await clock.FireNextAsync());
        var failure = await Assert.ThrowsAsync<Exception>(() => operation);
        Assert.AreEqual("Polly.Timeout.TimeoutRejectedException", failure.GetType().FullName);
        Assert.AreEqual(typeof(ResiliencePipeline).Assembly, failure.GetType().Assembly);
        Assert.IsTrue(new WebApiTransientFailureClassifier().IsTransient(failure));
        Assert.IsTrue(observed.IsCancellationRequested);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task RetryDelaysUseExponentialBackoffWithoutWallClockWaiting()
    {
        var clock = new ManualClock();
        var options = RetryOptions();
        options.RetryDelay = TimeSpan.FromMilliseconds(200);
        options.RetryAttemptCount = 2;
        var pipeline = Pipeline(options, clock: clock);
        int calls = 0;
        Task operation = pipeline.ExecuteAsync(token =>
        {
            if (++calls < 3) throw new HttpRequestException();
            return ValueTask.CompletedTask;
        }).AsTask();
        Assert.AreEqual(TimeSpan.FromMilliseconds(200), await clock.FireNextAsync());
        Assert.AreEqual(TimeSpan.FromMilliseconds(400), await clock.FireNextAsync());
        await operation;
        Assert.AreEqual(3, calls);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task PerAttemptTimeoutCanRetryAndCallerCancellationStopsRetryDelay()
    {
        var clock = new ManualClock();
        var options = RetryOptions();
        options.EnableTimeout = true;
        var pipeline = Pipeline(options, clock: clock);
        int calls = 0;
        Task operation = pipeline.ExecuteAsync(async token =>
        {
            if (++calls == 1) await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token);
        }).AsTask();
        await clock.FireNextAsync();
        await operation;
        Assert.AreEqual(2, calls);

        using var source = new CancellationTokenSource();
        clock = new ManualClock();
        options = RetryOptions();
        options.RetryDelay = TimeSpan.FromSeconds(1);
        pipeline = Pipeline(options, clock: clock);
        calls = 0;
        operation = pipeline.ExecuteAsync(token =>
        {
            calls++;
            throw new HttpRequestException();
        }, source.Token).AsTask();
        await clock.WaitForTimerAsync();
        source.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => operation);
        Assert.AreEqual(1, calls);
    }

    private sealed class Classifier : IWebApiTransientFailureClassifier
    {
        public bool Transient { get; init; }
        public int Calls { get; set; }
        public bool IsTransient(Exception exception) { Calls++; return Transient; }
    }

    // Every timer is advanced explicitly after observing its scheduled delay.
    private sealed class ManualClock : TimeProvider
    {
        private readonly Channel<(ManualTimer Timer, TimeSpan Due)> scheduled = Channel.CreateUnbounded<(ManualTimer, TimeSpan)>();
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new ManualTimer(this, callback, state);
            timer.Change(dueTime, period);
            return timer;
        }
        public async Task<TimeSpan> FireNextAsync()
        {
            var item = await scheduled.Reader.ReadAsync();
            item.Timer.Fire();
            return item.Due;
        }
        public async Task WaitForTimerAsync() => await scheduled.Reader.ReadAsync();
        private sealed class ManualTimer(ManualClock clock, TimerCallback callback, object? state) : ITimer
        {
            private bool disposed;
            public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                if (dueTime != global::System.Threading.Timeout.InfiniteTimeSpan)
                    clock.scheduled.Writer.TryWrite((this, dueTime));
                return true;
            }
            public void Fire() { if (!disposed) callback(state); }
            public void Dispose() => disposed = true;
            public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
        }
    }
}
