---
name: design-smell-detection
title: Design Smell Detection
description: Detect structural and architectural design smells that indicate poor modularity, coupling, or separation of concerns.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 900
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - antipattern-detection-bundle
  - dotnet-architectural-layers
  - dependency-injection-patterns
  - resolve-project-references
related_docs:
  - https://en.wikipedia.org/wiki/Design_smell
appliesTo: '**/*.{cs,csproj,sln,slnx}'
tags:
  - design-smell
  - architecture
  - coupling
  - cohesion
  - modularity
---

# Design Smell Detection

Agent detects design smells—structural quality issues at module, namespace, and project levels that indicate poor separation of concerns or excessive coupling.

## When to Use

| Condition | Use |
|---|---|
| Project or namespace dependency analysis | Use this skill |
| Architectural review | Use this skill |
| Modularity or coupling concerns | Use this skill |
| Solution structure refactoring | Use this skill |

## When Not to Use

| Condition | Alternative |
|---|---|
| Method or class-level issues | Use `code-smell-detection` |
| Performance issues | Use `performance-antipatterns` |

## Design Smell Catalog

| Smell | Detection Signal | Impact | Remediation |
|---|---|---|---|
| Cyclic Dependencies | Project or namespace circular references | Build order ambiguity, tight coupling | Break cycle with interface or event |
| Tight Coupling | Concrete class depends on concrete class | Hard to test, fragile | Introduce abstraction (interface) |
| Insufficient Modularization | Large project with many unrelated concerns | Poor cohesion | Split into focused projects |
| Broken Hierarchy | Derived class violates Liskov Substitution | Inheritance misuse | Replace with composition or strategy |
| Missing Abstraction | Direct dependency on third-party library | Vendor lock-in | Introduce adapter or port/adapter |
| Unstable Dependencies | Stable components depend on volatile ones | Ripple effect from changes | Invert dependency with abstraction |
| Feature Envy (module-level) | Module uses another module's internals heavily | Misplaced responsibility | Move feature to correct module |

## Workflow

1. Agent analyzes project references, namespace dependencies, and type relationships.
2. Agent detects design smells using dependency analysis.
3. Agent classifies smells by severity and refactoring effort.
4. Agent proposes architectural refactoring.
5. Agent validates refactoring improves modularity metrics.

Test: Build solution and run architectural tests.
Pass: Cycles eliminated. Coupling metrics improve. All tests pass.

## Detection Rules

| Rule | Pattern | Refactoring |
|---|---|---|
| DESIGN-001 | Project A references B, B references A | Extract shared abstractions to project C |
| DESIGN-002 | Namespace A.B references A.C.D (skip level) | Remove skip-level dependency, use A.C |
| DESIGN-003 | Concrete class directly instantiates concrete dependency | Use DI and interface |
| DESIGN-004 | Project >50 classes with unrelated concerns | Split by bounded context or layer |
| DESIGN-005 | Derived class throws NotImplementedException on base method | Use composition instead of inheritance |
| DESIGN-006 | Direct reference to third-party library in business layer | Introduce adapter in infrastructure layer |
| DESIGN-007 | Stable core depends on volatile UI or infrastructure | Invert dependency with port/adapter |

## Integration with Architecture Analysis

**Tools:**
- NDepend — .NET dependency analysis and metrics
- ArchUnitNET — Architecture unit tests for .NET
- Structure101 — Dependency structure matrix
- Roslyn architectural analyzers

**Metrics:**
- Afferent coupling (Ca) — Number of incoming dependencies
- Efferent coupling (Ce) — Number of outgoing dependencies
- Instability (I = Ce / (Ce + Ca)) — Ranges 0 (stable) to 1 (unstable)
- Abstractness (A) — Ratio of abstract types to all types

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Build verification | `dotnet build` | Zero circular dependency errors |
| Architectural tests | `dotnet test` (ArchUnit tests) | All architectural constraints satisfied |
| Dependency metrics | Dependency analyzer | Coupling decreased, cohesion increased |
| Layer violations | Architectural analyzer | Zero violations |

## Prevention Guidance

Agent prevents design smells when structuring solutions:

- Define clear layer boundaries (Domain, Application, Infrastructure, Presentation)
- Use Dependency Inversion Principle (depend on abstractions)
- Keep projects focused on single bounded context
- Avoid skip-level namespace references
- Use ports and adapters for external dependencies
- Enforce one-way dependencies (inner layers independent of outer)

## Verification Checklist

Agent verifies:
- [ ] Dependency analysis completed
- [ ] Design smells classified by severity
- [ ] Refactoring approach proposed
- [ ] Build succeeds after refactoring
- [ ] Architectural tests pass
- [ ] Coupling metrics improve
