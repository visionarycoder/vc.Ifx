using System.Reflection;

namespace VisionaryCoder.Framework.Querying.Serialization;

/// <summary>
/// Provides access to the QueryFilter JSON Schema.
/// </summary>
public static class QueryFilterSchema
{

    private const string DefaultResourceName = "VisionaryCoder.Framework.Schemas.queryfilter.schema.json";
    private static readonly Lazy<string> schemaContent = new(LoadSchemaFromResource);

    /// <summary>
    /// Gets the QueryFilter JSON Schema as a string.
    /// </summary>
    public static string Content => schemaContent.Value;

    private static string LoadSchemaFromResource()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(DefaultResourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {DefaultResourceName}");
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
