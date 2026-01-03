// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Abstractions.CQRS;
using VisionaryCoder.Framework.Core.CQRS;

namespace VisionaryCoder.Framework.Core.Tests.CQRS;

[TestClass]
public class MediatorTests
{
    // Test command and handler
    private record TestCommand(string Value) : ICommand;

    private class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public bool WasCalled { get; private set; }
        public string? ReceivedValue { get; private set; }

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedValue = command.Value;
            return Task.CompletedTask;
        }
    }

    // Test command with response
    private record TestCommandWithResponse(int Value) : ICommand<string>;

    private class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, string>
    {
        public Task<string> HandleAsync(TestCommandWithResponse command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Result: {command.Value}");
        }
    }

    // Test query
    private record TestQuery(int Id) : IQuery<string>;

    private class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Query result for ID: {query.Id}");
        }
    }

    [TestMethod]
    public async Task SendAsync_VoidCommand_ShouldInvokeHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = new TestCommandHandler();
        services.AddScoped<ICommandHandler<TestCommand>>(_ => handler);
        services.AddScoped<IMediator, Mediator>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand("test-value");

        // Act
        await mediator.SendAsync(command);

        // Assert
        handler.WasCalled.Should().BeTrue();
        handler.ReceivedValue.Should().Be("test-value");
    }

    [TestMethod]
    public async Task SendAsync_CommandWithResponse_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommandWithResponse, string>, TestCommandWithResponseHandler>();
        services.AddScoped<IMediator, Mediator>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommandWithResponse(42);

        // Act
        string result = await mediator.SendAsync<TestCommandWithResponse, string>(command);

        // Assert
        result.Should().Be("Result: 42");
    }

    [TestMethod]
    public async Task QueryAsync_Query_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();
        services.AddScoped<IMediator, Mediator>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var query = new TestQuery(123);

        // Act
        string result = await mediator.QueryAsync<TestQuery, string>(query);

        // Assert
        result.Should().Be("Query result for ID: 123");
    }

    [TestMethod]
    public async Task SendAsync_NoHandlerRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand("test");

        // Act
        Func<Task> act = async () => await mediator.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered*");
    }

    [TestMethod]
    public void Unit_Value_ShouldReturnSameInstance()
    {
        // Act
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Assert
        unit1.Should().Be(unit2);
        (unit1 == unit2).Should().BeTrue();
    }

    [TestMethod]
    public void Unit_Equals_ShouldAlwaysBeTrue()
    {
        // Arrange
        var unit1 = new Unit();
        var unit2 = new Unit();

        // Assert
        unit1.Equals(unit2).Should().BeTrue();
        unit1.Equals(Unit.Value).Should().BeTrue();
    }
}
