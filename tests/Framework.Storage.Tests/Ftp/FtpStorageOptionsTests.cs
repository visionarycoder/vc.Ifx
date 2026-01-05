using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionaryCoder.Framework.Storage.Ftp;

namespace VisionaryCoder.Framework.Storage.Tests.Ftp;

[TestClass]
public class FtpStorageOptionsTests
{
    [TestMethod]
    public void Validate_WithNullHost_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = null!,
            Username = "testuser"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Host*");
    }

    [TestMethod]
    public void Validate_WithEmptyHost_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "",
            Username = "testuser"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Host*");
    }

    [TestMethod]
    public void Validate_WithNullUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Username = null!
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Validate_WithPortTooLow_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Port = 0,
            Username = "testuser"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*between 1 and 65535*");
    }

    [TestMethod]
    public void Validate_WithPortTooHigh_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Port = 65536,
            Username = "testuser"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*between 1 and 65535*");
    }

    [TestMethod]
    public void Validate_WithNegativeConnectTimeout_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Username = "testuser",
            ConnectTimeout = -1
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*timeout*");
    }

    [TestMethod]
    public void Validate_WithNegativeReadTimeout_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Username = "testuser",
            ReadTimeout = -1
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*timeout*");
    }

    [TestMethod]
    public void Validate_WithNegativeDataConnectionTimeout_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Username = "testuser",
            DataConnectionTimeout = -1
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Data connection timeout*");
    }

    [TestMethod]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        // Arrange
        var options = new FtpStorageOptions
        {
            Host = "ftp.example.com",
            Port = 21,
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var options = new FtpStorageOptions();

        // Assert
        options.Port.Should().Be(21);
        options.ConnectTimeout.Should().Be(30);
        options.ReadTimeout.Should().Be(60);
        options.DataConnectionTimeout.Should().Be(60);
        options.RetryAttempts.Should().Be(3);
        options.ValidateCertificate.Should().BeTrue();
    }
}
