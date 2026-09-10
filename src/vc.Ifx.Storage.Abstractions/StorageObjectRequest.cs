namespace VisionaryCoder.Framework.Storage;

/// <summary>Identifies one object using a provider-defined, opaque path.</summary>
public sealed record StorageObjectRequest
{
    /// <summary>Initializes an object request without normalizing its path.</summary>
    /// <param name="path">A nonblank provider-relative path.</param>
    /// <exception cref="ArgumentNullException">The path is null.</exception>
    /// <exception cref="ArgumentException">The path is empty or whitespace.</exception>
    public StorageObjectRequest(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = path;
    }

    /// <summary>Gets the opaque object path.</summary>
    public string Path { get; }
}
