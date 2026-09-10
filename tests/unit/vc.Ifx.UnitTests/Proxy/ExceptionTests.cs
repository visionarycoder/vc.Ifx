using System.Net.Sockets;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public class ExceptionTests
{
    #region ProxyException Tests

    [TestMethod]
    public void ProxyException_DefaultConstructor_ShouldCreateException()
    {
        // Act
        var exception = new ProxyException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void ProxyException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        string message = "Test error message";

        // Act
        var exception = new ProxyException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void ProxyException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        string message = "Outer error";
        var inner = new InvalidOperationException("Inner error");

        // Act
        var exception = new ProxyException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    #endregion

    #region RetryException Tests

    [TestMethod]
    public void RetryException_WithAttemptCount_ShouldCreateDefaultMessage()
    {
        // Arrange
        int attemptCount = 3;

        // Act
        var exception = new RetryException(attemptCount);

        // Assert
        exception.Message.Should().Contain("failed after 3 retry attempts");
        exception.AttemptCount.Should().Be(attemptCount);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void RetryException_WithMessageAndAttemptCount_ShouldStoreCustomMessage()
    {
        // Arrange
        string message = "Custom retry failure";
        int attemptCount = 5;

        // Act
        var exception = new RetryException(message, attemptCount);

        // Assert
        exception.Message.Should().Be(message);
        exception.AttemptCount.Should().Be(attemptCount);
    }

    [TestMethod]
    public void RetryException_WithAllParameters_ShouldStoreAll()
    {
        // Arrange
        string message = "Retry failed";
        int attemptCount = 10;
        var inner = new TimeoutException("Inner timeout");

        // Act
        var exception = new RetryException(message, attemptCount, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.AttemptCount.Should().Be(attemptCount);
        exception.InnerException.Should().BeSameAs(inner);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    public void RetryException_WithVariousAttemptCounts_ShouldStore(int attemptCount)
    {
        // Act
        var exception = new RetryException(attemptCount);

        // Assert
        exception.AttemptCount.Should().Be(attemptCount);
    }

    #endregion

    #region TransientProxyException Tests

    [TestMethod]
    public void TransientProxyException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        // Act
        var exception = new TransientProxyException();

        // Assert
        exception.Message.Should().Contain("transient proxy error");
    }

    [TestMethod]
    public void TransientProxyException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        string message = "Temporary network issue";

        // Act
        var exception = new TransientProxyException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [TestMethod]
    public void TransientProxyException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        string message = "Transient error";
        var inner = new IOException("Network timeout");

        // Act
        var exception = new TransientProxyException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    #endregion

    #region BusinessException Tests

    [TestMethod]
    public void BusinessException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        string message = "Business rule violation";

        // Act
        var exception = new BusinessException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void BusinessException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        string message = "Invalid customer state";
        var inner = new ArgumentException("Invalid argument");

        // Act
        var exception = new BusinessException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    #endregion

    #region ProxyCanceledException Tests

    [TestMethod]
    public void ProxyCanceledException_WithMessage_ShouldStoreMessage()
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
    public void ProxyCanceledException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        string message = "Request canceled by user";
        var inner = new OperationCanceledException();

        // Act
        var exception = new ProxyCanceledException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    #endregion

    #region ProxyTimeoutException Tests

    [TestMethod]
    public void ProxyTimeoutException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        // Act
        var exception = new ProxyTimeoutException();

        // Assert
        exception.Message.Should().Contain("timed out");
    }

    [TestMethod]
    public void ProxyTimeoutException_WithTimeSpan_ShouldIncludeTimeoutInMessage()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        var exception = new ProxyTimeoutException(timeout);

        // Assert
        exception.Message.Should().Contain("30");
    }

    [TestMethod]
    public void ProxyTimeoutException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        string message = "Custom timeout message";

        // Act
        var exception = new ProxyTimeoutException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [TestMethod]
    public void ProxyTimeoutException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        string message = "Request timeout";
        var inner = new TimeoutException("Inner timeout");

        // Act
        var exception = new ProxyTimeoutException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    #endregion

    #region RetryableTransportException Tests

    [TestMethod]
    public void RetryableTransportException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        string message = "Network error - retryable";

        // Act
        var exception = new RetryableTransportException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void RetryableTransportException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        string message = "Connection reset";
        var inner = new SocketException();

        // Act
        var exception = new RetryableTransportException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    #endregion

    #region NonRetryableTransportException Tests

    [TestMethod]
    public void NonRetryableTransportException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        string message = "Authentication failed";

        // Act
        var exception = new NonRetryableTransportException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void NonRetryableTransportException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        string message = "Certificate validation failed";
        var inner = new UnauthorizedAccessException();

        // Act
        var exception = new NonRetryableTransportException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void AllExceptions_ShouldInheritFromProxyException()
    {
        // Arrange & Act
        var retryException = new RetryException(1);
        var transientException = new TransientProxyException();
        var businessException = new BusinessException("test");
        var canceledException = new ProxyCanceledException("test");
        var timeoutException = new ProxyTimeoutException();
        var retryableTransport = new RetryableTransportException("test");
        var nonRetryableTransport = new NonRetryableTransportException("test");

        // Assert
        retryException.Should().BeAssignableTo<ProxyException>();
        transientException.Should().BeAssignableTo<ProxyException>();
        businessException.Should().BeAssignableTo<ProxyException>();
        canceledException.Should().BeAssignableTo<ProxyException>();
        timeoutException.Should().BeAssignableTo<ProxyException>();
        retryableTransport.Should().BeAssignableTo<ProxyException>();
        nonRetryableTransport.Should().BeAssignableTo<ProxyException>();
    }

    [TestMethod]
    public void AllExceptions_ShouldInheritFromException()
    {
        // Arrange & Act
        var proxyException = new ProxyException();

        // Assert
        proxyException.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region Unicode and Edge Case Tests

    [TestMethod]
    public void ProxyException_WithUnicodeMessage_ShouldPreserve()
    {
        // Arrange
        string unicodeMessage = "エラーが発生しました: 操作に失敗";

        // Act
        var exception = new ProxyException(unicodeMessage);

        // Assert
        exception.Message.Should().Be(unicodeMessage);
    }

    [TestMethod]
    public void RetryException_WithLargeAttemptCount_ShouldStore()
    {
        // Arrange
        int largeCount = int.MaxValue;

        // Act
        var exception = new RetryException(largeCount);

        // Assert
        exception.AttemptCount.Should().Be(largeCount);
    }

    [TestMethod]
    public void ProxyTimeoutException_WithMaxTimeSpan_ShouldIncludeInMessage()
    {
        // Arrange
        TimeSpan maxTimeout = TimeSpan.MaxValue;

        // Act
        var exception = new ProxyTimeoutException(maxTimeout);

        // Assert
        exception.Message.Should().Contain(maxTimeout.ToString());
    }

    #endregion
}
