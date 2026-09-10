using System.Text.Json;
using System.Text.Json.Serialization;

namespace VisionaryCoder.Framework.Querying.Serialization;

/// <summary>Reads and writes the version-1 structural query-filter format.</summary>
public static class QueryFilterSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new FilterConverter() }
    };

    /// <summary>Serializes a valid filter tree. Invalid shapes are rejected.</summary>
    public static string Serialize(FilterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        string json = JsonSerializer.Serialize(node, Options);
        Deserialize(json);
        return json;
    }

    /// <summary>Deserializes a filter tree, rejecting malformed or unsupported shapes.</summary>
    public static FilterNode? Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<FilterNode>(json, Options)
            ?? throw new JsonException("A filter must be a JSON object.");
    }

    private sealed class FilterConverter : JsonConverter<FilterNode>
    {
        public override FilterNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            return ReadNode(document.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, FilterNode value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case PropertyFilter property:
                    JsonSerializer.Serialize(writer, property, options);
                    break;
                case CompositeFilter composite:
                    JsonSerializer.Serialize(writer, composite, options);
                    break;
                default:
                    throw new NotSupportedException($"Filter '{value.GetType().Name}' is not supported.");
            }
        }
    }

    private static FilterNode ReadNode(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) throw new JsonException("A filter must be a JSON object.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
            if (!seen.Add(property.Name)) throw new JsonException($"Duplicate property '{property.Name}'.");
        string operation = RequiredString(element, "operator");
        if (element.TryGetProperty("children", out JsonElement children))
        {
            if (seen.Any(name => name is not ("operator" or "children"))) throw new JsonException("Unknown composite property.");
            if (operation is not ("And" or "Or" or "Not")) throw new JsonException($"Invalid composite operator '{operation}'.");
            if (children.ValueKind != JsonValueKind.Array || children.GetArrayLength() == 0)
                throw new JsonException("children must be a nonempty array.");
            if (operation == "Not" && children.GetArrayLength() != 1) throw new JsonException("Not requires exactly one child.");
            return new CompositeFilter(operation, children.EnumerateArray().Select(ReadNode).ToList());
        }
        if (seen.Any(name => name is not ("operator" or "property" or "value" or "ignoreCase")))
            throw new JsonException("Unknown filter property.");
        string path = RequiredString(element, "property");
        if (!QueryFilterOperations.All.ContainsKey(operation))
            throw new JsonException($"Invalid operator '{operation}'.");
        string? value = null;
        if (element.TryGetProperty("value", out JsonElement valueElement))
        {
            if (valueElement.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)) throw new JsonException("value must be a string or null.");
            value = valueElement.GetString();
        }
        bool ignoreCase = false;
        if (element.TryGetProperty("ignoreCase", out JsonElement ignore))
        {
            if (ignore.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new JsonException("ignoreCase must be Boolean.");
            ignoreCase = ignore.GetBoolean();
        }
        return new PropertyFilter(operation, path, value, ignoreCase);
    }

    private static string RequiredString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement property) || property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
            throw new JsonException($"{name} is required and must be a nonempty string.");
        return property.GetString()!;
    }
}
