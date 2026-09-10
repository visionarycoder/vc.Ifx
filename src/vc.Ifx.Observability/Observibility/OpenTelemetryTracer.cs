using System.Diagnostics;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Observibility;

public sealed class OpenTelemetryTracer : ITracer
{
    private static readonly ActivitySource source = new("PipelineInvoker");

    public ISpan StartSpan(string name)
    {
        Activity? activity = source.StartActivity(name, ActivityKind.Internal);
        return new ActivitySpan(activity);
    }

    private sealed class ActivitySpan(Activity? activity) : ISpan
    {
        public void SetTag(string key, string value) => activity?.SetTag(key, value);
        public void End() => activity?.Stop();
        public void Dispose() => End();
    }
}
