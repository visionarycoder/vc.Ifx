using VisionaryCoder.Framework.Logging;

namespace VisionaryCoder.Framework.Tests.Logging;

[TestClass]
public class LogDelegatesTests
{
    #region LogDebug Tests

    [TestMethod]
    public void LogDebug_ShouldBeInvokable()
    {
        // Arrange
        string? capturedMessage = null;
        object[]? capturedArgs = null;
        LogDebug logDebug = (message, args) =>
        {
            capturedMessage = message;
            capturedArgs = args;
        };

        // Act
        logDebug("Test message", 1, "arg2");

        // Assert
        capturedMessage.Should().Be("Test message");
        capturedArgs.Should().NotBeNull();
        capturedArgs.Should().HaveCount(2);
    }

    [TestMethod]
    public void LogDebug_WithNoArgs_ShouldWork()
    {
        // Arrange
        string? capturedMessage = null;
        LogDebug logDebug = (message, args) => capturedMessage = message;

        // Act
        logDebug("Simple message");

        // Assert
        capturedMessage.Should().Be("Simple message");
    }

    [TestMethod]
    [DataRow("Debug message 1")]
    [DataRow("Another debug message")]
    [DataRow("")]
    public void LogDebug_WithVariousMessages_ShouldCapture(string message)
    {
        // Arrange
        string? captured = null;
        LogDebug logDebug = (msg, args) => captured = msg;

        // Act
        logDebug(message);

        // Assert
        captured.Should().Be(message);
    }

    #endregion

    #region LogInformation Tests

    [TestMethod]
    public void LogInformation_ShouldBeInvokable()
    {
        // Arrange
        string? capturedMessage = null;
        LogInformation logInfo = (message, args) => capturedMessage = message;

        // Act
        logInfo("Info message", 123);

        // Assert
        capturedMessage.Should().Be("Info message");
    }

    [TestMethod]
    public void LogInformation_WithMultipleArgs_ShouldPassThrough()
    {
        // Arrange
        object[]? capturedArgs = null;
        LogInformation logInfo = (message, args) => capturedArgs = args;

        // Act
        logInfo("Message", "arg1", 2, true, 4.5);

        // Assert
        capturedArgs.Should().HaveCount(4);
        capturedArgs![0].Should().Be("arg1");
        capturedArgs[1].Should().Be(2);
        capturedArgs[2].Should().Be(true);
        capturedArgs[3].Should().Be(4.5);
    }

    #endregion

    #region LogWarning Tests

    [TestMethod]
    public void LogWarning_ShouldBeInvokable()
    {
        // Arrange
        string? capturedMessage = null;
        LogWarning logWarning = (message, args) => capturedMessage = message;

        // Act
        logWarning("Warning message");

        // Assert
        capturedMessage.Should().Be("Warning message");
    }

    [TestMethod]
    public void LogWarning_WithException_ShouldCaptureMessage()
    {
        // Arrange
        string? captured = null;
        LogWarning logWarning = (message, args) => captured = message;
        var exception = new InvalidOperationException();

        // Act
        logWarning("Warning with exception", exception);

        // Assert
        captured.Should().Be("Warning with exception");
    }

    #endregion

    #region LogError Tests

    [TestMethod]
    public void LogError_ShouldBeInvokable()
    {
        // Arrange
        string? capturedMessage = null;
        LogError logError = (message, args) => capturedMessage = message;

        // Act
        logError("Error occurred");

        // Assert
        capturedMessage.Should().Be("Error occurred");
    }

    [TestMethod]
    public void LogError_WithMultipleExceptions_ShouldPassArgs()
    {
        // Arrange
        object[]? capturedArgs = null;
        LogError logError = (message, args) => capturedArgs = args;
        var ex1 = new Exception("Error 1");
        var ex2 = new InvalidOperationException("Error 2");

        // Act
        logError("Multiple errors", ex1, ex2);

        // Assert
        capturedArgs.Should().HaveCount(2);
        capturedArgs![0].Should().Be(ex1);
        capturedArgs[1].Should().Be(ex2);
    }

    #endregion

    #region LogCritical Tests

    [TestMethod]
    public void LogCritical_ShouldBeInvokable()
    {
        // Arrange
        string? capturedMessage = null;
        LogCritical logCritical = (message, args) => capturedMessage = message;

        // Act
        logCritical("Critical failure");

        // Assert
        capturedMessage.Should().Be("Critical failure");
    }

    [TestMethod]
    public void LogCritical_WithSystemException_ShouldWork()
    {
        // Arrange
        string? captured = null;
        object[]? capturedArgs = null;
        LogCritical logCritical = (message, args) =>
        {
            captured = message;
            capturedArgs = args;
        };
        var exception = new OutOfMemoryException();

        // Act
        logCritical("System failure", exception);

        // Assert
        captured.Should().Be("System failure");
        capturedArgs.Should().Contain(exception);
    }

    #endregion

    #region LogTrace Tests

    [TestMethod]
    public void LogTrace_ShouldBeInvokable()
    {
        // Arrange
        string? capturedMessage = null;
        LogTrace logTrace = (message, args) => capturedMessage = message;

        // Act
        logTrace("Trace message");

        // Assert
        capturedMessage.Should().Be("Trace message");
    }

    [TestMethod]
    [DataRow("Trace 1", 1)]
    [DataRow("Trace 2", 2)]
    [DataRow("Trace 3", 3)]
    public void LogTrace_WithDataRow_ShouldCapture(string message, int arg)
    {
        // Arrange
        string? captured = null;
        object[]? capturedArgs = null;
        LogTrace logTrace = (msg, args) =>
        {
            captured = msg;
            capturedArgs = args;
        };

        // Act
        logTrace(message, arg);

        // Assert
        captured.Should().Be(message);
        capturedArgs.Should().ContainSingle().Which.Should().Be(arg);
    }

    #endregion

    #region LogNone Tests

    [TestMethod]
    public void LogNone_ShouldBeInvokable()
    {
        // Arrange
        string? capturedMessage = null;
        LogNone logNone = (message, args) => capturedMessage = message;

        // Act
        logNone("None level message");

        // Assert
        capturedMessage.Should().Be("None level message");
    }

    [TestMethod]
    public void LogNone_WithEmptyArgs_ShouldWork()
    {
        // Arrange
        bool invoked = false;
        LogNone logNone = (message, args) => invoked = true;

        // Act
        logNone("Message");

        // Assert
        invoked.Should().BeTrue();
    }

    #endregion

    #region Multi-Delegate Tests

    [TestMethod]
    public void AllLogDelegates_ShouldHaveSameSignature()
    {
        // Arrange
        var capturedMessages = new List<string>();
        Action<string, object[]> handler = (msg, args) => capturedMessages.Add(msg);

        LogDebug logDebug = handler.Invoke;
        LogInformation logInfo = handler.Invoke;
        LogWarning logWarning = handler.Invoke;
        LogError logError = handler.Invoke;
        LogCritical logCritical = handler.Invoke;
        LogTrace logTrace = handler.Invoke;
        LogNone logNone = handler.Invoke;

        // Act
        logDebug("Debug");
        logInfo("Info");
        logWarning("Warning");
        logError("Error");
        logCritical("Critical");
        logTrace("Trace");
        logNone("None");

        // Assert
        capturedMessages.Should().HaveCount(7);
        capturedMessages.Should().ContainInOrder("Debug", "Info", "Warning", "Error", "Critical", "Trace", "None");
    }

    [TestMethod]
    public void LogDelegate_WithNullArgs_ShouldNotThrow()
    {
        // Arrange
        LogDebug logDebug = (message, args) => { };

        // Act
        Action act = () => logDebug("Message", null!);

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void LogDelegate_WithLargeNumberOfArgs_ShouldHandle()
    {
        // Arrange
        object[]? capturedArgs = null;
        LogInformation logInfo = (message, args) => capturedArgs = args;
        object[] args = Enumerable.Range(1, 100).Cast<object>().ToArray();

        // Act
        logInfo("Many args", args);

        // Assert
        capturedArgs.Should().HaveCount(100);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void LogDelegate_WithSpecialCharacters_ShouldPreserve()
    {
        // Arrange
        string? captured = null;
        LogError logError = (message, args) => captured = message;
        string specialMessage = "Error: \n\t\r Special \"chars\" & symbols! @#$%";

        // Act
        logError(specialMessage);

        // Assert
        captured.Should().Be(specialMessage);
    }

    [TestMethod]
    public void LogDelegate_WithUnicode_ShouldPreserve()
    {
        // Arrange
        string? captured = null;
        LogWarning logWarning = (message, args) => captured = message;
        string unicodeMessage = "警告: émile naïve Übermensch";

        // Act
        logWarning(unicodeMessage);

        // Assert
        captured.Should().Be(unicodeMessage);
    }

    [TestMethod]
    public void LogDelegate_WithVeryLongMessage_ShouldNotTruncate()
    {
        // Arrange
        string? captured = null;
        LogCritical logCritical = (message, args) => captured = message;
        string longMessage = new('A', 10000);

        // Act
        logCritical(longMessage);

        // Assert
        captured.Should().HaveLength(10000);
        captured.Should().Be(longMessage);
    }

    #endregion
}
