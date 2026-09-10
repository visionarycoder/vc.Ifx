using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Pipeline;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors;
using VisionaryCoder.Framework.Pipeline.Observibility;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Tests.Observability;

[TestClass]
public sealed class PipelineIntegrationTests
{
    [TestMethod]
    public async Task DiResolvedAdaptersRecordSuccessFailureAndCancellationExactlyOnce()
    {
        foreach (var outcome in new[] { "success", "error", "canceled" })
        {
            using var source = new ActivitySource(Guid.NewGuid().ToString());
            using var meter = new Meter(Guid.NewGuid().ToString());
            using var traces = new TraceCapture(source);
            using var metrics = new MetricCapture(instrument => ReferenceEquals(instrument.Meter, meter));
            using var caller = new CallerActivity("caller");
            caller.Start();
            using var cancellation = new CancellationTokenSource();
            var clock = new Clock();
            var logger = new Logger();
            var failure = new InvalidOperationException("operation failed");
            var local = new Local { Invoke = async token =>
            {
                Assert.AreEqual(cancellation.Token, token);
                var active = Activity.Current;
                Assert.AreSame(caller, active!.Parent);
                await Task.Yield();
                Assert.AreSame(active, Activity.Current);
                clock.Ticks = 42;
                if (outcome == "error") throw failure;
                if (outcome == "canceled") { cancellation.Cancel(); token.ThrowIfCancellationRequested(); }
                return "result";
            } };
            var services = new ServiceCollection().AddSingleton(source).AddSingleton(meter)
                .AddSingleton<ITracer, OpenTelemetryTracer>().AddSingleton<IMetrics, OpenTelemetryMetrics>();
            using (var provider = services.BuildServiceProvider())
            {
                using var scope = provider.CreateScope();
                var tracer = scope.ServiceProvider.GetRequiredService<ITracer>();
                var metricAdapter = scope.ServiceProvider.GetRequiredService<IMetrics>();
                Assert.AreSame(tracer, provider.GetRequiredService<ITracer>());
                var invoker = new PipelineInvoker([new LoggingInterceptor(logger, clock), new TracingInterceptor(tracer), new MetricsInterceptor(metricAdapter, clock)], new Resolver(), local, new Remote());
                async Task Run() => Assert.AreEqual("result", await invoker.InvokeAsync<Request, string>(new(), cancellation.Token));
                if (outcome == "success") await Run();
                else if (outcome == "error") Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(Run));
                else
                {
                    var canceled = await Assert.ThrowsExactlyAsync<OperationCanceledException>(Run);
                    Assert.AreEqual(cancellation.Token, canceled.CancellationToken);
                }
            }
            Assert.AreSame(caller, Activity.Current);
            Assert.IsFalse(caller.IsStopped);
            Assert.AreEqual(0, caller.Disposals);
            Assert.AreEqual(1, traces.Started.Count);
            Assert.AreEqual(1, traces.Stopped.Count);
            var activity = traces.Stopped.Single();
            Assert.AreEqual(outcome, activity.GetTagItem("status"));
            Assert.AreEqual(outcome == "success" ? ActivityStatusCode.Ok : outcome == "error" ? ActivityStatusCode.Error : ActivityStatusCode.Unset, activity.Status);
            if (outcome == "error") Assert.AreEqual(failure.Message, activity.StatusDescription);
            Assert.AreEqual(0, activity.Events.Count());
            Assert.AreEqual(1, metrics.Samples.Count(sample => sample.Instrument.Name == "requests_total"));
            var duration = metrics.Samples.Single(sample => sample.Instrument.Name == "request_duration_ms");
            Assert.AreEqual(42d, duration.Value);
            Assert.AreEqual(nameof(Request), duration.Tags.Single().Value);
            Assert.AreEqual(outcome == "success" ? 2 : 3, metrics.Samples.Count);
            if (outcome != "success")
                Assert.AreEqual(1, metrics.Samples.Count(sample => sample.Instrument.Name == (outcome == "error" ? "requests_failed_total" : "requests_canceled_total")));
            Assert.AreEqual(2, logger.Levels.Count);
            Assert.AreEqual(outcome == "error" ? LogLevel.Error : LogLevel.Information, logger.Levels.Last());
            Assert.AreEqual(1, logger.Disposals);
            using var alive = new OpenTelemetryTracer(source).StartSpan("host-still-alive");
            new OpenTelemetryMetrics(meter).IncrementCounter("host-still-alive", "host");
            Assert.AreEqual(2, traces.Started.Count);
            Assert.AreEqual(1, metrics.Samples.Count(sample => sample.Instrument.Name == "host-still-alive"));
        }
    }

    [TestMethod]
    public async Task PreCanceledPipelineDoesNotEmitDiagnosticEvents()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var meter = new Meter(Guid.NewGuid().ToString());
        using var traces = new TraceCapture(source);
        using var metrics = new MetricCapture(instrument => ReferenceEquals(instrument.Meter, meter));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var invoker = new PipelineInvoker([new TracingInterceptor(new OpenTelemetryTracer(source)), new MetricsInterceptor(new OpenTelemetryMetrics(meter))], new Resolver(), new Local(), new Remote());
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => invoker.InvokeAsync<Request, string>(new(), cancellation.Token));
        Assert.AreEqual(0, traces.Started.Count);
        Assert.AreEqual(0, traces.Stopped.Count);
        Assert.AreEqual(0, metrics.Samples.Count);
    }

    private sealed record Request : IRequest<string>;
    private sealed class Resolver : IEndpointResolver { public EndpointResolution Resolve(Type requestType) => new(true); }
    private sealed class Remote : IRemoteDispatcher
    {
        public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint) where TRequest : IRequest<TResponse> => throw new AssertFailedException("Local expected.");
    }
    private sealed class Local : ILocalDispatcher
    {
        public Func<CancellationToken, Task<string>> Invoke { get; init; } = token => throw new AssertFailedException("Dispatch was not expected.");
        public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse> => DispatchAsync<TRequest, TResponse>(request, default);
        public async Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse> => (TResponse)(object)await Invoke(cancellationToken);
    }
    private sealed class Clock : TimeProvider
    {
        public long Ticks { get; set; }
        public override long TimestampFrequency => 1000;
        public override long GetTimestamp() => Ticks;
    }
    private sealed class Logger : ILogger<LoggingInterceptor>
    {
        public List<LogLevel> Levels { get; } = [];
        public int Disposals { get; private set; }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new Scope(this);
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Levels.Add(logLevel);
        private sealed class Scope(Logger logger) : IDisposable { public void Dispose() => logger.Disposals++; }
    }
}
