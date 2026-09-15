using System.IO.Compression;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Archiving.GZip;

namespace Util.Archiving.GZip.UnitTests;

[TestClass]
public sealed class GZipArchiveTests
{
    #region Format and CanRead Tests

    [TestMethod]
    public void Format_WhenResolved_ShouldReturnGZipIdentifier()
    {
        GZipArchive archive = new();

        archive.Format.Should().Be("gzip");
    }

    [DataTestMethod]
    [DataRow("sample.txt.gz")]
    [DataRow("bundle.tar.gz")]
    [DataRow("bundle.tgz")]
    public void CanRead_WhenPathUsesSupportedExtension_ShouldReturnTrue(string archivePath)
    {
        GZipArchive archive = new();

        bool canRead = archive.CanRead(archivePath);

        canRead.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("sample.txt")]
    [DataRow("sample.zip")]
    [DataRow("sample.tar")]
    public void CanRead_WhenPathUsesUnsupportedExtension_ShouldReturnFalse(string archivePath)
    {
        GZipArchive archive = new();

        bool canRead = archive.CanRead(archivePath);

        canRead.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CanRead_WhenPathIsMissing_ShouldThrowArgumentException(string? archivePath)
    {
        GZipArchive archive = new();

        Action action = () => archive.CanRead(archivePath!);

        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region ExtractAsync Tests

    [TestMethod]
    public async Task ExtractAsync_WhenArchiveContainsSingleFile_ShouldWriteDecompressedFile()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string sourceDirectory = Path.Combine(rootDirectory, "source");
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        string archivePath = await CreateArchiveAsync(sourceDirectory, "sample.txt.gz", "hello gzip");

        ArchiveExtractionResult result = await archive.ExtractAsync(archivePath, destinationDirectory);

        result.ExtractedFiles.Should().ContainSingle();
        string extractedPath = result.ExtractedFiles[0];
        extractedPath.Should().Be(Path.Combine(Path.GetFullPath(destinationDirectory), "sample.txt"));
        (await File.ReadAllTextAsync(extractedPath)).Should().Be("hello gzip");
    }

    [TestMethod]
    public async Task ExtractAsync_WhenArchiveUsesTgzSuffix_ShouldWriteTarNamedFile()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string sourceDirectory = Path.Combine(rootDirectory, "source");
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        string archivePath = await CreateArchiveAsync(sourceDirectory, "bundle.tgz", "tar payload");

        ArchiveExtractionResult result = await archive.ExtractAsync(archivePath, destinationDirectory);

        result.ExtractedFiles.Should().ContainSingle()
            .Which.Should().Be(Path.Combine(Path.GetFullPath(destinationDirectory), "bundle.tar"));
        (await File.ReadAllTextAsync(result.ExtractedFiles[0])).Should().Be("tar payload");
    }

    [TestMethod]
    public async Task ExtractAsync_WhenTargetExistsAndOverwriteDisabled_ShouldSkipExistingFile()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string sourceDirectory = Path.Combine(rootDirectory, "source");
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        string archivePath = await CreateArchiveAsync(sourceDirectory, "sample.txt.gz", "updated");
        Directory.CreateDirectory(destinationDirectory);
        string destinationPath = Path.Combine(Path.GetFullPath(destinationDirectory), "sample.txt");
        await File.WriteAllTextAsync(destinationPath, "existing");

        ArchiveExtractionResult result = await archive.ExtractAsync(archivePath, destinationDirectory);

        result.ExtractedFiles.Should().BeEmpty();
        (await File.ReadAllTextAsync(destinationPath)).Should().Be("existing");
    }

    [TestMethod]
    public async Task ExtractAsync_WhenTargetExistsAndOverwriteEnabled_ShouldReplaceExistingFile()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string sourceDirectory = Path.Combine(rootDirectory, "source");
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        string archivePath = await CreateArchiveAsync(sourceDirectory, "sample.txt.gz", "updated");
        Directory.CreateDirectory(destinationDirectory);
        string destinationPath = Path.Combine(Path.GetFullPath(destinationDirectory), "sample.txt");
        await File.WriteAllTextAsync(destinationPath, "existing");

        ArchiveExtractionResult result = await archive.ExtractAsync(
            archivePath,
            destinationDirectory,
            new ArchiveExtractionOptions { OverwriteFiles = true });

        result.ExtractedFiles.Should().ContainSingle()
            .Which.Should().Be(destinationPath);
        (await File.ReadAllTextAsync(destinationPath)).Should().Be("updated");
    }

    [TestMethod]
    public async Task ExtractAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string sourceDirectory = Path.Combine(rootDirectory, "source");
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        string archivePath = await CreateArchiveAsync(sourceDirectory, "sample.txt.gz", "content");
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        Func<Task> action = () => archive.ExtractAsync(
            archivePath,
            destinationDirectory,
            cancellationToken: cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ExtractAsync_WhenExtensionIsUnsupported_ShouldThrowInvalidDataException()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = Path.Combine(rootDirectory, "sample.zip");
        await File.WriteAllTextAsync(archivePath, "not gzip");

        Func<Task> action = () => archive.ExtractAsync(archivePath, Path.Combine(rootDirectory, "output"));

        await action.Should().ThrowAsync<InvalidDataException>();
    }

    #endregion

    #region ListEntriesAsync Tests

    [TestMethod]
    public async Task ListEntriesAsync_WhenArchiveContainsSingleFile_ShouldReturnLogicalEntry()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(rootDirectory, "sample.txt.gz", "hello gzip");

        IReadOnlyList<ArchiveEntry> entries = await archive.ListEntriesAsync(archivePath);

        entries.Should().ContainSingle();
        entries[0].Name.Should().Be("sample.txt");
        entries[0].Length.Should().BeNull();
        entries[0].LastModified.Should().BeNull();
    }

    [TestMethod]
    public async Task ListEntriesAsync_WhenArchiveUsesTgzSuffix_ShouldReturnTarLogicalEntry()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(rootDirectory, "bundle.tgz", "tar payload");

        IReadOnlyList<ArchiveEntry> entries = await archive.ListEntriesAsync(archivePath);

        entries.Should().ContainSingle();
        entries[0].Name.Should().Be("bundle.tar");
    }

    [TestMethod]
    public async Task ListEntriesAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(rootDirectory, "sample.txt.gz", "content");
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        Func<Task> action = () => archive.ListEntriesAsync(archivePath, cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ListEntriesAsync_WhenExtensionIsUnsupported_ShouldThrowInvalidDataException()
    {
        GZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = Path.Combine(rootDirectory, "sample.zip");
        await File.WriteAllTextAsync(archivePath, "not gzip");

        Func<Task> action = () => archive.ListEntriesAsync(archivePath);

        await action.Should().ThrowAsync<InvalidDataException>();
    }

    #endregion

    #region Registration Tests

    [TestMethod]
    public void AddGZipArchiveProvider_WhenRegistered_ShouldExposeSingletonProvider()
    {
        ServiceCollection services = new();

        services.AddGZipArchiveProvider();

        using ServiceProvider provider = services.BuildServiceProvider();
        GZipArchive archive = provider.GetRequiredService<GZipArchive>();
        IArchiveProvider abstraction = provider.GetRequiredService<IArchiveProvider>();

        abstraction.Should().BeSameAs(archive);
    }

    #endregion

    private static string CreateTestDirectory([System.Runtime.CompilerServices.CallerMemberName] string? testName = null)
    {
        string directory = Path.Combine(
            AppContext.BaseDirectory,
            "TestArtifacts",
            nameof(GZipArchiveTests),
            testName ?? "Unknown");

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        Directory.CreateDirectory(directory);
        return directory;
    }

    private static async Task<string> CreateArchiveAsync(string directory, string fileName, string content)
    {
        Directory.CreateDirectory(directory);
        string archivePath = Path.Combine(directory, fileName);
        byte[] bytes = Encoding.UTF8.GetBytes(content);

        await using FileStream fileStream = File.Create(archivePath);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
        await gzipStream.WriteAsync(bytes);

        return archivePath;
    }
}
