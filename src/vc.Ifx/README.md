# VisionaryCoder.Framework

A comprehensive core framework library providing foundational features for the VisionaryCoder ecosystem.

## Overview

`VisionaryCoder.Framework` is the foundational library for the VisionaryCoder Framework ecosystem. It provides core services, utilities and small, focused sub-systems that other framework components and applications can reuse.

This project contains several concerns, including the filtering model and execution strategies that make it easy to build, serialize and apply query filters across in-memory POCO collections and EF Core queryables.

## Highlights (what's included today)

- Core framework helpers and DI registration utilities
- A flexible filtering model (portable abstractions)
  - `VisionaryCoder.Framework.Filtering.Abstractions` contains the filter node model: `FilterNode`, `FilterCondition`, `FilterGroup`, `FilterOperation`, and related enums
  - Expression translation from LINQ predicates via `ExpressionToFilterNode` (produces the portable `FilterNode` tree)
- Pluggable filter execution strategies
  - POCO strategy (default) for in-memory `IEnumerable<T>` filtering
  - EF Core strategy (sub-library) which translates `FilterNode` into EF expressions and is optimized to produce SQL (including `IN` support)
- Sample code showing usage and new `IN` operator support (constant collection `.Contains(member)` and array literal `.Contains(member)` are translated)

## Filtering and Querying

The filtering subsystem is intentionally split into a stable `Abstractions` surface and provider-specific execution code:

- `VisionaryCoder.Framework.Filtering.Abstractions`
  - Portable POCO types used to represent filters (`FilterNode`, `FilterCondition`, `FilterGroup`, etc.)
  - `IFilterExecutionStrategy` interface to apply a `FilterNode` to `IQueryable<T>` or `IEnumerable<T>`

- `VisionaryCoder.Framework.Filtering` (helpers)
  - `Filter.For<T>()` builder and `ExpressionToFilterNode` translator used to create `FilterNode` trees from LINQ expressions

- `VisionaryCoder.Framework.Filtering.Poco` (default)
  - In-memory application of `FilterNode` trees; intended as the default execution strategy for POCO collections

- `VisionaryCoder.Framework.Filtering.EFCore` (optional provider)
  - EF Core optimized execution strategy that translates `FilterNode` into expression trees EF can turn into SQL
  - Includes optimized handling of `IN` by generating a typed constant array and using `Enumerable.Contains(array, member)`, which EF Core providers translate to `IN (...)` in SQL

## New: IN operator support

You can now write filters using constant collections or array literals and have them translated to an `IN` operation for EF Core:

```csharp
// variable-backed collection -> translated to IN for EF
var allowedNames = new[] { "John Smith", "Bob Brown" };
var filter = Filter.For<User>()
    .Where(u => allowedNames.Contains(u.Name))
    .Build();

// literal array -> also translated to IN
var filter2 = Filter.For<User>()
    .Where(u => new[] { "Ann Smith", "Bob Brown" }.Contains(u.Name))
    .Build();
```

For POCO execution the framework evaluates the same semantics in-memory; for EF Core the expression is optimized to a SQL `IN` clause where possible.

## Samples

A small sample application is included under `src/VisionaryCoder.Framework/Filtering/Sample` demonstrating:

- Building a filter with `Filter.For<T>()` and `ExpressionToFilterNode`
- Applying the filter to an in-memory collection via the POCO execution strategy
- Applying the same filter to an EF Core `IQueryable<T>` via the EF Core execution strategy
- Examples for `IN` usage (variable collection and array literal)

## Project structure (high level)

```text
src/VisionaryCoder.Framework/
├── Filtering/
│   ├── Abstractions/             # Filter model (FilterNode, FilterCondition, FilterOperation...)
│   ├── EFCore/                   # EF Core execution strategy and expression builder
│   ├── Poco/                     # Default POCO execution strategy
│   └── Sample/                   # Samples demonstrating usage
├── Querying/                     # Thin query helpers and serialization
└── VisionaryCoder.Framework.csproj
```

## Dependencies and targets

- Target: .NET 8
- C# language level: modern (C# 13+ where applicable)
- Notable NuGet: `Microsoft.EntityFrameworkCore` (for EF strategy / samples)

## Testing

Unit tests for filtering and expression translation exist in the `tests/VisionaryCoder.Framework.Tests` project. They cover:

- Expression -> FilterNode translation
- Collection operator translation (Any, All, HasElements)
- `IN` translation cases

## Contribution notes

- The filtering model is intentionally small and provider-agnostic; new execution strategies can be added by implementing `IFilterExecutionStrategy` and wiring it through DI or via helper classes.
- Prefer immutable records for filter model types; keep translation logic focused and testable.

---

This README reflects the current state of the `VisionaryCoder.Framework` repository and the filtering features available in the codebase.
