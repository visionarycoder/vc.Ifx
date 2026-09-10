---
name: migrate-static-to-wrapper
description: Replace static dependency call sites with existing wrappers or built-in abstractions inside a bounded scope.
license: MIT
title: Migrate Static to Wrapper
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 978
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - detect-static-dependencies
  - generate-testability-wrappers
appliesTo: '**/*'
tags:
  - migrate
  - static
  - wrapper
  - dependency-injection
---
# Migrate Static to Wrapper

Agent replaces static dependency calls with existing seams inside a bounded scope.

## When to Use

| User prompt | Use |
|---|---|
| User already has `TimeProvider`, `IFileSystem`, or a wrapper | Use this skill |
| User asks for codemod-style replacement in one project or namespace | Use this skill |
| User asks to add constructor injection for an existing seam | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| Wrapper does not exist | Use `generate-testability-wrappers` |
| User asks only for detection | Use `detect-static-dependencies` |
| User asks for broad architecture changes | Open a separate task |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Static pattern | Yes | Example: `DateTime.UtcNow` |
| Replacement seam | Yes | Example: `TimeProvider` |
| Scope | Yes | File, directory, project, or namespace |
| Injection strategy | No | Constructor, primary constructor, or ambient static seam |

## Replacement Matrix

| Original | Replacement |
|---|---|
| `DateTime.Now` | `timeProvider.GetLocalNow().DateTime` |
| `DateTime.UtcNow` | `timeProvider.GetUtcNow().DateTime` |
| `DateTimeOffset.UtcNow` | `timeProvider.GetUtcNow()` |
| `File.*` | `fileSystem.File.*` |
| `Directory.*` | `fileSystem.Directory.*` |
| `Environment.*` | `environmentProvider.*` |
| `Console.*` | `console.*` |
| `Process.*` | `processRunner.*` |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies the replacement seam and registration already exist. | Run `rg` for the abstraction and registration. | Both exist in scope. |
| 2 | Agent limits scope to production `.cs` files. | Read the file list. | Tests, `bin`, `obj`, and generated files are excluded. |
| 3 | Agent picks one injection strategy per class. | Read changed class declarations. | Each changed class uses one consistent strategy. |
| 4 | Agent replaces static calls using the replacement matrix. | Run `rg` for the old pattern in the edited scope. | Old calls are zero in the edited scope. |
| 5 | Agent updates tests or fakes for changed constructors. | Read touched test files. | Tests compile with the new seam. |
| 6 | Agent runs build and target tests. | Run `dotnet build` and target test commands. | Zero new build errors and zero new test failures. |

## Injection Matrix

| Class shape | Use |
|---|---|
| Existing constructor-based class | Add constructor injection |
| Existing primary-constructor file | Add one primary constructor parameter |
| Static class | Use an ambient static seam and restore it in tests |

## Verification Checklist

- [ ] Agent verified the seam and registration before edits.
- [ ] Agent limited edits to the named scope.
- [ ] Agent removed old static calls from the edited scope.
- [ ] Agent updated tests for new dependencies.
- [ ] Agent ran build and target tests.
- [ ] Agent preserved runtime behavior.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent adds a new seam during migration | Agent routes seam generation to `generate-testability-wrappers`. |
| Agent edits test code first | Agent migrates production code first, then test constructors. |
| Agent converts static classes to instance classes | Agent uses an ambient static seam. |
| Agent migrates too much at once | Agent keeps the named scope small. |
