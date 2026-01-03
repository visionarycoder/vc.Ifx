using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.CQRS.Behaviors;

namespace VisionaryCoder.Framework.Tests.CQRS.Behaviors;

/// <summary>
/// Unit tests for ValidationBehavior.
/// </summary>
[TestClass]
public class ValidationBehaviorTests
{
    [TestMethod]
    public async Task HandleAsync_WithNoValidators_ShouldContinuePipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(provider);
        
        var request = new TestRequest { Value = "test" };
        var expectedResponse = new TestResponse { Result = "success" };
        Task<TestResponse> Next() => Task.FromResult(expectedResponse);

        // Act
        TestResponse result = await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [TestMethod]
    public async Task HandleAsync_WithValidRequest_ShouldContinuePipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        var provider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(provider);
        
        var request = new TestRequest { Value = "valid" };
        var expectedResponse = new TestResponse { Result = "success" };
        Task<TestResponse> Next() => Task.FromResult(expectedResponse);

        // Act
        TestResponse result = await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [TestMethod]
    public async Task HandleAsync_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        var provider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(provider);
        
        var request = new TestRequest { Value = "" }; // Invalid - empty
        Task<TestResponse> Next() => Task.FromResult(new TestResponse());

        // Act
        Func<Task> act = async () => await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "Value"));
    }

    [TestMethod]
    public async Task HandleAsync_WithMultipleValidators_ShouldRunAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        services.AddScoped<IValidator<TestRequest>, AnotherTestRequestValidator>();
        var provider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(provider);
        
        var request = new TestRequest { Value = "a" }; // Invalid for both validators
        Task<TestResponse> Next() => Task.FromResult(new TestResponse());

        // Act
        Func<Task> act = async () => await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        exception.Which.Errors.Should().HaveCountGreaterThan(1);
    }

    [TestMethod]
    public async Task HandleAsync_WithMultipleFailures_ShouldIncludeAllErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidator<ComplexRequest>, ComplexRequestValidator>();
        var provider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<ComplexRequest, TestResponse>(provider);
        
        var request = new ComplexRequest 
        { 
            Name = "",  // Invalid
            Age = -1,   // Invalid
            Email = "not-an-email" // Invalid
        };
        Task<TestResponse> Next() => Task.FromResult(new TestResponse());

        // Act
        Func<Task> act = async () => await behavior.HandleAsync(request, Next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        exception.Which.Errors.Should().HaveCount(3);
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "Name");
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "Age");
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [TestMethod]
    public async Task HandleAsync_RespectsCancellationToken()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        var provider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(provider);
        
        var request = new TestRequest { Value = "valid" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        
        Task<TestResponse> Next() => Task.FromResult(new TestResponse());

        // Act
        Func<Task> act = async () => await behavior.HandleAsync(request, Next, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new ValidationBehavior<TestRequest, TestResponse>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    #region Test Types

    public class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }

    public class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MinimumLength(3);
        }
    }

    public class AnotherTestRequestValidator : AbstractValidator<TestRequest>
    {
        public AnotherTestRequestValidator()
        {
            RuleFor(x => x.Value).MaximumLength(10);
        }
    }

    public class ComplexRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class ComplexRequestValidator : AbstractValidator<ComplexRequest>
    {
        public ComplexRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Age).GreaterThan(0);
            RuleFor(x => x.Email).EmailAddress();
        }
    }

    #endregion
}
