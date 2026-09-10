using System.Text.Json;
using VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

/// <summary>Default JSON serializer for non-null response documents.</summary>
public sealed class SystemTextJsonSerializer : ISerializer
{
    /// <inheritdoc />
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value);
    /// <inheritdoc />
    public T Deserialize<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json) ?? throw new JsonException("A response document cannot be null.");
    }
}
