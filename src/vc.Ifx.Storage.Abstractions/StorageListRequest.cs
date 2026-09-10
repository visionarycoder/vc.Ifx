namespace VisionaryCoder.Framework.Storage;

/// <summary>Requests an unordered listing of all objects with an ordinal path prefix.</summary>
public sealed record StorageListRequest
{
    /// <summary>Initializes a listing request.</summary>
    /// <param name="prefix">An opaque prefix; an empty string selects all objects.</param>
    /// <exception cref="ArgumentNullException">The prefix is null.</exception>
    public StorageListRequest(string prefix = "")
    {
        ArgumentNullException.ThrowIfNull(prefix);
        Prefix = prefix;
    }

    /// <summary>Gets the literal prefix. Wildcards and directory traversal are not interpreted.</summary>
    public string Prefix { get; }
}
