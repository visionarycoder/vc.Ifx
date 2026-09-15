using System.IO.Compression;
using SystemZipArchive = System.IO.Compression.ZipArchive;

namespace Util.Archiving.Zip;

/// <summary>
/// Provides ZIP archive listing and extraction operations.
/// </summary>
public sealed class ZipArchive : IArchiveProvider
{
    /// <inheritdoc />
    public string Format => "zip";

    /// <inheritdoc />
    public bool CanRead(string archivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        return string.Equals(Path.GetExtension(archivePath), ".zip", StringComparison.OrdinalIgnoreCase);
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

        using SystemZipArchive archive = ZipFile.OpenRead(archivePath);

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }

            string destinationPath = ResolveDestinationPath(destinationRoot, entry.FullName);

            if (File.Exists(destinationPath) && !options.OverwriteFiles)
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            await using Stream source = entry.Open();
            await using FileStream target = File.Create(destinationPath);
            await source.CopyToAsync(target, cancellationToken);

            extractedFiles.Add(destinationPath);
        }

        return new ArchiveExtractionResult(extractedFiles);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ArchiveEntry>> ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        cancellationToken.ThrowIfCancellationRequested();

        using SystemZipArchive archive = ZipFile.OpenRead(archivePath);

        IReadOnlyList<ArchiveEntry> entries = archive.Entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
            .Select(entry => new ArchiveEntry(entry.FullName, entry.Length, entry.LastWriteTime))
            .ToArray();

        return Task.FromResult(entries);
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
