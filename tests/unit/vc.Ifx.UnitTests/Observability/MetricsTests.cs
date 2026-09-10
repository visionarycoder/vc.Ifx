using System.Diagnostics.Metrics;
using VisionaryCoder.Framework.Pipeline.Observibility;

namespace VisionaryCoder.Framework.Tests.Observability;

[TestClass]
public sealed class MetricsTests
{
    [TestMethod]
    public void MeasurementsUseStableTypesUnitsAndRequestTagsExactlyOnce()
    {
        using var meter = new Meter(Guid.NewGuid().ToString());
        using var capture = new MetricCapture(instrument => ReferenceEquals(instrument.Meter, meter));
        var metrics = new OpenTelemetryMetrics(meter);
        metrics.IncrementCounter("total", "one");
        metrics.IncrementCounter("total", "two");
        metrics.ObserveHistogram("duration", "one", 0);
        metrics.ObserveHistogram("duration", "two", 42);
        Assert.AreEqual(2, capture.Published.Count);
        var counter = capture.Published.Single(instrument => instrument.Name == "total");
        var histogram = capture.Published.Single(instrument => instrument.Name == "duration");
        Assert.IsInstanceOfType<Counter<long>>(counter);
        Assert.IsInstanceOfType<Histogram<double>>(histogram);
        Assert.AreEqual("count", counter.Unit);
        Assert.AreEqual("ms", histogram.Unit);
        Assert.AreEqual("Pipeline request count.", counter.Description);
        Assert.AreEqual("Pipeline request duration.", histogram.Description);
        Assert.AreEqual(4, capture.Samples.Count);
        CollectionAssert.AreEqual(new[] { 1d, 1d, 0d, 42d }, capture.Samples.Select(sample => sample.Value).ToArray());
        CollectionAssert.AreEqual(new[] { "one", "two", "one", "two" }, capture.Samples.Select(sample => (string)sample.Tags.Single().Value!).ToArray());
        Assert.IsTrue(capture.Samples.All(sample => sample.Tags.Single().Key == "request"));
    }

    [TestMethod]
    public async Task SharedMeterConcurrentFirstUsePublishesOneInstrumentPerKind()
    {
        using var meter = new Meter(Guid.NewGuid().ToString());
        using var capture = new MetricCapture(instrument => ReferenceEquals(instrument.Meter, meter));
        var adapters = Enumerable.Range(0, 16).Select(index => new OpenTelemetryMetrics(meter)).ToArray();
        await Task.WhenAll(Enumerable.Range(0, 256).Select(index => Task.Run(() =>
        {
            var adapter = adapters[index % adapters.Length];
            adapter.IncrementCounter("requests", (index % 4).ToString());
            adapter.ObserveHistogram("duration", (index % 4).ToString(), index);
        })));
        Assert.AreEqual(2, capture.Published.Count);
        var counts = capture.Samples.Where(sample => sample.Instrument.Name == "requests").ToArray();
        var durations = capture.Samples.Where(sample => sample.Instrument.Name == "duration").ToArray();
        Assert.AreEqual(256, counts.Length);
        Assert.AreEqual(256d, counts.Sum(sample => sample.Value));
        Assert.AreEqual(256, durations.Length);
        CollectionAssert.AreEqual(Enumerable.Range(0, 256).Select(value => (double)value).ToArray(), durations.Select(sample => sample.Value).Order().ToArray());
        Assert.AreEqual(1, counts.Select(sample => sample.Instrument).Distinct().Count());
        Assert.AreEqual(1, durations.Select(sample => sample.Instrument).Distinct().Count());
    }

    [TestMethod]
    public void ColonsCannotCollideAndMetricKindsRemainDistinct()
    {
        using var meter = new Meter(Guid.NewGuid().ToString());
        using var capture = new MetricCapture(instrument => ReferenceEquals(instrument.Meter, meter));
        var metrics = new OpenTelemetryMetrics(meter);
        metrics.IncrementCounter("a:b", "c");
        metrics.IncrementCounter("a", "b:c");
        metrics.ObserveHistogram("a:b", "c", 1);
        metrics.ObserveHistogram("a", "b:c", 2);
        Assert.AreEqual(4, capture.Published.Count);
        CollectionAssert.AreEqual(new[] { "a:b", "a", "a:b", "a" }, capture.Samples.Select(sample => sample.Instrument.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "c", "b:c", "c", "b:c" }, capture.Samples.Select(sample => (string)sample.Tags.Single().Value!).ToArray());
    }

    [TestMethod]
    public void InvalidArgumentsDoNotPublishOrEmit()
    {
        using var meter = new Meter(Guid.NewGuid().ToString());
        using var capture = new MetricCapture(instrument => ReferenceEquals(instrument.Meter, meter));
        var metrics = new OpenTelemetryMetrics(meter);
        Assert.ThrowsExactly<ArgumentNullException>(() => new OpenTelemetryMetrics(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => metrics.IncrementCounter(null!, "label"));
        Assert.ThrowsExactly<ArgumentNullException>(() => metrics.IncrementCounter("metric", null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => metrics.ObserveHistogram(null!, "label", 1));
        Assert.ThrowsExactly<ArgumentNullException>(() => metrics.ObserveHistogram("metric", null!, 1));
        foreach (var value in new[] { "", " " })
        {
            Assert.ThrowsExactly<ArgumentException>(() => metrics.IncrementCounter(value, "label"));
            Assert.ThrowsExactly<ArgumentException>(() => metrics.IncrementCounter("metric", value));
            Assert.ThrowsExactly<ArgumentException>(() => metrics.ObserveHistogram(value, "label", 1));
            Assert.ThrowsExactly<ArgumentException>(() => metrics.ObserveHistogram("metric", value, 1));
        }
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => metrics.ObserveHistogram("metric", "label", -1));
        Assert.AreEqual(0, capture.Published.Count);
        Assert.AreEqual(0, capture.Samples.Count);
    }

    [TestMethod]
    public void DefaultMeterIdentityAndSharedAdapterDefinitionsAreStable()
    {
        var prefix = Guid.NewGuid().ToString("N");
        using var capture = new MetricCapture(instrument => instrument.Meter.Name == "PipelineInvoker.Metrics" && instrument.Name.StartsWith(prefix, StringComparison.Ordinal));
        var first = new OpenTelemetryMetrics();
        var second = new OpenTelemetryMetrics();
        first.IncrementCounter(prefix + "count", "one");
        second.IncrementCounter(prefix + "count", "two");
        first.ObserveHistogram(prefix + "duration", "one", 1);
        second.ObserveHistogram(prefix + "duration", "two", 2);
        Assert.AreEqual(2, capture.Published.Count);
        Assert.AreEqual(4, capture.Samples.Count);
        Assert.IsTrue(capture.Published.All(instrument => instrument.Meter.Name == "PipelineInvoker.Metrics"));
    }

    [TestMethod]
    public void NoListenerAndLateListenerFollowSdkSemantics()
    {
        using var meter = new Meter(Guid.NewGuid().ToString());
        var metrics = new OpenTelemetryMetrics(meter);
        metrics.IncrementCounter("count", "before");
        metrics.ObserveHistogram("duration", "before", 5);
        using var capture = new MetricCapture(instrument => ReferenceEquals(instrument.Meter, meter));
        Assert.AreEqual(2, capture.Published.Count);
        Assert.AreEqual(0, capture.Samples.Count);
        metrics.IncrementCounter("count", "after");
        metrics.ObserveHistogram("duration", "after", long.MaxValue);
        Assert.AreEqual(2, capture.Samples.Count);
        Assert.AreEqual((double)long.MaxValue, capture.Samples.Last().Value);
        meter.Dispose();
        metrics.IncrementCounter("count", "disposed");
        metrics.ObserveHistogram("duration", "disposed", 2);
        Assert.AreEqual(2, capture.Samples.Count);
    }

    [TestMethod]
    public void MeasurementListenerFailuresPropagateWithoutRetry()
    {
        using var meter = new Meter(Guid.NewGuid().ToString());
        var failure = new InvalidOperationException("listener");
        int callbacks = 0;
        using var listener = new MeterListener { InstrumentPublished = (instrument, current) =>
        {
            if (ReferenceEquals(instrument.Meter, meter)) current.EnableMeasurementEvents(instrument);
        } };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, state) => { callbacks++; throw failure; });
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, state) => { callbacks++; throw failure; });
        listener.Start();
        var metrics = new OpenTelemetryMetrics(meter);
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => metrics.IncrementCounter("count", "request")));
        Assert.AreEqual(1, callbacks);
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => metrics.ObserveHistogram("duration", "request", 4)));
        Assert.AreEqual(2, callbacks);
    }
}
