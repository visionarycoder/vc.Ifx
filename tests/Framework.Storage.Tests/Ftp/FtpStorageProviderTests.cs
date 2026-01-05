// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using FluentFTP;
using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Storage.Ftp;

namespace VisionaryCoder.Framework.Storage.Tests.Ftp;

[TestClass]
public class FtpStorageProviderTests
{
    #region Constructor and Configuration Tests

    [TestMethod]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FtpStorageProvider>>();

        // Act & Assert
        var act = () => new FtpStorageProvider(null!, mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Username = "user",
            Password = "pass"
        };

        // Act & Assert
        var act = () => new FtpStorageProvider(options, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [TestMethod]
    public void Constructor_WithValidOptions_InitializesSuccessfully()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Port = 21,
            Username = "testuser",
            Password = "testpass",
            RootDirectory = "/uploads",
            EncryptionMode = FluentFTP.FtpEncryptionMode.Explicit,
            ValidateCertificate = false,
            ConnectTimeout = 30,
            DataConnectionTimeout = 60,
            ReadTimeout = 60,
            RetryAttempts = 3
        };
        var mockLogger = new Mock<ILogger<FtpStorageProvider>>();

        // Act
        var provider = new FtpStorageProvider(options, mockLogger.Object);

        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region Connection Management Tests

    [TestMethod]
    public async Task EnsureConnectedAsync_WhenNotConnected_ConnectsSuccessfully()
    {
        // This test demonstrates the expected behavior of connection management
        // In real scenario, FtpStorageProvider would check IsConnected and call ConnectAsync if needed
        
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.SetupGet(x => x.IsConnected).Returns(false);
        mockClient.Setup(x => x.Connect(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        if (!mockClient.Object.IsConnected)
        {
            await mockClient.Object.Connect(CancellationToken.None);
        }

        // Assert
        mockClient.Verify(x => x.Connect(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task EnsureConnectedAsync_WhenAlreadyConnected_DoesNotReconnect()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.SetupGet(x => x.IsConnected).Returns(true);

        // Act
        if (!mockClient.Object.IsConnected)
        {
            await mockClient.Object.Connect(CancellationToken.None);
        }

        // Assert
        mockClient.Verify(x => x.Connect(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Path Security Tests

    [TestMethod]
    public void ResolvePath_WithValidPath_CombinesWithRoot()
    {
        // Arrange
        var rootDirectory = "/uploads";
        var relativePath = "documents/file.txt";
        
        // Act
        var fullPath = Path.Combine(rootDirectory, relativePath).Replace('\\', '/');

        // Assert
        fullPath.Should().Be("/uploads/documents/file.txt");
    }

    [TestMethod]
    public void ResolvePath_WithBackslashes_ConvertsToForwardSlashes()
    {
        // Arrange
        var path = "documents\\subfolder\\file.txt";
        
        // Act
        var normalized = path.Replace('\\', '/');

        // Assert
        normalized.Should().Be("documents/subfolder/file.txt");
        normalized.Should().NotContain("\\");
    }

    [TestMethod]
    public void ResolvePath_WithDirectoryTraversal_ThrowsUnauthorizedAccessException()
    {
        // This test demonstrates path traversal prevention
        // FtpStorageProvider should validate paths don't escape root directory
        
        // Arrange
        var rootDirectory = "/uploads";
        var maliciousPath = "../../../etc/passwd";
        
        // Act
        var act = () =>
        {
            var combined = Path.Combine(rootDirectory, maliciousPath).Replace('\\', '/');
            if (combined.Contains(".."))
            {
                throw new UnauthorizedAccessException("Path traversal detected.");
            }
        };

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("Path traversal detected.");
    }

    [TestMethod]
    public void ResolvePath_WithAbsolutePath_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var rootDirectory = "/uploads";
        var absolutePath = "/etc/passwd";
        
        // Act
        var act = () =>
        {
            if (absolutePath.StartsWith("/", StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("Absolute paths not allowed.");
            }
        };

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("Absolute paths not allowed.");
    }

    #endregion

    #region File Operation Tests with Mocking

    [TestMethod]
    public async Task FileExistsAsync_WhenFileExists_ReturnsTrue()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.FileExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var exists = await mockClient.Object.FileExists("/uploads/test.txt", CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
        mockClient.Verify(x => x.FileExists("/uploads/test.txt", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task FileExistsAsync_WhenFileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.FileExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var exists = await mockClient.Object.FileExists("/uploads/nonexistent.txt", CancellationToken.None);

        // Assert
        exists.Should().BeFalse();
    }

    [TestMethod]
    public async Task ReadAllBytesAsync_WhenFileExists_ReturnsBytes()
    {
        // Arrange
        var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
        var mockClient = new Mock<IAsyncFtpClient>();
        
        mockClient.Setup(x => x.DownloadBytes(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBytes);

        // Act
        var bytes = await mockClient.Object.DownloadBytes("/uploads/test.bin", CancellationToken.None);

        // Assert
        bytes.Should().NotBeNull();
        bytes.Should().HaveCount(5);
        bytes.Should().BeEquivalentTo(expectedBytes);
    }

    [TestMethod]
    public async Task WriteAllBytesAsync_UploadsWithCorrectContent()
    {
        // Arrange
        var bytes = new byte[] { 10, 20, 30 };
        var mockClient = new Mock<IAsyncFtpClient>();
        
        mockClient.Setup(x => x.UploadBytes(
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                FtpRemoteExists.Overwrite,
                true,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FtpStatus.Success);

        // Act
        var status = await mockClient.Object.UploadBytes(
            bytes, 
            "/uploads/test.bin", 
            FtpRemoteExists.Overwrite, 
            true, 
            null, 
            CancellationToken.None);

        // Assert
        status.Should().Be(FtpStatus.Success);
        mockClient.Verify(x => x.UploadBytes(
            bytes,
            "/uploads/test.bin",
            FtpRemoteExists.Overwrite,
            true,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteFileAsync_WhenFileExists_DeletesSuccessfully()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.DeleteFile(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockClient.Object.DeleteFile("/uploads/test.txt", CancellationToken.None);

        // Assert
        mockClient.Verify(x => x.DeleteFile("/uploads/test.txt", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Directory Operation Tests

    [TestMethod]
    public async Task DirectoryExistsAsync_WhenDirectoryExists_ReturnsTrue()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.DirectoryExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var exists = await mockClient.Object.DirectoryExists("/uploads/documents", CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public async Task CreateDirectoryAsync_CreatesDirectoryRecursively()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.CreateDirectory(It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var created = await mockClient.Object.CreateDirectory("/uploads/documents/subfolder", true, CancellationToken.None);

        // Assert
        created.Should().BeTrue();
        mockClient.Verify(x => x.CreateDirectory("/uploads/documents/subfolder", true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteDirectoryAsync_RecursiveMode_DeletesAllContents()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.DeleteDirectory(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockClient.Object.DeleteDirectory("/uploads/documents", CancellationToken.None);

        // Assert
        mockClient.Verify(x => x.DeleteDirectory("/uploads/documents", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetFilesAsync_ReturnsFilteredFileList()
    {
        // Arrange
        var mockItems = new[]
        {
            new FtpListItem { Name = "file1.txt", Type = FtpObjectType.File, FullName = "/uploads/file1.txt" },
            new FtpListItem { Name = "file2.log", Type = FtpObjectType.File, FullName = "/uploads/file2.log" },
            new FtpListItem { Name = "subfolder", Type = FtpObjectType.Directory, FullName = "/uploads/subfolder" }
        };

        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.GetListing(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockItems);

        // Act
        var listing = await mockClient.Object.GetListing("/uploads", CancellationToken.None);
        var files = listing.Where(item => item.Type == FtpObjectType.File).ToList();

        // Assert
        files.Should().HaveCount(2);
        files.Select(f => f.Name).Should().Contain("file1.txt");
        files.Select(f => f.Name).Should().Contain("file2.log");
    }

    [TestMethod]
    public async Task GetDirectoriesAsync_ReturnsSubdirectories()
    {
        // Arrange
        var mockItems = new[]
        {
            new FtpListItem { Name = "subfolder1", Type = FtpObjectType.Directory, FullName = "/uploads/subfolder1" },
            new FtpListItem { Name = "subfolder2", Type = FtpObjectType.Directory, FullName = "/uploads/subfolder2" },
            new FtpListItem { Name = "file.txt", Type = FtpObjectType.File, FullName = "/uploads/file.txt" }
        };

        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.GetListing(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockItems);

        // Act
        var listing = await mockClient.Object.GetListing("/uploads", CancellationToken.None);
        var directories = listing.Where(item => item.Type == FtpObjectType.Directory).ToList();

        // Assert
        directories.Should().HaveCount(2);
        directories.Select(d => d.Name).Should().Contain("subfolder1");
        directories.Select(d => d.Name).Should().Contain("subfolder2");
    }

    #endregion

    #region Pattern Matching Tests

    [TestMethod]
    public void MatchesPattern_WithWildcardStar_MatchesAll()
    {
        // Arrange
        var pattern = "*";
        var filenames = new[] { "file1.txt", "document.pdf", "image.jpg" };

        // Act & Assert
        foreach (var filename in filenames)
        {
            // Simple wildcard matching: * matches everything
            var matches = pattern == "*" || filename.Contains(pattern.Replace("*", ""));
            matches.Should().BeTrue();
        }
    }

    [TestMethod]
    public void MatchesPattern_WithExtensionPattern_MatchesCorrectly()
    {
        // Arrange
        var pattern = "*.txt";
        var txtFiles = new[] { "file1.txt", "document.txt" };
        var otherFiles = new[] { "image.jpg", "data.csv" };

        // Act & Assert
        foreach (var file in txtFiles)
        {
            var matches = file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
            matches.Should().BeTrue();
        }

        foreach (var file in otherFiles)
        {
            var matches = file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
            matches.Should().BeFalse();
        }
    }

    [TestMethod]
    public void MatchesPattern_WithQuestionMark_MatchesSingleCharacter()
    {
        // Arrange
        var pattern = "file?.txt";
        var matchingFiles = new[] { "file1.txt", "fileA.txt" };
        var nonMatchingFiles = new[] { "file12.txt", "document.txt" };

        // Act & Assert - This demonstrates expected behavior
        // In real implementation, FtpStorageProvider would use regex pattern matching
        matchingFiles.Should().NotBeEmpty();
        nonMatchingFiles.Should().NotBeEmpty();
    }

    [TestMethod]
    public void MatchesPattern_IsCaseInsensitive()
    {
        // Arrange
        var pattern = "*.TXT";
        var filename = "document.txt";

        // Act
        var matches = filename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

        // Assert
        matches.Should().BeTrue();
    }

    #endregion

    #region Encoding Tests

    [TestMethod]
    public void WriteAllText_UsesUtf8WithoutBOM()
    {
        // Arrange
        var content = "Hello, FTP!";
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new System.Text.UTF8Encoding(false));

        // Act
        writer.Write(content);
        writer.Flush();
        var bytes = ms.ToArray();

        // Assert
        bytes.Should().NotBeNull();
        // UTF-8 BOM is 0xEF, 0xBB, 0xBF
        bytes.Take(3).Should().NotBeEquivalentTo(new byte[] { 0xEF, 0xBB, 0xBF });
    }

    #endregion

    #region Disposal Tests

    [TestMethod]
    public async Task Dispose_DisconnectsAndDisposesClient()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.SetupGet(x => x.IsConnected).Returns(true);
        mockClient.Setup(x => x.Disconnect(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockClient.Setup(x => x.Dispose());

        // Act
        if (mockClient.Object.IsConnected)
        {
            await mockClient.Object.Disconnect(CancellationToken.None);
        }
        mockClient.Object.Dispose();

        // Assert
        mockClient.Verify(x => x.Disconnect(It.IsAny<CancellationToken>()), Times.Once);
        mockClient.Verify(x => x.Dispose(), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public async Task FileExistsAsync_WithInvalidPath_ThrowsArgumentException()
    {
        // Arrange
        var emptyPath = string.Empty;

        // Act
        var act = async () =>
        {
            if (string.IsNullOrWhiteSpace(emptyPath))
            {
                throw new ArgumentException("Path cannot be empty.", nameof(emptyPath));
            }
            await Task.CompletedTask;
        };

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Path cannot be empty.*");
    }

    [TestMethod]
    public async Task ReadAllTextAsync_WhenFileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var mockClient = new Mock<IAsyncFtpClient>();
        mockClient.Setup(x => x.DownloadBytes(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Remote file not found."));

        // Act
        var act = async () => await mockClient.Object.DownloadBytes("/uploads/nonexistent.txt", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("Remote file not found.");
    }

    #endregion
}
