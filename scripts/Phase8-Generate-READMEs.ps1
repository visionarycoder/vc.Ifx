# README Generator Script for All Framework Packages
# Generates comprehensive README.md files for each package

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "README Generation Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$srcPath = "C:\Dev\VisionaryCoder\App.Framework\main\src"

# Package metadata
$packages = @{
    "Framework.Abstractions" = @{
        Title = "Framework.Abstractions"
        Description = "Core abstractions and contracts for VisionaryCoder Framework including CQRS patterns, Result monads, domain events, and entity primitives."
        Features = @(
            "**CQRS Patterns** - Command, Query, Mediator abstractions",
            "**Result Monad** - Functional error handling with Result<T> and Error types",
            "**Domain Events** - Event-driven architecture support",
            "**Entity Primitives** - Strongly-typed EntityId<T> for domain entities",
            "**Specifications** - Specification pattern for queries",
            "**Service Results** - Standardized service response patterns"
        )
        Usage = @"
// CQRS Command
public record CreateOrderCommand(string CustomerId, decimal Amount) : ICommand;

// Result Pattern
public Result<Order> CreateOrder(CreateOrderCommand command)
{
    if (command.Amount <= 0)
        return Result<Order>.Failure(Error.Validation("Amount must be positive"));
    
    var order = new Order(command.CustomerId, command.Amount);
    return Result<Order>.Success(order);
}

// EntityId
public class Order
{
    public EntityId<Order, Guid> Id { get; init; }
}
"@
    }
    
    "Framework.Patterns" = @{
        Title = "Framework.Patterns"
        Description = "Implementation of Gang of Four design patterns and enterprise patterns optimized for .NET 10."
        Features = @(
            "**Creational Patterns** - Factory, Builder, Singleton with DI integration",
            "**Structural Patterns** - Adapter, Decorator, Proxy, Composite",
            "**Behavioral Patterns** - Strategy, Observer, Command, Chain of Responsibility",
            "**Repository Pattern** - Generic repository with specification support",
            "**Unit of Work** - Transaction management pattern"
        )
        Usage = @"
// Repository Pattern
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(DbContext context) : base(context) { }
    
    public async Task<IEnumerable<Customer>> GetActiveCustomers()
    {
        var spec = new ActiveCustomerSpecification();
        return await GetAsync(spec);
    }
}
"@
    }
    
    "Framework.Core" = @{
        Title = "Framework.Core"
        Description = "Core utilities, extension methods, and helper classes used across the VisionaryCoder Framework."
        Features = @(
            "**Extension Methods** - String, DateTime, Collection extensions",
            "**Helper Classes** - Common utility functions",
            "**Guard Clauses** - Parameter validation helpers",
            "**Base Types** - Common base classes"
        )
        Usage = @"
// Extension Methods
string name = customer.Name.ToTitleCase();
DateTime nextMonth = DateTime.Now.AddMonths(1).StartOfMonth();

// Guard Clauses
Guard.AgainstNull(customer, nameof(customer));
Guard.AgainstNullOrEmpty(customer.Name, nameof(customer.Name));
"@
    }
    
    "Framework.DataAccess" = @{
        Title = "Framework.DataAccess"
        Description = "Data access patterns and abstractions including filtering, querying, and Azure Storage integrations."
        Features = @(
            "**Repository Pattern** - Generic repository implementations",
            "**Query Filtering** - Dynamic LINQ-based filtering with FilterNode",
            "**Specifications** - Query specification pattern",
            "**Azure Table Storage** - Table storage abstractions and utilities"
        )
        Usage = @"
// Dynamic Filtering
var filter = new FilterGroup(FilterCombination.And)
{
    Children = {
        new FilterCondition("Status", FilterOperation.Equals, "Active"),
        new FilterCondition("Amount", FilterOperation.GreaterThan, 1000)
    }
};

var customers = await repository.QueryAsync(filter);
"@
    }
    
    "Framework.EntityFrameworkCore" = @{
        Title = "Framework.EntityFrameworkCore"
        Description = "Entity Framework Core integrations including EntityId value converters, filtering, and model building extensions."
        Features = @(
            "**EntityId Converters** - Automatic conversion of strongly-typed entity IDs",
            "**EF Core Filtering** - Dynamic LINQ expression building from filter objects",
            "**Model Extensions** - Fluent API extensions for entity configuration",
            "**Query Optimizations** - Performance-optimized query patterns"
        )
        Usage = @"
// EntityId Configuration
modelBuilder.Entity<Order>()
    .Property(o => o.Id)
    .UseEntityId()
    .ValueGeneratedOnAdd();

// Dynamic Filtering
var filter = new FilterCondition("CustomerName", FilterOperation.Contains, "Smith");
var orders = await dbContext.Orders.ApplyFilter(filter).ToListAsync();
"@
    }
    
    "Framework.Messaging" = @{
        Title = "Framework.Messaging"
        Description = "Message-based communication patterns including event-driven architecture and Azure Service Bus integration."
        Features = @(
            "**Message Bus** - Pub/Sub messaging patterns",
            "**Azure Queue Storage** - Queue-based messaging",
            "**Azure Service Bus** - Enterprise messaging with topics/subscriptions",
            "**Event Publishing** - Domain event publishing and handling"
        )
        Usage = @"
// Publish Message
var message = new OrderCreatedEvent(orderId, customerId, amount);
await messageBus.PublishAsync(message);

// Subscribe to Messages
public class OrderCreatedHandler : IMessageConsumer<OrderCreatedEvent>
{
    public async Task ConsumeAsync(OrderCreatedEvent message)
    {
        // Handle the event
        await ProcessOrder(message.OrderId);
    }
}
"@
    }
    
    "Framework.Storage" = @{
        Title = "Framework.Storage"
        Description = "Blob storage abstractions and implementations for Azure Blob Storage and file system storage."
        Features = @(
            "**Blob Storage** - Azure Blob Storage abstractions",
            "**File System** - Local file system storage provider",
            "**Content Management** - Upload, download, delete operations",
            "**Streaming** - Efficient large file handling"
        )
        Usage = @"
// Upload File
await blobStorage.UploadAsync("container/file.pdf", stream, "application/pdf");

// Download File
using var stream = await blobStorage.DownloadAsync("container/file.pdf");

// Delete File
await blobStorage.DeleteAsync("container/file.pdf");
"@
    }
    
    "Framework.Observability" = @{
        Title = "Framework.Observability"
        Description = "Observability infrastructure including structured logging, distributed tracing, and metrics collection."
        Features = @(
            "**Structured Logging** - Serilog integration with JSON formatting",
            "**Distributed Tracing** - OpenTelemetry integration",
            "**Metrics Collection** - Custom metrics and counters",
            "**Application Insights** - Azure Application Insights integration",
            "**Logging Interceptors** - Automatic request/response logging"
        )
        Usage = @"
// Structured Logging
logger.LogInformation("Order created: {OrderId} for customer {CustomerId}", 
    orderId, customerId);

// Tracing
using var activity = activitySource.StartActivity("ProcessOrder");
activity?.SetTag("order.id", orderId);
activity?.SetTag("customer.id", customerId);

// Metrics
orderCounter.Add(1, new KeyValuePair<string, object>("status", "completed"));
"@
    }
    
    "Framework.Resilience" = @{
        Title = "Framework.Resilience"
        Description = "Resilience patterns including retry policies, circuit breakers, rate limiting, and health checks using Polly."
        Features = @(
            "**Retry Policies** - Polly-based retry with exponential backoff",
            "**Circuit Breakers** - Fault tolerance with circuit breaker pattern",
            "**Rate Limiting** - Request rate limiting and throttling",
            "**Bulkhead Isolation** - Resource isolation patterns",
            "**Health Checks** - SQL Server, Redis, custom health checks",
            "**Resilience Interceptors** - Automatic resilience for proxy calls"
        )
        Usage = @"
// Retry Policy
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

await retryPolicy.ExecuteAsync(() => httpClient.GetAsync(url));

// Circuit Breaker
var circuitBreaker = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// Rate Limiting
services.AddRateLimiting(options => 
{
    options.MaxRequests = 100;
    options.TimeWindow = TimeSpan.FromMinutes(1);
});
"@
    }
    
    "Framework.Security" = @{
        Title = "Framework.Security"
        Description = "Security features including authentication, authorization, JWT handling, and data protection."
        Features = @(
            "**Authentication** - JWT, OAuth 2.0, OpenID Connect support",
            "**Authorization** - Role-based and attribute-based access control",
            "**Password Hashing** - BCrypt password hashing",
            "**Data Protection** - ASP.NET Core Data Protection integration",
            "**Security Interceptors** - Automatic authentication for proxy calls",
            "**Auditing** - Security audit logging"
        )
        Usage = @"
// JWT Authentication
services.AddJwtAuthentication(options =>
{
    options.SecretKey = configuration["Jwt:SecretKey"];
    options.Issuer = "VisionaryCoder";
    options.Audience = "Framework";
});

// Password Hashing
string hashedPassword = passwordHasher.HashPassword(plainTextPassword);
bool isValid = passwordHasher.VerifyPassword(plainTextPassword, hashedPassword);

// Authorization
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteOrder(Guid orderId)
{
    await orderService.DeleteAsync(orderId);
    return Ok();
}
"@
    }
    
    "Framework.Identity" = @{
        Title = "Framework.Identity"
        Description = "User identity and access management including user management, roles, and claims."
        Features = @(
            "**User Management** - Create, update, delete users",
            "**Role Management** - Role-based access control",
            "**Claims Management** - Claims-based identity",
            "**Token Management** - JWT token generation and validation"
        )
        Usage = @"
// Create User
var user = new ApplicationUser
{
    UserName = "john.doe@example.com",
    Email = "john.doe@example.com"
};
await userManager.CreateAsync(user, password);

// Assign Role
await userManager.AddToRoleAsync(user, "Customer");

// Add Claim
await userManager.AddClaimAsync(user, new Claim("subscription", "premium"));
"@
    }
}

Write-Host "Generating README files..." -ForegroundColor Green
Write-Host ""

$generated = 0
$errors = 0

foreach ($packageName in $packages.Keys) {
    $packagePath = Join-Path $srcPath $packageName
    $readmePath = Join-Path $packagePath "README.md"
    $metadata = $packages[$packageName]
    
    if (-not (Test-Path $packagePath)) {
        Write-Host "  ⊘ Skipped: $packageName (folder not found)" -ForegroundColor Gray
        continue
    }
    
    try {
        $features = $metadata.Features | ForEach-Object { "- $_" }
        $featuresText = $features -join "`n"
        
        $readme = @"
# $($metadata.Title)

$($metadata.Description)

## Features

$featuresText

## Installation

``````bash
dotnet add package VisionaryCoder.$packageName
``````

## Quick Start

``````csharp
$($metadata.Usage)
``````

## Package Dependencies

- **.NET 10** - Target framework
- **C# 12** - Latest language features
- **Nullable Reference Types** - Enabled

## Documentation

For complete documentation, see the [Framework Documentation](https://github.com/visionarycoder/Framework/wiki).

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## Contributing

Contributions are welcome! Please read our [Contributing Guide](../../CONTRIBUTING.md) for details.

## Support

- **Issues:** [GitHub Issues](https://github.com/visionarycoder/Framework/issues)
- **Discussions:** [GitHub Discussions](https://github.com/visionarycoder/Framework/discussions)

## Version

**Current Version:** 1.0.0

---

*Part of the VisionaryCoder Framework - Building the future, one pattern at a time.*
"@
        
        Set-Content -Path $readmePath -Value $readme -NoNewline
        Write-Host "  ✓ Generated: $packageName/README.md" -ForegroundColor Green
        $generated++
    }
    catch {
        Write-Host "  ✗ FAILED: $packageName - $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "README Generation Complete!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Statistics:" -ForegroundColor Yellow
Write-Host "  READMEs generated: $generated" -ForegroundColor Green
Write-Host "  Errors: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Review generated README files" -ForegroundColor White
Write-Host "2. Customize package-specific content as needed" -ForegroundColor White
Write-Host "3. Add package-specific examples" -ForegroundColor White
Write-Host "4. Commit README files to Git" -ForegroundColor White
Write-Host ""
