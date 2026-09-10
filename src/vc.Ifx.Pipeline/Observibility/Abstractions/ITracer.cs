using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Observibility.Abstractions;

public interface ITracer
{
    ISpan StartSpan(string name);
}