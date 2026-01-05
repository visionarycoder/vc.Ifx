using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VisionaryCoder.Framework.Storage.Abstractions.Tests;

[TestClass]
public class StorageExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        const string message = "Test error message";

        // Act
        var exception = new StorageException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Path.Should().BeNull();
        exception.ProviderType.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        // Arrange
        const string message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new StorageException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.Path.Should().BeNull();
        exception.ProviderType.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithInitializer_SetsPathAndProviderType()
    {
        // Arrange
        const string message = "Test error message";
        const string path = "/test/path";
        const string providerType = "LocalFile";

        // Act
        var exception = new StorageException(message)
        {
            Path = path,
            ProviderType = providerType
        };

        // Assert
        exception.Message.Should().Be(message);
        exception.Path.Should().Be(path);
        exception.ProviderType.Should().Be(providerType);
    }

    [TestMethod]
    public void Constructor_WithInnerExceptionAndInitializer_SetsAllProperties()
    {
        // Arrange
        const string message = "Test error message";
        const string path = "/test/path";
        const string providerType = "LocalFile";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new StorageException(message, innerException)
        {
            Path = path,
            ProviderType = providerType
        };

        // Assert
        exception.Message.Should().Be(message);
        exception.Path.Should().Be(path);
        exception.ProviderType.Should().Be(providerType);
        exception.InnerException.Should().Be(innerException);
    }

    [TestMethod]
    public void Path_WithoutInitializer_IsNull()
    {
        // Arrange
        var exception = new StorageException("Test");

        // Assert
        exception.Path.Should().BeNull();
    }

    [TestMethod]
    public void ProviderType_WithoutInitializer_IsNull()
    {
        // Arrange
        var exception = new StorageException("Test");

        // Assert
        exception.ProviderType.Should().BeNull();
    }
}
