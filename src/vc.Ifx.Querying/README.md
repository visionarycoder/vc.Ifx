# vc.Ifx.Querying

Describe database query shape while retaining the caller's query provider.
Filtering owns predicates; Querying adds immutable ordering, pagination and
projection for database access code.

```csharp
var specification = new QuerySpec<Customer>()
    .Where(customer => customer.IsActive)
    .OrderBy(customer => customer.Name)
    .ThenBy(customer => customer.Id)
    .Page(offset: 0, size: 50)
    .Select(customer => new CustomerSummary(customer.Id, customer.Name));
var rows = await specification.Apply(db.Customers).ToListAsync(cancellationToken);
```

Every fluent operation returns a new specification. Pagination requires explicit
ordering; applications supply a unique tie-breaker. Apply never enumerates the
query. Database access code owns cancellation, context lifetime, transactions,
tracking and provider-specific includes.

QueryFilter and existing extensions remain available. The version-1 structural
JSON format has a real embedded schema, working polymorphic serialization without
runtime type discriminators, strict validation and operator rehydration through
core Filtering. Invalid input never becomes an unrestricted query. Case-insensitive
compatibility filters use CLR semantics; database provider support varies.

See [the contract](../../docs/filtering/query-contract.md) for versioning and
validation. Tests include SQLite queries, malformed payloads, schema/operator
consistency, null semantics and serialization roundtrips.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope Querying -TestPackage vc.Ifx.Querying -CoveragePackage vc.Ifx.Querying -Filter FullyQualifiedName~Querying
```

Verified 110 tests, 323/323 lines and 156/156 branches. Whole-solution and final
package verification remain separate gates in the parallel upgrade plan.

