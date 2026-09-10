# vc.Ifx.Filtering

Compose reusable predicates and portable filters without coupling applications to
a database provider. Database query shape and execution belong to QuerySpec/access
packages; EF-specific translation belongs to vc.Ifx.Filtering.EntityFrameworkCore.

## Predicate Specifications

```csharp
var activeAdults = new FilterSpec<Customer>(customer => customer.IsActive)
    .Where(customer => customer.Age >= 18);
IQueryable<Customer> matching = activeAdults.Apply(customers);
FilterNode snapshot = activeAdults.ToFilterNode();
```

Where, And, Or and Not return new specifications. Expressions are combined by
parameter substitution, without Invoke or premature compilation for IQueryable.
The legacy Filter.For<T>().Where(...).Build() API remains supported and snapshots
its builder state.

## Portable Execution

FilterExpression.Create<T>(node) produces a LINQ predicate without executing a
query. PocoFilterExecutionStrategy accepts both IEnumerable and IQueryable.
Unsupported expressions, malformed values, unknown paths and unsupported node
types throw rather than silently dropping conditions.

Supported portable operations include comparisons, Boolean members/constants,
groups, general negation, string operations, nested Any/All, collection Contains
and captured-list membership. Empty AND matches all; empty OR and empty IN match
none. Null comparisons are preserved. Null dereferences follow CLR behavior and
need explicit guards. Nullable comparisons and floating-point negation retain
their original Boolean semantics.

Groups hold read-only snapshots. Polymorphic JSON uses a $type discriminator;
leaf field names and existing numeric operator values are unchanged. Capture
values are snapshotted only when translating and use invariant formatting.
TimeOnly membership values preserve seconds and fractional ticks during portable
and JSON round trips.
See [the contract](../../docs/filtering/filter-contract.md) for supported syntax,
scalar collection paths, serialization, compatibility and application validation.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Filtering -Filter FullyQualifiedName~Filtering
```

Tests compare translated and JSON-roundtripped predicates with CLR execution,
including nested collections, nulls, NaN, captured values and malformed filters.

