---
title: FilterSpec and QuerySpec
doc_type: guide
status: active
last_updated: 2026-11-08
---

# FilterSpec and QuerySpec

Ifx.Filtering owns predicates and the portable FilterNode contract.
Ifx.Querying owns local query composition. Ifx.Integrations supplies the
Ifx.Querying.EntityFrameworkCore execution extensions. All packages start at
SemVer 0.1.0; the unpublished vc.Ifx.FilterSpec APIs and compatibility adapters
have been removed.

## Portable predicates

Author a predicate locally, then snapshot it as a FilterNode for a request DTO.
An expression tree, DbContext or IQueryable must not cross the component boundary.

```csharp
using Ifx.Filtering;
using System.Text.Json;

var filter = new FilterSpec<Order>(order => order.IsOpen)
    .Where(order => order.Total > 100m);
FilterNode portable = filter.ToFilterNode();
string json = JsonSerializer.Serialize(portable);

// In the receiving component:
FilterNode restored = JsonSerializer.Deserialize<FilterNode>(json)!;
FilterSpec<Order> received = FilterSpec<Order>.FromFilterNode(restored);
```

ToFilterNode snapshots captured values. Unsupported expressions throw rather than
silently dropping a predicate. FilterSpec composes AND, OR and NOT by parameter
substitution, without Expression.Invoke. The receiver must validate permitted
fields, operators and query cost before applying caller-supplied filters.

## Provider queries

QuerySpec is immutable. Each fluent call returns a new shape; retain its result.
A source-independent shape uses Apply(source). A shape bound with For(source)
supports ToQueryable and provider execution extensions.

```csharp
using Ifx.Filtering;
using Ifx.Querying;
using Ifx.Querying.EntityFrameworkCore;

var specification = QuerySpec<Order>.For(db.Orders)
    .ApplyFilter(restored)
    .Or(group => group.Where(order => order.IsPriority)
                      .And(order => order.Total > 10m))
    .Not(order => order.IsCancelled)
    .OrderByDescending(order => order.CreatedUtc)
    .ThenBy(order => order.Id)
    .Include(order => order.Customer)
    .Page(offset: 0, size: 25);
PagedResult<Order> result = await specification.ToPagedResultAsync(ct);
```

Page takes a zero-based item offset and positive size, and requires explicit
ordering. Page(offset: 25, size: 25) selects the second window of 25 items.
Supply a unique tie-breaker to make ordering deterministic. OrderBy replaces the
primary ordering; ThenBy adds a tie-breaker.

Grouped Where, And, Or and Not clauses preserve their parentheses. Straight
predicate composition is left-to-right. Not(predicate) appends AND NOT predicate;
FilterSpec.Not negates its entire predicate.

The provider translates the resulting LINQ expression to SQL where supported.
ToListAsync applies includes and no-tracking reads. ToPagedResultAsync executes
page and count queries sequentially on the context; the count ignores pagination
and honors filtering. These are two queries and do not promise a transactionally
consistent snapshot. CountAsync and AnyAsync also ignore pagination.

## Cursor paging

```csharp
var first = await QuerySpec<Order>.For(db.Orders)
    .OrderBy(order => order.CreatedUtc)
    .UseKey(order => order.Id)
    .ToCursorPageAsync(pageSize: 25, ct);

var next = await QuerySpec<Order>.For(db.Orders)
    .OrderBy(order => order.CreatedUtc)
    .UseKey(order => order.Id)
    .AfterCursor(first.NextCursor)
    .ToCursorPageAsync(pageSize: 25, ct);
```

UseKey supplies a stable identity key and final tie-breaker. Cursor and offset
paging cannot be combined. Tokens encode sort values; they are not encrypted or
an authorization boundary. Keep the filter and ordering consistent between pages.

## Ownership

FilterGroupBuilder resides in Ifx.Filtering. SortClause, SortDirection, PagedResult
and CursorPage reside in Ifx.Querying. One internal QueryPlan implements provider
composition, sorting and pagination; it is not a public compatibility facade.
Database access code owns context lifetime, transactions and cancellation.
