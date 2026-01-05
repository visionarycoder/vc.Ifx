using FluentAssertions;

namespace VisionaryCoder.Framework.Patterns.Tests.Patterns;

/// <summary>
/// Unit tests for ResultExtensions with 100% coverage.
/// Tests all extension methods with various scenarios.
/// </summary>
[TestClass]
public class ResultExtensionsTests
{
    #region Match Tests

    [TestMethod]
    public void Match_OnSuccess_ShouldExecuteSuccessFunction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var successCalled = false;
        var failureCalled = false;

        // Act
        var output = result.Match(
            onSuccess: value => { successCalled = true; return value * 2; },
            onFailure: error => { failureCalled = true; return 0; });

        // Assert
        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        output.Should().Be(84);
    }

    [TestMethod]
    public void Match_OnFailure_ShouldExecuteFailureFunction()
    {
        // Arrange
        var result = Result<int>.Failure("CODE", "Message");
        var successCalled = false;
        var failureCalled = false;

        // Act
        var output = result.Match(
            onSuccess: value => { successCalled = true; return value * 2; },
            onFailure: error => { failureCalled = true; return -1; });

        // Assert
        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
        output.Should().Be(-1);
    }

    [TestMethod]
    public async Task MatchAsync_OnSuccess_ShouldExecuteSuccessFunction()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var output = await result.MatchAsync(
            onSuccess: async value => { await Task.Delay(1); return value * 2; },
            onFailure: async error => { await Task.Delay(1); return 0; });

        // Assert
        output.Should().Be(84);
    }

    [TestMethod]
    public async Task MatchAsync_OnFailure_ShouldExecuteFailureFunction()
    {
        // Arrange
        var result = Result<int>.Failure("CODE", "Message");

        // Act
        var output = await result.MatchAsync(
            onSuccess: async value => { await Task.Delay(1); return value * 2; },
            onFailure: async error => { await Task.Delay(1); return -1; });

        // Assert
        output.Should().Be(-1);
    }

    #endregion

    #region Map Tests

    [TestMethod]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    [TestMethod]
    public void Map_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var error = new Error("CODE", "Message");
        var result = Result<int>.Failure(error);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    [TestMethod]
    public async Task MapAsync_OnSuccess_ShouldTransformValue()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    [TestMethod]
    public async Task MapAsync_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var error = new Error("CODE", "Message");
        var result = Result<int>.Failure(error);

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    #endregion

    #region Bind Tests

    [TestMethod]
    public void Bind_OnSuccess_ShouldExecuteBinder()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [TestMethod]
    public void Bind_OnSuccessButBinderFails_ShouldReturnFailure()
    {
        // Arrange
        var result = Result<int>.Success(10);
        var binderError = new Error("BINDER", "Binder failed");

        // Act
        var bound = result.Bind(x => Result<string>.Failure(binderError));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(binderError);
    }

    [TestMethod]
    public void Bind_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var error = new Error("CODE", "Message");
        var result = Result<int>.Failure(error);

        // Act
        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(error);
    }

    [TestMethod]
    public async Task BindAsync_OnSuccess_ShouldExecuteBinder()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var bound = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success(x.ToString());
        });

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [TestMethod]
    public async Task BindAsync_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var error = new Error("CODE", "Message");
        var result = Result<int>.Failure(error);

        // Act
        var bound = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success(x.ToString());
        });

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(error);
    }

    #endregion

    #region Tap Tests

    [TestMethod]
    public void Tap_OnSuccess_ShouldExecuteAction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var actionCalled = false;
        var capturedValue = 0;

        // Act
        var tapped = result.Tap(x =>
        {
            actionCalled = true;
            capturedValue = x;
        });

        // Assert
        actionCalled.Should().BeTrue();
        capturedValue.Should().Be(42);
        tapped.Should().BeSameAs(result);
    }

    [TestMethod]
    public void Tap_OnFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var result = Result<int>.Failure("CODE", "Message");
        var actionCalled = false;

        // Act
        var tapped = result.Tap(x => actionCalled = true);

        // Assert
        actionCalled.Should().BeFalse();
        tapped.Should().BeSameAs(result);
    }

    [TestMethod]
    public async Task TapAsync_OnSuccess_ShouldExecuteAction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var actionCalled = false;

        // Act
        var tapped = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            actionCalled = true;
        });

        // Assert
        actionCalled.Should().BeTrue();
        tapped.Should().BeSameAs(result);
    }

    [TestMethod]
    public async Task TapAsync_OnFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var result = Result<int>.Failure("CODE", "Message");
        var actionCalled = false;

        // Act
        var tapped = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            actionCalled = true;
        });

        // Assert
        actionCalled.Should().BeFalse();
        tapped.Should().BeSameAs(result);
    }

    #endregion

    #region ValueOrDefault Tests

    [TestMethod]
    public void ValueOrDefault_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.ValueOrDefault(0);

        // Assert
        value.Should().Be(42);
    }

    [TestMethod]
    public void ValueOrDefault_OnFailure_ShouldReturnDefault()
    {
        // Arrange
        var result = Result<int>.Failure("CODE", "Message");

        // Act
        var value = result.ValueOrDefault(99);

        // Assert
        value.Should().Be(99);
    }

    [TestMethod]
    public void ValueOrDefault_OnFailureWithoutDefault_ShouldReturnTypeDefault()
    {
        // Arrange
        var result = Result<int>.Failure("CODE", "Message");

        // Act
        var value = result.ValueOrDefault();

        // Assert
        value.Should().Be(0);
    }

    #endregion

    #region Ensure Tests

    [TestMethod]
    public void Ensure_PredicateTrue_ShouldReturnSameResult()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var ensured = result.Ensure(x => x > 0, new Error("NEG", "Negative"));

        // Assert
        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [TestMethod]
    public void Ensure_PredicateFalse_ShouldReturnFailure()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var error = new Error("LARGE", "Too large");

        // Act
        var ensured = result.Ensure(x => x < 10, error);

        // Assert
        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be(error);
    }

    [TestMethod]
    public void Ensure_OnExistingFailure_ShouldPropagateError()
    {
        // Arrange
        var originalError = new Error("ORIG", "Original");
        var result = Result<int>.Failure(originalError);

        // Act
        var ensured = result.Ensure(x => x > 0, new Error("NEG", "Negative"));

        // Assert
        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be(originalError);
    }

    #endregion

    #region Combine Tests

    [TestMethod]
    public void Combine_AllSuccess_ShouldReturnSuccess()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        // Act
        var combined = ResultExtensions.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public void Combine_OneFailure_ShouldReturnFailure()
    {
        // Arrange
        var result1 = Result.Success();
        var error = new Error("FAIL", "Failed");
        var result2 = Result.Failure(error);
        var result3 = Result.Success();

        // Act
        var combined = ResultExtensions.Combine(result1, result2, result3);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be(error);
    }

    [TestMethod]
    public void Combine_MultipleFailures_ShouldReturnFirstFailure()
    {
        // Arrange
        var error1 = new Error("FAIL1", "First failure");
        var error2 = new Error("FAIL2", "Second failure");
        var result1 = Result.Failure(error1);
        var result2 = Result.Failure(error2);

        // Act
        var combined = ResultExtensions.Combine(result1, result2);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be(error1);
    }

    [TestMethod]
    public void CombineT_AllSuccess_ShouldReturnAllValues()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);

        // Act
        var combined = ResultExtensions.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [TestMethod]
    public void CombineT_OneFailure_ShouldReturnFailure()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var error = new Error("FAIL", "Failed");
        var result2 = Result<int>.Failure(error);
        var result3 = Result<int>.Success(3);

        // Act
        var combined = ResultExtensions.Combine(result1, result2, result3);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be(error);
    }

    #endregion

    #region Complex Chaining Tests

    [TestMethod]
    public void ComplexChain_AllSuccess_ShouldWork()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var final = result
            .Map(x => x * 2)
            .Bind(x => Result<int>.Success(x + 5))
            .Tap(x => { /* side effect */ })
            .Ensure(x => x > 0, new Error("NEG", "Negative"))
            .Map(x => x.ToString());

        // Assert
        final.IsSuccess.Should().BeTrue();
        final.Value.Should().Be("25");
    }

    [TestMethod]
    public void ComplexChain_FailureInMiddle_ShouldPropagateError()
    {
        // Arrange
        var result = Result<int>.Success(10);
        var error = new Error("BIND", "Bind failed");

        // Act
        var final = result
            .Map(x => x * 2)
            .Bind(x => Result<int>.Failure(error))
            .Tap(x => { /* should not execute */ })
            .Map(x => x.ToString());

        // Assert
        final.IsFailure.Should().BeTrue();
        final.Error.Should().Be(error);
    }

    #endregion
}
