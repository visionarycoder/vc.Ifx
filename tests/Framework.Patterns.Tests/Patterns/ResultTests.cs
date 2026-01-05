using FluentAssertions;

namespace VisionaryCoder.Framework.Patterns.Tests.Patterns;

/// <summary>
/// Unit tests for Result and Result{T} classes with 100% coverage.
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

    [TestMethod]
    public void Result_Constructor_SuccessWithError_ShouldThrow()
    {
        // Act
        Action act = () => new TestResult(true, new Error("CODE", "Message"));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful result with an error*");
    }

    [TestMethod]
    public void Result_Constructor_FailureWithoutError_ShouldThrow()
    {
        // Act
        Action act = () => new TestResult(false, Error.None);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result without an error*");
    }

    private class TestResult : Result
    {
        public TestResult(bool isSuccess, Error error) : base(isSuccess, error) { }
    }

    #endregion

    #region Result<T> Tests

    [TestMethod]
    public void ResultT_Success_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result<string>.Success(value);

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
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [TestMethod]
    public void ResultT_Failure_WithCodeAndMessage_ShouldCreateFailedResult()
    {
        // Act
        var result = Result<string>.Failure("CODE", "Message");

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
        var result = Result<string>.Failure("CODE", "Message");

        // Act
        Action act = () => { var _ = result.Value; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*value of a failed result*");
    }

    [TestMethod]
    public void ResultT_ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        // Arrange
        string value = "test";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [TestMethod]
    public void ResultT_ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange
        var error = new Error("CODE", "Message");

        // Act
        Result<string> result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [TestMethod]
    public void ResultT_WithNullValue_ShouldAllowNull()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [TestMethod]
    public void ResultT_WithReferenceType_ShouldWork()
    {
        // Arrange
        var obj = new TestClass { Value = "test" };

        // Act
        var result = Result<TestClass>.Success(obj);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(obj);
        result.Value.Value.Should().Be("test");
    }

    [TestMethod]
    public void ResultT_WithValueType_ShouldWork()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [TestMethod]
    public void ResultT_WithStruct_ShouldWork()
    {
        // Arrange
        var value = new TestStruct { Number = 123 };

        // Act
        var result = Result<TestStruct>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be(123);
    }

    private class TestClass
    {
        public string Value { get; set; } = string.Empty;
    }

    private struct TestStruct
    {
        public int Number { get; set; }
    }

    #endregion
}
