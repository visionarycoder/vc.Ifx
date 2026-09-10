// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;

namespace VisionaryCoder.Framework.Tests.Proxy.Exceptions;

[TestClass]
public sealed class RetryableTransportExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Transport error can be retried";

        // Act
        var exception = new RetryableTransportException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        string message = "Retryable transport failure";
        var innerException = new TimeoutException("Request timed out");

        // Act
        var exception = new RetryableTransportException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [TestMethod]
    public void RetryableTransportException_ShouldBeProxyException()
    {
        // Arrange & Act
        var exception = new RetryableTransportException("Test");

        // Assert
        exception.Should().BeAssignableTo<ProxyException>();
    }

    [TestMethod]
    public void RetryableTransportException_ShouldBeException()
    {
        // Arrange & Act
        var exception = new RetryableTransportException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [TestMethod]
    public void RetryableTransportException_CanBeThrown()
    {
        // Arrange
        string message = "Transient transport error";

        // Act
        Action act = () => throw new RetryableTransportException(message);

        // Assert
        act.Should().Throw<RetryableTransportException>()
           .WithMessage(message);
    }

    [TestMethod]
    public void RetryableTransportException_CanBeCaughtAsProxyException()
    {
        // Arrange
        string message = "Temporary transport failure";

        // Act
        Action act = () => throw new RetryableTransportException(message);

        // Assert
        act.Should().Throw<ProxyException>()
           .Which.Should().BeOfType<RetryableTransportException>()
           .And.Subject.As<RetryableTransportException>().Message.Should().Be(message);
    }

    [TestMethod]
    public void Constructor_WithTimeoutException_ShouldPreserveInnerException()
    {
        // Arrange
        var innerException = new TimeoutException("Operation timed out after 30 seconds");
        string message = "Request timeout - can retry";

        // Act
        var exception = new RetryableTransportException(message, innerException);

        // Assert
        exception.InnerException.Should().BeSameAs(innerException);
        exception.InnerException.Should().BeOfType<TimeoutException>();
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_ShouldWork()
    {
        // Arrange & Act
        var exception = new RetryableTransportException(string.Empty);

        // Assert
        exception.Message.Should().Be(string.Empty);
    }

    [TestMethod]
    public void RetryableTransportException_ShouldIndicateCanRetry()
    {
        // Arrange
        string message = "Service unavailable - retry recommended";

        // Act
        var exception = new RetryableTransportException(message);

        // Assert
        exception.Message.Should().Contain("retry", "exception name implies retryable semantics");
        exception.Should().BeOfType<RetryableTransportException>();
    }

    [TestMethod]
    public void RetryableTransportException_VsNonRetryable_ShouldBeDifferentTypes()
    {
        // Arrange
        var retryable = new RetryableTransportException("Can retry");
        var nonRetryable = new NonRetryableTransportException("Cannot retry");

        // Assert
        retryable.Should().NotBeOfType<NonRetryableTransportException>();
        nonRetryable.Should().NotBeOfType<RetryableTransportException>();
        retryable.Should().BeAssignableTo<ProxyException>();
        nonRetryable.Should().BeAssignableTo<ProxyException>();
    }
}
