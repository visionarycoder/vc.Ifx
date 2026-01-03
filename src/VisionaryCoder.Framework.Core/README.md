# VisionaryCoder.Framework.Core

Core implementations package providing lightweight, reusable implementations of framework patterns.

## Features

- **Minimal Dependencies**: Only depends on `Framework.Abstractions` and `Microsoft.Extensions.DependencyInjection.Abstractions`
- **CQRS Implementation**: In-memory Mediator with pipeline behavior support
- **Domain Events**: Simple domain event dispatcher
- **Specifications**: Reusable specification implementations
- **Enumeration Pattern**: Type-safe enumeration base class
- **Provider Implementations**: Correlation ID, Request ID, Framework Info providers
- **Microsoft Best Practices**: Clean architecture, dependency injection, testability

## Installation

```bash
dotnet add package VisionaryCoder.Framework.Core
```

## Contents

### CQRS (`CQRS/`)
- `Mediator` - In-memory mediator implementation
- `ServiceCollectionExtensions` - DI registration helpers

### Domain Events (`Events/`)
- `DomainEventDispatcher` - Simple synchronous event dispatcher
- `DomainEventServiceCollectionExtensions` - DI registration

### Models (`Models/`)
- `Enumeration` - Type-safe enumeration base class

### Providers (`Providers/`)
- `CorrelationIdProvider` - Correlation ID generation and tracking
- `RequestIdProvider` - Request ID generation
- `FrameworkInfoProvider` - Framework metadata provider

### Extensions (`Extensions/`)
- `CollectionExtensions` - Collection utility methods
- `DictionaryExtensions` - Dictionary helpers
- `HashSetExtensions` - HashSet utilities
- `EnumerableExtensions` - LINQ extensions
- `DateTimeExtensions` - DateTime helpers
- `ReflectionExtensions` - Reflection utilities
- `TypeExtensions` - Type inspection helpers

## Usage Examples

### CQRS with Mediator

```csharp
// Register services
services.AddMediator(typeof(Program).Assembly);

// Define a command
public record CreateUserCommand(string Email, string Name) : ICommand<Guid>;

// Implement handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid();
        // ... save user
        return userId;
    }
}

// Use mediator
var userId = await mediator.SendAsync(new CreateUserCommand("user@example.com", "John Doe"));
```

### Domain Events

```csharp
// Register dispatcher
services.AddDomainEventDispatcher();

// Define event
public record UserCreatedEvent(Guid UserId, string Email) : DomainEvent;

// Implement handler
public class UserCreatedEventHandler : IDomainEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Send welcome email, etc.
    }
}

// Dispatch event
await dispatcher.DispatchAsync(new UserCreatedEvent(userId, email));
```

### Enumeration Pattern

```csharp
public class OrderStatus : Enumeration
{
    public static readonly OrderStatus Pending = new(1, nameof(Pending));
    public static readonly OrderStatus Confirmed = new(2, nameof(Confirmed));
    public static readonly OrderStatus Shipped = new(3, nameof(Shipped));
    public static readonly OrderStatus Delivered = new(4, nameof(Delivered));

    protected OrderStatus(int id, string name) : base(id, name) { }
}

// Usage
var status = OrderStatus.Confirmed;
var allStatuses = Enumeration.GetAll<OrderStatus>();
var fromId = Enumeration.FromValue<OrderStatus>(2); // Confirmed
```

### Providers

```csharp
// Register providers
services.AddFrameworkProviders();

// Use correlation ID
var correlationId = correlationIdProvider.Get(); // Gets or creates correlation ID
correlationIdProvider.Set("custom-correlation-id");

// Use request ID
var requestId = requestIdProvider.Get(); // Generates unique request ID

// Get framework info
var version = frameworkInfoProvider.GetVersion();
var name = frameworkInfoProvider.GetName();
```

## Dependencies

- **VisionaryCoder.Framework.Abstractions** (>= 1.0.0)
- **Microsoft.Extensions.DependencyInjection.Abstractions** (>= 10.0.0)

## License

MIT License - see LICENSE file for details.
