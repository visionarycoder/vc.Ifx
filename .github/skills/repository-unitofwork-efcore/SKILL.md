---
name: repository-unitofwork-efcore
title: Repository and Unit of Work for EF Core
description: Apply repository and unit-of-work patterns over EF Core with hand-rolled abstractions, explicit transaction boundaries, and guidance on repository use versus direct DbContext.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1494
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - specification-pattern-dotnet
  - cqrs-patterns-dotnet
  - efcore-dbcontext-design
  - vbd-system-design
appliesTo: '**/*.{cs,csproj,md}'
tags:
  - dotnet
  - ddd
  - efcore
  - repository
  - unit-of-work
---
# Repository and Unit of Work for EF Core

Agent applies focused repository and unit-of-work patterns where aggregate boundaries and transaction ownership need explicit shape.

## When to Use

| Condition | Use |
|---|---|
| Aggregate roots need explicit persistence contracts | Use this skill |
| One write use case coordinates several repositories in one transaction | Use this skill |
| Team needs consistent save, concurrency, and transaction rules across handlers | Use this skill |
| Work asks when repository abstraction adds value over raw `DbContext` | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Small feature reads one table with one short query | Use direct `DbContext` query flow |
| Repository abstraction mirrors every `DbSet` member with no domain meaning | Keep `DbContext` direct access |
| Work asks only for reusable predicates | Use `specification-pattern-dotnet` |
| Work asks only for read-side query optimization | Use `optimizing-ef-core-queries` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Aggregate map | Yes | Tie repositories to aggregate roots, not tables only. |
| Transaction boundaries | Yes | Record save scope and rollback expectations. |
| Concurrency rules | Yes | Record row version or conflict handling needs. |
| Query-shape needs | Yes | Record where direct `DbContext` reads stay simpler. |
| VBD ownership | No | Align repository location with the boundary that owns persistence policy. |

## Pattern Matrix

| Persistence Need | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Aggregate rehydration with invariants | Aggregate-focused repository | Avoid generic CRUD mirrors | Repository methods express domain retrieval intent |
| One-off projection or dashboard read | Direct `DbContext` query | Keep projection caller-shaped | Query flow stays simple and projection-focused |
| Cross-aggregate write in one use case | Repositories plus unit of work | Centralize save ownership | Commit and rollback rules stay explicit |
| Shared eager-load or concurrency policy | Repository contract | Avoid leaking `IQueryable` | Reused policy stays consistent across handlers |
| Multi-step write that needs explicit transaction | Unit of work plus explicit transaction | Avoid opening transactions for single-save paths | Transaction scope matches the use case boundary |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent identifies aggregates that need repositories and reads that stay better with direct `DbContext`. | Review use cases and entities. | Repository use exists only where aggregate semantics add value. |
| 2 | Agent defines focused repository interfaces plus one unit-of-work contract. | Search for repository and unit-of-work contracts in scope. | Contracts exist with aggregate-oriented members. |
| 3 | Agent implements repositories over EF Core with hand-written queries and mappings. | Review repository classes. | Repositories express domain retrieval intent with no generic CRUD noise. |
| 4 | Agent centralizes `SaveChangesAsync`, transaction start, and concurrency handling in the unit of work. | Review the save pipeline. | Transaction ownership stays explicit and consistent. |
| 5 | Agent keeps read models free to use direct `DbContext` when abstraction adds no value. | Review query handlers or read services. | Query flow stays simple. |
| 6 | Agent validates build and targeted persistence tests. | Run targeted build and tests. | Build succeeds and changed persistence flows pass. |

## Rules

| Topic | Rule |
|---|---|
| Granularity | Create repositories per aggregate or cohesive access concern, not one generic repository per entity. |
| Mapping | Use hand-written projections and mappings. |
| Save ownership | Centralize commits in the unit of work or in the `DbContext` implementation of that contract. |
| Read side | Use direct `DbContext` queries for simple projections. |
| Transactions | Open explicit transactions only for multi-step coordination that needs them. |
| Concurrency | Handle concurrency at the unit-of-work boundary with explicit conflict flow. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Repository focus | Review interfaces and implementations. | Members express aggregate intent instead of generic CRUD repetition. |
| Direct read guidance | Review query handlers. | Simple projections use direct `DbContext` where abstraction adds no value. |
| `IQueryable` exposure | Search repository contracts. | Repository interfaces do not leak open-ended query surfaces. |
| Transaction ownership | Review handlers and unit-of-work flow. | Commit and transaction steps stay explicit and consistent. |
| Concurrency path | Run targeted conflict tests when concurrency tokens exist. | Conflicts follow the expected outcome path. |
| Build and tests | Run targeted build and tests. | Build succeeds and changed tests pass. |

## Outputs

- Aggregate-focused repository contracts
- EF Core repository implementations
- Unit-of-work contract and save flow
- Transaction-boundary guidance
- Repository-versus-`DbContext` decision record
- Verification steps for persistence behavior

## Reference Files

- [Repository and unit-of-work examples](references/examples.md)
