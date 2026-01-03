// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using VisionaryCoder.Framework.Abstractions.Patterns;

namespace VisionaryCoder.Framework.Abstractions.Tests.Patterns;

/// <summary>
/// Unit tests for Result and Result{T} classes.
/// </summary>
[TestClass]
public class ResultTests
{
    #region Result (void) Tests

    [TestMethod]
    public void Result_Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [TestMethod]
    public void Result_Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TEST", "Test error");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [TestMethod]
    public void Result_Failure_WithCodeAndMessage_ShouldCreateFailedResult()
    {
        // Act
        var result = Result.Failure("CODE", "Message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CODE");
        result.Error.Message.Should().Be("Message");
    }

    [TestMethod]
    public void Result_ImplicitBool_Success_ShouldReturnTrue()
    {
        // Arrange
        var result = Result.Success();

        // Act
        bool isSuccess = result;

        // Assert
        isSuccess.Should().BeTrue();
    }

    [TestMethod]
    public void Result_ImplicitBool_Failure_ShouldReturnFalse()
    {
        // Arrange
        var result = Result.Failure("CODE", "Message");

        // Act
        bool isSuccess = result;

        // Assert
        isSuccess.Should().BeFalse();
    }

    #endregion

    #region Result<T> Tests

    [TestMethod]
    public void ResultT_Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        const int value = 42;

        // Act
        var result = Result<int>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().Be(Error.None);
    }

    [TestMethod]
    public void ResultT_Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TEST", "Test error");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [TestMethod]
    public void ResultT_Failure_WithCodeAndMessage_ShouldCreateFailedResult()
    {
        // Act
        var result = Result<int>.Failure("CODE", "Message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CODE");
        result.Error.Message.Should().Be("Message");
    }

    [TestMethod]
    public void ResultT_Value_OnFailure_ShouldThrow()
    {
        // Arrange
        var result = Result<int>.Failure("CODE", "Message");

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*value of a failed result*");
    }

    [TestMethod]
    public void ResultT_ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [TestMethod]
    public void ResultT_ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange
        var error = new Error("CODE", "Message");

        // Act
        Result<int> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    #endregion

    #region Error Tests

    [TestMethod]
    public void Error_None_ShouldHaveEmptyValues()
    {
        // Assert
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
    }

    [TestMethod]
    public void Error_NullValue_ShouldHaveCorrectValues()
    {
        // Assert
        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NullValue.Message.Should().Be("A null value was provided");
    }

    [TestMethod]
    public void Error_Validation_ShouldCreateValidationError()
    {
        // Act
        var error = Error.Validation("VAL001", "Validation failed");

        // Assert
        error.Code.Should().Be("VAL001");
        error.Message.Should().Be("Validation failed");
    }

    [TestMethod]
    public void Error_NotFound_ShouldCreateNotFoundError()
    {
        // Act
        var error = Error.NotFound("NF001", "Resource not found");

        // Assert
        error.Code.Should().Be("NF001");
        error.Message.Should().Be("Resource not found");
    }

    [TestMethod]
    public void Error_Conflict_ShouldCreateConflictError()
    {
        // Act
        var error = Error.Conflict("CF001", "Resource conflict");

        // Assert
        error.Code.Should().Be("CF001");
        error.Message.Should().Be("Resource conflict");
    }

    [TestMethod]
    public void Error_Failure_ShouldCreateFailureError()
    {
        // Act
        var error = Error.Failure("FL001", "Operation failed");

        // Assert
        error.Code.Should().Be("FL001");
        error.Message.Should().Be("Operation failed");
    }

    #endregion
}
