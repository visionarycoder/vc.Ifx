using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SystemZipArchive = System.IO.Compression.ZipArchive;
using vc.Ifx.Archiving.Zip;

namespace vc.Ifx.Archiving.GZip.UnitTests;

[TestClass]
public sealed class ZipArchiveTests
{
    [TestMethod]
    public void FormatAndCanRead_IdentifyZipArchives()
    {
        ZipArchive archive = new();

        archive.Format.Should().Be("zip");
        archive.CanRead("sample.zip").Should().BeTrue();
        archive.CanRead("sample.ZIP").Should().BeTrue();
        archive.CanRead("sample.tar").Should().BeFalse();
        Action missing = () => archive.CanRead("");
        missing.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public async Task ListEntriesAsync_ReturnsFilesAndSupportsEmptyArchives()
    {
        ZipArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "sample.zip");
        await CreateZipAsync(archivePath, ("nested/", null), ("nested/sample.txt", "payload"));

        IReadOnlyList<ArchiveEntry> entries = await archive.ListEntriesAsync(archivePath);

        entries.Should().ContainSingle();
        entries[0].Name.Should().Be("nested/sample.txt");
        entries[0].Length.Should().Be(7);

        string emptyArchivePath = Path.Combine(root, "empty.zip");
        await CreateZipAsync(emptyArchivePath);
        (await archive.ListEntriesAsync(emptyArchivePath)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task ExtractAsync_CreatesNestedFilesAndHonorsOverwriteOptions()
    {
        ZipArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "sample.zip");
        await CreateZipAsync(archivePath, ("nested/", null), ("nested/sample.txt", "updated"));
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
        ZipArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "unsafe.zip");
        await CreateZipAsync(archivePath, ("../outside.txt", "outside"));

        Func<Task> extract = () => archive.ExtractAsync(archivePath, Path.Combine(root, "output"));

        await extract.Should().ThrowAsync<InvalidDataException>();
    }

    [TestMethod]
    public async Task ExtractAndListAsync_HonorCancellation()
    {
        ZipArchive archive = new();
        string root = CreateTestDirectory();
        string archivePath = Path.Combine(root, "sample.zip");
        await CreateZipAsync(archivePath, ("sample.txt", "payload"));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Func<Task> extract = () => archive.ExtractAsync(
            archivePath,
            Path.Combine(root, "output"),
            cancellationToken: cancellation.Token);
        Func<Task> list = () => archive.ListEntriesAsync(archivePath, cancellation.Token);

        await extract.Should().ThrowAsync<OperationCanceledException>();
        await list.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public void AddZipArchiveProvider_RegistersOneProviderInstance()
    {
        ServiceCollection services = new();
        services.AddZipArchiveProvider();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IArchiveProvider>()
            .Should().BeSameAs(provider.GetRequiredService<ZipArchive>());
    }

    private static string CreateTestDirectory([System.Runtime.CompilerServices.CallerMemberName] string? testName = null)
    {
        string directory = Path.Combine(
            AppContext.BaseDirectory,
            "TestArtifacts",
            nameof(ZipArchiveTests),
            testName ?? "Unknown");
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return Directory.CreateDirectory(directory).FullName;
    }

    private static async Task CreateZipAsync(string archivePath, params (string Name, string? Content)[] entries)
    {
        await using FileStream stream = File.Create(archivePath);
        using SystemZipArchive archive = new(stream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true);
        foreach ((string name, string? content) in entries)
        {
            System.IO.Compression.ZipArchiveEntry entry = archive.CreateEntry(name);
            if (content is null)
            {
                continue;
            }

            await using Stream destination = entry.Open();
            await destination.WriteAsync(Encoding.UTF8.GetBytes(content));
        }
    }
}
