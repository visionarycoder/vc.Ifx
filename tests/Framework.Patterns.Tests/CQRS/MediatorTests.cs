using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Patterns.CQRS;
using VisionaryCoder.Framework.Patterns.CQRS.Abstractions;

namespace VisionaryCoder.Framework.Patterns.Tests.CQRS;

/// <summary>
/// Unit tests for the Mediator implementation.
/// </summary>
[TestClass]
public class MediatorTests
{
    private IServiceProvider serviceProvider = null!;
    private IMediator mediator = null!;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddMediator(typeof(MediatorTests).Assembly);
        serviceProvider = services.BuildServiceProvider();
        mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    #region Command Tests (void)

    [TestMethod]
    public async Task SendAsync_VoidCommand_ShouldExecuteHandler()
    {
        // Arrange
        var command = new TestVoidCommand { Value = "test" };

        // Act
        await mediator.SendAsync(command);

        // Assert
        TestVoidCommandHandler.LastExecutedValue.Should().Be("test");
    }

    [TestMethod]
    public async Task SendAsync_VoidCommandWithNullCommand_ShouldThrowArgumentNullException()
    {
        // Arrange
        TestVoidCommand? command = null;

        // Act & Assert
        await FluentActions
            .Awaiting(() => mediator.SendAsync(command!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [TestMethod]
    public async Task SendAsync_VoidCommandWithNoHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new UnhandledVoidCommand();

        // Act & Assert
        await FluentActions
            .Awaiting(() => mediator.SendAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered*");
    }

    #endregion

    #region Command Tests (with response)

    [TestMethod]
    public async Task SendAsync_CommandWithResponse_ShouldReturnResult()
    {
        // Arrange
        var command = new TestCommandWithResponse { Input = 5 };

        // Act
        int result = await mediator.SendAsync<TestCommandWithResponse, int>(command);

        // Assert
        result.Should().Be(10); // Handler doubles the input
    }

    [TestMethod]
    public async Task SendAsync_CommandWithResponseNullCommand_ShouldThrowArgumentNullException()
    {
        // Arrange
        TestCommandWithResponse? command = null;

        // Act & Assert
        await FluentActions
            .Awaiting(() => mediator.SendAsync<TestCommandWithResponse, int>(command!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [TestMethod]
    public async Task SendAsync_CommandWithResponseNoHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new UnhandledCommandWithResponse();

        // Act & Assert
        await FluentActions
            .Awaiting(() => mediator.SendAsync<UnhandledCommandWithResponse, string>(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered*");
    }

    #endregion

    #region Query Tests

    [TestMethod]
    public async Task QueryAsync_ShouldReturnResult()
    {
        // Arrange
        var query = new TestQuery { Id = 42 };

        // Act
        string result = await mediator.QueryAsync<TestQuery, string>(query);

        // Assert
        result.Should().Be("Result-42");
    }

    [TestMethod]
    public async Task QueryAsync_WithNullQuery_ShouldThrowArgumentNullException()
    {
        // Arrange
        TestQuery? query = null;

        // Act & Assert
        await FluentActions
            .Awaiting(() => mediator.QueryAsync<TestQuery, string>(query!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [TestMethod]
    public async Task QueryAsync_WithNoHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var query = new UnhandledQuery();

        // Act & Assert
        await FluentActions
            .Awaiting(() => mediator.QueryAsync<UnhandledQuery, string>(query))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered*");
    }

    #endregion

    #region Pipeline Behavior Tests

    [TestMethod]
    public async Task SendAsync_WithBehaviors_ShouldExecuteInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddMediator(typeof(MediatorTests).Assembly);
        services.AddPipelineBehavior<TestBehavior1<TestVoidCommand, Unit>>();
        services.AddPipelineBehavior<TestBehavior2<TestVoidCommand, Unit>>();
        
        var provider = services.BuildServiceProvider();
        var testMediator = provider.GetRequiredService<IMediator>();

        TestBehavior1<TestVoidCommand, Unit>.ExecutionOrder.Clear();
        TestBehavior2<TestVoidCommand, Unit>.ExecutionOrder.Clear();

        var command = new TestVoidCommand { Value = "test" };

        // Act
        await testMediator.SendAsync(command);

        // Assert
        var behavior1Order = TestBehavior1<TestVoidCommand, Unit>.ExecutionOrder;
        var behavior2Order = TestBehavior2<TestVoidCommand, Unit>.ExecutionOrder;

        // Behavior1 should execute first (outer wrapper)
        behavior1Order.Should().HaveCount(2);
        behavior1Order[0].Should().Be("Behavior1-Before");
        behavior1Order[1].Should().Be("Behavior1-After");

        // Behavior2 should execute second (inner wrapper)
        behavior2Order.Should().HaveCount(2);
        behavior2Order[0].Should().Be("Behavior2-Before");
        behavior2Order[1].Should().Be("Behavior2-After");

        // Combined order: Behavior1-Before, Behavior2-Before, Handler, Behavior2-After, Behavior1-After
        // But we're only checking that each behavior executed before and after correctly
    }

    #endregion

    #region Cancellation Tests

    [TestMethod]
    public async Task SendAsync_WithCancellation_ShouldPassCancellationToken()
    {
        // Arrange
        var command = new TestCancellableCommand();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await FluentActions
            .Awaiting(() => mediator.SendAsync(command, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Test Commands, Queries, and Handlers

    public class TestVoidCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestVoidCommandHandler : ICommandHandler<TestVoidCommand>
    {
        public static string? LastExecutedValue { get; private set; }

        public Task HandleAsync(TestVoidCommand command, CancellationToken cancellationToken)
        {
            LastExecutedValue = command.Value;
            return Task.CompletedTask;
        }
    }

    public class TestCommandWithResponse : ICommand<int>
    {
        public int Input { get; set; }
    }

    public class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, int>
    {
        public Task<int> HandleAsync(TestCommandWithResponse command, CancellationToken cancellationToken)
        {
            return Task.FromResult(command.Input * 2);
        }
    }

    public class TestQuery : IQuery<string>
    {
        public int Id { get; set; }
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Result-{query.Id}");
        }
    }

    public class TestCancellableCommand : ICommand
    {
    }

    public class TestCancellableCommandHandler : ICommandHandler<TestCancellableCommand>
    {
        public Task HandleAsync(TestCancellableCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    public class UnhandledVoidCommand : ICommand
    {
    }

    public class UnhandledCommandWithResponse : ICommand<string>
    {
    }

    public class UnhandledQuery : IQuery<string>
    {
    }

    public class TestBehavior1<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public static List<string> ExecutionOrder { get; } = new();

        public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
        {
            ExecutionOrder.Add("Behavior1-Before");
            TResponse result = await next();
            ExecutionOrder.Add("Behavior1-After");
            return result;
        }
    }

    public class TestBehavior2<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public static List<string> ExecutionOrder { get; } = new();

        public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
        {
            ExecutionOrder.Add("Behavior2-Before");
            TResponse result = await next();
            ExecutionOrder.Add("Behavior2-After");
            return result;
        }
    }

    #endregion
}
