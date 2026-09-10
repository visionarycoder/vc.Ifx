---
name: code-smell-detection
title: Code Smell Detection
description: Detect code smells that indicate design or implementation quality issues requiring refactoring.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1100
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - antipattern-detection-bundle
  - refactor
  - dotnet-code-quality-standards
  - breaking-primitive-obsession
related_docs:
  - https://en.wikipedia.org/wiki/Code_smell
  - https://refactoring.guru/refactoring/smells
appliesTo: '**/*.{cs,js,ts}'
tags:
  - code-smell
  - refactoring
  - quality
  - maintainability
---

# Code Smell Detection

Agent detects code smells—characteristics in source code that hint at deeper design or implementation quality problems.

## When to Use

| Condition | Use |
|---|---|
| Code review for quality issues | Use this skill |
| Refactoring existing codebase | Use this skill |
| Generating new code | Use this skill (prevention) |
| Complexity or maintainability concerns | Use this skill |

## When Not to Use

| Condition | Alternative |
|---|---|
| Performance anti-patterns | Use `performance-antipatterns` |
| Architecture-level design issues | Use `design-smell-detection` |

## Code Smell Catalog

Agent detects these code smells:

| Smell | Detection Signal | Impact | Reference |
|---|---|---|---|
| Duplicated Code | Identical or very similar code >10 lines | Maintenance burden | [Reference](references/duplicated-code.md) |
| Long Method | Method >50 lines or cyclomatic complexity >10 | Difficult to understand | [Reference](references/long-method.md) |
| Large Class | Class >500 lines or >10 public methods | Too many responsibilities | [Reference](references/large-class.md) |
| Too Many Parameters | Method >5 parameters | Hard to call and test | [Reference](references/too-many-parameters.md) |
| Data Clump | Same group of variables passed together | Missing abstraction | [Reference](references/data-clump.md) |
| Shotgun Surgery | Single change affects many classes | Poor cohesion | [Reference](references/shotgun-surgery.md) |
| Feature Envy | Method uses another class's data heavily | Misplaced responsibility | [Reference](references/feature-envy.md) |
| Magic Numbers | Unexplained numeric or string literals | Reduced readability | [Reference](references/magic-numbers.md) |
| Primitive Obsession | Primitives instead of value objects | Weak domain model | [Reference](references/primitive-obsession.md) |
| Refused Bequest | Derived class violates base contract | Liskov violation | [Reference](references/refused-bequest.md) |

## Workflow

1. Agent scans code for smell detection signals.
2. Agent classifies smells by severity and refactoring effort.
3. Agent references detailed remediation guidance.
4. Agent proposes specific refactoring steps.
5. Agent validates refactoring preserves behavior.

Test: Run existing tests after refactoring.
Pass: All tests pass. Code complexity metrics improve.

## Detection Rules

| Rule | Pattern | Refactoring |
|---|---|---|
| SMELL-001 | Code block duplicated >2 locations | Extract Method or Extract Class |
| SMELL-002 | Method >50 lines | Extract Method (compose method pattern) |
| SMELL-003 | Class >500 lines or >10 public methods | Extract Class or Extract Subclass |
| SMELL-004 | Method >5 parameters | Introduce Parameter Object or Builder |
| SMELL-005 | Same 3+ variables passed together | Introduce Value Object |
| SMELL-006 | Change requires edits to >5 classes | Move Method or Move Field |
| SMELL-007 | Method uses >3 fields from another class | Move Method to owner class |
| SMELL-008 | Unexplained numeric literal | Replace Magic Number with Symbolic Constant |
| SMELL-009 | Primitive for domain concept | Replace Data Value with Object |
| SMELL-010 | Derived class doesn't use base members | Replace Inheritance with Delegation |

## Integration with Static Analysis

**Tools:**
- SonarQube — Comprehensive code smell detection
- Roslyn analyzers — C#-specific smell detection
- ESLint — JavaScript/TypeScript smell detection
- PMD/CheckStyle — Java smell detection

**Metrics:**
- Cyclomatic complexity (>10 indicates Long Method)
- Lines of code per method/class
- Coupling metrics (afferent/efferent coupling)
- Maintainability index

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Existing tests | `dotnet test` | All tests pass (behavior preserved) |
| Code metrics | Static analyzer | Complexity decreased, maintainability increased |
| Code duplication | Duplication detector | Duplication reduced by >50% |
| Build verification | `dotnet build` | Zero compile errors |

## Prevention Guidance

Agent prevents smells when generating code:

- Keep methods <30 lines, single level of abstraction
- Keep classes <300 lines, single responsibility
- Use parameter objects for >3 related parameters
- Extract constants for domain-meaningful literals
- Introduce value objects for domain concepts
- Favor composition over inheritance

## Verification Checklist

Agent verifies:
- [ ] Code smell catalog scanned
- [ ] Severity assigned based on impact
- [ ] Refactoring approach selected
- [ ] Behavior preservation validated via tests
- [ ] Code metrics improved
- [ ] No new smells introduced
