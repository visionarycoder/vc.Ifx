---
name: dotnet-engine-standards
title: .NET Engine Standards
description: Govern Engine-layer business rules, stateless design, and infrastructure-free domain logic in .NET applications.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1170
prerequisites:
  - dotnet-architectural-layers
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-manager-standards
  - dotnet-access-standards
  - dotnet-value-object-standards
  - dotnet-dependency-injection-standards
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - engine-layer
  - business-logic
  - stateless
  - domain-rules
---

# .NET Engine Standards

Agent keeps Engines focused on business rules, deterministic transforms, and stateless design.

## When to Use

| Condition | Use |
|---|---|
| Agent writes `Engine.*` code | Use this skill |
| Agent extracts rules from Managers, handlers, or repositories | Use this skill |
| Agent reviews domain verification or calculations | Use this skill |
| Agent replaces infrastructure calls in business code | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Code coordinates workflow steps | Route to `dotnet-manager-standards` |
| Code loads or saves data | Route to `dotnet-access-standards` |
| Code defines transport contracts | Route to `dotnet-dto-standards` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Business capability | Yes | Rule set, decision, or calculation |
| Inputs and outputs | Yes | Domain types, result types, or value objects |
| Allowed dependencies | No | Clock, logger, verifier, or policy helper |
| Caller scope | No | Manager, job, or handler |

## Workflow

1. Agent writes one sentence that defines the Engine capability.
2. Agent moves all infrastructure access outside the Engine scope.
3. Agent keeps dependencies immutable and method state local.
4. Agent verifies deterministic behavior, naming, and test scope.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero new infrastructure dependencies in Engine scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| ENG-001 | Engine owns one business capability | Utility bag or mixed-purpose service | Split by capability |
| ENG-002 | Engine state stays stateless | Mutable fields, cached workflow data, or request-scoped state | Keep state local to method |
| ENG-003 | Engine code contains no infrastructure calls | `DbContext`, repository, HTTP client, queue, or storage SDK usage | Load data outside Engine |
| ENG-004 | Engine code contains no workflow orchestration | Engine calls Engine for ordering or transaction control | Move ordering to Manager |
| ENG-005 | Engine inputs and outputs use domain-safe types | ORM entities or transport DTOs leak into Engine methods | Map to domain or value objects |
| ENG-006 | Dependencies stay explicit | Service location or manual collaborator construction | Use constructor injection |
| ENG-007 | Naming exposes business intent | Generic service names hide rule scope | Rename to `<Capability>Engine` |
| ENG-008 | Behavior stays deterministic | Hidden time, random, or environment state drives output | Inject abstractions for external state |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Calculation | Pure method or stateless Engine method | Repository-backed calculation |
| Verification | Engine returns domain result or exception | Controller-only verification copy |
| Transformation | Map domain inputs to domain outputs | Return ORM entity shape |
| Shared rule | Constructor-injected helper abstraction | Static global state |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed Engine files for `DbContext`, `HttpClient`, repository interfaces, or queue clients | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter Engine` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Engine owns one business capability
- [ ] Engine state stays stateless
- [ ] Engine code contains no infrastructure access
- [ ] Manager code owns workflow ordering
- [ ] Domain-safe types cross the Engine boundary

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Repository injected for convenience | Load data before Engine call |
| Engine calls another Engine for workflow order | Move order to Manager |
| Engine returns ORM entities | Map to domain type |
| Mutable field stores request data | Keep data local |
