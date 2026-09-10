namespace VisionaryCoder.Framework.Storage;

/// <summary>Requests a write from a caller-owned stream at its current position.</summary>
/// <remarks>Record equality compares stream identity, not stream contents.</remarks>
public sealed record StorageWriteRequest
{
    /// <summary>Initializes a write request. The provider must leave the stream open.</summary>
    /// <param name="path">A nonblank provider-relative path.</param>
    /// <param name="content">A readable stream; seeking is not required.</param>
    /// <param name="overwrite">Whether an existing object may be replaced.</param>
    /// <exception cref="ArgumentNullException">The path or content is null.</exception>
    /// <exception cref="ArgumentException">The path is blank or the stream is not readable.</exception>
    public StorageWriteRequest(string path, Stream content, bool overwrite = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        if (!content.CanRead)
        {
            throw new ArgumentException("The content stream must be readable.", nameof(content));
        }

        Path = path;
        Content = content;
        Overwrite = overwrite;
    }

    /// <summary>Gets the opaque object path.</summary>
    public string Path { get; }
    /// <summary>Gets the caller-owned stream.</summary>
    public Stream Content { get; }
    /// <summary>Gets whether replacement is allowed. False requires atomic create-only support.</summary>
    public bool Overwrite { get; }
}
