using System.Diagnostics;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Observibility;

public sealed class OpenTelemetryTracer : ITracer
{
    private static readonly ActivitySource defaultSource = new("PipelineInvoker");
    private readonly ActivitySource source;

    /// <summary>Uses the process-lifetime PipelineInvoker source.</summary>
    public OpenTelemetryTracer() : this(defaultSource) { }

    /// <summary>Borrows a source without taking ownership of its lifetime.</summary>
    public OpenTelemetryTracer(ActivitySource source) =>
        this.source = source ?? throw new ArgumentNullException(nameof(source));

    /// <inheritdoc />
    public ISpan StartSpan(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Activity? previous = Activity.Current;
        try
        {
            Activity? activity = source.StartActivity(name, ActivityKind.Internal);
            return new ActivitySpan(activity, previous);
        }
        catch
        {
            Activity.Current = previous;
            throw;
        }
    }

    private sealed class ActivitySpan(Activity? activity, Activity? previous) : ISpan
    {
        private Activity? activity = activity;

        public void SetTag(string key, string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
            var current = Volatile.Read(ref activity);
            if (current is null) return;
            current.SetTag(key, value);
            if (key == "status")
                current.SetStatus(value switch
                {
                    "success" => ActivityStatusCode.Ok,
                    "error" => ActivityStatusCode.Error,
                    _ => ActivityStatusCode.Unset
                });
            else if (key == "error.message")
                current.SetStatus(ActivityStatusCode.Error, value);
        }

        public void End()
        {
            var owned = Interlocked.Exchange(ref activity, null);
            if (owned is null) return;
            var current = Activity.Current;
            try { owned.Dispose(); }
            finally { Activity.Current = ReferenceEquals(current, owned) ? previous : current; }
        }

        public void Dispose() => End();
    }
}
