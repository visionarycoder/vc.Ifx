using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Logging;

[TestClass]
public class LoggingInterceptorTests
{
    private Mock<ILogger<LoggingInterceptor>> mockLogger = null!;
    private LoggingInterceptor interceptor = null!;

    [TestInitialize]
    public void Setup()
    {
        mockLogger = new Mock<ILogger<LoggingInterceptor>>();
        interceptor = new LoggingInterceptor(mockLogger.Object);
    }

    [TestMethod]
    public void Order_ShouldBe100()
    {
        // Assert
        interceptor.Order.Should().Be(100);
    }

    [TestMethod]
    public async Task InvokeAsync_WithSuccessfulOperation_ShouldLogDebugAndInformation()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "TestOp", CorrelationId = "corr-123" };

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<string>.Success("data"));

        // Act
        ProxyResponse<string> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify debug log at start
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting proxy operation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information log on success
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WithFailedResponse_ShouldLogWarning()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "FailOp", CorrelationId = "corr-456" };
        string errorMessage = "Operation failed";

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<int>.Failure(errorMessage));

        // Act
        ProxyResponse<int> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed with failure")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WithProxyException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "ErrorOp", CorrelationId = "corr-789" };
        var expectedException = new ProxyException("Test proxy error");

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct) =>
            throw expectedException;

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ProxyException>(
            async () => await interceptor.InvokeAsync(context, Next, CancellationToken.None));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed with proxy exception")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WithUnexpectedException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "CrashOp", CorrelationId = "corr-000" };
        var expectedException = new InvalidOperationException("Unexpected error");

        Task<ProxyResponse<bool>> Next(ProxyContext ctx, CancellationToken ct) =>
            throw expectedException;

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await interceptor.InvokeAsync(context, Next, CancellationToken.None));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed with unexpected exception")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WithUnknownOperation_ShouldLogWithUnknownName()
    {
        // Arrange
        var context = new ProxyContext(); // No OperationName

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<string>.Success("data"));

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_WithNoCorrelationId_ShouldLogWithNone()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "TestOp" }; // No CorrelationId

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<int>.Success(42));

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("None")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldReturnResultFromNext()
    {
        // Arrange
        var context = new ProxyContext();
        var expectedData = new { Id = 1, Value = "test" };

        Task<ProxyResponse<object>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<object>.Success(expectedData));

        // Act
        ProxyResponse<object> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(expectedData);
    }

    [TestMethod]
    public async Task InvokeAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<string>.Success("data"));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }
}
