# VisionaryCoder.Framework.EntityFrameworkCore

Entity Framework Core integrations for VisionaryCoder Framework, providing production-ready extensions and patterns.

## Features

### EntityId Value Converters
- Automatic conversion of strongly-typed `EntityId<T>` to database primitives
- Support for GUID, int, long, and string-based entity identifiers
- Seamless integration with EF Core change tracking

### Model Building Extensions
- Fluent API extensions for EntityId configuration
- Convention-based entity configuration
- Audit field auto-configuration (CreatedAt, ModifiedAt, etc.)

### Filtering & Querying
- Dynamic LINQ expression building from filter objects
- Type-safe querying with strongly-typed filters
- JSON schema validation for query filters
- Integration with specification pattern

### Database Providers
- SQL Server optimizations
- Azure Cosmos DB support
- PostgreSQL with snake_case naming conventions
- In-memory database for testing

## Installation

```bash
dotnet add package VisionaryCoder.Framework.EntityFrameworkCore
```

## Quick Start

### EntityId Configuration

```csharp
public class Customer
{
    public EntityId<Customer> Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Automatically configure all EntityId properties
        modelBuilder.ConfigureEntityIds();
        
        // Or configure individually
        modelBuilder.Entity<Customer>()
            .Property(e => e.Id)
            .HasEntityIdConversion();
    }
}
```

### Dynamic Filtering

```csharp
public class CustomerRepository
{
    private readonly AppDbContext context;
    
    public async Task<IEnumerable<Customer>> GetFilteredCustomersAsync(Filter filter)
    {
        var query = context.Customers.AsQueryable();
        
        // Apply dynamic filter
        query = query.ApplyFilter(filter);
        
        return await query.ToListAsync();
    }
}

// Usage
var filter = new Filter
{
    Conditions = new[]
    {
        new FilterCondition { Property = "Name", Operator = "Contains", Value = "Smith" },
        new FilterCondition { Property = "CreatedAt", Operator = "GreaterThan", Value = DateTime.Now.AddDays(-30) }
    },
    Combination = FilterCombination.And
};

var customers = await repository.GetFilteredCustomersAsync(filter);
```

### Query Filter Serialization

```csharp
// Serialize complex queries to JSON
var queryFilter = new QueryFilter
{
    Filters = new[]
    {
        new PropertyFilter 
        { 
            Property = "Status", 
            Operator = "Equals", 
            Value = "Active" 
        }
    },
    Sorting = new[] { new SortDescriptor { Property = "Name", Direction = "Asc" } },
    Pagination = new PageRequest { PageNumber = 1, PageSize = 20 }
};

string json = queryFilter.Serialize();

// Deserialize and execute
var restored = QueryFilter.Deserialize(json);
var results = await context.Customers.ApplyQueryFilter(restored).ToListAsync();
```

### Specification Pattern with EF Core

```csharp
public class ActiveCustomerSpecification : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return customer => customer.Status == CustomerStatus.Active 
                        && customer.DeletedAt == null;
    }
}

// Usage
var spec = new ActiveCustomerSpecification();
var activeCustomers = await context.Customers
    .Where(spec.ToExpression())
    .ToListAsync();
```

## Configuration

### SQL Server with Conventions

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .UseEntityIdConversions()
           .EnableSensitiveDataLogging(isDevelopment)
           .EnableDetailedErrors(isDevelopment);
});
```

### PostgreSQL with Snake Case

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
           .UseEntityIdConversions();
});
```

### Cosmos DB

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseCosmos(endpoint, authKey, databaseName)
           .UseEntityIdConversions();
});
```

## Advanced Features

### Audit Fields

```csharp
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}

// Auto-populate audit fields
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ConfigureAuditFields();
}
```

### Soft Delete

```csharp
public abstract class SoftDeletableEntity
{
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

// Global query filter for soft delete
modelBuilder.Entity<Customer>()
    .HasQueryFilter(e => e.DeletedAt == null);
```

### Optimistic Concurrency

```csharp
public class Customer
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

## Testing

### In-Memory Database

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("TestDb")
    .UseEntityIdConversions()
    .Options;

using var context = new AppDbContext(options);
// Run tests...
```

## Performance Tips

### Query Optimization
- Use `AsNoTracking()` for read-only queries
- Enable query splitting for collections
- Use compiled queries for frequently executed queries
- Implement proper indexing strategies

### Batch Operations
```csharp
// Batch insert
await context.BulkInsertAsync(customers);

// Batch update
await context.BulkUpdateAsync(customers);
```

## Dependencies

This package depends on:
- `VisionaryCoder.Framework` - Base types and abstractions
- Entity Framework Core 10.0.1
- EFCore.NamingConventions

## Best Practices

### Migration Strategy
- Use code-first migrations in development
- Generate SQL scripts for production deployments
- Never run migrations automatically in production
- Test migrations on production copy first

### Connection Management
- Use connection pooling (enabled by default)
- Configure appropriate timeouts
- Implement retry logic for transient failures
- Monitor active connections

### Performance Monitoring
- Enable query logging in development
- Use `ToQueryString()` to inspect generated SQL
- Monitor slow queries with Application Insights
- Implement query performance budgets

## Version Compatibility

| Framework.EntityFrameworkCore | .NET Version | EF Core Version |
|-------------------------------|--------------|-----------------|
| 1.0.0                        | .NET 10 LTS  | 10.0.1          |

## License

MIT License - see LICENSE file for details

## Support

- GitHub Issues: https://github.com/visionarycoder/Framework/issues
- Documentation: https://github.com/visionarycoder/Framework/wiki
