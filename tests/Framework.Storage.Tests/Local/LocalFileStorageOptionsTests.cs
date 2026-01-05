using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionaryCoder.Framework.Storage.Local;

namespace VisionaryCoder.Framework.Storage.Tests.Local;

[TestClass]
public class LocalFileStorageOptionsTests
{
    [TestMethod]
    public void Validate_WithNullRootDirectory_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new LocalFileStorageOptions { RootDirectory = null! };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*RootDirectory*");
    }

    [TestMethod]
    public void Validate_WithEmptyRootDirectory_ThrowsArgumentException()
    {
        // Arrange
        var options = new LocalFileStorageOptions { RootDirectory = "" };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*RootDirectory*");
    }

    [TestMethod]
    public void Validate_WithValidRootDirectory_DoesNotThrow()
    {
        // Arrange
        var options = new LocalFileStorageOptions { RootDirectory = "C:\\Temp" };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var options = new LocalFileStorageOptions { RootDirectory = "C:\\Temp" };

        // Assert
        options.CreateRootIfNotExists.Should().BeTrue();
        options.RestrictToRootDirectory.Should().BeTrue();
        options.BufferSize.Should().Be(81920);
        options.FileOptions.Should().Be(FileOptions.Asynchronous);
    }
}
