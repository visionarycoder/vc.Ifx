using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Observibility;

public sealed class OpenTelemetryMetrics : IMetrics
{
    private static readonly Meter meter = new("PipelineInvoker.Metrics");
    private readonly ConcurrentDictionary<string, Counter<long>> counters = new();
    private readonly ConcurrentDictionary<string, Histogram<double>> histograms = new();

    public void IncrementCounter(string metric, string label)
    {
        Counter<long> c = counters.GetOrAdd($"{metric}:{label}",
            _ => meter.CreateCounter<long>(metric, unit: "count", description: $"Counter for {label}"));
        c.Add(1, new KeyValuePair<string, object?>("request", label));
    }

    public void ObserveHistogram(string metric, string label, long valueMs)
    {
        Histogram<double> h = histograms.GetOrAdd($"{metric}:{label}",
            _ => meter.CreateHistogram<double>(metric, unit: "ms", description: $"Latency for {label}"));
        h.Record(valueMs, new KeyValuePair<string, object?>("request", label));
    }
}