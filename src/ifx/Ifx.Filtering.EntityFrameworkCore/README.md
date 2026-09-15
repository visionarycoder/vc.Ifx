# vc.Ifx.Filtering.EntityFrameworkCore

Apply portable vc.Ifx filters to EF Core queryables while preserving provider
execution, projection, pagination and cancellation at the database access boundary.

EfFilterExecutionStrategy delegates predicate construction to the verified
FilterExpression.Create<T> API in core Filtering. It never catches translation
failures to return an unfiltered query or switches IQueryable to client evaluation.
The IEnumerable overload is explicitly in-memory and compiles the same predicate.

The existing DbContext constructor remains source-compatible and rejects null.
The caller owns the context lifetime. A null filter returns the original source;
an invalid source, path, value or operator throws. Applications own field allowlists
and query complexity limits for externally supplied filters.

Tests cover EF InMemory and SQLite relational execution, including nested Any/All,
null/nullable comparisons, string predicates, IN, ordering/projection/paging,
and canceled database execution. SQLite evidence does not guarantee identical
collation or translation on every provider; test the deployed provider as well.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Filtering.EntityFrameworkCore -Filter FullyQualifiedName~Filtering.EntityFrameworkCore
```
