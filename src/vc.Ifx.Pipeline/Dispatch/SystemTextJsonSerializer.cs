using System.Text.Json;
using VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

public sealed class SystemTextJsonSerializer : ISerializer
{
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value);
    public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json)!;
}
