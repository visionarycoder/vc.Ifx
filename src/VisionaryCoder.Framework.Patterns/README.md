# VisionaryCoder.Framework.Patterns

Enterprise design patterns package with zero infrastructure dependencies. Perfect for clean architecture and domain-driven design.

## 🎯 Features

- ✅ **Result Pattern** - Type-safe error handling without exceptions
- ✅ **Specification Pattern** - Reusable query logic with EF Core support
- ✅ **CQRS** - Command Query Responsibility Segregation with mediator
- ✅ **Domain Events** - Event-driven architecture foundation
- ✅ **100% Tested** - Comprehensive test coverage
- ✅ **Zero Infrastructure** - No Azure, database, or messaging dependencies
- ✅ **.NET 10 LTS** - Built for long-term support

## 📦 Installation

```bash
dotnet add package VisionaryCoder.Framework.Patterns
```

## 🚀 Quick Start

### Result Pattern

```csharp
using VisionaryCoder.Framework.Patterns;

public async Task<Result<User>> CreateUserAsync(string email)
{
    if (string.IsNullOrEmpty(email))
        return Result<User>.Failure("User.InvalidEmail", "Email is required");

    if (await ExistsAsync(email))
        return Result<User>.Failure(
            Error.Conflict("User.EmailExists", "User already exists"));

    var user = new User { Email = email };
    await SaveAsync(user);
    
    return Result<User>.Success(user);
}

// Usage
var result = await CreateUserAsync("test@example.com");

if (result.IsSuccess)
{
    Console.WriteLine($"Created user: {result.Value.Email}");
}
else
{
    Console.WriteLine($"Error: {result.Error.Message}");
}
```

### Specification Pattern

```csharp
using VisionaryCoder.Framework.Specifications;

public class ActiveUserSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.IsActive && !user.IsDeleted;
}

// Usage with EF Core
var activeUsers = await dbContext.Users
    .Where(new ActiveUserSpecification().ToExpression())
    .ToListAsync();

// Combine specifications
var premiumActiveUsers = new ActiveUserSpecification()
    .And(new PremiumUserSpecification());
```

### CQRS

```csharp
using VisionaryCoder.Framework.CQRS;

// Define command
public record CreateOrderCommand(string CustomerId, decimal Total) : ICommand<Guid>;

// Define handler
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken ct)
    {
        var order = new Order { CustomerId = command.CustomerId, Total = command.Total };
        await dbContext.Orders.AddAsync(order, ct);
        await dbContext.SaveChangesAsync(ct);
        return order.Id;
    }
}

// Use mediator
var orderId = await mediator.SendAsync(new CreateOrderCommand("CUST123", 99.99m));
```

### Domain Events

```csharp
using VisionaryCoder.Framework.Events;

// Define event
public record OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; }
}

// Define handler
public class SendOrderConfirmationHandler : IDomainEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct)
    {
        await emailService.SendOrderConfirmationAsync(@event.OrderId);
    }
}

// Dispatch event
await domainEventDispatcher.DispatchAsync(new OrderCreatedEvent 
{ 
    OrderId = order.Id, 
    CustomerId = order.CustomerId 
});
```

## 📖 Documentation

Full documentation available at [GitHub](https://github.com/visionarycoder/Framework)

## 🎓 When to Use

### Perfect For
- ✅ Clean Architecture projects
- ✅ Domain-Driven Design (DDD)
- ✅ Microservices
- ✅ CQRS applications
- ✅ Event-driven systems
- ✅ Any .NET application (Blazor, MAUI, Console, Web API)

### Benefits
- ✅ No infrastructure coupling
- ✅ Easy to test
- ✅ Minimal dependencies
- ✅ Production-ready
- ✅ Well-documented

## 📊 Package Stats

- **Dependencies**: 2 (Microsoft.Extensions only)
- **Size**: ~150 KB
- **Tests**: 105 tests with 100% coverage
- **Stability**: Production-ready

## 🤝 Related Packages

- `VisionaryCoder.Framework.Core` - Core abstractions
- `VisionaryCoder.Framework.Azure` - Azure integrations
- `VisionaryCoder.Framework.Messaging` - Message bus abstractions

## 📄 License

MIT License - see LICENSE file for details

## 🎉 Version 3.0.0

Initial release with comprehensive pattern implementations extracted from VisionaryCoder.Framework v2.0.0
