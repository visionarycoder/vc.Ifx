// Copyright (c) VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Storage.Azure.Blob;

namespace VisionaryCoder.Framework.Storage.Tests.Azure.Blob;

/// <summary>
/// Tests for AzureBlobStorageProvider using injected mock clients.
/// Demonstrates the new constructor overload that enables full mocking.
/// </summary>
[TestClass]
public class AzureBlobStorageProviderWithMockTests
{
    private Mock<BlobContainerClient> mockContainerClient = null!;
    private Mock<ILogger<AzureBlobStorageProvider>> mockLogger = null!;
    private AzureBlobStorageOptions options = null!;
    private AzureBlobStorageProvider provider = null!;

    [TestInitialize]
    public void Setup()
    {
        mockContainerClient = new Mock<BlobContainerClient>();
        mockLogger = new Mock<ILogger<AzureBlobStorageProvider>>();
        options = new AzureBlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "test-container"
        };

        // Use the new constructor overload with injected client
        provider = new AzureBlobStorageProvider(options, mockContainerClient.Object, mockLogger.Object);
    }

    [TestMethod]
    public void FileExists_WithMockedClient_ShouldCallBlobClientExists()
    {
        // Arrange
        string blobName = "test.txt";
        Mock<BlobClient> mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.Exists(default))
            .Returns(Response.FromValue(true, Mock.Of<Response>()));

        mockContainerClient.Setup(x => x.GetBlobClient(blobName))
            .Returns(mockBlobClient.Object);

        // Act
        bool result = provider.FileExists(blobName);

        // Assert
        result.Should().BeTrue();
        mockContainerClient.Verify(x => x.GetBlobClient(blobName), Times.Once);
        mockBlobClient.Verify(x => x.Exists(default), Times.Once);
    }

    [TestMethod]
    public void FileExists_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        string blobName = "nonexistent.txt";
        Mock<BlobClient> mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.Exists(default))
            .Returns(Response.FromValue(false, Mock.Of<Response>()));

        mockContainerClient.Setup(x => x.GetBlobClient(blobName))
            .Returns(mockBlobClient.Object);

        // Act
        bool result = provider.FileExists(blobName);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void DeleteFile_WithMockedClient_ShouldCallBlobClientDelete()
    {
        // Arrange
        string blobName = "test.txt";
        Mock<BlobClient> mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.DeleteIfExists(DeleteSnapshotsOption.None, null, default))
            .Returns(Response.FromValue(true, Mock.Of<Response>()));

        mockContainerClient.Setup(x => x.GetBlobClient(blobName))
            .Returns(mockBlobClient.Object);

        // Act
        provider.DeleteFile(blobName);

        // Assert
        mockContainerClient.Verify(x => x.GetBlobClient(blobName), Times.Once);
        mockBlobClient.Verify(x => x.DeleteIfExists(DeleteSnapshotsOption.None, null, default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteFileAsync_WithMockedClient_ShouldCallBlobClientDeleteAsync()
    {
        // Arrange
        string blobName = "test.txt";
        Mock<BlobClient> mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, default))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        mockContainerClient.Setup(x => x.GetBlobClient(blobName))
            .Returns(mockBlobClient.Object);

        // Act
        await provider.DeleteFileAsync(blobName);

        // Assert
        mockContainerClient.Verify(x => x.GetBlobClient(blobName), Times.Once);
        mockBlobClient.Verify(x => x.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, default), Times.Once);
    }

    [TestMethod]
    public void Constructor_WithInjectedClient_ShouldNotThrow()
    {
        // Arrange & Act
        Action act = () => new AzureBlobStorageProvider(
            options,
            mockContainerClient.Object,
            mockLogger.Object);

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new AzureBlobStorageProvider(
            null!,
            mockContainerClient.Object,
            mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [TestMethod]
    public void Constructor_WithNullContainerClient_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new AzureBlobStorageProvider(
            options,
            null!,
            mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("containerClient");
    }
}
