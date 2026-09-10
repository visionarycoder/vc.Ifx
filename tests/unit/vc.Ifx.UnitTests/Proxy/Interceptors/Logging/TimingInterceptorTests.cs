using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Logging;

[TestClass]
public class TimingInterceptorTests
{
    private Mock<ILogger<TimingInterceptor>> mockLogger = null!;
    private TimingInterceptor interceptor = null!;

    [TestInitialize]
    public void Setup()
    {
        mockLogger = new Mock<ILogger<TimingInterceptor>>();
        interceptor = new TimingInterceptor(mockLogger.Object);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldMeasureExecutionTime()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "TestOperation" };
        bool wasCalled = false;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            wasCalled = true;
            Thread.Sleep(10); // Small delay to ensure measurable time
            return Task.FromResult(ProxyResponse<string>.Success("data"));
        }

        // Act
        ProxyResponse<string> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        context.Metadata.Should().ContainKey("ExecutionTimeMs");
        context.Metadata["ExecutionTimeMs"].Should().BeOfType<long>();
        ((long)(context.Metadata["ExecutionTimeMs"] ?? -1)).Should().BeGreaterThanOrEqualTo(0);
    }

    [TestMethod]
    public async Task InvokeAsync_WithFastOperation_ShouldLogDebug()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "FastOp", CorrelationId = "corr-123" };

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<int>.Success(42));

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert - Should log debug (not warning) for fast operations
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("FastOp")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_WithSlowOperation_ShouldLogWarning()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "SlowOp", CorrelationId = "corr-456" };

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            Thread.Sleep(1100); // More than 1 second threshold
            return Task.FromResult(ProxyResponse<string>.Success("data"));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert - Should log warning for slow operations (>1000ms)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow proxy operation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "FailingOp", CorrelationId = "corr-789" };
        var expectedException = new InvalidOperationException("Test failure");

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct) =>
            throw expectedException;

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await interceptor.InvokeAsync(context, Next, CancellationToken.None));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WithUnknownOperation_ShouldUseDefaultName()
    {
        // Arrange
        var context = new ProxyContext(); // No OperationName set

        Task<ProxyResponse<bool>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<bool>.Success(true));

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert - Should use "Unknown" as operation name
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
    public async Task InvokeAsync_WithNoCorrelationId_ShouldUseNone()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "TestOp" };

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<string>.Success("data"));

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert - Should use "None" for correlation ID
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
        var expectedData = new { Id = 1, Name = "Test" };

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

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct)
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<int>.Success(100));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }
}
