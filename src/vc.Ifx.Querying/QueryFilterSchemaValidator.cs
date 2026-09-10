using System.Text.Json;
using VisionaryCoder.Framework.Querying.Serialization;

namespace VisionaryCoder.Framework.Querying;

/// <summary>Validates the version-1 structural contract using the deserializer's reader.</summary>
public static class QueryFilterSchemaValidator
{
    /// <summary>Returns structural errors. Runtime member types are checked during rehydration.</summary>
    public static IReadOnlyList<string> Validate(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            QueryFilterSerializer.Deserialize(json);
            return [];
        }
        catch (JsonException exception)
        {
            return [exception.Message];
        }
    }

    /// <summary>Tests a parsed document against the version-1 structural contract.</summary>
    public static bool IsValid(JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(jsonDocument);
        return Validate(jsonDocument.RootElement.GetRawText()).Count == 0;
    }
}
