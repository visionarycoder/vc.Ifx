---
name: dotnet-dto-standards
title: .NET DTO Standards
description: Define immutable, scope-owned DTO shapes, serialization rules, and mapping scope for .NET applications.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1156
prerequisites:
  - dotnet-architectural-layers
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-architectural-layers
  - dotnet-mapping-standards
  - dotnet-immutability-standards
  - breaking-primitive-obsession
appliesTo: "**/*.{cs,csproj,json}"
tags:
  - dotnet
  - dto
  - contracts
  - immutability
  - serialization
---

# .NET DTO Standards

Agent keeps DTOs immutable, data-only, and owned by the scope that exposes them.

## When to Use

| Condition | Use |
|---|---|
| Agent writes request, response, event, or message DTOs | Use this skill |
| Agent reviews DTO mutability or ownership drift | Use this skill |
| Agent standardizes scope contract naming | Use this skill |
| Agent fixes serialization shape or collection choices | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Type owns identity or behavior | Route to entity or value-object guidance |
| Type is ORM persistence model | Route to Access guidance |
| Type exists for UI local state only | Use UI-specific pattern |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| DTO role | Yes | Request, response, event, message, or adapter contract |
| Owning scope | Yes | Client, Manager, Access, or integration scope |
| Serialization needs | Yes | Nullability, collections, and field names |
| Mapping source or target | No | Entity, domain type, or external payload |

## Workflow

1. Agent identifies the receiving scope and names the DTO for that scope.
2. Agent selects immutable members and explicit collection types.
3. Agent keeps DTO code data-only and mapping code separate.
4. Agent verifies ownership, nullability, and version-safe serialization changes.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero business methods added to changed DTO types.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| DTO-001 | DTO state is immutable | Public setters, mutable fields, or mutable collections | Use record or init-only members |
| DTO-002 | DTO code is data-only | Business methods, persistence helpers, or service access | Move behavior outside DTO |
| DTO-003 | Owning scope is explicit | Foreign DTO reused across layers | Create scope-owned DTO and map |
| DTO-004 | Naming exposes contract role | `Data`, `Info`, or bag-style names | Rename to request, response, event, or message name |
| DTO-005 | Collections are scope-safe | Public `List<T>` or mutable dictionaries | Use `IReadOnlyCollection<T>` or array |
| DTO-006 | Nullability and required fields are explicit | Hidden null assumptions or breaking field drift | Add explicit nullability and version-safe fields |
| DTO-007 | Mapping stays outside DTO type | DTO constructs entities or runs conversions inline | Move logic to mapper |
| DTO-008 | Serialization shape stays predictable | Breaking renames without migration plan | Add optional field or wrapper |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| API request | Immutable request DTO with explicit required fields | Mutable bag with optional everything |
| API response | Scope-owned response DTO | ORM entity returned directly |
| Message contract | Small immutable event or command DTO | Shared DTO reused across unrelated layers |
| External adapter contract | Adapter-specific DTO and mapper | Domain type bound directly to external JSON |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed DTO files for methods, repositories, or `DbContext` | Zero unauthorized matches |
| Contract verification | Run `dotnet test [test-project].csproj --filter DTO` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] DTOs are immutable after construction
- [ ] DTOs contain data only
- [ ] Each DTO belongs to one scope
- [ ] Nullability and collections are explicit
- [ ] Mapping code stays outside DTO type

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| One DTO serves many layers | Split by scope and map |
| DTO exposes `List<T>` | Use read-only collection type |
| DTO adds helper methods that mutate state | Keep DTO data-only |
| Persistence or UI names leak into contract | Rename for consumer contract |
