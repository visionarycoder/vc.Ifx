namespace VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

public interface ISerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(string json);
}
