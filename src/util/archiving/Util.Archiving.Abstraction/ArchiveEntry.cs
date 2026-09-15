namespace Util.Archiving;

/// <summary>
/// Represents a single entry within an archive.
/// </summary>
public sealed record ArchiveEntry(
    string Name,
    long? Length,
    DateTimeOffset? LastModified);
