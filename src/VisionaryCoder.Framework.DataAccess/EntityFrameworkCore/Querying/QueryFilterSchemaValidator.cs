using System.Text.Json;

namespace VisionaryCoder.Framework.Querying;

/// <summary>
/// Validates QueryFilter JSON against expected structure.
/// This is a lightweight, self-contained validator tailored to the project's tests
/// and avoids an external JSON Schema dependency.
/// </summary>
public static class QueryFilterSchemaValidator
{
    /// <summary>
    /// Validates a QueryFilter JSON string against expected structure.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <returns>A list of validation errors, or empty if valid.</returns>
    public static IReadOnlyList<string> Validate(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return ValidateElement(doc.RootElement);
        }
        catch (JsonException je)
        {
            return new List<string> { $"Invalid JSON: {je.Message}" };
        }
    }

    /// <summary>
    /// Validates a QueryFilter JSON document against the expected structure.
    /// </summary>
    /// <param name="jsonDocument">The JSON document to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(JsonDocument jsonDocument)
    {
        string json = jsonDocument.RootElement.GetRawText();
        return Validate(json).Count == 0;
    }

    private static List<string> ValidateElement(JsonElement element)
    {
        var errors = new List<string>();

        if (element.ValueKind != JsonValueKind.Object)
        {
            errors.Add("Root element must be a JSON object.");
            return errors;
        }

        if (!element.TryGetProperty("operator", out JsonElement opElem) || opElem.ValueKind != JsonValueKind.String)
        {
            errors.Add("operator is required and must be a string.");
            return errors;
        }

        string? op = opElem.GetString();
        if (string.IsNullOrWhiteSpace(op))
        {
            errors.Add("operator must be a non-empty string.");
            return errors;
        }

        // If 'children' exists, treat as composite filter
        if (element.TryGetProperty("children", out JsonElement childrenElem))
        {
            if (childrenElem.ValueKind != JsonValueKind.Array)
            {
                errors.Add("children must be an array.");
                return errors;
            }

            if (childrenElem.GetArrayLength() == 0)
            {
                errors.Add("children must contain at least one item.");
            }

            // Basic composite operators allowed
            var allowedComposite = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "And", "Or", "Not" };
            if (!allowedComposite.Contains(op))
            {
                errors.Add($"Invalid composite operator: {op}");
            }

            int idx = 0;
            foreach (JsonElement child in childrenElem.EnumerateArray())
            {
                List<string> childErrors = ValidateElement(child);
                foreach (string ce in childErrors)
                {
                    errors.Add($"children[{idx}]: {ce}");
                }
                idx++;
            }

            return errors;
        }

        // Otherwise treat as a property filter
        if (!element.TryGetProperty("property", out JsonElement propElem) || propElem.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(propElem.GetString()))
        {
            errors.Add("property is required.");
        }

        // Basic property operators allowed
        var allowedPropertyOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Contains",
            "Equals",
            "StartsWith",
            "EndsWith",
            "GreaterThan",
            "LessThan",
            "GreaterThanOrEqual",
            "LessThanOrEqual",
            "In",
            "NotIn"
        };

        if (!allowedPropertyOps.Contains(op))
        {
            errors.Add($"Invalid operator: {op}");
        }

        return errors;
    }
}
