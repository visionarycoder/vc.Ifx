using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.SevenZip;
using Util.Archiving.SevenZip;

namespace Util.Archiving.SevenZip.UnitTests;

[TestClass]
public sealed class SevenZipArchiveTests
{
    #region Format and CanRead Tests

    [TestMethod]
    public void Format_WhenResolved_ShouldReturnSevenZipIdentifier()
    {
        SevenZipArchive archive = new();

        archive.Format.Should().Be("7z");
    }

    [DataTestMethod]
    [DataRow("sample.7z")]
    [DataRow("SAMPLE.7Z")]
    public void CanRead_WhenPathUsesSupportedExtension_ShouldReturnTrue(string archivePath)
    {
        SevenZipArchive archive = new();

        bool canRead = archive.CanRead(archivePath);

        canRead.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("sample.zip")]
    [DataRow("sample.tar")]
    [DataRow("sample.gz")]
    public void CanRead_WhenPathUsesUnsupportedExtension_ShouldReturnFalse(string archivePath)
    {
        SevenZipArchive archive = new();

        bool canRead = archive.CanRead(archivePath);

        canRead.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CanRead_WhenPathIsMissing_ShouldThrowArgumentException(string? archivePath)
    {
        SevenZipArchive archive = new();

        Action action = () => archive.CanRead(archivePath!);

        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region ExtractAsync Tests

    [TestMethod]
    public async Task ExtractAsync_WhenArchiveContainsFiles_ShouldWriteExtractedFiles()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(
            rootDirectory,
            "sample.7z",
            new TestArchiveItem("nested", null, IsDirectory: true),
            new TestArchiveItem("alpha.txt", "alpha-content"),
            new TestArchiveItem("nested/beta.txt", "beta-content"));
        string destinationDirectory = Path.Combine(rootDirectory, "output") + Path.DirectorySeparatorChar;

        ArchiveExtractionResult result = await archive.ExtractAsync(archivePath, destinationDirectory);

        result.ExtractedFiles.Should().BeEquivalentTo(
            [
                Path.Combine(Path.GetFullPath(destinationDirectory), "alpha.txt"),
                Path.Combine(Path.GetFullPath(destinationDirectory), "nested", "beta.txt")
            ]);
        (await File.ReadAllTextAsync(Path.Combine(Path.GetFullPath(destinationDirectory), "alpha.txt"))).Should().Be("alpha-content");
        (await File.ReadAllTextAsync(Path.Combine(Path.GetFullPath(destinationDirectory), "nested", "beta.txt"))).Should().Be("beta-content");
        Directory.Exists(Path.Combine(Path.GetFullPath(destinationDirectory), "nested")).Should().BeTrue();
    }

    [TestMethod]
    public async Task ExtractAsync_WhenTargetExistsAndOverwriteDisabled_ShouldSkipExistingFile()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(
            rootDirectory,
            "sample.7z",
            new TestArchiveItem("alpha.txt", "alpha-updated"),
            new TestArchiveItem("nested/beta.txt", "beta-content"));
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        Directory.CreateDirectory(Path.Combine(destinationDirectory, "nested"));
        string existingFilePath = Path.Combine(Path.GetFullPath(destinationDirectory), "alpha.txt");
        await File.WriteAllTextAsync(existingFilePath, "existing");

        ArchiveExtractionResult result = await archive.ExtractAsync(archivePath, destinationDirectory);

        result.ExtractedFiles.Should().ContainSingle()
            .Which.Should().Be(Path.Combine(Path.GetFullPath(destinationDirectory), "nested", "beta.txt"));
        (await File.ReadAllTextAsync(existingFilePath)).Should().Be("existing");
        (await File.ReadAllTextAsync(Path.Combine(Path.GetFullPath(destinationDirectory), "nested", "beta.txt"))).Should().Be("beta-content");
    }

    [TestMethod]
    public async Task ExtractAsync_WhenTargetExistsAndOverwriteEnabled_ShouldReplaceExistingFile()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(
            rootDirectory,
            "sample.7z",
            new TestArchiveItem("alpha.txt", "alpha-updated"),
            new TestArchiveItem("nested/beta.txt", "beta-content"));
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        Directory.CreateDirectory(Path.Combine(destinationDirectory, "nested"));
        string existingFilePath = Path.Combine(Path.GetFullPath(destinationDirectory), "alpha.txt");
        await File.WriteAllTextAsync(existingFilePath, "existing");

        ArchiveExtractionResult result = await archive.ExtractAsync(
            archivePath,
            destinationDirectory,
            new ArchiveExtractionOptions { OverwriteFiles = true });

        result.ExtractedFiles.Should().BeEquivalentTo(
            [
                existingFilePath,
                Path.Combine(Path.GetFullPath(destinationDirectory), "nested", "beta.txt")
            ]);
        (await File.ReadAllTextAsync(existingFilePath)).Should().Be("alpha-updated");
    }

    [TestMethod]
    public async Task ExtractAsync_WhenArchiveContainsTraversalEntry_ShouldThrowInvalidDataException()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(
            rootDirectory,
            "sample.7z",
            new TestArchiveItem("../escape.txt", "escape"));
        string destinationDirectory = Path.Combine(rootDirectory, "output");
        string outsidePath = Path.Combine(rootDirectory, "escape.txt");

        Func<Task> action = () => archive.ExtractAsync(archivePath, destinationDirectory);

        await action.Should().ThrowAsync<InvalidDataException>();
        File.Exists(outsidePath).Should().BeFalse();
    }

    [TestMethod]
    public async Task ExtractAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(rootDirectory, "sample.7z", new TestArchiveItem("alpha.txt", "content"));
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        Func<Task> action = () => archive.ExtractAsync(
            archivePath,
            Path.Combine(rootDirectory, "output"),
            cancellationToken: cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ExtractAsync_WhenExtensionIsUnsupported_ShouldThrowInvalidDataException()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = Path.Combine(rootDirectory, "sample.zip");
        await File.WriteAllTextAsync(archivePath, "not a 7z archive");

        Func<Task> action = () => archive.ExtractAsync(archivePath, Path.Combine(rootDirectory, "output"));

        await action.Should().ThrowAsync<InvalidDataException>();
    }

    #endregion

    #region ListEntriesAsync Tests

    [TestMethod]
    public async Task ListEntriesAsync_WhenArchiveContainsFiles_ShouldReturnEntries()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        DateTime modifiedAt = new(2026, 9, 13, 21, 0, 0, DateTimeKind.Utc);
        string archivePath = await CreateArchiveAsync(
            rootDirectory,
            "sample.7z",
            new TestArchiveItem("nested", null, modifiedAt, IsDirectory: true),
            new TestArchiveItem("alpha.txt", "alpha-content", modifiedAt),
            new TestArchiveItem("nested/beta.txt", "beta-content"));

        IReadOnlyList<ArchiveEntry> entries = await archive.ListEntriesAsync(archivePath);

        entries.Select(entry => entry.Name).Should().Contain(["nested", "alpha.txt", "nested/beta.txt"]);

        ArchiveEntry alphaEntry = entries.Single(entry => entry.Name == "alpha.txt");
        alphaEntry.Length.Should().Be(Encoding.UTF8.GetByteCount("alpha-content"));
        alphaEntry.LastModified.Should().Be(new DateTimeOffset(modifiedAt));

        ArchiveEntry directoryEntry = entries.Single(entry => entry.Name == "nested");
        directoryEntry.Length.Should().BeNull();
        directoryEntry.LastModified.Should().Be(new DateTimeOffset(modifiedAt));

        ArchiveEntry betaEntry = entries.Single(entry => entry.Name == "nested/beta.txt");
        betaEntry.Length.Should().Be(Encoding.UTF8.GetByteCount("beta-content"));
        betaEntry.LastModified.Should().BeNull();
    }

    [TestMethod]
    public async Task ListEntriesAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = await CreateArchiveAsync(rootDirectory, "sample.7z", new TestArchiveItem("alpha.txt", "content"));
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        Func<Task> action = () => archive.ListEntriesAsync(archivePath, cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ListEntriesAsync_WhenExtensionIsUnsupported_ShouldThrowInvalidDataException()
    {
        SevenZipArchive archive = new();
        string rootDirectory = CreateTestDirectory();
        string archivePath = Path.Combine(rootDirectory, "sample.zip");
        await File.WriteAllTextAsync(archivePath, "not a 7z archive");

        Func<Task> action = () => archive.ListEntriesAsync(archivePath);

        await action.Should().ThrowAsync<InvalidDataException>();
    }

    #endregion

    #region Registration Tests

    [TestMethod]
    public void AddSevenZipArchiving_WhenRegistered_ShouldExposeSingletonProvider()
    {
        ServiceCollection services = new();

        services.AddSevenZipArchiving();

        using ServiceProvider provider = services.BuildServiceProvider();
        SevenZipArchive archive = provider.GetRequiredService<SevenZipArchive>();
        IArchiveProvider abstraction = provider.GetRequiredService<IArchiveProvider>();

        abstraction.Should().BeSameAs(archive);
    }

    #endregion

    private static string CreateTestDirectory([System.Runtime.CompilerServices.CallerMemberName] string? testName = null)
    {
        string directory = Path.Combine(
            AppContext.BaseDirectory,
            "TestArtifacts",
            nameof(SevenZipArchiveTests),
            testName ?? "Unknown");

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        Directory.CreateDirectory(directory);
        return directory;
    }

    private static async Task<string> CreateArchiveAsync(
        string directory,
        string fileName,
        params TestArchiveItem[] items)
    {
        Directory.CreateDirectory(directory);
        string archivePath = Path.Combine(directory, fileName);

        await using FileStream archiveStream = File.Create(archivePath);
        await using IAsyncWriter writer = await WriterFactory.OpenAsyncWriter(
            archiveStream,
            ArchiveType.SevenZip,
            new SevenZipWriterOptions(CompressionType.LZMA2));

        foreach (TestArchiveItem item in items)
        {
            if (item.IsDirectory)
            {
                await writer.WriteDirectoryAsync(item.Path, item.LastModified);
                continue;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(item.Content!);
            await using MemoryStream source = new(bytes);
            await writer.WriteAsync(item.Path, source, item.LastModified);
        }

        return archivePath;
    }

    private sealed record TestArchiveItem(
        string Path,
        string? Content,
        DateTime? LastModified = null,
        bool IsDirectory = false);
}
