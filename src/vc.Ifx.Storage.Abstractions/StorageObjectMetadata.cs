namespace VisionaryCoder.Framework.Storage;

/// <summary>Provider-neutral facts about a stored object. Null facts are unavailable, not zero.</summary>
public sealed record StorageObjectMetadata
{
    /// <summary>Initializes immutable object metadata.</summary>
    /// <param name="path">The nonblank provider-relative object path.</param>
    /// <param name="length">The content length in bytes, or null if unavailable.</param>
    /// <param name="lastModified">The last modification time, or null if unavailable.</param>
    /// <param name="contentType">The media type, or null if unavailable.</param>
    /// <param name="version">An opaque version or entity tag, or null if unavailable.</param>
    /// <exception cref="ArgumentNullException">The path is null.</exception>
    /// <exception cref="ArgumentException">The path is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The length is negative.</exception>
    public StorageObjectMetadata(string path, long? length = null, DateTimeOffset? lastModified = null,
        string? contentType = null, string? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Content length must not be negative.");
        }

        Path = path;
        Length = length;
        LastModified = lastModified;
        ContentType = contentType;
        Version = version;
    }

    /// <summary>Gets the opaque object path.</summary>
    public string Path { get; }
    /// <summary>Gets the content length in bytes when available.</summary>
    public long? Length { get; }
    /// <summary>Gets the last modification time when available.</summary>
    public DateTimeOffset? LastModified { get; }
    /// <summary>Gets the media type when available.</summary>
    public string? ContentType { get; }
    /// <summary>Gets the opaque version when available.</summary>
    public string? Version { get; }
}
