using SharpCompress.Archives;
using SharpCompressSevenZipArchive = SharpCompress.Archives.SevenZip.SevenZipArchive;

namespace Util.Archiving.SevenZip;

/// <summary>
/// Provides 7z archive listing and extraction operations.
/// </summary>
public sealed class SevenZipArchive : IArchiveProvider
{
    /// <inheritdoc />
    public string Format => "7z";

    /// <inheritdoc />
    public bool CanRead(string archivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        return string.Equals(Path.GetExtension(archivePath), ".7z", StringComparison.OrdinalIgnoreCase);
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

        if (!CanRead(archivePath))
        {
            throw new InvalidDataException($"Archive '{archivePath}' is not a supported 7z file.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        options ??= ArchiveExtractionOptions.Default;
        Directory.CreateDirectory(destinationDirectory);

        string destinationRoot = Path.GetFullPath(destinationDirectory);
        var extractedFiles = new List<string>();

        await using IAsyncArchive archive = await SharpCompressSevenZipArchive.OpenAsyncArchive(
            archivePath,
            cancellationToken: cancellationToken);

        await foreach (IArchiveEntry entry in archive.EntriesAsync)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string destinationPath = ResolveDestinationPath(destinationRoot, entry.Key!);

            if (entry.IsDirectory)
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            if (File.Exists(destinationPath) && !options.OverwriteFiles)
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            await using Stream source = await entry.OpenEntryStreamAsync(cancellationToken);
            await using FileStream target = File.Create(destinationPath);
            await source.CopyToAsync(target, cancellationToken);

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

        if (!CanRead(archivePath))
        {
            throw new InvalidDataException($"Archive '{archivePath}' is not a supported 7z file.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var entries = new List<ArchiveEntry>();

        await using IAsyncArchive archive = await SharpCompressSevenZipArchive.OpenAsyncArchive(
            archivePath,
            cancellationToken: cancellationToken);

        await foreach (IArchiveEntry entry in archive.EntriesAsync)
        {
            cancellationToken.ThrowIfCancellationRequested();

            entries.Add(new ArchiveEntry(
                entry.Key!,
                entry.IsDirectory ? null : entry.Size,
                entry.LastModifiedTime is { } lastModified
                    ? new DateTimeOffset(lastModified)
                    : null));
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
