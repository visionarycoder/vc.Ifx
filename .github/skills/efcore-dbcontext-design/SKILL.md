---
name: efcore-dbcontext-design
description: Design EF Core DbContext types with explicit lifetime, model configuration, resilience, and measurable validation.
title: EF Core DbContext Design
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1350
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - dotnet-dependency-injection-standards
related_skills:
  - efcore-migrations
  - efcore-testing-patterns
  - optimizing-ef-core-queries
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - efcore
  - dbcontext
  - design
---
# EF Core DbContext Design

Agent designs `DbContext` types that keep model mapping explicit, lifetime-safe, and testable.

## When to Use

| Condition | Use |
|---|---|
| Agent creates or restructures a `DbContext` | Use this skill |
| Agent maps entities, relationships, keys, or indexes | Use this skill |
| Agent adds interceptors, query filters, or provider options | Use this skill |
| Agent reviews `DbContext` lifetime or connection resilience | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Project does not use EF Core | Use a non-EF data skill |
| Work is only query tuning | Use `optimizing-ef-core-queries` |
| Work is only migration authoring | Use `efcore-migrations` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Provider | Yes | SQL Server, PostgreSQL, SQLite, or equivalent |
| Aggregate/entity set | Yes | Types, relationships, and ownership boundaries |
| Lifetime model | Yes | Web request, worker scope, or command scope |
| Configuration style | No | Fluent API, conventions, or annotations |

## Required Workflow

Agent performs these steps:
1. Agent keeps `DbContext` registration scoped unless a proven alternative exists.
2. Agent moves entity mapping into `IEntityTypeConfiguration<T>` types or equivalent centralized model code.
3. Agent configures keys, lengths, indexes, relationships, and delete behavior explicitly where domain rules matter.
4. Agent adds provider-specific resilience, timeout, and logging settings only in startup or composition-root code.
5. Agent keeps tenant, soft-delete, or audit behavior deterministic through filters or interceptors.
6. Agent validates model creation, migration generation, and runtime resolution.

Test: Agent runs `dotnet build [project].csproj` and `dotnet ef migrations add __SteVerification --no-build` when EF tooling exists, then agent removes the temporary migration.
Pass: Agent observes zero compile errors. Agent resolves `DbContext` from DI. Agent sees only expected model changes in the temporary migration.

## Design Matrix

| Rule | Agent verifies | Preferred pattern | Avoid |
|---|---|---|---|
| DBC-001 | Lifetime matches unit of work | Scoped registration | Singleton `DbContext` |
| DBC-002 | Model configuration stays centralized | `ApplyConfigurationsFromAssembly` or explicit config classes | Large inline `OnModelCreating` blocks with mixed concerns |
| DBC-003 | Relationship behavior is intentional | Explicit FK and delete behavior | Provider defaults relied on implicitly |
| DBC-004 | Cross-cutting behavior is reusable | Interceptors, conventions, filters | Copying audit code into repositories |
| DBC-005 | Query filters stay deterministic | Stable tenant/context inputs | `DateTime.Now`, random values, or ambient mutable state in filters |
| DBC-006 | Provider options stay environment-safe | Dev-only sensitive logging | Sensitive logging in production |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| DI verification | Existing app or test startup path | `DbContext` resolves once per scope |
| Model verification | `dotnet ef migrations add __SteVerification --no-build` | Expected model diff only |
| Isolation verification | Existing EF tests | Zero failures caused by shared state |

## Verification Checklist

Agent verifies:
- [ ] `DbContext` lifetime matches request or job scope
- [ ] Entity configuration is explicit for critical schema rules
- [ ] Query filters and interceptors use deterministic inputs
- [ ] Provider options keep logging and retries environment-appropriate
- [ ] Model changes generate the expected migration diff

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| `DbContext` registered as singleton | Use scoped lifetime |
| Query filter reads unstable ambient state | Route stable scope data into the context |
| Audit logic duplicated across handlers | Move logic into interceptor or save pipeline |
| `OnModelCreating` becomes unreviewable | Split mapping into configuration classes |
