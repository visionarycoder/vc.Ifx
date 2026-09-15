using System.Reflection;

namespace Ifx.Tests;

/// <summary>
/// Unit tests for FrameworkResult classes to ensure 100% code coverage.
/// </summary>
[TestClass]
public class FrameworkResultTests
{
    #region Generic FrameworkResult<T> Tests

    [TestClass]
    public class FrameworkResultGenericTests
    {
        #region Success Tests

        [TestMethod]
        public void Success_WithValue_ShouldCreateSuccessfulResult()
        {
            // Arrange
            string value = "test value";

            // Act
            var result = ComponentResult<string>.Success(value);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Value.Should().Be(value);
            result.ErrorMessage.Should().BeNull();
            result.Exception.Should().BeNull();
        }

        [TestMethod]
        public void Success_WithNullValue_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = ComponentResult<string?>.Success(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Value.Should().BeNull();
            result.ErrorMessage.Should().BeNull();
            result.Exception.Should().BeNull();
        }

        [TestMethod]
        public void Success_WithComplexType_ShouldCreateSuccessfulResult()
        {
            // Arrange
            var value = new { Id = 1, Name = "Test" };

            // Act
            var result = ComponentResult<object>.Success(value);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(value);
        }

        #endregion

        #region Failure Tests

        [TestMethod]
        public void Failure_WithErrorMessage_ShouldCreateFailedResult()
        {
            // Arrange
            string errorMessage = "Something went wrong";

            // Act
            var result = ComponentResult<string>.Failure(errorMessage);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Value.Should().BeNull();
            result.ErrorMessage.Should().Be(errorMessage);
            result.Exception.Should().BeNull();
        }

        [TestMethod]
        public void Failure_WithException_ShouldCreateFailedResult()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");

            // Act
            var result = ComponentResult<string>.Failure(exception);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Value.Should().BeNull();
            result.ErrorMessage.Should().Be(exception.Message);
            result.Exception.Should().Be(exception);
        }

        [TestMethod]
        public void Failure_WithErrorMessageAndException_ShouldCreateFailedResult()
        {
            // Arrange
            string errorMessage = "Custom error message";
            var exception = new ArgumentException("Argument exception");

            // Act
            var result = ComponentResult<string>.Failure(errorMessage, exception);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Value.Should().BeNull();
            result.ErrorMessage.Should().Be(errorMessage);
            result.Exception.Should().Be(exception);
        }

        #endregion

        #region Match Method Tests

        [TestMethod]
        public void Match_WithSuccessfulResult_ShouldExecuteSuccessAction()
        {
            // Arrange
            string value = "test value";
            var result = ComponentResult<string>.Success(value);
            bool successCalled = false;
            bool failureCalled = false;
            string? capturedValue = null;

            // Act
            result.Match(
                val => { successCalled = true; capturedValue = val; },
                (error, ex) => { failureCalled = true; }
            );

            // Assert
            successCalled.Should().BeTrue();
            failureCalled.Should().BeFalse();
            capturedValue.Should().Be(value);
        }

        [TestMethod]
        public void Match_WithFailedResult_ShouldExecuteFailureAction()
        {
            // Arrange
            string errorMessage = "Test error";
            var exception = new InvalidOperationException("Test exception");
            var result = ComponentResult<string>.Failure(errorMessage, exception);
            bool successCalled = false;
            bool failureCalled = false;
            string? capturedError = null;
            Exception? capturedException = null;

            // Act
            result.Match(
                val => { successCalled = true; },
                (error, ex) => { failureCalled = true; capturedError = error; capturedException = ex; }
            );

            // Assert
            successCalled.Should().BeFalse();
            failureCalled.Should().BeTrue();
            capturedError.Should().Be(errorMessage);
            capturedException.Should().Be(exception);
        }

        [TestMethod]
        public void Match_WithSuccessfulResultButNullValue_ShouldExecuteSuccessAction()
        {
            // Arrange
            var result = ComponentResult<string?>.Success(null);
            bool successCalled = false;
            bool failureCalled = false;

            // Act
            result.Match(
                val => { val.Should().BeNull(); successCalled = true; },
                (error, ex) => { failureCalled = true; }
            );

            // Assert
            successCalled.Should().BeTrue();
            failureCalled.Should().BeFalse();
        }

        [TestMethod]
        public void Match_WithFailedResultWithoutException_ShouldPassNullException()
        {
            // Arrange
            string errorMessage = "Test error";
            var result = ComponentResult<string>.Failure(errorMessage);
            Exception? capturedException = new Exception("should be null");

            // Act
            result.Match(
                val => { },
                (error, ex) => { capturedException = ex; }
            );

            // Assert
            capturedException.Should().BeNull();
        }

        #endregion

        #region Map Method Tests

        [TestMethod]
        public void Map_WithSuccessfulResult_ShouldMapValue()
        {
            // Arrange
            int originalValue = 42;
            var result = ComponentResult<int>.Success(originalValue);

            // Act
            ComponentResult<string> mappedResult = result.Map(x => x.ToString());

            // Assert
            mappedResult.IsSuccess.Should().BeTrue();
            mappedResult.Value.Should().Be("42");
            mappedResult.ErrorMessage.Should().BeNull();
            mappedResult.Exception.Should().BeNull();
        }

        [TestMethod]
        public void Map_WithFailedResult_ShouldReturnFailedResultWithSameError()
        {
            // Arrange
            string errorMessage = "Original error";
            var exception = new InvalidOperationException("Original exception");
            var result = ComponentResult<int>.Failure(errorMessage, exception);

            // Act
            ComponentResult<string> mappedResult = result.Map(x => x.ToString());

            // Assert
            mappedResult.IsSuccess.Should().BeFalse();
            mappedResult.Value.Should().BeNull();
            mappedResult.ErrorMessage.Should().Be(errorMessage);
            mappedResult.Exception.Should().Be(exception);
        }

        [TestMethod]
        public void Map_WithFailedResultWithoutException_ShouldReturnFailedResultWithoutException()
        {
            // Arrange
            string errorMessage = "Original error";
            var result = ComponentResult<int>.Failure(errorMessage);

            // Act
            ComponentResult<string> mappedResult = result.Map(x => x.ToString());

            // Assert
            mappedResult.IsSuccess.Should().BeFalse();
            mappedResult.Value.Should().BeNull();
            mappedResult.ErrorMessage.Should().Be(errorMessage);
            mappedResult.Exception.Should().BeNull();
        }

        [TestMethod]
        public void Map_WithSuccessfulResultButMapperThrows_ShouldReturnFailedResult()
        {
            // Arrange
            int originalValue = 42;
            var result = ComponentResult<int>.Success(originalValue);
            var mapperException = new InvalidOperationException("Mapper failed");

            // Act
            ComponentResult<string> mappedResult = result.Map<string>(x => throw mapperException);

            // Assert
            mappedResult.IsSuccess.Should().BeFalse();
            mappedResult.Value.Should().BeNull();
            mappedResult.ErrorMessage.Should().Be(mapperException.Message);
            mappedResult.Exception.Should().Be(mapperException);
        }

        [TestMethod]
        public void Map_WithSuccessfulResultButNullValue_ShouldMapSuccessfully()
        {
            // Arrange
            var result = ComponentResult<string?>.Success(null);

            // Act
            ComponentResult<int> mappedResult = result.Map(x => x?.Length ?? 0);

            // Assert
            mappedResult.IsSuccess.Should().BeTrue();
            mappedResult.Value.Should().Be(0);
            mappedResult.ErrorMessage.Should().BeNull();
            mappedResult.Exception.Should().BeNull();
        }

        [TestMethod]
        public void Map_WithComplexTypeMapping_ShouldWork()
        {
            // Arrange
            var person = new { Name = "John", Age = 30 };
            var result = ComponentResult<object>.Success(person);

            // Act
            ComponentResult<string> mappedResult = result.Map(p => $"{((dynamic)p).Name} is {((dynamic)p).Age} years old");

            // Assert
            mappedResult.IsSuccess.Should().BeTrue();
            mappedResult.Value.Should().Be("John is 30 years old");
        }

        #endregion
    }

    #endregion

    #region Non-Generic FrameworkResult Tests

    [TestClass]
    public class FrameworkResultNonGenericTests
    {
        #region Success Tests

        [TestMethod]
        public void Success_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = ComponentResult.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.ErrorMessage.Should().BeNull();
            result.Exception.Should().BeNull();
        }

        #endregion

        #region Failure Tests

        [TestMethod]
        public void Failure_WithErrorMessage_ShouldCreateFailedResult()
        {
            // Arrange
            string errorMessage = "Something went wrong";

            // Act
            var result = ComponentResult.Failure(errorMessage);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be(errorMessage);
            result.Exception.Should().BeNull();
        }

        [TestMethod]
        public void Failure_WithException_ShouldCreateFailedResult()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");

            // Act
            var result = ComponentResult.Failure(exception);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be(exception.Message);
            result.Exception.Should().Be(exception);
        }

        [TestMethod]
        public void Failure_WithErrorMessageAndException_ShouldCreateFailedResult()
        {
            // Arrange
            string errorMessage = "Custom error message";
            var exception = new ArgumentException("Argument exception");

            // Act
            var result = ComponentResult.Failure(errorMessage, exception);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be(errorMessage);
            result.Exception.Should().Be(exception);
        }

        #endregion

        #region Match Method Tests

        [TestMethod]
        public void Match_WithSuccessfulResult_ShouldExecuteSuccessAction()
        {
            // Arrange
            var result = ComponentResult.Success();
            bool successCalled = false;
            bool failureCalled = false;

            // Act
            result.Match(
                () => { successCalled = true; },
                (error, ex) => { failureCalled = true; }
            );

            // Assert
            successCalled.Should().BeTrue();
            failureCalled.Should().BeFalse();
        }

        [TestMethod]
        public void Match_WithFailedResult_ShouldExecuteFailureAction()
        {
            // Arrange
            string errorMessage = "Test error";
            var exception = new InvalidOperationException("Test exception");
            var result = ComponentResult.Failure(errorMessage, exception);
            bool successCalled = false;
            bool failureCalled = false;
            string? capturedError = null;
            Exception? capturedException = null;

            // Act
            result.Match(
                () => { successCalled = true; },
                (error, ex) => { failureCalled = true; capturedError = error; capturedException = ex; }
            );

            // Assert
            successCalled.Should().BeFalse();
            failureCalled.Should().BeTrue();
            capturedError.Should().Be(errorMessage);
            capturedException.Should().Be(exception);
        }

        [TestMethod]
        public void Match_WithFailedResultWithoutException_ShouldPassNullException()
        {
            // Arrange
            string errorMessage = "Test error";
            var result = ComponentResult.Failure(errorMessage);
            Exception? capturedException = new Exception("should be null");

            // Act
            result.Match(
                () => { },
                (error, ex) => { capturedException = ex; }
            );

            // Assert
            capturedException.Should().BeNull();
        }

        [TestMethod]
        public void Match_WithFailedResultWithNullErrorMessage_ShouldUseUnknownError()
        {
            // This tests the "Unknown error" fallback in Match method
            // We need to create a result through reflection to test this edge case
            Type resultType = typeof(ComponentResult);
            ConstructorInfo constructor = resultType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            var result = (ComponentResult)constructor.Invoke([false, null, null]);

            string? capturedError = null;

            // Act
            result.Match(
                () => { },
                (error, ex) => { capturedError = error; }
            );

            // Assert
            capturedError.Should().Be("Unknown error");
        }

        #endregion
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FrameworkResult_GenericAndNonGeneric_ShouldWorkTogether()
    {
        // Arrange
        var nonGenericSuccess = ComponentResult.Success();
        var nonGenericFailure = ComponentResult.Failure("Non-generic error");
        var genericSuccess = ComponentResult<string>.Success("test");
        var genericFailure = ComponentResult<string>.Failure("Generic error");

        // Assert
        nonGenericSuccess.IsSuccess.Should().BeTrue();
        nonGenericFailure.IsFailure.Should().BeTrue();
        genericSuccess.IsSuccess.Should().BeTrue();
        genericFailure.IsFailure.Should().BeTrue();

        // Verify they're different types
        nonGenericSuccess.GetType().Should().Be(typeof(ComponentResult));
        genericSuccess.GetType().Should().Be(typeof(ComponentResult<string>));
    }

    [TestMethod]
    public void FrameworkResult_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var initialResult = ComponentResult<int>.Success(42);

        // Act
        ComponentResult<string> stringResult = initialResult.Map(x => x.ToString());
        ComponentResult<int> lengthResult = stringResult.Map(s => s.Length);

        // Assert
        lengthResult.IsSuccess.Should().BeTrue();
        lengthResult.Value.Should().Be(2); // "42".Length
    }

    [TestMethod]
    public void FrameworkResult_ErrorPropagation_ShouldMaintainOriginalError()
    {
        // Arrange
        var originalException = new ArgumentException("Original error");
        var failedResult = ComponentResult<int>.Failure("Custom message", originalException);

        // Act
        ComponentResult<string> mappedResult = failedResult.Map(x => x.ToString());

        // Assert
        mappedResult.IsFailure.Should().BeTrue();
        mappedResult.ErrorMessage.Should().Be("Custom message");
        mappedResult.Exception.Should().Be(originalException);
    }

    #endregion
}
