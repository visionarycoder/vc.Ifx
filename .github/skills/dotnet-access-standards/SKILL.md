---
name: dotnet-access-standards
title: .NET Access Standards
description: Govern Access-layer contracts, repositories, query composition, and mapping scope in .NET applications.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1257
prerequisites:
  - dotnet-architectural-layers
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-manager-standards
  - dotnet-engine-standards
  - optimizing-ef-core-queries
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - access-layer
  - repositories
  - entity-framework
  - query-composition
---

# .NET Access Standards

Agent keeps Access code query-focused, infrastructure-aware, and free of business rules.

## When to Use

| Condition | Use |
|---|---|
| Agent writes `Access.*.Contract`, `Access.*.Service`, or `Access.*.Orm.*` code | Use this skill |
| Agent reviews repository shape, `DbContext` usage, or projection drift | Use this skill |
| Agent fixes caller references to ORM types or concrete repositories | Use this skill |
| Agent standardizes Access scaffolding across features | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Workflow orchestration belongs in a Manager | Route to `dotnet-manager-standards` |
| Business rules belong in an Engine | Route to `dotnet-engine-standards` |
| Type is request or response transport only | Route to `dotnet-dto-standards` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Data scope | Yes | Entity set, external dataset, or aggregate root |
| Storage technology | Yes | EF Core, SQL adapter, storage SDK, or external system adapter |
| Query shape | Yes | Lookup, list, paging, projection, or mutation |
| Mapping target | No | Contract DTO, entity, or adapter result |

## Workflow

1. Agent classifies the Access scope and owning contract.
2. Agent applies the rule matrix before writing repository or adapter code.
3. Agent keeps query composition, mapping, and persistence scope explicit.
4. Agent verifies caller scope, `DbContext` lifetime, and materialization points.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero new layer violations in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| ACC-001 | Access contract owns repository interfaces | Callers depend on concrete repositories or ORM projects | Move interface to `Access.*.Contract` |
| ACC-002 | Repository code contains no business rules | Pricing logic, policy branching, or domain verification in repository methods | Move rule to an Engine |
| ACC-003 | Query composition stays server-side | Repository materializes early, then filters in memory | Return `IQueryable<T>` or project in query |
| ACC-004 | `DbContext` lifetime stays scoped | Manual `new`, singleton context, static cache, or manual disposal | Inject scoped context through dependency injection |
| ACC-005 | Mapping stays at the Access scope | Controller or Manager maps ORM entities inline | Map inside Access service or mapper |
| ACC-006 | Method names expose persistence intent | Generic helper names hide query or mutation behavior | Rename to explicit query or mutation names |
| ACC-007 | Access code exposes scope-owned DTOs | Higher layers consume ORM entities directly | Map to contract types before return |
| ACC-008 | Transactions stay outside repository orchestration | Repository opens multi-step workflow transaction | Move transaction ownership to Manager |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Read-only list | Query projection in Access, materialization in caller | `ToList()` before filtering |
| Single aggregate save | Scoped repository or adapter method | Static context reuse |
| External adapter call | Access gateway behind contract | HTTP or SDK call from Manager |
| Cross-layer DTO handoff | Contract DTO mapped at Access scope | ORM entity leak into Web API |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for `new DbContext`, static context fields, or business formulas | Zero unauthorized matches |
| Query verification | Review changed queries for provider-side filtering and projection | Zero early materialization defects |

## Verification Checklist

Agent verifies:
- [ ] Access contracts live in `Access.*.Contract`
- [ ] Repository code contains no business rules
- [ ] Query composition stays provider-side until caller materializes
- [ ] `DbContext` or storage client lifetime stays scoped
- [ ] Mapping stays inside Access scope

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Repository returns `List<T>` for convenience | Return query or explicit projection |
| Repository verifies policy rules | Move rule to Engine |
| Manager references ORM entity directly | Return contract DTO |
| Access helper hides side effects | Keep persistence calls explicit |
