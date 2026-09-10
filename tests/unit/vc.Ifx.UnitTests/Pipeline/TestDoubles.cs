using System.Threading.Channels;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors.Abstractions;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Tests.Pipeline;

public sealed record ReadQuery(string Key = "item") : IRequest<string?>;
public sealed record WriteRequest(string Key = "item") : IRequest<string?>;

internal sealed class LegacyInvoker : IInvoker
{
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse> => Task.FromResult(default(TResponse)!);
}
internal sealed class LocalFake : ILocalDispatcher
{
    public Func<object, CancellationToken, Task<object?>> Call { get; set; } = (request, token) => Task.FromResult<object?>("ok");
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse> => DispatchAsync<TRequest, TResponse>(request, default);
    public async Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse> =>
        (TResponse)(await Call(request, cancellationToken))!;
}
internal sealed class LegacyLocal : ILocalDispatcher
{
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse> => Task.FromResult(default(TResponse)!);
}
internal sealed class LegacyRemote : IRemoteDispatcher
{
    public EndpointResolution? Endpoint { get; private set; }
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint) where TRequest : IRequest<TResponse>
    {
        Endpoint = endpoint;
        return Task.FromResult(default(TResponse)!);
    }
}
internal sealed class ResolverFake : IEndpointResolver
{
    public EndpointResolution Endpoint { get; set; } = new(true);
    public int Calls { get; private set; }
    public EndpointResolution Resolve(Type requestType) { Calls++; return Endpoint; }
}
internal sealed class LegacyInterceptor : IInterceptor
{
    public Action? Before { get; set; }
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next) where TRequest : IRequest<TResponse>
    {
        Before?.Invoke();
        return next(request);
    }
}
internal sealed class CacheFake : ICache
{
    public bool Hit { get; set; }
    public object? Value { get; set; }
    public string? Key { get; private set; }
    public TimeSpan Ttl { get; private set; }
    public int Reads { get; private set; }
    public int Writes { get; private set; }
    public Action? OnRead { get; set; }
    public Exception? WriteFailure { get; set; }
    public Task<(bool Hit, T value)> TryGetAsync<T>(string key)
    {
        Reads++;
        Key = key;
        OnRead?.Invoke();
        return Task.FromResult((Hit, (T)Value!));
    }
    public Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        if (WriteFailure is not null) throw WriteFailure;
        Key = key; Value = value; Ttl = ttl; Writes++;
        return Task.CompletedTask;
    }
}
internal sealed class AuthFake : IAuthorizationService
{
    public Action<object>? Authorize { get; set; }
    public Task AuthorizeAsync(object request) { Authorize?.Invoke(request); return Task.CompletedTask; }
}
internal sealed class SpanFake : ISpan
{
    public Dictionary<string, string> Tags { get; } = [];
    public int Ends { get; private set; }
    public int Disposals { get; private set; }
    public void SetTag(string key, string value) => Tags[key] = value;
    public void End() => Ends++;
    public void Dispose() => Disposals++;
}
internal sealed class TracerFake : ITracer
{
    public SpanFake Span { get; } = new();
    public string? Name { get; private set; }
    public ISpan StartSpan(string name) { Name = name; return Span; }
}
internal sealed class MetricsFake : IMetrics
{
    public List<(string Metric, string Label)> Counters { get; } = [];
    public List<(string Metric, string Label, long Value)> Durations { get; } = [];
    public void IncrementCounter(string metric, string label) => Counters.Add((metric, label));
    public void ObserveHistogram(string metric, string label, long value) => Durations.Add((metric, label, value));
}
internal sealed class ManualClock : TimeProvider
{
    private readonly Channel<(Timer Timer, TimeSpan Delay)> timers = Channel.CreateUnbounded<(Timer, TimeSpan)>();
    public long Ticks { get; set; }
    public override long TimestampFrequency => 1000;
    public override long GetTimestamp() => Ticks;
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new Timer(this, callback, state);
        timer.Change(dueTime, period);
        return timer;
    }
    public async Task<TimeSpan> FireNextAsync()
    {
        var scheduled = await timers.Reader.ReadAsync();
        scheduled.Timer.Fire();
        return scheduled.Delay;
    }
    private sealed class Timer(ManualClock clock, TimerCallback callback, object? state) : ITimer
    {
        private bool disposed;
        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if (dueTime != global::System.Threading.Timeout.InfiniteTimeSpan)
                clock.timers.Writer.TryWrite((this, dueTime));
            return true;
        }
        public void Fire() { if (!disposed) callback(state); }
        public void Dispose() => disposed = true;
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}
