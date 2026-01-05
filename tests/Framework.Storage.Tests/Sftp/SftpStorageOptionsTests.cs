using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionaryCoder.Framework.Storage.Sftp;

namespace VisionaryCoder.Framework.Storage.Tests.Sftp;

[TestClass]
public class SftpStorageOptionsTests
{
    [TestMethod]
    public void Validate_WithNullHost_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = null!,
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Host*");
    }

    [TestMethod]
    public void Validate_WithEmptyHost_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "",
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Host*");
    }

    [TestMethod]
    public void Validate_WithNullUsername_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = null!,
            Password = "testpass"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Username*");
    }

    [TestMethod]
    public void Validate_WithPortTooLow_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Port = 0,
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*between 1 and 65535*");
    }

    [TestMethod]
    public void Validate_WithPortTooHigh_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Port = 65536,
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*between 1 and 65535*");
    }

    [TestMethod]
    public void Validate_WithNeitherPasswordNorPrivateKey_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser"
            // Neither Password nor PrivateKeyPath is set
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password*PrivateKeyPath*");
    }

    [TestMethod]
    public void Validate_WithNegativeConnectTimeout_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser",
            Password = "testpass",
            ConnectTimeout = -1
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ConnectTimeout*");
    }

    [TestMethod]
    public void Validate_WithNegativeOperationTimeout_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser",
            Password = "testpass",
            OperationTimeout = -1
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*OperationTimeout*");
    }

    [TestMethod]
    public void Validate_WithNegativeKeepAliveInterval_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser",
            Password = "testpass",
            KeepAliveInterval = -1
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*KeepAliveInterval*");
    }

    [TestMethod]
    public void Validate_WithNegativeRetryAttempts_ThrowsArgumentException()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser",
            Password = "testpass",
            RetryAttempts = -1
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*RetryAttempts*");
    }

    [TestMethod]
    public void Validate_WithPasswordAuth_DoesNotThrow()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Port = 22,
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Validate_WithPrivateKeyAuth_DoesNotThrow()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Port = 22,
            Username = "testuser",
            PrivateKeyPath = "/path/to/key"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Validate_WithPrivateKeyAuthAndPassphrase_DoesNotThrow()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Port = 22,
            Username = "testuser",
            PrivateKeyPath = "/path/to/key",
            PrivateKeyPassphrase = "passphrase"
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
        var options = new SftpStorageOptions();

        // Assert
        options.Port.Should().Be(22);
        options.ConnectTimeout.Should().Be(30);
        options.OperationTimeout.Should().Be(60);
        options.KeepAliveInterval.Should().Be(10);
        options.RetryAttempts.Should().Be(3);
        options.BufferSize.Should().Be(32768);
    }

    [TestMethod]
    public void RootDirectory_NormalizesPath()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser",
            Password = "testpass",
            RootDirectory = "data/uploads" // No leading slash
        };

        // Act
        options.Validate();

        // Assert
        options.RootDirectory.Should().Be("/data/uploads");
    }

    [TestMethod]
    public void RootDirectory_RemovesTrailingSlash()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser",
            Password = "testpass",
            RootDirectory = "/data/uploads/" // Trailing slash
        };

        // Act
        options.Validate();

        // Assert
        options.RootDirectory.Should().Be("/data/uploads");
    }

    [TestMethod]
    public void RootDirectory_DefaultsToRoot()
    {
        // Arrange
        var options = new SftpStorageOptions
        {
            Host = "sftp.example.com",
            Username = "testuser",
            Password = "testpass"
            // RootDirectory not set
        };

        // Act
        options.Validate();

        // Assert
        options.RootDirectory.Should().Be("/");
    }
}
