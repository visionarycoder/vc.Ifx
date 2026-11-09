using System.Formats.Tar;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using vc.Ifx.Archiving.Tar;

namespace vc.Ifx.Archiving.GZip.UnitTests;

[TestClass]
public sealed class TarArchiveTests
{
    [TestMethod]
    public void FormatAndCanRead_IdentifyTarArchives()
    {
        TarArchive archive = new();

        archive.Format.Should().Be("tar");
        archive.CanRead("sample.tar").Should().BeTrue();
        archive.CanRead("sample.TAR").Should().BeTrue();
        archive.CanRead("sample.tar.gz").Should().BeFalse();
        Action missing = () => archive.CanRead(" ");
        missing.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public async Task ListEntriesAsync_ReturnsArchiveMetadataAndSupportsEmptyArchives()
    {
        TarArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "sample.tar");
        using MemoryStream data = new(Encoding.UTF8.GetBytes("payload"));
        TarEntry file = new PaxTarEntry(TarEntryType.RegularFile, "folder/sample.txt")
        {
            DataStream = data
        };
        TarEntry directory = new PaxTarEntry(TarEntryType.Directory, "folder");
        await CreateTarAsync(archivePath, file, directory);

        IReadOnlyList<ArchiveEntry> entries = await archive.ListEntriesAsync(archivePath);

        entries.Should().HaveCount(2);
        entries[0].Name.Should().Be("folder/sample.txt");
        entries[0].Length.Should().Be(7);
        entries[1].Name.Should().Be("folder");

        string emptyArchivePath = Path.Combine(root, "empty.tar");
        await CreateTarAsync(emptyArchivePath);
        (await archive.ListEntriesAsync(emptyArchivePath)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task ExtractAsync_CreatesDirectoriesAndFilesAndHonorsOverwriteOptions()
    {
        TarArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "sample.tar");
        using MemoryStream data = new(Encoding.UTF8.GetBytes("updated"));
        await CreateTarAsync(
            archivePath,
            new PaxTarEntry(TarEntryType.Directory, "nested"),
            new PaxTarEntry(TarEntryType.RegularFile, "nested/sample.txt") { DataStream = data });
        string destination = Path.Combine(root, "output");
        Directory.CreateDirectory(destination);
        string destinationFile = Path.Combine(destination, "nested", "sample.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
        await File.WriteAllTextAsync(destinationFile, "existing");

        ArchiveExtractionResult skipped = await archive.ExtractAsync(archivePath, destination);

        skipped.ExtractedFiles.Should().BeEmpty();
        (await File.ReadAllTextAsync(destinationFile)).Should().Be("existing");

        ArchiveExtractionResult extracted = await archive.ExtractAsync(
            archivePath,
            destination + Path.DirectorySeparatorChar,
            new ArchiveExtractionOptions { OverwriteFiles = true });

        extracted.ExtractedFiles.Should().ContainSingle().Which.Should().Be(destinationFile);
        (await File.ReadAllTextAsync(destinationFile)).Should().Be("updated");
    }

    [TestMethod]
    public async Task ExtractAsync_RejectsEntriesOutsideDestination()
    {
        TarArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "unsafe.tar");
        using MemoryStream data = new(Encoding.UTF8.GetBytes("outside"));
        await CreateTarAsync(
            archivePath,
            new PaxTarEntry(TarEntryType.RegularFile, "../outside.txt") { DataStream = data });

        Func<Task> extract = () => archive.ExtractAsync(archivePath, Path.Combine(root, "output"));

        await extract.Should().ThrowAsync<InvalidDataException>();
    }

    [TestMethod]
    public async Task ExtractAsync_SkipsEntriesWithoutDataAndHonorsCancellation()
    {
        TarArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "entries.tar");
        TarEntry link = new PaxTarEntry(TarEntryType.SymbolicLink, "link")
        {
            LinkName = "target.txt"
        };
        await CreateTarAsync(archivePath, link);

        ArchiveExtractionResult extracted = await archive.ExtractAsync(
            archivePath,
            Path.Combine(root, "output"));

        extracted.ExtractedFiles.Should().BeEmpty();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Func<Task> extractCanceled = () => archive.ExtractAsync(
            archivePath,
            Path.Combine(root, "canceled"),
            cancellationToken: cancellation.Token);
        Func<Task> listCanceled = () => archive.ListEntriesAsync(archivePath, cancellation.Token);

        await extractCanceled.Should().ThrowAsync<OperationCanceledException>();
        await listCanceled.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public void AddTarArchiveProvider_RegistersOneProviderInstance()
    {
        ServiceCollection services = new();
        services.AddTarArchiveProvider();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IArchiveProvider>()
            .Should().BeSameAs(provider.GetRequiredService<TarArchive>());
    }

    private static string CreateTestDirectory([System.Runtime.CompilerServices.CallerMemberName] string? testName = null)
    {
        string directory = Path.Combine(
            AppContext.BaseDirectory,
            "TestArtifacts",
            nameof(TarArchiveTests),
            testName ?? "Unknown");
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return Directory.CreateDirectory(directory).FullName;
    }

    private static async Task CreateTarAsync(string archivePath, params TarEntry[] entries)
    {
        await using FileStream archiveStream = File.Create(archivePath);
        using var writer = new TarWriter(archiveStream, leaveOpen: true);
        try
        {
            foreach (TarEntry entry in entries)
            {
                await writer.WriteEntryAsync(entry);
            }
        }
        finally
        {
            foreach (TarEntry entry in entries)
            {
                if (entry.DataStream is { } dataStream)
                {
                    await dataStream.DisposeAsync();
                }
            }
        }
    }
}
