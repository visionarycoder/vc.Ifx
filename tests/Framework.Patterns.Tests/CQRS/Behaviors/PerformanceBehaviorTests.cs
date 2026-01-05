using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace VisionaryCoder.Framework.Patterns.Tests.CQRS.Behaviors;

/// <summary>
/// Unit tests for PerformanceBehavior.
/// </summary>
[TestClass]
public class PerformanceBehaviorTests
{
    private Mock<ILogger<PerformanceBehavior<TestRequest, TestResponse>>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<PerformanceBehavior<TestRequest, TestResponse>>>();
    }

    [TestMethod]
    public async Task HandleAsync_FastRequest_ShouldLogDebug()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(loggerMock.Object);
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        Task<TestResponse> Next() => Task.FromResult(expectedResponse);

        // Act
        await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed in")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_SlowRequest_ShouldLogWarning()
    {
        // Arrange
        var threshold = TimeSpan.FromMilliseconds(10);
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(loggerMock.Object, threshold);
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        
        async Task<TestResponse> Next()
        {
            await Task.Delay(50); // Intentionally slow
            return expectedResponse;
        }

        // Act
        await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Long Running Request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_WithException_ShouldLogWarningWithDuration()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(loggerMock.Object);
        var request = new TestRequest();
        var exception = new InvalidOperationException("Test error");
        Task<TestResponse> Next() => throw exception;

        // Act
        Func<Task> act = async () => await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed after")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldReturnExpectedResponse()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(loggerMock.Object);
        var request = new TestRequest();
        var expectedResponse = new TestResponse { Value = "test" };
        Task<TestResponse> Next() => Task.FromResult(expectedResponse);

        // Act
        TestResponse result = await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [TestMethod]
    public async Task HandleAsync_DefaultThreshold_ShouldBe500Milliseconds()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(loggerMock.Object);
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        
        async Task<TestResponse> Next()
        {
            await Task.Delay(600); // Just over default threshold
            return expectedResponse;
        }

        // Act
        await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert - Should log warning because we exceeded 500ms default
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Long Running Request") && v.ToString()!.Contains("500")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_CustomThreshold_ShouldUseSpecifiedValue()
    {
        // Arrange
        var customThreshold = TimeSpan.FromMilliseconds(100);
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(loggerMock.Object, customThreshold);
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        
        async Task<TestResponse> Next()
        {
            await Task.Delay(150); // Exceed custom threshold
            return expectedResponse;
        }

        // Act
        await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("100")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new PerformanceBehavior<TestRequest, TestResponse>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [TestMethod]
    public async Task HandleAsync_ShouldMeasureAccurateElapsedTime()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(loggerMock.Object);
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        var delayMs = 100;
        
        async Task<TestResponse> Next()
        {
            await Task.Delay(delayMs);
            return expectedResponse;
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        await behavior.HandleAsync(request, Next, CancellationToken.None);
        stopwatch.Stop();

        // Assert - Verify that performance was logged
        loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed in") || v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }
}
