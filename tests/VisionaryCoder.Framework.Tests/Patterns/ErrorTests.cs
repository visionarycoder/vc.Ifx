using FluentAssertions;
using VisionaryCoder.Framework.Patterns;

namespace VisionaryCoder.Framework.Tests.Patterns;

/// <summary>
/// Unit tests for Error class with 100% coverage.
/// </summary>
[TestClass]
public class ErrorTests
{
    [TestMethod]
    public void Error_Constructor_ShouldSetProperties()
    {
        // Arrange & Act
        var error = new Error("TestCode", "Test message");

        // Assert
        error.Code.Should().Be("TestCode");
        error.Message.Should().Be("Test message");
    }

    [TestMethod]
    public void Error_None_ShouldHaveEmptyValues()
    {
        // Arrange & Act
        var error = Error.None;

        // Assert
        error.Code.Should().BeEmpty();
        error.Message.Should().BeEmpty();
    }

    [TestMethod]
    public void Error_NullValue_ShouldHavePredefinedValues()
    {
        // Arrange & Act
        var error = Error.NullValue;

        // Assert
        error.Code.Should().Be("Error.NullValue");
        error.Message.Should().Be("A null value was provided");
    }

    [TestMethod]
    public void Error_Validation_ShouldCreateValidationError()
    {
        // Arrange & Act
        var error = Error.Validation("VAL001", "Invalid input");

        // Assert
        error.Code.Should().Be("VAL001");
        error.Message.Should().Be("Invalid input");
    }

    [TestMethod]
    public void Error_NotFound_ShouldCreateNotFoundError()
    {
        // Arrange & Act
        var error = Error.NotFound("NF001", "Entity not found");

        // Assert
        error.Code.Should().Be("NF001");
        error.Message.Should().Be("Entity not found");
    }

    [TestMethod]
    public void Error_Conflict_ShouldCreateConflictError()
    {
        // Arrange & Act
        var error = Error.Conflict("CONF001", "Resource conflict");

        // Assert
        error.Code.Should().Be("CONF001");
        error.Message.Should().Be("Resource conflict");
    }

    [TestMethod]
    public void Error_Failure_ShouldCreateFailureError()
    {
        // Arrange & Act
        var error = Error.Failure("FAIL001", "Operation failed");

        // Assert
        error.Code.Should().Be("FAIL001");
        error.Message.Should().Be("Operation failed");
    }

    [TestMethod]
    public void Error_WithEmptyCode_ShouldAllowEmptyCode()
    {
        // Arrange & Act
        var error = new Error("", "Message only");

        // Assert
        error.Code.Should().BeEmpty();
        error.Message.Should().Be("Message only");
    }

    [TestMethod]
    public void Error_WithEmptyMessage_ShouldAllowEmptyMessage()
    {
        // Arrange & Act
        var error = new Error("CODE001", "");

        // Assert
        error.Code.Should().Be("CODE001");
        error.Message.Should().BeEmpty();
    }

    [TestMethod]
    public void Error_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var error1 = new Error("CODE", "Message");
        var error2 = new Error("CODE", "Message");
        var error3 = new Error("CODE", "Different");

        // Assert
        error1.Should().Be(error2);
        error1.Should().NotBe(error3);
        (error1 == error2).Should().BeTrue();
        (error1 != error3).Should().BeTrue();
    }
}
