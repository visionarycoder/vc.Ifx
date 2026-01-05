using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VisionaryCoder.Framework.Storage.Abstractions;
using VisionaryCoder.Framework.Storage.Local;

namespace VisionaryCoder.Framework.Storage.Tests.Local;

[TestClass]
public class LocalFileStorageProviderTests
{
    private Mock<ILogger<LocalFileStorageProvider>> _mockLogger = null!;
    private string _testDirectory = null!;
    private LocalFileStorageOptions _options = null!;
    private LocalFileStorageProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<LocalFileStorageProvider>>();
        _testDirectory = Path.Combine(Path.GetTempPath(), "StorageTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);

        _options = new LocalFileStorageOptions
        {
            RootDirectory = _testDirectory,
            CreateRootIfNotExists = true,
            RestrictToRootDirectory = true
        };

        _provider = new LocalFileStorageProvider(_options, _mockLogger.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [TestMethod]
    public void FileExists_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var filePath = "test.txt";
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.WriteAllText(fullPath, "test content");

        // Act
        var exists = _provider.FileExists(filePath);

        // Assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_WithNonExistingFile_ReturnsFalse()
    {
        // Arrange
        var filePath = "nonexistent.txt";

        // Act
        var exists = _provider.FileExists(filePath);

        // Assert
        exists.Should().BeFalse();
    }

    [TestMethod]
    public void FileExists_WithFileInfo_ReturnsCorrectValue()
    {
        // Arrange
        var filePath = "test.txt";
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.WriteAllText(fullPath, "test content");
        var fileInfo = new FileInfo(fullPath);

        // Act
        var exists = _provider.FileExists(fileInfo);

        // Assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_WithNullFileInfo_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _provider.FileExists((FileInfo)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void ReadAllText_WithExistingFile_ReturnsContent()
    {
        // Arrange
        const string content = "Test content";
        var filePath = "test.txt";
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.WriteAllText(fullPath, content);

        // Act
        var result = _provider.ReadAllText(filePath);

        // Assert
        result.Should().Be(content);
    }

    [TestMethod]
    public void ReadAllText_WithNonExistingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = "nonexistent.txt";

        // Act
        Action act = () => _provider.ReadAllText(filePath);

        // Assert
        act.Should().Throw<StorageException>()
            .WithInnerException<FileNotFoundException>();
    }

    [TestMethod]
    public async Task ReadAllTextAsync_WithExistingFile_ReturnsContent()
    {
        // Arrange
        const string content = "Test content";
        var filePath = "test.txt";
        var fullPath = Path.Combine(_testDirectory, filePath);
        await File.WriteAllTextAsync(fullPath, content);

        // Act
        var result = await _provider.ReadAllTextAsync(filePath);

        // Assert
        result.Should().Be(content);
    }

    [TestMethod]
    public void ReadAllBytes_WithExistingFile_ReturnsContent()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var filePath = "test.bin";
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.WriteAllBytes(fullPath, content);

        // Act
        var result = _provider.ReadAllBytes(filePath);

        // Assert
        result.Should().BeEquivalentTo(content);
    }

    [TestMethod]
    public void WriteAllText_CreatesNewFile()
    {
        // Arrange
        const string content = "Test content";
        var filePath = "test.txt";

        // Act
        _provider.WriteAllText(filePath, content);

        // Assert
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.Exists(fullPath).Should().BeTrue();
        File.ReadAllText(fullPath).Should().Be(content);
    }

    [TestMethod]
    public void WriteAllText_OverwritesExistingFile()
    {
        // Arrange
        const string originalContent = "Original content";
        const string newContent = "New content";
        var filePath = "test.txt";
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.WriteAllText(fullPath, originalContent);

        // Act
        _provider.WriteAllText(filePath, newContent);

        // Assert
        File.ReadAllText(fullPath).Should().Be(newContent);
    }

    [TestMethod]
    public void WriteAllBytes_CreatesNewFile()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var filePath = "test.bin";

        // Act
        _provider.WriteAllBytes(filePath, content);

        // Assert
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.Exists(fullPath).Should().BeTrue();
        File.ReadAllBytes(fullPath).Should().BeEquivalentTo(content);
    }

    [TestMethod]
    public void DeleteFile_RemovesExistingFile()
    {
        // Arrange
        var filePath = "test.txt";
        var fullPath = Path.Combine(_testDirectory, filePath);
        File.WriteAllText(fullPath, "test");

        // Act
        _provider.DeleteFile(filePath);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [TestMethod]
    public void DeleteFile_WithNonExistingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = "nonexistent.txt";

        // Act
        Action act = () => _provider.DeleteFile(filePath);

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void DirectoryExists_WithExistingDirectory_ReturnsTrue()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(fullPath);

        // Act
        var exists = _provider.DirectoryExists(dirPath);

        // Assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void DirectoryExists_WithNonExistingDirectory_ReturnsFalse()
    {
        // Arrange
        var dirPath = "nonexistentdir";

        // Act
        var exists = _provider.DirectoryExists(dirPath);

        // Assert
        exists.Should().BeFalse();
    }

    [TestMethod]
    public void CreateDirectory_CreatesNewDirectory()
    {
        // Arrange
        var dirPath = "newdir";

        // Act
        var result = _provider.CreateDirectory(dirPath);

        // Assert
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.Exists(fullPath).Should().BeTrue();
        result.Should().NotBeNull();
        result.Exists.Should().BeTrue();
    }

    [TestMethod]
    public void CreateDirectory_WithNestedPath_CreatesAllDirectories()
    {
        // Arrange
        var dirPath = "level1/level2/level3";

        // Act
        var result = _provider.CreateDirectory(dirPath);

        // Assert
        var fullPath = Path.Combine(_testDirectory, dirPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.Exists(fullPath).Should().BeTrue();
    }

    [TestMethod]
    public void DeleteDirectory_RemovesEmptyDirectory()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(fullPath);

        // Act
        _provider.DeleteDirectory(dirPath, false);

        // Assert
        Directory.Exists(fullPath).Should().BeFalse();
    }

    [TestMethod]
    public void DeleteDirectory_WithNonEmptyDirectoryAndRecursiveFalse_ThrowsIOException()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(fullPath);
        File.WriteAllText(Path.Combine(fullPath, "file.txt"), "test");

        // Act
        Action act = () => _provider.DeleteDirectory(dirPath, false);

        // Assert
        act.Should().Throw<StorageException>()
            .WithInnerException<IOException>();
    }

    [TestMethod]
    public void DeleteDirectory_WithRecursiveTrue_RemovesDirectoryAndContents()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(fullPath);
        Directory.CreateDirectory(Path.Combine(fullPath, "subdir"));
        File.WriteAllText(Path.Combine(fullPath, "file.txt"), "test");
        File.WriteAllText(Path.Combine(fullPath, "subdir", "file2.txt"), "test2");

        // Act
        _provider.DeleteDirectory(dirPath, true);

        // Assert
        Directory.Exists(fullPath).Should().BeFalse();
    }

    [TestMethod]
    public void GetFiles_ReturnsAllFilesInDirectory()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(fullPath);
        File.WriteAllText(Path.Combine(fullPath, "file1.txt"), "test1");
        File.WriteAllText(Path.Combine(fullPath, "file2.txt"), "test2");
        File.WriteAllText(Path.Combine(fullPath, "file3.dat"), "test3");

        // Act
        var files = _provider.GetFiles(dirPath);

        // Assert
        files.Should().HaveCount(3);
        files.Should().Contain(f => f.EndsWith("file1.txt"));
        files.Should().Contain(f => f.EndsWith("file2.txt"));
        files.Should().Contain(f => f.EndsWith("file3.dat"));
    }

    [TestMethod]
    public void GetFiles_WithPattern_ReturnsMatchingFiles()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(fullPath);
        File.WriteAllText(Path.Combine(fullPath, "file1.txt"), "test1");
        File.WriteAllText(Path.Combine(fullPath, "file2.txt"), "test2");
        File.WriteAllText(Path.Combine(fullPath, "file3.dat"), "test3");

        // Act
        var files = _provider.GetFiles(dirPath, "*.txt");

        // Assert
        files.Should().HaveCount(2);
        files.Should().Contain(f => f.EndsWith("file1.txt"));
        files.Should().Contain(f => f.EndsWith("file2.txt"));
    }

    [TestMethod]
    public void GetDirectories_ReturnsAllSubdirectories()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(Path.Combine(fullPath, "sub1"));
        Directory.CreateDirectory(Path.Combine(fullPath, "sub2"));
        Directory.CreateDirectory(Path.Combine(fullPath, "sub3"));

        // Act
        var directories = _provider.GetDirectories(dirPath);

        // Assert
        directories.Should().HaveCount(3);
        directories.Should().Contain(d => d.EndsWith("sub1"));
        directories.Should().Contain(d => d.EndsWith("sub2"));
        directories.Should().Contain(d => d.EndsWith("sub3"));
    }

    [TestMethod]
    public async Task EnumerateFilesAsync_ReturnsAllFiles()
    {
        // Arrange
        var dirPath = "testdir";
        var fullPath = Path.Combine(_testDirectory, dirPath);
        Directory.CreateDirectory(fullPath);
        File.WriteAllText(Path.Combine(fullPath, "file1.txt"), "test1");
        File.WriteAllText(Path.Combine(fullPath, "file2.txt"), "test2");

        // Act
        var files = new List<string>();
        await foreach (var file in _provider.EnumerateFilesAsync(dirPath))
        {
            files.Add(file);
        }

        // Assert
        files.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetFullPath_ReturnsCorrectPath()
    {
        // Arrange
        var relativePath = "test.txt";

        // Act
        var fullPath = _provider.GetFullPath(relativePath);

        // Assert
        fullPath.Should().Contain(_testDirectory);
        fullPath.Should().EndWith("test.txt");
    }

    [TestMethod]
    public void GetDirectoryName_ReturnsParentDirectory()
    {
        // Arrange
        var filePath = "subdir/test.txt";

        // Act
        var dirName = _provider.GetDirectoryName(filePath);

        // Assert
        dirName.Should().NotBeNull();
        dirName.Should().Contain("subdir");
    }

    [TestMethod]
    public void GetFileName_ReturnsFileName()
    {
        // Arrange
        var filePath = "subdir/test.txt";

        // Act
        var fileName = _provider.GetFileName(filePath);

        // Assert
        fileName.Should().Be("test.txt");
    }

    [TestMethod]
    public void PathTraversal_WithDotDotPath_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var filePath = "../outside.txt";

        // Act
        Action act = () => _provider.FileExists(filePath);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*outside*root*");
    }

    [TestMethod]
    public void PathTraversal_WithAbsolutePath_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var filePath = "C:\\Windows\\System32\\test.txt";

        // Act
        Action act = () => _provider.FileExists(filePath);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>();
    }
}
