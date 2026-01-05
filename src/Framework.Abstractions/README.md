# VisionaryCoder.Framework.Abstractions

Core abstractions package providing foundational interfaces, base types, and patterns for the VisionaryCoder framework.

## Features

- **Zero External Dependencies**: Pure .NET 10 abstractions with no third-party dependencies
- **Core Patterns**: Result pattern, CQRS, Domain Events, Specifications
- **Service Infrastructure**: Base classes and interfaces for service implementation
- **Primitives**: Strongly-typed entity identifiers
- **Microsoft Best Practices**: Follows official Microsoft packaging guidelines

## Installation

```bash
dotnet add package VisionaryCoder.Framework.Abstractions
```

## Contents

### Service Infrastructure
- `ServiceBase<T>` - Base class for framework services
- `ServiceResult` / `ServiceResult<T>` - Operation result types
- `ServiceRequest` / `ServiceRequest<T>` - Request abstractions
- `Options` - Base configuration options

### Patterns
- `Result` / `Result<T>` - Railway-oriented programming pattern
- `Error` - Structured error representation
- `Specification<T>` - Specification pattern for business rules

### CQRS
- `ICommand` / `ICommand<TResponse>` - Command abstractions
- `IQuery<TResponse>` - Query abstractions
- `ICommandHandler<TCommand>` / `ICommandHandler<TCommand, TResponse>` - Command handlers
- `IQueryHandler<TQuery, TResponse>` - Query handlers
- `IMediator` - Mediator pattern abstraction
- `IPipelineBehavior<TRequest, TResponse>` - Pipeline behavior abstraction

### Domain Events
- `IDomainEvent` - Domain event marker interface
- `DomainEvent` - Base domain event record
- `IDomainEventHandler<TEvent>` - Event handler abstraction

### Primitives
- `IEntityId` - Entity identifier interface
- `EntityId<TEntity, TKey>` - Strongly-typed entity identifiers

### Proxy Infrastructure
- `IProxyInterceptor` - Proxy interceptor abstraction
- `IProxyTransport` - Transport layer abstraction
- `IProxyPipeline` - Pipeline abstraction

### Messaging
- `IMessage` - Message marker interface
- `IMessagePublisher` - Message publishing abstraction
- `IMessageConsumer` - Message consumption abstraction
- `IMessageBus` - Message bus abstraction

## Usage Example

```csharp
// Define a command
public record CreateUserCommand(string Email, string Name) : ICommand<Guid>;

// Implement a handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Implementation
        return Result<Guid>.Success(Guid.NewGuid());
    }
}

// Use Result pattern
var result = await mediator.Send(new CreateUserCommand("user@example.com", "John Doe"));
if (result.IsSuccess)
{
    Console.WriteLine($"User created: {result.Value}");
}
else
{
    Console.WriteLine($"Error: {result.Error.Message}");
}
```

## License

MIT License - see LICENSE file for details.
