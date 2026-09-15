using System.IO.Compression;
using Util.Archiving;

namespace Util.Archiving.GZip;

/// <summary>
/// Provides GZip archive listing and extraction operations.
/// </summary>
public sealed class GZipArchive : IArchiveProvider
{
    /// <inheritdoc />
    public string Format => "gzip";

    /// <inheritdoc />
    public bool CanRead(string archivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        return archivePath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            || archivePath.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase);
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
            throw new InvalidDataException($"Archive '{archivePath}' is not a supported GZip file.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        options ??= ArchiveExtractionOptions.Default;
        Directory.CreateDirectory(destinationDirectory);

        string destinationPath = Path.Combine(Path.GetFullPath(destinationDirectory), GetOutputFileName(archivePath));

        if (File.Exists(destinationPath) && !options.OverwriteFiles)
        {
            return new ArchiveExtractionResult([]);
        }

        await using FileStream archiveStream = File.OpenRead(archivePath);
        await using var gzipStream = new GZipStream(archiveStream, CompressionMode.Decompress);
        await using FileStream destinationStream = File.Create(destinationPath);
        await gzipStream.CopyToAsync(destinationStream, cancellationToken);

        return new ArchiveExtractionResult([destinationPath]);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArchiveEntry>> ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        if (!CanRead(archivePath))
        {
            throw new InvalidDataException($"Archive '{archivePath}' is not a supported GZip file.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        await ValidateArchiveAsync(archivePath, cancellationToken);

        return [new ArchiveEntry(GetOutputFileName(archivePath), null, null)];
    }

    private static string GetOutputFileName(string archivePath)
    {
        string fileName = Path.GetFileName(archivePath);

        return fileName.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase)
            ? fileName[..^4] + ".tar"
            : Path.GetFileNameWithoutExtension(fileName);
    }

    private static async Task ValidateArchiveAsync(string archivePath, CancellationToken cancellationToken)
    {
        await using FileStream archiveStream = File.OpenRead(archivePath);
        await using var gzipStream = new GZipStream(archiveStream, CompressionMode.Decompress);
        byte[] buffer = new byte[1];
        _ = await gzipStream.ReadAsync(buffer, cancellationToken);
    }
}
