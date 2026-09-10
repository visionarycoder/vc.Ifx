---
name: dotnet-dependency-injection-standards
title: .NET Dependency Injection Standards
description: Govern dependency injection lifetimes, registrations, and composition-root usage across .NET services.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1350
prerequisites:
  - dotnet-architectural-layers
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-architectural-layers
  - dotnet-logging-standards
  - dotnet-code-quality-standards
appliesTo: "**/*.{cs,csproj,json}"
tags:
  - dotnet
  - dependency-injection
  - lifetimes
  - composition-root
  - services
  - analyzers
---

# .NET Dependency Injection Standards

Agent keeps dependency injection explicit, lifetime-safe, and layer-aligned. Agent keeps the composition root responsible for graph assembly.

## When to Use

| Condition | Use |
|---|---|
| Agent registers services in `Program.cs` or setup extensions | Use this skill |
| Agent reviews lifetime defects or constructor dependency graphs | Use this skill |
| Agent fixes service location or ad hoc provider building | Use this skill |
| Agent standardizes registration patterns for repositories, Managers, Engines, or handlers | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Type is pure data or static helper | Do not register type |
| Test scope uses isolated manual construction | Manual construction is acceptable |
| Framework owns object activation outside local registration control | Preserve framework shape |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Service role | Yes | Repository, Engine, Manager, handler, validator, hosted service, or adapter |
| Dependency graph | Yes | Direct dependencies and their lifetimes |
| Host type | Yes | Web app, worker, function, library, or test root |
| State model | No | Mutable state, cache, scope data, or disposable ownership |

## Workflow

Agent follows these steps:

1. Agent selects service abstraction and lifetime before writing registration code.
2. Agent applies the rule matrix to constructor injection, lifetime flow, and disposal ownership.
3. Agent keeps registrations in the composition root or approved setup extension.
4. Agent verifies the graph for captive dependency defects and container misuse.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero captive dependency defects in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| DI-001 | Lifetime matches state and resource ownership | Singleton `DbContext`, transient cache owner, or stateful singleton | Use scoped, singleton, or transient intentionally |
| DI-002 | No captive dependency exists | Singleton depends on scoped service | Lower parent lifetime or route work through scoped factory |
| DI-003 | Required collaborators use constructor injection | Service location or hidden late resolution | Inject explicit contract through constructor |
| DI-004 | Composition root owns registrations | Feature code calls `BuildServiceProvider()` or mutates provider graph | Move registration to startup extension or root |
| DI-005 | Cross-layer dependencies use abstractions | Higher layer injects concrete infrastructure type | Inject contract owned by lower layer |
| DI-006 | Factory use is intentional | Ad hoc factory hides lifetime defect | Use container-supported factory or narrow abstraction |
| DI-007 | Disposal ownership is clear | Container-owned service is disposed manually | Let container dispose owned service |
| DI-008 | Registration pattern stays repeatable | Same service family uses different registration shapes across projects | Use shared extension or repository pattern |

## Lifetime Matrix

| Service Kind | Default Lifetime | Notes |
|---|---|---|
| `DbContext`, repository, Manager, Engine with scoped dependency | Scoped | Request or job scope owns instance |
| Stateless mapper or validator with no scoped dependency | Singleton or scoped | Match framework pattern |
| Lightweight helper with no state | Transient | Keep construction cheap |
| Hosted service or cache owner | Singleton | Do not capture scoped service |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Registration behavior verification | `dotnet test [test-project].csproj --filter DependencyInjection` when tests exist | Zero failing tests |
| Provider scan verification | Search changed files for `BuildServiceProvider()` or `GetService` in feature code | Zero unauthorized matches |

## Verification Checklist

Agent verifies:
- [ ] Every registration has intentional lifetime
- [ ] Singleton scope does not capture scoped state
- [ ] Constructors expose required collaborators
- [ ] Composition root owns registrations
- [ ] Disposal ownership is clear
- [ ] Registration pattern stays repeatable

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Transient becomes default for every service | Pick lifetime intentionally |
| Concrete infrastructure type crosses layer line | Inject contract |
| `BuildServiceProvider()` appears inside setup code | Use existing composition root |
| Service location hides lifetime defect | Fix graph instead |
