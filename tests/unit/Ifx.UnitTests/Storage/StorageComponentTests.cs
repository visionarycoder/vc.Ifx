using Microsoft.Extensions.Logging.Abstractions;
using Ifx.Storage;

namespace Ifx.Tests.Storage;

[TestClass]
public sealed class StorageComponentTests
{
    public TestContext TestContext { get; set; } = null!;

    private StorageComponent component = null!;
    private string rootPath = null!;

    [TestInitialize]
    public void Initialize()
    {
        component = new StorageComponent(NullLogger<StorageComponent>.Instance);
        rootPath = Path.Combine(AppContext.BaseDirectory, "storage-component-tests", TestContext.TestName);

        if (Directory.Exists(rootPath))
        {
            Directory.Delete(rootPath, recursive: true);
        }

        Directory.CreateDirectory(rootPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(rootPath))
        {
            Directory.Delete(rootPath, recursive: true);
        }
    }

    #region Constructor tests

    [TestMethod]
    public void Constructor_ValidatesLogger()
    {
        component.Should().NotBeNull();
        Action action = () => _ = new StorageComponent(null!);
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    #endregion

    #region File operations

    [TestMethod]
    public async Task FileOperations_ReadWriteAndDeleteRoundTrip()
    {
        string textPath = Path.Combine(rootPath, "text.txt");
        string bytesPath = Path.Combine(rootPath, "bytes.bin");
        FileInfo fileInfo = new(textPath);

        component.FileExists(textPath).Should().BeFalse();
        component.FileExists(fileInfo).Should().BeFalse();

        component.WriteAllText(textPath, "alpha");
        component.WriteAllBytes(bytesPath, [1, 2, 3]);
        await component.WriteAllTextAsync(textPath, "beta");
        await component.WriteAllBytesAsync(bytesPath, [4, 5]);

        component.FileExists(textPath).Should().BeTrue();
        component.FileExists(new FileInfo(textPath)).Should().BeTrue();
        component.ReadAllText(textPath).Should().Be("beta");
        (await component.ReadAllTextAsync(textPath)).Should().Be("beta");
        (await component.ReadAllTextAsync(textPath, CancellationToken.None)).Should().Be("beta");
        component.ReadAllBytes(bytesPath).Should().Equal([4, 5]);
        (await component.ReadAllBytesAsync(bytesPath)).Should().Equal([4, 5]);

        component.DeleteFile(textPath);
        await component.DeleteFileAsync(bytesPath);
        component.FileExists(textPath).Should().BeFalse();
        component.FileExists(bytesPath).Should().BeFalse();
    }

    [TestMethod]
    public async Task FileOperations_ValidatePathsAndContent()
    {
        foreach (string? path in new string?[] { null, string.Empty, " " })
        {
            Action exists = () => component.FileExists(path!);
            Action readText = () => component.ReadAllText(path!);
            Func<Task> readTextAsync = () => component.ReadAllTextAsync(path!);
            Func<Task> readTextAsyncWithToken = () => component.ReadAllTextAsync(path!, CancellationToken.None);
            Action readBytes = () => component.ReadAllBytes(path!);
            Func<Task> readBytesAsync = () => component.ReadAllBytesAsync(path!);
            Action deleteFile = () => component.DeleteFile(path!);
            Func<Task> deleteFileAsync = () => component.DeleteFileAsync(path!);
            Action getFullPath = () => component.GetFullPath(path!);
            Action getDirectoryName = () => component.GetDirectoryName(path!);
            Action getFileName = () => component.GetFileName(path!);
            Action directoryExists = () => component.DirectoryExists(path!);
            Action createDirectory = () => component.CreateDirectory(path!);
            Func<Task> createDirectoryAsync = () => component.CreateDirectoryAsync(path!);
            Action deleteDirectory = () => component.DeleteDirectory(path!);
            Func<Task> deleteDirectoryAsync = () => component.DeleteDirectoryAsync(path!);
            Action getFiles = () => component.GetFiles(path!);
            Action getDirectories = () => component.GetDirectories(path!);
            Func<Task> enumerate = () => ReadAllAsync(component.EnumerateFilesAsync(path!));

            exists.Should().Throw<ArgumentException>();
            readText.Should().Throw<ArgumentException>();
            await readTextAsync.Should().ThrowAsync<ArgumentException>();
            await readTextAsyncWithToken.Should().ThrowAsync<ArgumentException>();
            readBytes.Should().Throw<ArgumentException>();
            await readBytesAsync.Should().ThrowAsync<ArgumentException>();
            deleteFile.Should().Throw<ArgumentException>();
            await deleteFileAsync.Should().ThrowAsync<ArgumentException>();
            getFullPath.Should().Throw<ArgumentException>();
            getDirectoryName.Should().Throw<ArgumentException>();
            getFileName.Should().Throw<ArgumentException>();
            directoryExists.Should().Throw<ArgumentException>();
            createDirectory.Should().Throw<ArgumentException>();
            await createDirectoryAsync.Should().ThrowAsync<ArgumentException>();
            deleteDirectory.Should().Throw<ArgumentException>();
            await deleteDirectoryAsync.Should().ThrowAsync<ArgumentException>();
            getFiles.Should().Throw<ArgumentException>();
            getDirectories.Should().Throw<ArgumentException>();
            await enumerate.Should().ThrowAsync<ArgumentException>();
        }

        Action nullFileInfo = () => component.FileExists((FileInfo)null!);
        nullFileInfo.Should().Throw<ArgumentNullException>().WithParameterName("fileInfo");

        Action writeTextNullPath = () => component.WriteAllText(null!, "value");
        Func<Task> writeTextNullPathAsync = () => component.WriteAllTextAsync(null!, "value");
        Action writeBytesNullPath = () => component.WriteAllBytes(null!, [1]);
        Func<Task> writeBytesNullPathAsync = () => component.WriteAllBytesAsync(null!, [1]);
        writeTextNullPath.Should().Throw<ArgumentNullException>().WithParameterName("path");
        await writeTextNullPathAsync.Should().ThrowAsync<ArgumentNullException>().WithParameterName("path");
        writeBytesNullPath.Should().Throw<ArgumentNullException>().WithParameterName("path");
        await writeBytesNullPathAsync.Should().ThrowAsync<ArgumentNullException>().WithParameterName("path");

        Action writeTextBlankPath = () => component.WriteAllText(" ", "value");
        Func<Task> writeTextBlankPathAsync = () => component.WriteAllTextAsync(" ", "value");
        Action writeBytesBlankPath = () => component.WriteAllBytes(" ", [1]);
        Func<Task> writeBytesBlankPathAsync = () => component.WriteAllBytesAsync(" ", [1]);
        writeTextBlankPath.Should().Throw<ArgumentException>();
        await writeTextBlankPathAsync.Should().ThrowAsync<ArgumentException>();
        writeBytesBlankPath.Should().Throw<ArgumentException>();
        await writeBytesBlankPathAsync.Should().ThrowAsync<ArgumentException>();

        Action writeTextNullContent = () => component.WriteAllText(Path.Combine(rootPath, "x.txt"), null!);
        Func<Task> writeTextNullContentAsync = () => component.WriteAllTextAsync(Path.Combine(rootPath, "x.txt"), null!);
        Action writeBytesNullContent = () => component.WriteAllBytes(Path.Combine(rootPath, "x.bin"), null!);
        Func<Task> writeBytesNullContentAsync = () => component.WriteAllBytesAsync(Path.Combine(rootPath, "x.bin"), null!);
        writeTextNullContent.Should().Throw<ArgumentNullException>().WithParameterName("content");
        await writeTextNullContentAsync.Should().ThrowAsync<ArgumentNullException>().WithParameterName("content");
        writeBytesNullContent.Should().Throw<ArgumentNullException>().WithParameterName("bytes");
        await writeBytesNullContentAsync.Should().ThrowAsync<ArgumentNullException>().WithParameterName("bytes");
    }

    #endregion

    #region Directory operations

    [TestMethod]
    public async Task DirectoryOperations_CreateEnumerateDeleteAndInspect()
    {
        string directory = Path.Combine(rootPath, "folder");
        string subdirectory = Path.Combine(directory, "sub");
        string alphaPath = Path.Combine(directory, "alpha.txt");
        string betaPath = Path.Combine(directory, "beta.bin");
        string gammaPath = Path.Combine(subdirectory, "gamma.txt");

        component.DirectoryExists(directory).Should().BeFalse();
        component.CreateDirectory(directory).FullName.Should().Be(directory);
        (await component.CreateDirectoryAsync(subdirectory)).FullName.Should().Be(subdirectory);
        component.DirectoryExists(directory).Should().BeTrue();

        File.WriteAllText(alphaPath, "alpha");
        File.WriteAllBytes(betaPath, [9]);
        File.WriteAllText(gammaPath, "gamma");

        component.GetFiles(directory).Should().BeEquivalentTo([alphaPath, betaPath]);
        component.GetFiles(directory, "*.txt").Should().Equal([alphaPath]);
        component.GetDirectories(directory).Should().Equal([subdirectory]);
        component.GetDirectories(directory, "s*").Should().Equal([subdirectory]);
        component.GetFullPath(alphaPath).Should().Be(Path.GetFullPath(alphaPath));
        component.GetDirectoryName(alphaPath).Should().Be(directory);
        component.GetFileName(alphaPath).Should().Be("alpha.txt");

        (await ReadAllAsync(component.EnumerateFilesAsync(directory))).Should().BeEquivalentTo([alphaPath, betaPath]);
        (await ReadAllAsync(component.EnumerateFilesAsync(directory, "*.txt"))).Should().Equal([alphaPath]);
        (await ReadAllAsync(component.EnumerateFilesAsync(directory, "*.*", CancellationToken.None))).Should().BeEquivalentTo([alphaPath, betaPath]);

        component.DeleteDirectory(directory, recursive: true);
        component.DirectoryExists(directory).Should().BeFalse();

        await component.DeleteDirectoryAsync(directory, recursive: true);
        component.DirectoryExists(directory).Should().BeFalse();
    }

    [TestMethod]
    public async Task DirectoryOperations_DeletionAndEnumerationObserveBranchesAndCancellation()
    {
        string missing = Path.Combine(rootPath, "missing");
        component.DeleteDirectory(missing, recursive: true);
        await component.DeleteDirectoryAsync(missing, recursive: true);

        string directory = Path.Combine(rootPath, "data");
        Directory.CreateDirectory(directory);
        string firstPath = Path.Combine(directory, "first.txt");
        string secondPath = Path.Combine(directory, "second.txt");
        File.WriteAllText(firstPath, "first");
        File.WriteAllText(secondPath, "second");

        using var canceledBeforeEnumeration = new CancellationTokenSource();
        canceledBeforeEnumeration.Cancel();
        Func<Task> canceledEnumeration = () => ReadAllAsync(component.EnumerateFilesAsync(directory, "*.txt", canceledBeforeEnumeration.Token));
        await canceledEnumeration.Should().ThrowAsync<OperationCanceledException>();

        using var canceledDuringEnumeration = new CancellationTokenSource();
        await using var iterator = component.EnumerateFilesAsync(directory, "*.txt", canceledDuringEnumeration.Token).GetAsyncEnumerator();
        (await iterator.MoveNextAsync()).Should().BeTrue();
        canceledDuringEnumeration.Cancel();
        Func<Task> moveNext = async () => await iterator.MoveNextAsync();
        await moveNext.Should().ThrowAsync<OperationCanceledException>();

        await component.DeleteDirectoryAsync(directory, recursive: true);
        component.DirectoryExists(directory).Should().BeFalse();
    }

    [TestMethod]
    public async Task DirectoryOperations_ValidateSearchPatterns()
    {
        string directory = Path.Combine(rootPath, "filters");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "item.txt"), "value");

        foreach (string? pattern in new string?[] { null, string.Empty, " " })
        {
            Action getFiles = () => component.GetFiles(directory, pattern!);
            Action getDirectories = () => component.GetDirectories(directory, pattern!);
            Func<Task> enumerate = () => ReadAllAsync(component.EnumerateFilesAsync(directory, pattern!));
            Func<Task> enumerateWithToken = () => ReadAllAsync(component.EnumerateFilesAsync(directory, pattern!, CancellationToken.None));

            getFiles.Should().Throw<ArgumentException>();
            getDirectories.Should().Throw<ArgumentException>();
            await enumerate.Should().ThrowAsync<ArgumentException>();
            await enumerateWithToken.Should().ThrowAsync<ArgumentException>();
        }
    }

    #endregion

    private static async Task<List<string>> ReadAllAsync(IAsyncEnumerable<string> source)
    {
        var results = new List<string>();
        await foreach (string item in source)
        {
            results.Add(item);
        }

        return results;
    }
}
