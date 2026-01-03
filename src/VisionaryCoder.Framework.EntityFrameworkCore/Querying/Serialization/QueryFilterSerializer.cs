using System.Text.Json;

namespace VisionaryCoder.Framework.Querying.Serialization;

public static class QueryFilterSerializer
{

    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(FilterNode node) => JsonSerializer.Serialize(node, options);

    public static FilterNode? Deserialize(string json)  => JsonSerializer.Deserialize<FilterNode>(json, options);

}
