namespace Util.Archiving;

/// <summary>
/// Provides archive listing and extraction operations for a supported archive format.
/// </summary>
public interface IArchiveProvider
{
    /// <summary>
    /// Gets the archive format identifier.
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Determines whether the provider can read the specified archive path.
    /// </summary>
    bool CanRead(string archivePath);

    /// <summary>
    /// Extracts an archive into the specified destination directory.
    /// </summary>
    Task<ArchiveExtractionResult> ExtractAsync(
        string archivePath,
        string destinationDirectory,
        ArchiveExtractionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the entries contained in the specified archive.
    /// </summary>
    Task<IReadOnlyList<ArchiveEntry>> ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default);
}
