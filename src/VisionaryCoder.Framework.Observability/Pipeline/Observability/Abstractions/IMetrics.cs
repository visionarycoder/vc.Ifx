namespace VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

public interface IMetrics
{
    void IncrementCounter(string metric, string label);
    void ObserveHistogram(string metric, string label, long value);
}