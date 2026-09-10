namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface ISpan : IDisposable
{
    void SetTag(string key, string value);
    void End();
}