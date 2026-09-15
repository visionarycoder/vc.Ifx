using System.Formats.Tar;

namespace Util.Archiving.Tar;

/// <summary>
/// Provides TAR archive listing and extraction operations.
/// </summary>
public sealed class TarArchive : IArchiveProvider
{
    /// <inheritdoc />
    public string Format => "tar";

    /// <inheritdoc />
    public bool CanRead(string archivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        return string.Equals(Path.GetExtension(archivePath), ".tar", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<ArchiveExtractionResult> ExtractAsync(
        string archivePath,
        string destinationDirectory,
        ArchiveExtractionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);

        options ??= ArchiveExtractionOptions.Default;
        Directory.CreateDirectory(destinationDirectory);

        string destinationRoot = Path.GetFullPath(destinationDirectory);
        var extractedFiles = new List<string>();

        await using FileStream archiveStream = File.OpenRead(archivePath);
        using var reader = new TarReader(archiveStream);

        while (await reader.GetNextEntryAsync(copyData: false, cancellationToken) is { } entry)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.EntryType is TarEntryType.Directory)
            {
                Directory.CreateDirectory(ResolveDestinationPath(destinationRoot, entry.Name));
                continue;
            }

            if (entry.DataStream is null)
            {
                continue;
            }

            string destinationPath = ResolveDestinationPath(destinationRoot, entry.Name);

            if (File.Exists(destinationPath) && !options.OverwriteFiles)
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            await using FileStream target = File.Create(destinationPath);
            await entry.DataStream.CopyToAsync(target, cancellationToken);

            extractedFiles.Add(destinationPath);
        }

        return new ArchiveExtractionResult(extractedFiles);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArchiveEntry>> ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        var entries = new List<ArchiveEntry>();

        await using FileStream archiveStream = File.OpenRead(archivePath);
        using var reader = new TarReader(archiveStream);

        while (await reader.GetNextEntryAsync(copyData: false, cancellationToken) is { } entry)
        {
            entries.Add(new ArchiveEntry(entry.Name, entry.Length, entry.ModificationTime));
        }

        return entries;
    }

    private static string ResolveDestinationPath(string destinationRoot, string entryName)
    {
        string root = Path.EndsInDirectorySeparator(destinationRoot)
            ? destinationRoot
            : destinationRoot + Path.DirectorySeparatorChar;
        string destinationPath = Path.GetFullPath(Path.Combine(root, entryName));

        if (!destinationPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Archive entry '{entryName}' resolves outside the destination directory.");
        }

        return destinationPath;
    }
}
