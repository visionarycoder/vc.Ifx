---
name: dotnet-architectural-layers
title: .NET Architectural Layers
description: Enforce repository layering rules, allowed dependency directions, and boundary ownership across Client, Manager, Engine, Access, and Ifx components.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1300
prerequisites:
  - vbd-system-design
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-code-review
  - vbd-system-design
  - ifx-component-communication
  - detect-static-dependencies
appliesTo: "**/*.{cs,csproj,props}"
tags:
  - dotnet
  - architecture
  - layering
  - vbd
  - dependencies
  - boundaries
---

# .NET Architectural Layers

Agent preserves downward-only dependency flow and owned boundaries across solution. Layering is a correctness rule.

## When to Use

Use when:
- Adding project references, new components, or cross-layer calls
- Reviewing DTO ownership, proxy placement, or message-bus usage
- Refactoring code that bypasses Managers, Engines, or Access contracts
- Designing analyzers or validation scripts for dependency direction

## When Not to Use

Do not use when:
- Framework or vendor code outside repository architecture
- Throwaway spikes isolated from production architecture
- Pure formatting or naming cleanup with zero boundary changes

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source component | Yes | Project, namespace, type, or method introducing dependency |
| Target component | Yes | Project, namespace, type, or service consumed |
| Layer pair | Yes | Client, Manager, Engine, Access.Contract, Access.Service, Access.Orm, Ifx |
| Interaction pattern | Yes | Project reference, DI call, proxy, mapper, or message bus |

## Workflow

Agent follows these steps:

1. Agent classifies both sides of dependency
2. Agent applies rule matrix to reference direction, DTO ownership, communication style
3. Agent inserts proxies, contracts, or message-bus boundaries when direct edge is forbidden
4. Agent verifies references, namespaces, call flows
Test: Run `dotnet build Wa.Wsdot.Fin.Idl.slnx`
Pass: Zero build errors. Zero architecture analyzer warnings.

## Rule Matrix

| Rule | Enforcement | Detection Pattern | Fix |
|---|---|---|---|
| ARCH-001 | Dependencies flow downward only | Client → Engine, Client → Access, Engine → Manager, other upward edges | Route calls through owning intermediate layer |
| ARCH-002 | Client code calls through Managers | Controllers or UI calling Engines, repositories, ORM directly | Move orchestration into Manager services |
| ARCH-003 | Managers do not call other Managers directly | Cross-manager references or workflow chaining without boundary | Use message bus or boundary abstraction |
| ARCH-004 | Engines own business rules, not persistence | Repositories, DbContexts, HTTP clients inside Engine code | Push I/O to Access, orchestration to Managers |
| ARCH-005 | Access split by contract, service, ORM | Higher layers referencing `Access.*.Orm` or service implementations | Depend on `Access.*.Contract`. Keep ORM local. |
| ARCH-006 | Each boundary owns its DTOs | Client using Manager DTOs or layers reusing foreign transport models | Map into receiving layer's contract types |
| ARCH-007 | Ifx/component/util layers stay infrastructure | Business branching or domain terms in shared infrastructure | Keep shared code generic, boundary-neutral |
| ARCH-008 | Use analyzable, explicit dependency declarations | Hidden static calls, reflection coupling, implicit service discovery | Use project references, contracts, explicit DI edges |

## Allowed Dependencies

| Layer | Depends On |
|---|---|
| Client | Manager, Ifx, shared component/util libraries |
| Manager | Engine, Access.Contract, message bus, component infrastructure, Ifx |
| Engine | Domain-safe shared libraries, minimal cross-cutting abstractions |
| Access.Service | Access.Contract, Access.Orm, shared infrastructure |
| Access.Orm | Persistence-specific dependencies only |

## Validation Checklist

Agent verifies:
- [ ] Both sides of every new dependency classified by layer
- [ ] Zero upward or layer-skipping references introduced
- [ ] DTOs owned by receiving boundary (not reused across layers)
- [ ] Managers, Engines, Access components keep intended responsibilities
- [ ] Exceptions are explicit, rare, justified

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Presentation code calls repositories directly | Keep orchestration in Managers |
| Share one DTO across multiple layers | Preserve boundary ownership. Map deliberately. |
| Treat shared infrastructure as business logic location | Keep Ifx/component code boundary-neutral |
| Add direct reference when proxy feels inconvenient | Use allowed seam |
