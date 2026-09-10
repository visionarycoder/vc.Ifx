// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;

namespace VisionaryCoder.Framework.Tests.Proxy.Exceptions;

[TestClass]
public sealed class NonRetryableTransportExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Transport error cannot be retried";

        // Act
        var exception = new NonRetryableTransportException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        string message = "Non-retryable transport failure";
        var innerException = new HttpRequestException("Connection refused");

        // Act
        var exception = new NonRetryableTransportException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [TestMethod]
    public void NonRetryableTransportException_ShouldBeProxyException()
    {
        // Arrange & Act
        var exception = new NonRetryableTransportException("Test");

        // Assert
        exception.Should().BeAssignableTo<ProxyException>();
    }

    [TestMethod]
    public void NonRetryableTransportException_ShouldBeException()
    {
        // Arrange & Act
        var exception = new NonRetryableTransportException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [TestMethod]
    public void NonRetryableTransportException_CanBeThrown()
    {
        // Arrange
        string message = "Fatal transport error";

        // Act
        Action act = () => throw new NonRetryableTransportException(message);

        // Assert
        act.Should().Throw<NonRetryableTransportException>()
           .WithMessage(message);
    }

    [TestMethod]
    public void NonRetryableTransportException_CanBeCaughtAsProxyException()
    {
        // Arrange
        string message = "Permanent transport failure";

        // Act
        Action act = () => throw new NonRetryableTransportException(message);

        // Assert
        act.Should().Throw<ProxyException>()
           .Which.Should().BeOfType<NonRetryableTransportException>()
           .And.Subject.As<NonRetryableTransportException>().Message.Should().Be(message);
    }

    [TestMethod]
    public void Constructor_WithHttpRequestException_ShouldPreserveInnerException()
    {
        // Arrange
        var innerException = new HttpRequestException("404 Not Found");
        string message = "Resource not found and cannot be retried";

        // Act
        var exception = new NonRetryableTransportException(message, innerException);

        // Assert
        exception.InnerException.Should().BeSameAs(innerException);
        exception.InnerException.Should().BeOfType<HttpRequestException>();
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_ShouldWork()
    {
        // Arrange & Act
        var exception = new NonRetryableTransportException(string.Empty);

        // Assert
        exception.Message.Should().Be(string.Empty);
    }

    [TestMethod]
    public void NonRetryableTransportException_ShouldIndicateNoRetry()
    {
        // Arrange
        string message = "Client authentication failed - do not retry";

        // Act
        var exception = new NonRetryableTransportException(message);

        // Assert
        exception.Message.Should().Contain("not retry", "exception name implies non-retryable semantics");
        exception.Should().BeOfType<NonRetryableTransportException>();
    }
}
