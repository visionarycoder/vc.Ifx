using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace VisionaryCoder.Framework.Patterns.Tests.CQRS.Behaviors;

/// <summary>
/// Unit tests for LoggingBehavior.
/// </summary>
[TestClass]
public class LoggingBehaviorTests
{
    private Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>> loggerMock = null!;
    private LoggingBehavior<TestRequest, TestResponse> behavior = null!;

    [TestInitialize]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
        behavior = new LoggingBehavior<TestRequest, TestResponse>(loggerMock.Object);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldLogBeforeExecution()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        Task<TestResponse> Next() => Task.FromResult(expectedResponse);

        // Act
        await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldLogAfterSuccessfulExecution()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        Task<TestResponse> Next() => Task.FromResult(expectedResponse);

        // Act
        await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully handled TestRequest")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var request = new TestRequest();
        var exception = new InvalidOperationException("Test error");
        Task<TestResponse> Next() => throw exception;

        // Act
        Func<Task> act = async () => await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling TestRequest")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldReturnExpectedResponse()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResponse = new TestResponse { Value = "test" };
        Task<TestResponse> Next() => Task.FromResult(expectedResponse);

        // Act
        TestResponse result = await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new LoggingBehavior<TestRequest, TestResponse>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
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
