using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Observibility;

public sealed class OpenTelemetryMetrics : IMetrics
{
    private static readonly Meter defaultMeter = new("PipelineInvoker.Metrics");
    private static readonly ConditionalWeakTable<Meter, Instruments> instrumentsByMeter = new();
    private readonly Instruments instruments;

    /// <summary>Uses the process-lifetime PipelineInvoker.Metrics meter.</summary>
    public OpenTelemetryMetrics() : this(defaultMeter) { }

    /// <summary>Borrows a meter and shares instrument publication across adapters using that meter.</summary>
    public OpenTelemetryMetrics(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);
        instruments = instrumentsByMeter.GetValue(meter, static value => new Instruments(value));
    }

    /// <inheritdoc />
    public void IncrementCounter(string metric, string label)
    {
        Validate(metric, label);
        var counter = instruments.Counters.GetOrAdd(metric, name => new Lazy<Counter<long>>(() =>
            instruments.Meter.CreateCounter<long>(name, unit: "count", description: "Pipeline request count."))).Value;
        counter.Add(1, new KeyValuePair<string, object?>("request", label));
    }

    /// <inheritdoc />
    public void ObserveHistogram(string metric, string label, long valueMs)
    {
        Validate(metric, label);
        ArgumentOutOfRangeException.ThrowIfNegative(valueMs);
        var histogram = instruments.Histograms.GetOrAdd(metric, name => new Lazy<Histogram<double>>(() =>
            instruments.Meter.CreateHistogram<double>(name, unit: "ms", description: "Pipeline request duration."))).Value;
        histogram.Record(valueMs, new KeyValuePair<string, object?>("request", label));
    }

    private static void Validate(string metric, string label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
    }

    private sealed class Instruments(Meter meter)
    {
        public Meter Meter { get; } = meter;
        public ConcurrentDictionary<string, Lazy<Counter<long>>> Counters { get; } = new(StringComparer.Ordinal);
        public ConcurrentDictionary<string, Lazy<Histogram<double>>> Histograms { get; } = new(StringComparer.Ordinal);
    }
}
