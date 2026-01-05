using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionaryCoder.Framework.Storage.Azure.Blob;

namespace VisionaryCoder.Framework.Storage.Tests.Azure.Blob;

[TestClass]
public class AzureBlobStorageOptionsTests
{
    [TestMethod]
    public void Validate_WithNullStorageAccountUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "test-container",
            StorageAccountUri = null!
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*ConnectionString*");
    }

    [TestMethod]
    public void Validate_WithNullContainerName_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = null!,
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*ContainerName*");
    }

    [TestMethod]
    public void Validate_WithEmptyContainerName_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "",
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ContainerName*");
    }

    [TestMethod]
    public void Validate_WithInvalidContainerName_TooShort_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "ab", // Less than 3 characters
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*3-63 characters*");
    }

    [TestMethod]
    public void Validate_WithInvalidContainerName_TooLong_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = new string('a', 64), // More than 63 characters
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*3-63 characters*");
    }

    [TestMethod]
    public void Validate_WithInvalidContainerName_UpperCase_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "TestContainer", // Contains uppercase letters
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*lowercase*");
    }

    [TestMethod]
    public void Validate_WithInvalidContainerName_ConsecutiveHyphens_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "test--container", // Contains consecutive hyphens
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*hyphen*");
    }

    [TestMethod]
    public void Validate_WithInvalidContainerName_StartsWithHyphen_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "-test-container", // Starts with hyphen
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*start*end*hyphen*");
    }

    [TestMethod]
    public void Validate_WithInvalidContainerName_EndsWithHyphen_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "test-container-", // Ends with hyphen
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*start*end*hyphen*");
    }

    [TestMethod]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "test-container",
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Validate_WithValidContainerName_WithNumbers_DoesNotThrow()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "test123container",
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Validate_WithValidContainerName_WithSingleHyphens_DoesNotThrow()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ContainerName = "test-container-name",
            StorageAccountUri = "https://test.blob.core.windows.net",
            UseManagedIdentity = true
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
