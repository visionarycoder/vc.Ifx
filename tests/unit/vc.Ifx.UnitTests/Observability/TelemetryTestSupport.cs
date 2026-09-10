using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace VisionaryCoder.Framework.Tests.Observability;

internal sealed class TraceCapture : IDisposable
{
    private readonly ActivityListener listener;
    public ConcurrentQueue<Activity> Started { get; } = new();
    public ConcurrentQueue<Activity> Stopped { get; } = new();
    public TraceCapture(ActivitySource source, ActivitySamplingResult sampling = ActivitySamplingResult.AllDataAndRecorded)
    {
        listener = new ActivityListener
        {
            ShouldListenTo = candidate => ReferenceEquals(source, candidate),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => sampling,
            SampleUsingParentId = (ref ActivityCreationOptions<string> options) => sampling,
            ActivityStarted = Started.Enqueue,
            ActivityStopped = Stopped.Enqueue
        };
        ActivitySource.AddActivityListener(listener);
    }
    public void Dispose() => listener.Dispose();
}

internal sealed class MetricCapture : IDisposable
{
    private readonly MeterListener listener;
    public ConcurrentQueue<Instrument> Published { get; } = new();
    public ConcurrentQueue<(Instrument Instrument, double Value, KeyValuePair<string, object?>[] Tags)> Samples { get; } = new();
    public MetricCapture(Func<Instrument, bool> include)
    {
        listener = new MeterListener { InstrumentPublished = (instrument, current) =>
        {
            if (!include(instrument)) return;
            Published.Enqueue(instrument);
            current.EnableMeasurementEvents(instrument);
        } };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, state) => Samples.Enqueue((instrument, value, tags.ToArray())));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, state) => Samples.Enqueue((instrument, value, tags.ToArray())));
        listener.Start();
    }
    public void Dispose() => listener.Dispose();
}

internal sealed class CallerActivity(string name) : Activity(name)
{
    public int Disposals { get; private set; }
    protected override void Dispose(bool disposing) { Disposals++; base.Dispose(disposing); }
}
