using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Storage;

namespace VisionaryCoder.Framework.Tests.Storage;

/// <summary>
/// Comprehensive data-driven unit tests for StorageService to ensure 100% code coverage.
/// Tests happy path, edge cases, and expected failures using temporary file system operations.
/// </summary>
[TestClass]
public class StorageServiceTests
{
    private Mock<ILogger<StorageService>>? mockLogger;
    private StorageService? service;
    private string? testDirectory;

    [TestInitialize]
    public void Initialize()
    {
        mockLogger = new Mock<ILogger<StorageService>>();
        service = new StorageService(mockLogger.Object);
        testDirectory = Path.Combine(Path.GetTempPath(), $"StorageServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (testDirectory != null && Directory.Exists(testDirectory))
        {
            try
            {
                Directory.Delete(testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidLogger_ShouldInitializeSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StorageService>>();

        // Act
        var service = new StorageService(mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<StorageService> action = () => new StorageService(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region FileExists Tests (FileInfo overload)

    [TestMethod]
    public void FileExists_FileInfo_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "test.txt");
        File.WriteAllText(filePath, "test content");
        var fileInfo = new FileInfo(filePath);

        // Act
        bool result = service!.FileExists(fileInfo);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_FileInfo_WithNonExistingFile_ShouldReturnFalse()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "nonexistent.txt");
        var fileInfo = new FileInfo(filePath);

        // Act
        bool result = service!.FileExists(fileInfo);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void FileExists_FileInfo_WithNullFileInfo_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<bool> action = () => service!.FileExists((FileInfo)null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("fileInfo");
    }

    #endregion

    #region FileExists Tests (string overload)

    [TestMethod]
    [DataRow("test.txt")]
    [DataRow("subdir/nested.txt")]
    [DataRow("file.with.multiple.dots.txt")]
    public void FileExists_String_WithExistingFile_ShouldReturnTrue(string relativePath)
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, relativePath);
        string? directory = Path.GetDirectoryName(filePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, "test content");

        // Act
        bool result = service!.FileExists(filePath);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_String_WithNonExistingFile_ShouldReturnFalse()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "nonexistent.txt");

        // Act
        bool result = service!.FileExists(filePath);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void FileExists_String_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        // Arrange & Act
        Func<bool> action = () => service!.FileExists(path!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region ReadAllText Tests

    [TestMethod]
    [DataRow("Hello World")]
    [DataRow("")]
    [DataRow("Line1\nLine2\nLine3")]
    [DataRow("Unicode: 你好世界 🌍")]
    public void ReadAllText_WithValidFile_ShouldReturnContent(string content)
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "test.txt");
        File.WriteAllText(filePath, content);

        // Act
        string result = service!.ReadAllText(filePath);

        // Assert
        result.Should().Be(content);
    }

    [TestMethod]
    public void ReadAllText_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "nonexistent.txt");

        // Act
        Func<string> action = () => service!.ReadAllText(filePath);

        // Assert
        action.Should().Throw<FileNotFoundException>();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void ReadAllText_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        // Arrange & Act
        Func<string> action = () => service!.ReadAllText(path!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region ReadAllTextAsync Tests

    [TestMethod]
    [DataRow("Async Content")]
    [DataRow("")]
    [DataRow("Multi\nLine\nAsync")]
    public async Task ReadAllTextAsync_WithValidFile_ShouldReturnContent(string content)
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "async_test.txt");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        string result = await service!.ReadAllTextAsync(filePath);

        // Assert
        result.Should().Be(content);
    }

    [TestMethod]
    public async Task ReadAllTextAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "nonexistent.txt");

        // Act
        Func<Task<string>> action = async () => await service!.ReadAllTextAsync(filePath);

        // Assert
        await action.Should().ThrowAsync<FileNotFoundException>();
    }

    [TestMethod]
    public async Task ReadAllTextAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "cancel_test.txt");
        await File.WriteAllTextAsync(filePath, "content");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task<string>> action = async () => await service!.ReadAllTextAsync(filePath, cts.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region ReadAllBytes Tests

    [TestMethod]
    [DataRow(new byte[] { 1, 2, 3, 4, 5 })]
    [DataRow(new byte[] { })]
    [DataRow(new byte[] { 0, 255, 128, 64 })]
    public void ReadAllBytes_WithValidFile_ShouldReturnBytes(byte[] bytes)
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "bytes.bin");
        File.WriteAllBytes(filePath, bytes);

        // Act
        byte[] result = service!.ReadAllBytes(filePath);

        // Assert
        result.Should().Equal(bytes);
    }

    [TestMethod]
    public void ReadAllBytes_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "nonexistent.bin");

        // Act
        Func<byte[]> action = () => service!.ReadAllBytes(filePath);

        // Assert
        action.Should().Throw<FileNotFoundException>();
    }

    #endregion

    #region ReadAllBytesAsync Tests

    [TestMethod]
    public async Task ReadAllBytesAsync_WithValidFile_ShouldReturnBytes()
    {
        // Arrange
        byte[] bytes = [10, 20, 30, 40, 50];
        string filePath = Path.Combine(testDirectory!, "async_bytes.bin");
        await File.WriteAllBytesAsync(filePath, bytes);

        // Act
        byte[] result = await service!.ReadAllBytesAsync(filePath);

        // Assert
        result.Should().Equal(bytes);
    }

    #endregion

    #region WriteAllText Tests

    [TestMethod]
    [DataRow("Write this text")]
    [DataRow("")]
    [DataRow("Multi\nLine\nText")]
    public void WriteAllText_WithValidPath_ShouldWriteContent(string content)
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "write_test.txt");

        // Act
        service!.WriteAllText(filePath, content);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Be(content);
    }

    [TestMethod]
    public void WriteAllText_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "null_content.txt");

        // Act
        Action action = () => service!.WriteAllText(filePath, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("content");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void WriteAllText_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        // Arrange & Act
        Action action = () => service!.WriteAllText(path!, "content");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region WriteAllTextAsync Tests

    [TestMethod]
    public async Task WriteAllTextAsync_WithValidPath_ShouldWriteContent()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "async_write.txt");
        string content = "Async written content";

        // Act
        await service!.WriteAllTextAsync(filePath, content);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        (await File.ReadAllTextAsync(filePath)).Should().Be(content);
    }

    #endregion

    #region WriteAllBytes Tests

    [TestMethod]
    public void WriteAllBytes_WithValidPath_ShouldWriteBytes()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "write_bytes.bin");
        byte[] bytes = [100, 200, 50];

        // Act
        service!.WriteAllBytes(filePath, bytes);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllBytes(filePath).Should().Equal(bytes);
    }

    [TestMethod]
    public void WriteAllBytes_WithNullBytes_ShouldThrowArgumentNullException()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "null_bytes.bin");

        // Act
        Action action = () => service!.WriteAllBytes(filePath, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("bytes");
    }

    #endregion

    #region WriteAllBytesAsync Tests

    [TestMethod]
    public async Task WriteAllBytesAsync_WithValidPath_ShouldWriteBytes()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "async_write_bytes.bin");
        byte[] bytes = [11, 22, 33, 44];

        // Act
        await service!.WriteAllBytesAsync(filePath, bytes);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        (await File.ReadAllBytesAsync(filePath)).Should().Equal(bytes);
    }

    #endregion

    #region DeleteFile Tests

    [TestMethod]
    public void DeleteFile_WithExistingFile_ShouldDeleteFile()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "delete_me.txt");
        File.WriteAllText(filePath, "content");

        // Act
        service!.DeleteFile(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [TestMethod]
    public void DeleteFile_WithNonExistentFile_ShouldNotThrow()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "nonexistent_delete.txt");

        // Act
        Action action = () => service!.DeleteFile(filePath);

        // Assert
        action.Should().NotThrow();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void DeleteFile_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        // Arrange & Act
        Action action = () => service!.DeleteFile(path!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region DeleteFileAsync Tests

    [TestMethod]
    public async Task DeleteFileAsync_WithExistingFile_ShouldDeleteFile()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "async_delete_me.txt");
        await File.WriteAllTextAsync(filePath, "content");

        // Act
        await service!.DeleteFileAsync(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    #endregion

    #region DirectoryExists Tests

    [TestMethod]
    public void DirectoryExists_WithExistingDirectory_ShouldReturnTrue()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "existing_dir");
        Directory.CreateDirectory(dirPath);

        // Act
        bool result = service!.DirectoryExists(dirPath);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void DirectoryExists_WithNonExistentDirectory_ShouldReturnFalse()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "nonexistent_dir");

        // Act
        bool result = service!.DirectoryExists(dirPath);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void DirectoryExists_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        // Arrange & Act
        Func<bool> action = () => service!.DirectoryExists(path!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region CreateDirectory Tests

    [TestMethod]
    public void CreateDirectory_WithValidPath_ShouldCreateDirectory()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "new_directory");

        // Act
        DirectoryInfo result = service!.CreateDirectory(dirPath);

        // Assert
        Directory.Exists(dirPath).Should().BeTrue();
        result.Should().NotBeNull();
        result.Exists.Should().BeTrue();
    }

    [TestMethod]
    public void CreateDirectory_WithNestedPath_ShouldCreateAllDirectories()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "level1", "level2", "level3");

        // Act
        DirectoryInfo result = service!.CreateDirectory(dirPath);

        // Assert
        Directory.Exists(dirPath).Should().BeTrue();
    }

    [TestMethod]
    public void CreateDirectory_WithExistingDirectory_ShouldNotThrow()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "existing");
        Directory.CreateDirectory(dirPath);

        // Act
        Func<DirectoryInfo> action = () => service!.CreateDirectory(dirPath);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region CreateDirectoryAsync Tests

    [TestMethod]
    public async Task CreateDirectoryAsync_WithValidPath_ShouldCreateDirectory()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "async_new_directory");

        // Act
        DirectoryInfo result = await service!.CreateDirectoryAsync(dirPath);

        // Assert
        Directory.Exists(dirPath).Should().BeTrue();
        result.Should().NotBeNull();
    }

    #endregion

    #region DeleteDirectory Tests

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void DeleteDirectory_WithEmptyDirectory_ShouldDeleteDirectory(bool recursive)
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "delete_dir");
        Directory.CreateDirectory(dirPath);

        // Act
        service!.DeleteDirectory(dirPath, recursive);

        // Assert
        Directory.Exists(dirPath).Should().BeFalse();
    }

    [TestMethod]
    public void DeleteDirectory_WithFilesAndRecursiveTrue_ShouldDeleteAll()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "delete_with_files");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        // Act
        service!.DeleteDirectory(dirPath, recursive: true);

        // Assert
        Directory.Exists(dirPath).Should().BeFalse();
    }

    [TestMethod]
    public void DeleteDirectory_WithFilesAndRecursiveFalse_ShouldThrowIOException()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "delete_fail");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        // Act
        Action action = () => service!.DeleteDirectory(dirPath, recursive: false);

        // Assert
        action.Should().Throw<IOException>();
    }

    [TestMethod]
    public void DeleteDirectory_WithNonExistentDirectory_ShouldNotThrow()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "nonexistent_delete_dir");

        // Act
        Action action = () => service!.DeleteDirectory(dirPath);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region DeleteDirectoryAsync Tests

    [TestMethod]
    public async Task DeleteDirectoryAsync_WithExistingDirectory_ShouldDeleteDirectory()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "async_delete_dir");
        Directory.CreateDirectory(dirPath);

        // Act
        await service!.DeleteDirectoryAsync(dirPath);

        // Assert
        Directory.Exists(dirPath).Should().BeFalse();
    }

    #endregion

    #region GetFiles Tests

    [TestMethod]
    [DataRow("*")]
    [DataRow("*.txt")]
    [DataRow("test*")]
    public void GetFiles_WithPattern_ShouldReturnMatchingFiles(string pattern)
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "files_dir");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "test1.txt"), "");
        File.WriteAllText(Path.Combine(dirPath, "test2.txt"), "");
        File.WriteAllText(Path.Combine(dirPath, "other.doc"), "");

        // Act
        string[] result = service!.GetFiles(dirPath, pattern);

        // Assert
        result.Should().NotBeNull();
        if (pattern == "*")
        {
            result.Should().HaveCount(3);
        }
        else if (pattern == "*.txt")
        {
            result.Should().HaveCount(2);
        }
    }

    [TestMethod]
    public void GetFiles_WithEmptyDirectory_ShouldReturnEmptyArray()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "empty_dir");
        Directory.CreateDirectory(dirPath);

        // Act
        string[] result = service!.GetFiles(dirPath);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(null, "*")]
    [DataRow("", "*")]
    [DataRow("   ", "*")]
    [DataRow("validpath", null)]
    [DataRow("validpath", "")]
    [DataRow("validpath", "   ")]
    public void GetFiles_WithInvalidParameters_ShouldThrowArgumentException(string? path, string? pattern)
    {
        // Arrange & Act
        Func<string[]> action = () => service!.GetFiles(path!, pattern!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region GetDirectories Tests

    [TestMethod]
    public void GetDirectories_WithExistingSubdirectories_ShouldReturnDirectories()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "parent_dir");
        Directory.CreateDirectory(Path.Combine(dirPath, "sub1"));
        Directory.CreateDirectory(Path.Combine(dirPath, "sub2"));
        Directory.CreateDirectory(Path.Combine(dirPath, "sub3"));

        // Act
        string[] result = service!.GetDirectories(dirPath);

        // Assert
        result.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetDirectories_WithPattern_ShouldReturnMatchingDirectories()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "pattern_dir");
        Directory.CreateDirectory(Path.Combine(dirPath, "test1"));
        Directory.CreateDirectory(Path.Combine(dirPath, "test2"));
        Directory.CreateDirectory(Path.Combine(dirPath, "other"));

        // Act
        string[] result = service!.GetDirectories(dirPath, "test*");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region EnumerateFilesAsync Tests

    [TestMethod]
    public async Task EnumerateFilesAsync_WithFiles_ShouldEnumerateAllFiles()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "enumerate_dir");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file1.txt"), "");
        File.WriteAllText(Path.Combine(dirPath, "file2.txt"), "");
        File.WriteAllText(Path.Combine(dirPath, "file3.txt"), "");

        // Act
        var files = new List<string>();
        await foreach (string file in service!.EnumerateFilesAsync(dirPath))
        {
            files.Add(file);
        }

        // Assert
        files.Should().HaveCount(3);
    }

    [TestMethod]
    public async Task EnumerateFilesAsync_WithCancellation_ShouldStopEnumeration()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "cancel_enumerate");
        Directory.CreateDirectory(dirPath);
        for (int i = 0; i < 100; i++)
        {
            File.WriteAllText(Path.Combine(dirPath, $"file{i}.txt"), "");
        }
        var cts = new CancellationTokenSource();

        // Act
        var files = new List<string>();
        Func<Task> action = async () =>
        {
            await foreach (string file in service!.EnumerateFilesAsync(dirPath, "*", cts.Token))
            {
                files.Add(file);
                if (files.Count == 5)
                {
                    cts.Cancel();
                }
            }
        };

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
        files.Should().HaveCountLessThanOrEqualTo(5);
    }

    #endregion

    #region GetFullPath Tests

    [TestMethod]
    [DataRow("relative/path/file.txt")]
    [DataRow("./file.txt")]
    [DataRow("../file.txt")]
    public void GetFullPath_WithRelativePath_ShouldReturnAbsolutePath(string relativePath)
    {
        // Act
        string result = service!.GetFullPath(relativePath);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        Path.IsPathRooted(result).Should().BeTrue();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void GetFullPath_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        // Arrange & Act
        Func<string> action = () => service!.GetFullPath(path!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region GetDirectoryName Tests

    [TestMethod]
    [DataRow("C:\\folder\\file.txt", "C:\\folder")]
    [DataRow("C:\\folder\\subfolder\\file.txt", "C:\\folder\\subfolder")]
    public void GetDirectoryName_WithValidPath_ShouldReturnDirectoryName(string path, string expected)
    {
        // Act
        string? result = service!.GetDirectoryName(path);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("C:\\")]
    public void GetDirectoryName_WithRootPath_ShouldReturnNull(string path)
    {
        // Act
        string? result = service!.GetDirectoryName(path);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetFileName Tests

    [TestMethod]
    [DataRow("C:\\folder\\file.txt", "file.txt")]
    [DataRow("C:\\folder\\subfolder\\document.doc", "document.doc")]
    [DataRow("filename.txt", "filename.txt")]
    public void GetFileName_WithValidPath_ShouldReturnFileName(string path, string expected)
    {
        // Act
        string? result = service!.GetFileName(path);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void GetFileName_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        // Arrange & Act
        Func<string?> action = () => service!.GetFileName(path!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void Integration_WriteReadDeleteFile_ShouldWorkEndToEnd()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "integration_test.txt");
        string content = "Integration test content";

        // Act & Assert - Write
        service!.WriteAllText(filePath, content);
        service.FileExists(filePath).Should().BeTrue();

        // Act & Assert - Read
        string readContent = service.ReadAllText(filePath);
        readContent.Should().Be(content);

        // Act & Assert - Delete
        service.DeleteFile(filePath);
        service.FileExists(filePath).Should().BeFalse();
    }

    [TestMethod]
    public async Task Integration_AsyncOperations_ShouldWorkEndToEnd()
    {
        // Arrange
        string filePath = Path.Combine(testDirectory!, "async_integration.txt");
        string content = "Async integration test";

        // Act & Assert - Write
        await service!.WriteAllTextAsync(filePath, content);
        service.FileExists(filePath).Should().BeTrue();

        // Act & Assert - Read
        string readContent = await service.ReadAllTextAsync(filePath);
        readContent.Should().Be(content);

        // Act & Assert - Delete
        await service.DeleteFileAsync(filePath);
        service.FileExists(filePath).Should().BeFalse();
    }

    [TestMethod]
    public void Integration_DirectoryOperations_ShouldWorkEndToEnd()
    {
        // Arrange
        string dirPath = Path.Combine(testDirectory!, "integration_dir");

        // Act & Assert - Create
        service!.CreateDirectory(dirPath);
        service.DirectoryExists(dirPath).Should().BeTrue();

        // Create files in directory
        File.WriteAllText(Path.Combine(dirPath, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(dirPath, "file2.txt"), "content2");

        // Act & Assert - Get Files
        string[] files = service.GetFiles(dirPath);
        files.Should().HaveCount(2);

        // Act & Assert - Delete
        service.DeleteDirectory(dirPath, recursive: true);
        service.DirectoryExists(dirPath).Should().BeFalse();
    }

    #endregion
}
