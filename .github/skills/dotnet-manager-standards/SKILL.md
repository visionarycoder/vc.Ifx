---
name: dotnet-manager-standards
title: .NET Manager Standards
description: Govern Manager-layer orchestration, transaction scope, Engine coordination, and event ordering in .NET applications.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1237
prerequisites:
  - dotnet-engine-standards
  - dotnet-access-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-engine-standards
  - dotnet-access-standards
  - dotnet-architectural-layers
  - dotnet-dependency-injection-standards
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - manager-layer
  - orchestration
  - transactions
  - events
---

# .NET Manager Standards

Agent keeps Managers orchestration-focused, stateless, and explicit about workflow ordering.

## When to Use

| Condition | Use |
|---|---|
| Agent writes `Manager.*` contracts or services | Use this skill |
| Agent moves workflow logic out of controllers, handlers, or jobs | Use this skill |
| Agent defines transaction scope across repositories or gateways | Use this skill |
| Agent orders persistence and event publication | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Rule is pure calculation or verification | Route to `dotnet-engine-standards` |
| Code talks directly to storage or ORM details | Route to `dotnet-access-standards` |
| Workflow is long-lived scheduler or state machine | Use domain-specific workflow abstraction |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Workflow goal | Yes | End-to-end business operation |
| Participating Engines | Yes | Rule or transformation services |
| Access contracts | Yes | Repositories, gateways, or unit-of-work contracts |
| Event and transaction scope | No | Post-commit events or multi-write coordination |

## Workflow

1. Agent names the workflow and defines start and end state.
2. Agent sequences data load, Engine calls, persistence, and event publication.
3. Agent keeps business rules in Engines and persistence details in Access code.
4. Agent verifies stateless construction, transaction scope, and event order.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero new layer violations in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| MGR-001 | Manager code orchestrates instead of calculating | Domain formulas or policy branches in Manager methods | Move rule to Engine |
| MGR-002 | Manager scope owns workflow ordering | Controller, handler, or job sequences Engines directly | Move workflow into Manager |
| MGR-003 | Manager depends on Access contracts only | Direct `DbContext`, ORM entity, or storage SDK usage | Call contract interface |
| MGR-004 | Transaction scope is explicit | Multi-write workflow with hidden repository transaction | Open transaction at Manager scope |
| MGR-005 | Event publication follows durable persistence | Event emitted before commit or after failed save | Commit first, then publish |
| MGR-006 | Manager state stays stateless | Mutable instance fields or cached request data | Keep state local to method |
| MGR-007 | Naming exposes workflow intent | Utility-style names hide business operation | Rename to `<Capability>Manager` and explicit method name |
| MGR-008 | Dependencies stay constructor-injected | Service location or manual collaborator construction | Inject dependencies through constructor |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| One Engine, one repository | Manager loads, calls Engine, saves | Controller calls Engine and repository |
| Multi-write workflow | One explicit transaction scope | Repository-owned nested transactions |
| Post-commit event | Publish after save succeeds | Publish before durable state |
| Cross-Engine workflow | Manager orders Engine calls | Engine calls Engine for orchestration |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for `DbContext`, storage SDK types, or domain formulas in Manager code | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter Manager` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Manager methods orchestrate only
- [ ] Engines own rules and transformations
- [ ] Access contracts hide persistence details
- [ ] Transaction scope is explicit for multi-write workflows
- [ ] Events publish after durable state changes

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Manager repeats Engine verification | Return clear Engine result |
| Controller coordinates multiple Engines | Move workflow into Manager |
| Manager touches ORM entity graph directly | Route through Access contract |
| Manager stores request state in fields | Keep state local |
