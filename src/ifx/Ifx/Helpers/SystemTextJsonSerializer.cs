using System.Text.Json;
using Ifx.Abstractions;

namespace Ifx.Helpers;

/// <summary>Default JSON serializer for non-null response documents.</summary>
public sealed class SystemTextJsonSerializer : ISerializer
{
    /// <inheritdoc />
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value);

    /// <inheritdoc />
    public T Deserialize<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json) ?? throw new JsonException($"The input could not be deserialized.");
    }
}
