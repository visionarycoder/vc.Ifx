using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Patterns.CQRS;
using VisionaryCoder.Framework.Patterns.CQRS.Abstractions;

namespace VisionaryCoder.Framework.Patterns.Tests.CQRS;

/// <summary>
/// Unit tests for ServiceCollectionExtensions.
/// </summary>
[TestClass]
public class ServiceCollectionExtensionsTests
{
    #region AddMediator Tests

    [TestMethod]
    public void AddMediator_ShouldRegisterMediator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging dependency

        // Act
        services.AddMediator(typeof(ServiceCollectionExtensionsTests).Assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
        mediator.Should().BeOfType<Mediator>();
    }

    [TestMethod]
    public void AddMediator_ShouldRegisterMediatorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging dependency
        services.AddMediator(typeof(ServiceCollectionExtensionsTests).Assembly);

        // Act
        var provider = services.BuildServiceProvider();
        IMediator? mediator1;
        IMediator? mediator2;
        IMediator? mediator3;

        using (var scope1 = provider.CreateScope())
        {
            mediator1 = scope1.ServiceProvider.GetService<IMediator>();
            mediator2 = scope1.ServiceProvider.GetService<IMediator>();
        }

        using (var scope2 = provider.CreateScope())
        {
            mediator3 = scope2.ServiceProvider.GetService<IMediator>();
        }

        // Assert
        mediator1.Should().BeSameAs(mediator2); // Same within scope
        mediator1.Should().NotBeSameAs(mediator3); // Different across scopes
    }

    [TestMethod]
    public void AddMediator_ShouldDiscoverAndRegisterHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(typeof(ServiceCollectionExtensionsTests).Assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        
        var commandHandler = provider.GetService<ICommandHandler<TestCommand>>();
        commandHandler.Should().NotBeNull();
        commandHandler.Should().BeOfType<TestCommandHandler>();

        var commandWithResponseHandler = provider.GetService<ICommandHandler<TestCommandWithResponse, string>>();
        commandWithResponseHandler.Should().NotBeNull();
        commandWithResponseHandler.Should().BeOfType<TestCommandWithResponseHandler>();

        var queryHandler = provider.GetService<IQueryHandler<TestQuery, int>>();
        queryHandler.Should().NotBeNull();
        queryHandler.Should().BeOfType<TestQueryHandler>();
    }

    [TestMethod]
    public void AddMediator_WithTypeMarker_ShouldScanCorrectAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator<ServiceCollectionExtensionsTests>();

        // Assert
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    [TestMethod]
    public void AddMediator_WithNoAssemblies_ShouldUseCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(); // No assemblies specified

        // Assert
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    [TestMethod]
    public void AddMediator_WithMultipleAssemblies_ShouldScanAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(
            typeof(ServiceCollectionExtensionsTests).Assembly,
            typeof(IMediator).Assembly
        );

        // Assert
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    #endregion

    #region AddPipelineBehavior Tests

    [TestMethod]
    public void AddPipelineBehavior_ShouldRegisterBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(typeof(ServiceCollectionExtensionsTests).Assembly);

        // Act
        services.AddPipelineBehavior<TestPipelineBehavior>();

        // Assert
        var provider = services.BuildServiceProvider();
        var behaviors = provider.GetServices<IPipelineBehavior<TestCommand, Unit>>();
        behaviors.Should().NotBeEmpty();
        behaviors.Should().Contain(b => b is TestPipelineBehavior);
    }

    [TestMethod]
    public void AddPipelineBehavior_MultipleBehaviors_ShouldRegisterAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(typeof(ServiceCollectionExtensionsTests).Assembly);

        // Act
        services.AddPipelineBehavior<TestPipelineBehavior>();
        services.AddPipelineBehavior<AnotherTestPipelineBehavior>();

        // Assert
        var provider = services.BuildServiceProvider();
        var behaviors = provider.GetServices<IPipelineBehavior<TestCommand, Unit>>().ToList();
        behaviors.Should().HaveCount(2);
        behaviors.Should().Contain(b => b is TestPipelineBehavior);
        behaviors.Should().Contain(b => b is AnotherTestPipelineBehavior);
    }

    [TestMethod]
    public void AddPipelineBehavior_ShouldBeScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(typeof(ServiceCollectionExtensionsTests).Assembly);
        services.AddPipelineBehavior<TestPipelineBehavior>();

        // Act
        var provider = services.BuildServiceProvider();
        IPipelineBehavior<TestCommand, Unit>? behavior1;
        IPipelineBehavior<TestCommand, Unit>? behavior2;
        IPipelineBehavior<TestCommand, Unit>? behavior3;

        using (var scope1 = provider.CreateScope())
        {
            behavior1 = scope1.ServiceProvider.GetServices<IPipelineBehavior<TestCommand, Unit>>().FirstOrDefault();
            behavior2 = scope1.ServiceProvider.GetServices<IPipelineBehavior<TestCommand, Unit>>().FirstOrDefault();
        }

        using (var scope2 = provider.CreateScope())
        {
            behavior3 = scope2.ServiceProvider.GetServices<IPipelineBehavior<TestCommand, Unit>>().FirstOrDefault();
        }

        // Assert
        behavior1.Should().NotBeNull();
        behavior1.Should().BeSameAs(behavior2); // Same within scope
        behavior1.Should().NotBeSameAs(behavior3); // Different across scopes
    }

    #endregion

    #region Test Types

    public class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCommandWithResponse : ICommand<string>
    {
        public int Value { get; set; }
    }

    public class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, string>
    {
        public Task<string> HandleAsync(TestCommandWithResponse command, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Response-{command.Value}");
        }
    }

    public class TestQuery : IQuery<int>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public Task<int> HandleAsync(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(42);
        }
    }

    public class TestPipelineBehavior : IPipelineBehavior<TestCommand, Unit>
    {
        public async Task<Unit> HandleAsync(TestCommand request, Func<Task<Unit>> next, CancellationToken cancellationToken)
        {
            return await next();
        }
    }

    public class AnotherTestPipelineBehavior : IPipelineBehavior<TestCommand, Unit>
    {
        public async Task<Unit> HandleAsync(TestCommand request, Func<Task<Unit>> next, CancellationToken cancellationToken)
        {
            return await next();
        }
    }

    #endregion
}
