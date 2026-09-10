using System.Diagnostics;
using VisionaryCoder.Framework.Pipeline.Observibility;

namespace VisionaryCoder.Framework.Tests.Observability;

[TestClass]
public sealed class TracerTests
{
    [TestMethod]
    public void NoListenerSpanNeverAdoptsCallerActivityAndValidatesArguments()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var caller = new CallerActivity("caller");
        caller.Start();
        var tracer = new OpenTelemetryTracer(source);
        Assert.ThrowsExactly<ArgumentNullException>(() => new OpenTelemetryTracer(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => tracer.StartSpan(null!));
        Assert.ThrowsExactly<ArgumentException>(() => tracer.StartSpan(" "));
        var span = tracer.StartSpan("ignored");
        Assert.AreSame(caller, Activity.Current);
        Assert.ThrowsExactly<ArgumentNullException>(() => span.SetTag(null!, "value"));
        Assert.ThrowsExactly<ArgumentException>(() => span.SetTag(" ", "value"));
        Assert.ThrowsExactly<ArgumentNullException>(() => span.SetTag("key", null!));
        span.SetTag("key", "");
        span.End();
        span.Dispose();
        span.End();
        Assert.AreSame(caller, Activity.Current);
        Assert.IsFalse(caller.IsStopped);
        Assert.AreEqual(0, caller.Disposals);
        Assert.IsNull(caller.GetTagItem("key"));
    }

    [TestMethod]
    public void SamplingRejectionLeavesCallerUntouched()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var capture = new TraceCapture(source, ActivitySamplingResult.None);
        using var caller = new CallerActivity("caller");
        caller.Start();
        using (var span = new OpenTelemetryTracer(source).StartSpan("not-sampled")) span.SetTag("status", "success");
        Assert.AreSame(caller, Activity.Current);
        Assert.AreEqual(0, capture.Started.Count);
        Assert.AreEqual(0, capture.Stopped.Count);
        Assert.AreEqual(0, caller.Disposals);
        Assert.IsFalse(caller.IsStopped);
    }

    [TestMethod]
    public void SpanTagsAndStandardStatusAreMappedWithoutExtraEvents()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var capture = new TraceCapture(source);
        using var caller = new CallerActivity("caller");
        caller.Start();
        var span = new OpenTelemetryTracer(source).StartSpan("operation");
        var activity = capture.Started.Single();
        Assert.AreEqual(ActivityKind.Internal, activity.Kind);
        Assert.AreSame(caller, activity.Parent);
        Assert.AreSame(activity, Activity.Current);
        span.SetTag("custom", "value");
        span.SetTag("status", "success");
        Assert.AreEqual(ActivityStatusCode.Ok, activity.Status);
        span.SetTag("status", "error");
        Assert.AreEqual(ActivityStatusCode.Error, activity.Status);
        span.SetTag("error.message", "detail");
        Assert.AreEqual("detail", activity.StatusDescription);
        span.SetTag("status", "canceled");
        Assert.AreEqual(ActivityStatusCode.Unset, activity.Status);
        Assert.IsNull(activity.StatusDescription);
        span.SetTag("status", "custom-state");
        Assert.AreEqual(ActivityStatusCode.Unset, activity.Status);
        span.End();
        span.Dispose();
        span.End();
        span.SetTag("custom", "ignored");
        Assert.AreEqual("value", activity.GetTagItem("custom"));
        Assert.AreEqual("custom-state", activity.GetTagItem("status"));
        Assert.AreEqual(0, activity.Events.Count());
        Assert.AreEqual(1, capture.Stopped.Count);
        Assert.AreSame(caller, Activity.Current);
        Assert.IsFalse(caller.IsStopped);
        Assert.AreEqual(0, caller.Disposals);
        using var subsequent = new OpenTelemetryTracer(source).StartSpan("still-usable");
        Assert.AreEqual(2, capture.Started.Count);
    }

    [TestMethod]
    public async Task NestedSpansFlowAcrossAwaitAndRestoreParents()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var capture = new TraceCapture(source);
        using var caller = new CallerActivity("caller");
        caller.Start();
        var tracer = new OpenTelemetryTracer(source);
        using var outer = tracer.StartSpan("outer");
        var outerActivity = Activity.Current;
        using (var inner = tracer.StartSpan("inner"))
        {
            var innerActivity = Activity.Current;
            await Task.Yield();
            Assert.AreSame(innerActivity, Activity.Current);
            Assert.AreSame(outerActivity, innerActivity!.Parent);
            await Task.Run(() => { Assert.AreSame(innerActivity, Activity.Current); inner.SetTag("async", "true"); });
            Assert.AreSame(innerActivity, Activity.Current);
        }
        Assert.AreSame(outerActivity, Activity.Current);
        outer.End();
        Assert.AreSame(caller, Activity.Current);
        Assert.AreEqual(2, capture.Stopped.Count);
        Assert.IsFalse(caller.IsStopped);
        CollectionAssert.AreEqual(new[] { "inner", "outer" }, capture.Stopped.Select(item => item.OperationName).ToArray());
    }

    [TestMethod]
    public void EndingNonCurrentSpanPreservesCallerOwnedActivity()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var capture = new TraceCapture(source);
        using var caller = new CallerActivity("caller");
        caller.Start();
        using var span = new OpenTelemetryTracer(source).StartSpan("owned");
        var owned = Activity.Current;
        using (var unrelated = new CallerActivity("caller-child"))
        {
            unrelated.Start();
            span.End();
            Assert.IsTrue(owned!.IsStopped);
            Assert.AreSame(unrelated, Activity.Current);
            Assert.IsFalse(unrelated.IsStopped);
            Assert.AreEqual(0, unrelated.Disposals);
        }
        Activity.Current = caller;
        Assert.AreEqual(1, capture.Stopped.Count);
        Assert.IsFalse(caller.IsStopped);
    }

    [TestMethod]
    public async Task ConcurrentEndAndDisposeStopExactlyOnce()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var capture = new TraceCapture(source);
        using var caller = new CallerActivity("caller");
        caller.Start();
        var span = new OpenTelemetryTracer(source).StartSpan("owned");
        await Task.WhenAll(Enumerable.Range(0, 64).Select(index => Task.Run(() => { if (index % 2 == 0) span.End(); else span.Dispose(); })));
        Assert.AreEqual(1, capture.Stopped.Count);
        Assert.AreEqual(0, caller.Disposals);
        Assert.IsFalse(caller.IsStopped);
        Activity.Current = caller;
        span.Dispose();
        Assert.AreSame(caller, Activity.Current);
    }

    [TestMethod]
    public async Task ConcurrentAsyncOperationsKeepIndependentCurrentSpans()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var capture = new TraceCapture(source);
        var tracer = new OpenTelemetryTracer(source);
        using var caller = new CallerActivity("caller");
        caller.Start();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int entered = 0;
        async Task<Activity?> Run(string name)
        {
            using var span = tracer.StartSpan(name);
            var current = Activity.Current;
            if (Interlocked.Increment(ref entered) == 2) gate.SetResult();
            await gate.Task;
            Assert.AreSame(current, Activity.Current);
            Assert.AreSame(caller, current!.Parent);
            return current;
        }
        var results = await Task.WhenAll(Run("one"), Run("two"));
        Assert.AreNotSame(results[0], results[1]);
        Assert.AreEqual(2, capture.Stopped.Count);
        Assert.AreSame(caller, Activity.Current);
    }

    [TestMethod]
    public void StopListenerFailurePropagatesOnceAndRestoresAmbientActivity()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var caller = new CallerActivity("caller");
        caller.Start();
        var failure = new InvalidOperationException("listener");
        int stops = 0;
        using var listener = new ActivityListener
        {
            ShouldListenTo = candidate => ReferenceEquals(source, candidate),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => { stops++; throw failure; }
        };
        ActivitySource.AddActivityListener(listener);
        var span = new OpenTelemetryTracer(source).StartSpan("owned");
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(span.End));
        Assert.AreSame(caller, Activity.Current);
        span.Dispose();
        Assert.AreEqual(1, stops);
        Assert.IsFalse(caller.IsStopped);
    }

    [TestMethod]
    public void StartListenerFailureRestoresCallerAndIsNotSwallowed()
    {
        using var source = new ActivitySource(Guid.NewGuid().ToString());
        using var caller = new CallerActivity("caller");
        caller.Start();
        var failure = new InvalidOperationException("listener");
        Activity? unreturned = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = candidate => ReferenceEquals(source, candidate),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => { unreturned = activity; throw failure; }
        };
        ActivitySource.AddActivityListener(listener);
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => new OpenTelemetryTracer(source).StartSpan("owned")));
        Assert.AreSame(caller, Activity.Current);
        Assert.IsFalse(caller.IsStopped);
        unreturned!.Dispose();
    }

    [TestMethod]
    public void DefaultConstructorRetainsSourceName()
    {
        var name = Guid.NewGuid().ToString();
        Activity? captured = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "PipelineInvoker",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => options.Name == name ? ActivitySamplingResult.AllData : ActivitySamplingResult.None,
            ActivityStarted = activity => { if (activity.OperationName == name) captured = activity; }
        };
        ActivitySource.AddActivityListener(listener);
        using (var span = new OpenTelemetryTracer().StartSpan(name)) span.SetTag("key", "value");
        Assert.IsNotNull(captured);
        Assert.AreEqual("PipelineInvoker", captured.Source.Name);
        Assert.IsTrue(captured.IsStopped);
    }
}
