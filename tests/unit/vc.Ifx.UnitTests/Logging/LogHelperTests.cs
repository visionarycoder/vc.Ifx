using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Logging;

namespace VisionaryCoder.Framework.Tests.Logging;

/// <summary>
/// Data-driven unit tests for the <see cref="LogHelper"/> class.
/// Tests all synchronous and asynchronous logging methods with various scenarios.
/// </summary>
[TestClass]
public class LogHelperTests
{
    private Mock<ILogger> mockLogger = null!;

    [TestInitialize]
    public void Initialize()
    {
        mockLogger = new Mock<ILogger>();
    }

    #region Synchronous Logging Tests

    [TestMethod]
    public void LogTraceMessage_WithMessageOnly_ShouldLogTrace()
    {
        // Arrange
        const string message = "Trace message";

        // Act
        LogHelper.LogTraceMessage(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogTraceMessage_WithMessageAndException_ShouldLogTraceWithException()
    {
        // Arrange
        const string message = "Trace with exception";
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogHelper.LogTraceMessage(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogDebugMessage_WithMessageOnly_ShouldLogDebug()
    {
        // Arrange
        const string message = "Debug message";

        // Act
        LogHelper.LogDebugMessage(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogDebugMessage_WithMessageAndException_ShouldLogDebugWithException()
    {
        // Arrange
        const string message = "Debug with exception";
        var exception = new ArgumentException("Test exception");

        // Act
        LogHelper.LogDebugMessage(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogInformationMessage_WithMessageOnly_ShouldLogInformation()
    {
        // Arrange
        const string message = "Information message";

        // Act
        LogHelper.LogInformationMessage(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogInformationMessage_WithMessageAndException_ShouldLogInformationWithException()
    {
        // Arrange
        const string message = "Information with exception";
        var exception = new Exception("Test exception");

        // Act
        LogHelper.LogInformationMessage(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogWarningMessage_WithMessageOnly_ShouldLogWarning()
    {
        // Arrange
        const string message = "Warning message";

        // Act
        LogHelper.LogWarningMessage(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogWarningMessage_WithMessageAndException_ShouldLogWarningWithException()
    {
        // Arrange
        const string message = "Warning with exception";
        var exception = new TimeoutException("Test exception");

        // Act
        LogHelper.LogWarningMessage(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogErrorMessage_WithMessageOnly_ShouldLogError()
    {
        // Arrange
        const string message = "Error message";

        // Act
        LogHelper.LogErrorMessage(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogErrorMessage_WithMessageAndException_ShouldLogErrorWithException()
    {
        // Arrange
        const string message = "Error with exception";
        var exception = new IOException("Test exception");

        // Act
        LogHelper.LogErrorMessage(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogCriticalMessage_WithMessageOnly_ShouldLogCritical()
    {
        // Arrange
        const string message = "Critical message";

        // Act
        LogHelper.LogCriticalMessage(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void LogCriticalMessage_WithMessageAndException_ShouldLogCriticalWithException()
    {
        // Arrange
        const string message = "Critical with exception";
        var exception = new OutOfMemoryException("Test exception");

        // Act
        LogHelper.LogCriticalMessage(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Log Method with LogLevel Parameter Tests

    [TestMethod]
    [DataRow(LogLevel.Trace, "Trace message")]
    [DataRow(LogLevel.Debug, "Debug message")]
    [DataRow(LogLevel.Information, "Information message")]
    [DataRow(LogLevel.Warning, "Warning message")]
    [DataRow(LogLevel.Error, "Error message")]
    [DataRow(LogLevel.Critical, "Critical message")]
    public void Log_WithValidLogLevel_ShouldLogAtCorrectLevel(LogLevel logLevel, string message)
    {
        // Act
        LogHelper.Log(mockLogger.Object, message, logLevel);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void Log_WithDefaultLogLevel_ShouldLogAtDebugLevel()
    {
        // Arrange
        const string message = "Default level message";

        // Act
        LogHelper.Log(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void Log_WithException_ShouldLogWithException()
    {
        // Arrange
        const string message = "Message with exception";
        var exception = new ApplicationException("Test exception");

        // Act
        LogHelper.Log(mockLogger.Object, message, LogLevel.Error, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void Log_WithInvalidLogLevel_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        const string message = "Invalid log level";
        const LogLevel invalidLogLevel = (LogLevel)999;

        // Act
        Action act = () => LogHelper.Log(mockLogger.Object, message, invalidLogLevel);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("logLevel")
            .WithMessage("*Invalid log level*");
    }

    #endregion

    #region Asynchronous Logging Tests

    [TestMethod]
    public async Task LogTraceMessageAsync_WithMessageOnly_ShouldLogTrace()
    {
        // Arrange
        const string message = "Async trace message";

        // Act
        await LogHelper.LogTraceMessageAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogTraceMessageAsync_WithMessageAndException_ShouldLogTraceWithException()
    {
        // Arrange
        const string message = "Async trace with exception";
        var exception = new InvalidOperationException("Test exception");

        // Act
        await LogHelper.LogTraceMessageAsync(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogTraceMessageAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        const string message = "Async trace message";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await LogHelper.LogTraceMessageAsync(mockLogger.Object, message, null, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task LogDebugMessageAsync_WithMessageOnly_ShouldLogDebug()
    {
        // Arrange
        const string message = "Async debug message";

        // Act
        await LogHelper.LogDebugMessageAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogDebugMessageAsync_WithMessageAndException_ShouldLogDebugWithException()
    {
        // Arrange
        const string message = "Async debug with exception";
        var exception = new ArgumentException("Test exception");

        // Act
        await LogHelper.LogDebugMessageAsync(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogInformationMessageAsync_WithMessageOnly_ShouldLogInformation()
    {
        // Arrange
        const string message = "Async information message";

        // Act
        await LogHelper.LogInformationMessageAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogInformationMessageAsync_WithMessageAndException_ShouldLogInformationWithException()
    {
        // Arrange
        const string message = "Async information with exception";
        var exception = new Exception("Test exception");

        // Act
        await LogHelper.LogInformationMessageAsync(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogWarningMessageAsync_WithMessageOnly_ShouldLogWarning()
    {
        // Arrange
        const string message = "Async warning message";

        // Act
        await LogHelper.LogWarningMessageAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogWarningMessageAsync_WithMessageAndException_ShouldLogWarningWithException()
    {
        // Arrange
        const string message = "Async warning with exception";
        var exception = new TimeoutException("Test exception");

        // Act
        await LogHelper.LogWarningMessageAsync(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogErrorMessageAsync_WithMessageOnly_ShouldLogError()
    {
        // Arrange
        const string message = "Async error message";

        // Act
        await LogHelper.LogErrorMessageAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogErrorMessageAsync_WithMessageAndException_ShouldLogErrorWithException()
    {
        // Arrange
        const string message = "Async error with exception";
        var exception = new IOException("Test exception");

        // Act
        await LogHelper.LogErrorMessageAsync(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogCriticalMessageAsync_WithMessageOnly_ShouldLogCritical()
    {
        // Arrange
        const string message = "Async critical message";

        // Act
        await LogHelper.LogCriticalMessageAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogCriticalMessageAsync_WithMessageAndException_ShouldLogCriticalWithException()
    {
        // Arrange
        const string message = "Async critical with exception";
        var exception = new OutOfMemoryException("Test exception");

        // Act
        await LogHelper.LogCriticalMessageAsync(mockLogger.Object, message, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region LogAsync Method Tests

    [TestMethod]
    [DataRow(LogLevel.Trace, "Async trace message")]
    [DataRow(LogLevel.Debug, "Async debug message")]
    [DataRow(LogLevel.Information, "Async information message")]
    [DataRow(LogLevel.Warning, "Async warning message")]
    [DataRow(LogLevel.Error, "Async error message")]
    [DataRow(LogLevel.Critical, "Async critical message")]
    public async Task LogAsync_WithValidLogLevel_ShouldLogAtCorrectLevel(LogLevel logLevel, string message)
    {
        // Act
        await LogHelper.LogAsync(mockLogger.Object, message, logLevel);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogAsync_WithDefaultLogLevel_ShouldLogAtDebugLevel()
    {
        // Arrange
        const string message = "Async default level message";

        // Act
        await LogHelper.LogAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogAsync_WithException_ShouldLogWithException()
    {
        // Arrange
        const string message = "Async message with exception";
        var exception = new ApplicationException("Test exception");

        // Act
        await LogHelper.LogAsync(mockLogger.Object, message, LogLevel.Error, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        const string message = "Async message";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await LogHelper.LogAsync(mockLogger.Object, message, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task LogAsync_WithInvalidLogLevel_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        const string message = "Invalid log level";
        const LogLevel invalidLogLevel = (LogLevel)999;

        // Act
        Func<Task> act = async () => await LogHelper.LogAsync(mockLogger.Object, message, invalidLogLevel);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("logLevel")
            .WithMessage("*Invalid log level*");
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void LogInformationMessage_WithEmptyMessage_ShouldStillLog()
    {
        // Arrange
        const string message = "";

        // Act
        LogHelper.LogInformationMessage(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogInformationMessageAsync_WithEmptyMessage_ShouldStillLog()
    {
        // Arrange
        const string message = "";

        // Act
        await LogHelper.LogInformationMessageAsync(mockLogger.Object, message);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void Log_WithNullException_ShouldLogWithoutException()
    {
        // Arrange
        const string message = "Message without exception";

        // Act
        LogHelper.Log(mockLogger.Object, message, LogLevel.Information, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogAsync_WithNullException_ShouldLogWithoutException()
    {
        // Arrange
        const string message = "Async message without exception";

        // Act
        await LogHelper.LogAsync(mockLogger.Object, message, LogLevel.Information, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
