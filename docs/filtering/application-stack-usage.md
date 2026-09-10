---
title: Using FilterSpec and QuerySpec in an Application Stack
doc_type: guide
status: active
last_updated: 2026-09-10
---
# Using FilterSpec and QuerySpec in an Application Stack

This guide places `FilterSpec<T>` (`vc.Ifx.Filtering`) and `QuerySpec<T>`
(`vc.Ifx.Querying`) inside a typical layered application: a Client layer
(Web API), a Manager layer (business logic) and an Access layer (EF Core or
POCO storage). See [filter-contract.md](./filter-contract.md) and
[query-contract.md](./query-contract.md) for the underlying semantics.

## Layer Responsibilities

| Layer | Owns | Package |
|---|---|---|
| Client (Web API) | Accepts external filter criteria, deserializes a portable `FilterNode` or DTO, never trusts raw predicates | `vc.Ifx.Querying.Serialization` |
| Manager (business logic) | Composes reusable `FilterSpec<T>` predicates; builds `QuerySpec<T>` for ordering, paging and projection | `vc.Ifx.Filtering`, `vc.Ifx.Querying` |
| Access (data) | Applies the specification to a query provider (EF Core `DbSet<T>` or in-memory collection); owns context lifetime, tracking, cancellation | `vc.Ifx.Filtering.EntityFrameworkCore` |

Filtering owns predicates only. Querying adds ordering, pagination and
projection around a `FilterSpec<T>`. EF Core integration owns provider
translation. Keep these concerns in their respective layers; do not let the
Access layer build ad hoc predicates or the Client layer execute queries
directly.

## 1. Access Layer: Define Reusable Predicates

Define `FilterSpec<T>` instances close to the entity they describe. Combine
them with `Where`, `And`, `Or` and `Not`; each call returns a new immutable
specification.

```csharp
public static class CustomerFilters
{
    public static FilterSpec<Customer> Active { get; } =
        new FilterSpec<Customer>(customer => customer.IsActive);

    public static FilterSpec<Customer> AdultsOnly { get; } =
        FilterSpec<Customer>.All.Where(customer => customer.Age >= 18);

    public static FilterSpec<Customer> HasHighValueOrder(decimal threshold) =>
        new(customer => customer.Orders.Any(order => order.Total > threshold));
}
```

## 2. Manager Layer: Compose Query Shape

Combine predicates and add ordering, pagination and projection with
`QuerySpec<T>`. `QuerySpec<T>` never enumerates the query; it only shapes it.
Pagination requires an explicit `OrderBy` so applications supply a unique
tie-breaker for stable pages.

```csharp
public sealed class CustomerQueryManager(ICustomerRepository repository)
{
    public async Task<IReadOnlyList<CustomerSummary>> GetActiveAdultsAsync(
        int pageOffset, int pageSize, CancellationToken cancellationToken)
    {
        var specification = new QuerySpec<Customer>()
            .Where(CustomerFilters.Active.And(CustomerFilters.AdultsOnly))
            .OrderBy(customer => customer.Name)
            .ThenBy(customer => customer.Id)
            .Page(pageOffset, pageSize)
            .Select(customer => new CustomerSummary(customer.Id, customer.Name));

        return await repository.QueryAsync(specification, cancellationToken);
    }
}
```

## 3. Access Layer: Apply Against EF Core

The Access layer applies the specification to the provider-owned `IQueryable<T>`
and performs enumeration, cancellation and tracking. Reference
`vc.Ifx.Filtering.EntityFrameworkCore` when portable `FilterNode` trees (built
from external input) need EF Core translation via `EfFilterExecutionStrategy`.

```csharp
public sealed class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public async Task<IReadOnlyList<TResult>> QueryAsync<TResult>(
        QuerySpec<Customer, TResult> specification, CancellationToken cancellationToken) =>
        await specification.Apply(dbContext.Customers).ToListAsync(cancellationToken);
}
```

For an in-memory or test double, apply the same `FilterSpec<T>` to an
`IEnumerable<T>`:

```csharp
IEnumerable<Customer> matches = CustomerFilters.Active.Apply(inMemoryCustomers);
```

## 4. Client Layer: Accept External Filter Criteria Safely

Never deserialize an external payload directly into an executable predicate.
Accept the version-1 structural JSON format, validate it against the embedded
schema, then rehydrate it through `vc.Ifx.Querying.Serialization` before
combining it with server-owned specifications. Enforce a field allowlist and
size/depth limits before accepting any externally supplied filter.

```csharp
[HttpPost("customers/search")]
public async Task<ActionResult<IReadOnlyList<CustomerSummary>>> Search(
    [FromBody] string filterJson, CancellationToken cancellationToken)
{
    FilterNode? externalFilter = QueryFilterSerializer.Deserialize(filterJson);
    CustomerFieldAllowlist.Validate(externalFilter);

    FilterSpec<Customer> predicate = FilterExpression
        .Create<Customer>(externalFilter)
        .AsFilterSpec(); // wrap the translated predicate as needed by the Manager API

    return Ok(await customerQueryManager.SearchAsync(predicate, cancellationToken));
}
```

Snapshot a server-built `FilterSpec<T>` back to a portable `FilterNode` with
`ToFilterNode()` when it needs to cross a process boundary (for example,
returning the effective filter to a caller for audit or caching).

## Composition Rules Across Layers

- `FilterSpec<T>` composition substitutes parameters; it never invokes or
  compiles a predicate before `Apply`.
- `QuerySpec<T>.Apply` never executes, compiles, or disposes a context; the
  Access layer owns enumeration, cancellation and transactions.
- Reapplying `OrderBy` replaces the ordering; reapplying `Page` replaces the
  window. Treat each call as producing a new specification, not a mutation.
- Unsupported expressions, invalid paths and malformed external filters throw
  rather than silently dropping conditions or returning an unfiltered query.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Filtering -Filter FullyQualifiedName~Filtering
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope Querying -TestPackage vc.Ifx.Querying -CoveragePackage vc.Ifx.Querying -Filter FullyQualifiedName~Querying
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Filtering.EntityFrameworkCore -Filter FullyQualifiedName~Filtering.EntityFrameworkCore
```
