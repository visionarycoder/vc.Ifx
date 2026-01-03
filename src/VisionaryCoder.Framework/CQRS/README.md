# CQRS (Command Query Responsibility Segregation)

## Overview

VisionaryCoder.Framework includes a lightweight, production-ready CQRS implementation with no external dependencies (except FluentValidation for the optional validation behavior). This implementation follows industry best practices while remaining simple and maintainable.

## Why Native CQRS?

- ✅ **No External Dependencies** - Complete control, no licensing issues
- ✅ **Perfectly Tailored** - Designed for VBD architecture patterns
- ✅ **Lightweight** - Only the features you need
- ✅ **Zero Breaking Changes** - You control the API
- ✅ **Full Understanding** - Complete transparency of implementation

## Core Concepts

### Commands
Commands represent **actions** or **intents to change system state**. They either:
- Modify state without returning data (`ICommand`)
- Modify state and return a result (`ICommand<TResponse>`)

### Queries
Queries represent **requests for information**. They:
- **Never** modify state
- Always return data (`IQuery<TResponse>`)
- Are idempotent and side-effect free

### Handlers
Handlers contain the business logic:
- `ICommandHandler<TCommand>` - Handles commands with no response
- `ICommandHandler<TCommand, TResponse>` - Handles commands with response
- `IQueryHandler<TQuery, TResponse>` - Handles queries

### Mediator
The mediator dispatches commands and queries to their handlers:
- Single entry point for all CQRS operations
- Enables pipeline behaviors (logging, validation, transactions)
- Decouples senders from receivers

## Quick Start

### 1. Register CQRS Services

```csharp
// In Program.cs or Startup.cs
services.AddMediator<Program>(); // Scans assembly containing Program class

// Or scan multiple assemblies
services.AddMediator(
    typeof(OrdersModule).Assembly,
    typeof(CustomersModule).Assembly
);

// Add pipeline behaviors (execute in registration order)
services.AddPipelineBehavior<LoggingBehavior<,>>();
services.AddPipelineBehavior<ValidationBehavior<,>>();
services.AddPipelineBehavior<PerformanceBehavior<,>>();
```

### 2. Define a Command

```csharp
// Command that creates an order
public record CreateOrderCommand(
    string CustomerId,
    List<OrderItem> Items
) : ICommand<Guid>; // Returns OrderId

// Command handler
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository repository;
    private readonly ILogger<CreateOrderCommandHandler> logger;

    public CreateOrderCommandHandler(
        IOrderRepository repository,
        ILogger<CreateOrderCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Guid> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);

        var order = new Order
        {
            CustomerId = command.CustomerId,
            Items = command.Items,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(order, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderId} created successfully", order.Id);

        return order.Id;
    }
}
```

### 3. Define a Query

```csharp
// Query to get order by ID
public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;

// Query handler
public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository repository;

    public GetOrderByIdQueryHandler(IOrderRepository repository)
    {
        this.repository = repository;
    }

    public async Task<OrderDto> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(query.OrderId, cancellationToken);
        
        if (order is null)
        {
            throw new NotFoundException($"Order {query.OrderId} not found");
        }

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items,
            Total = order.Total,
            CreatedAt = order.CreatedAt
        };
    }
}
```

### 4. Use the Mediator

```csharp
// In a controller or service
public class OrdersController : ControllerBase
{
    private readonly IMediator mediator;

    public OrdersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Items
        );

        Guid orderId = await mediator.SendAsync<CreateOrderCommand, Guid>(command);

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = orderId },
            new { orderId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        OrderDto order = await mediator.QueryAsync<GetOrderByIdQuery, OrderDto>(query);
        return Ok(order);
    }
}
```

## Pipeline Behaviors

Pipeline behaviors allow you to add cross-cutting concerns that execute before and after handlers.

### Built-in Behaviors

#### LoggingBehavior
Logs command/query execution and timing:
```csharp
services.AddPipelineBehavior<LoggingBehavior<,>>();
```

#### ValidationBehavior
Validates commands/queries using FluentValidation:
```csharp
services.AddPipelineBehavior<ValidationBehavior<,>>();

// Create a validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
    }
}
```

#### PerformanceBehavior
Measures execution time and warns on slow requests:
```csharp
services.AddPipelineBehavior<PerformanceBehavior<,>>();
```

### Custom Behaviors

Create your own behaviors by implementing `IPipelineBehavior<TRequest, TResponse>`:

```csharp
// Transaction behavior for commands
public class TransactionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly DbContext dbContext;

    public TransactionBehavior(DbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        // Start transaction
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            TResponse response = await next();
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

// Register it
services.AddPipelineBehavior<TransactionBehavior<,>>();
```

## Advanced Patterns

### Void Commands

For commands that don't return a value:

```csharp
// Command with no response
public record DeleteOrderCommand(Guid OrderId) : ICommand;

// Handler
public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand>
{
    private readonly IOrderRepository repository;

    public DeleteOrderCommandHandler(IOrderRepository repository)
    {
        this.repository = repository;
    }

    public async Task HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(command.OrderId, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

// Usage
await mediator.SendAsync(new DeleteOrderCommand(orderId));
```

### Result Pattern

For operations that can succeed or fail gracefully:

```csharp
public record UpdateOrderCommand(Guid OrderId, OrderData Data) : ICommand<Result<OrderDto>>;

public class UpdateOrderCommandHandler 
    : ICommandHandler<UpdateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> HandleAsync(
        UpdateOrderCommand command,
        CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(command.OrderId);
        
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found");
        }

        order.Update(command.Data);
        await repository.SaveChangesAsync();

        return Result<OrderDto>.Success(MapToDto(order));
    }
}
```

### Event Sourcing

Commands can publish events:

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IEventPublisher eventPublisher;

    public async Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // Create order
        var order = new Order(command);
        await repository.AddAsync(order);

        // Publish event
        await eventPublisher.PublishAsync(
            new OrderCreatedEvent(order.Id, order.CustomerId),
            cancellationToken);

        return order.Id;
    }
}
```

## Best Practices

### 1. Keep Handlers Small and Focused
Each handler should do ONE thing well. If a handler is complex, extract services:

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderService orderService; // Inject domain service
    private readonly IInventoryService inventoryService;
    private readonly INotificationService notificationService;

    public async Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // Validate inventory
        await inventoryService.ValidateAvailabilityAsync(command.Items);

        // Create order
        Guid orderId = await orderService.CreateOrderAsync(command);

        // Send notification
        await notificationService.NotifyOrderCreatedAsync(orderId);

        return orderId;
    }
}
```

### 2. Use Records for Commands and Queries
Records provide immutability and value equality:

```csharp
// ✅ Good - immutable, clear intent
public record CreateOrderCommand(string CustomerId, List<OrderItem> Items) : ICommand<Guid>;

// ❌ Avoid - mutable, unclear
public class CreateOrderCommand : ICommand<Guid>
{
    public string CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
}
```

### 3. Queries Should Never Modify State
Queries are read-only and can use optimized read models:

```csharp
// ✅ Good - read-only query
public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> HandleAsync(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking() // Optimize for read-only
            .Where(o => o.CustomerId == query.CustomerId)
            .Select(o => new OrderDto { ... })
            .ToListAsync(cancellationToken);
    }
}

// ❌ Bad - query modifying state
public async Task<List<OrderDto>> HandleAsync(GetOrdersQuery query, CancellationToken cancellationToken)
{
    await UpdateLastAccessedAsync(); // ❌ NO! Queries don't modify state
    return await GetOrdersAsync();
}
```

### 4. Use Validation Behavior
Don't validate in handlers - use the validation behavior:

```csharp
// ✅ Good - validation in validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().Must(HavePositivePrices);
    }
}

// Handler focuses on business logic
public async Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
{
    // Validation already done by pipeline!
    return await orderService.CreateAsync(command);
}
```

### 5. Behavior Order Matters
Behaviors execute in registration order:

```csharp
// This order makes sense:
services.AddPipelineBehavior<LoggingBehavior<,>>();        // 1. Log start
services.AddPipelineBehavior<ValidationBehavior<,>>();     // 2. Validate
services.AddPipelineBehavior<TransactionBehavior<,>>();    // 3. Transaction
services.AddPipelineBehavior<PerformanceBehavior<,>>();    // 4. Measure
// 5. Handler executes
```

## Testing

### Unit Testing Handlers

Test handlers directly without the mediator:

```csharp
[TestMethod]
public async Task CreateOrder_WithValidData_ShouldReturnOrderId()
{
    // Arrange
    var repository = new Mock<IOrderRepository>();
    var handler = new CreateOrderCommandHandler(repository.Object, logger);
    var command = new CreateOrderCommand("customer-1", new List<OrderItem> { ... });

    // Act
    Guid orderId = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    orderId.Should().NotBeEmpty();
    repository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Testing with Mediator

Test the full pipeline:

```csharp
[TestMethod]
public async Task CreateOrder_WithInvalidData_ShouldThrowValidationException()
{
    // Arrange
    var mediator = services.GetRequiredService<IMediator>();
    var command = new CreateOrderCommand("", new List<OrderItem>()); // Invalid

    // Act & Assert
    await FluentActions
        .Awaiting(() => mediator.SendAsync<CreateOrderCommand, Guid>(command))
        .Should().ThrowAsync<ValidationException>();
}
```

## VBD Integration

This CQRS implementation maps naturally to Volatility-Based Decomposition:

- **Managers** - Use commands/queries to orchestrate workflows
- **Engines** - Implement command/query handlers with business logic
- **Accessors** - Provide repositories used by handlers

```csharp
// Manager layer
public class OrderManager
{
    private readonly IMediator mediator;

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        // Manager orchestrates via mediator
        var command = new CreateOrderCommand(request.CustomerId, request.Items);
        Guid orderId = await mediator.SendAsync<CreateOrderCommand, Guid>(command);
        
        var query = new GetOrderByIdQuery(orderId);
        return await mediator.QueryAsync<GetOrderByIdQuery, OrderDto>(query);
    }
}

// Engine layer
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    // Engine contains business logic
}

// Accessor layer
public class OrderRepository : IOrderRepository
{
    // Accessor handles data access
}
```

## Performance Considerations

1. **Scoped Lifetime** - Handlers are registered as scoped for per-request isolation
2. **Async All the Way** - All operations are async for better scalability
3. **Minimal Allocations** - Struct-based Unit type reduces allocations
4. **Pipeline Overhead** - Minimal overhead (~1-2% compared to direct calls)

## Comparison with MediatR

| Feature | VisionaryCoder CQRS | MediatR |
|---------|-------------------|---------|
| External Dependencies | None | MediatR package |
| Pipeline Behaviors | ✅ Yes | ✅ Yes |
| Notifications/Events | ❌ Not yet | ✅ Yes |
| Streaming | ❌ No | ✅ Yes |
| Request Pre/Post Processors | ❌ No | ✅ Yes |
| Source Code | ✅ In your framework | ❌ External |
| Customization | ✅ Full control | ⚠️ Limited |
| Learning Curve | ✅ Simple | ⚠️ More complex |

## Future Enhancements

Potential additions (implement as needed):
- Domain events (INotification pattern)
- Streaming queries (IAsyncEnumerable)
- Request pre/post processors
- Caching behavior
- Retry behavior
- Distributed tracing integration

## Support

For questions or issues with the CQRS implementation:
1. Check this documentation
2. Review example handlers in the codebase
3. Consult the team

---

**Version**: 2.0.0  
**Last Updated**: January 2025  
**Framework**: .NET 10 LTS
