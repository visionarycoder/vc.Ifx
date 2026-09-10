using System.Reflection;

namespace VisionaryCoder.Framework.Querying.Serialization;

/// <summary>
/// Provides access to the QueryFilter JSON Schema.
/// </summary>
public static class QueryFilterSchema
{

    private const string DefaultResourceName = "VisionaryCoder.Framework.Schemas.queryfilter.schema.json";
    private static readonly Lazy<string> schemaContent = new(() => LoadSchemaFromResource(typeof(QueryFilterSchema).Assembly, DefaultResourceName));

    /// <summary>
    /// Gets the QueryFilter JSON Schema as a string.
    /// </summary>
    public static string Content => schemaContent.Value;

    internal static string LoadSchemaFromResource(Assembly assembly, string resourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Saves the schema to a file.
    /// </summary>
    /// <param name="path">The file path to save to.</param>
    public static void SaveToFile(string path)
    {
        File.WriteAllText(path, Content);
    }
    
}
