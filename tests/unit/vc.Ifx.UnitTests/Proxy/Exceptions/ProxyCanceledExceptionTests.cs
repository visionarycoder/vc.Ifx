// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Tests.Proxy.Exceptions;

[TestClass]
public sealed class ProxyCanceledExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Operation was canceled";

        // Act
        var exception = new ProxyCanceledException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        string message = "Proxy operation canceled";
        var innerException = new OperationCanceledException("Inner cancellation");

        // Act
        var exception = new ProxyCanceledException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [TestMethod]
    public void ProxyCanceledException_ShouldBeProxyException()
    {
        // Arrange & Act
        var exception = new ProxyCanceledException("Test");

        // Assert
        exception.Should().BeAssignableTo<ProxyException>();
    }

    [TestMethod]
    public void ProxyCanceledException_ShouldBeException()
    {
        // Arrange & Act
        var exception = new ProxyCanceledException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [TestMethod]
    public void ProxyCanceledException_CanBeThrown()
    {
        // Arrange
        string message = "Request was canceled by user";

        // Act
        Action act = () => throw new ProxyCanceledException(message);

        // Assert
        act.Should().Throw<ProxyCanceledException>()
           .WithMessage(message);
    }

    [TestMethod]
    public void ProxyCanceledException_CanBeCaughtAsProxyException()
    {
        // Arrange
        string message = "Cancellation occurred";

        // Act
        Action act = () => throw new ProxyCanceledException(message);

        // Assert
        act.Should().Throw<ProxyException>()
           .Which.Should().BeOfType<ProxyCanceledException>()
           .And.Subject.As<ProxyCanceledException>().Message.Should().Be(message);
    }

    [TestMethod]
    public void Constructor_WithCancellationTokenException_ShouldPreserveInnerException()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);
        var innerException = new OperationCanceledException(cancellationToken);
        string message = "Proxy canceled via token";

        // Act
        var exception = new ProxyCanceledException(message, innerException);

        // Assert
        exception.InnerException.Should().BeSameAs(innerException);
        exception.InnerException.Should().BeOfType<OperationCanceledException>();
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_ShouldWork()
    {
        // Arrange & Act
        var exception = new ProxyCanceledException(string.Empty);

        // Assert
        exception.Message.Should().Be(string.Empty);
    }
}
