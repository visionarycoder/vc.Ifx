---
name: cqrs-patterns-dotnet
title: CQRS Patterns for .NET
description: Implement command and query separation in .NET with hand-rolled handlers, validation, DI-based dispatch, and VBD-aligned boundaries.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1319
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-system-design
  - dotnet-dependency-injection-standards
  - dotnet-validation-standards
  - result-types-dotnet
appliesTo: '**/*.{cs,csproj,md}'
tags:
  - dotnet
  - ddd
  - cqrs
  - vbd
  - architecture
---
# CQRS Patterns for .NET

Agent separates write behavior from read behavior with hand-rolled contracts and VBD-owned boundaries.

## When to Use

| Condition | Use |
|---|---|
| Write flow changes independently from read shape | Use this skill |
| Use case orchestration needs explicit command handlers | Use this skill |
| Query endpoints project read models tuned to callers | Use this skill |
| Team avoids MediatR and reflection-driven dispatch | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| CRUD logic stays trivial and stable in one place | Keep the simpler pattern |
| Business rules fit one aggregate method with no separate reads | Use direct application-service orchestration |
| Work asks only for EF Core repository design | Use `repository-unitofwork-efcore` |
| Work asks only for result modeling | Use `result-types-dotnet` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Use-case list | Yes | Split write intent from read intent. |
| Volatility boundaries | Yes | Map handlers to the boundary that owns the change driver. |
| Validation rules | Yes | Place boundary validation before mutation or expensive queries. |
| Read-model consumers | Yes | Shape queries to caller needs. |
| Transaction boundary | No | Record save scope when one command spans several repositories. |

## Pattern Matrix

| Concern | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Write behavior | Command plus command handler | Keep return shape small and explicit | Mutation logic stays isolated from view-shaped payloads |
| Read behavior | Query plus query handler | Keep queries side-effect free | Projection stays tuned to caller needs |
| Dispatch | Hand-rolled DI dispatchers | Avoid MediatR, reflection pipelines, and hidden behaviors | Contracts and dispatch stay explicit |
| Validation | Validator at the boundary before work starts | Keep invariants in domain types and request checks at the boundary | Invalid requests stop before mutation or heavy I/O |
| Mapping | Hand-written DTO projection | Avoid AutoMapper-driven projection and hidden mapping rules | Result shapes stay readable and traceable |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies requests as commands or queries by side effects and ownership. | Review request contracts. | Each request falls into one category only. |
| 2 | Agent places each handler inside the boundary that owns the change driver. | Compare handler placement to the system boundaries. | No handler crosses an unrelated authority boundary. |
| 3 | Agent adds hand-rolled handler interfaces, validators, and dispatch registrations. | Search for handler and dispatcher contracts in scope. | Contracts and dispatchers exist with no MediatR references. |
| 4 | Agent adds validation before command execution and before expensive query work. | Review handler entry points. | Invalid requests stop before mutation or heavy I/O. |
| 5 | Agent returns explicit result types or read DTOs. | Review return types. | Command handlers avoid caller-shaped view payloads. |
| 6 | Agent validates targeted build and tests. | Run targeted build and tests. | Build succeeds and changed flows pass. |

## Rules

| Topic | Rule |
|---|---|
| MediatR | Avoid MediatR packages, pipeline behaviors, and notification abstractions. |
| AutoMapper | Use hand-written mapping and projection only. |
| Query purity | Keep queries side-effect free. |
| Command result shape | Return identifiers, version markers, or explicit result types. |
| Transaction scope | Commit once per write use case unless the use case defines a larger boundary. |
| Boundary ownership | Keep handlers in the scope that owns volatility. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Command and query split | Review contracts and handlers. | No request type mixes mutation and read projection concerns. |
| Hand-rolled dispatch | Search the changed scope for MediatR symbols. | No MediatR references exist. |
| Hand-written mapping | Search the changed scope for AutoMapper symbols. | No AutoMapper references exist. |
| Boundary ownership | Compare file placement to owning boundary. | Each handler sits in the scope that owns the change driver. |
| Validation entry | Review handler entry points. | Invalid input stops before side effects. |
| Build and tests | Run targeted build and tests. | Build succeeds and changed tests pass. |

## Outputs

- Command and query contracts
- Hand-rolled handler and dispatcher plan
- Validation entry points
- Boundary placement guidance
- Verification steps for build and tests
