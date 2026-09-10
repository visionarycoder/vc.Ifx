---
name: mvvm-testing-patterns
title: MVVM Testing Patterns
description: Test MVVM view models, commands, validation, navigation, messaging, and DI-driven behavior with MSTest v4 and .NET 10 repository conventions.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1382
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - dotnet-unit-testing
related_skills:
  - writing-mstest-tests
  - wpf-testing-patterns
  - validation-mvvm
appliesTo: '**/*.{cs,csproj}'
tags:
  - mvvm
  - testing
  - mstest
  - communitytoolkit
  - dotnet10
---
# MVVM Testing Patterns

Agent validates MVVM behavior through focused MSTest v4 coverage for observable state, commands, async flows, validation, messaging, navigation, and service-backed workflows.

## When to Use

| Condition | Use |
|---|---|
| Agent adds or changes view-model logic in WPF, WinUI, MAUI, or toolkit-based MVVM code. | Agent uses this skill. |
| Agent needs regression coverage for `PropertyChanged`, command guards, or async commands. | Agent uses this skill. |
| Agent tests navigation, messenger interactions, or validation-driven command readiness. | Agent uses this skill. |
| Agent measures view-model coverage with MSTest v4 and repository patterns. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent tests rendering, layout, or accessibility behavior only. | Agent uses a UI automation or screen-focused skill. |
| Agent tests Web API endpoints, handlers, or DTO validators. | Agent uses the matching backend testing skill. |
| Agent needs a broad suite audit with no implementation work. | Agent uses a testing audit skill. |
| Agent changes only passive models with no MVVM behavior. | Agent uses general unit testing guidance. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target view model | Yes | Identify state, commands, and collaborators. |
| Expected behavior | Yes | Identify notifications, command gates, and side effects. |
| Dependency seams | Yes | Identify services, repositories, messenger, and navigation boundaries. |
| Coverage scope | No | Identify unit, integration, or coverage collection targets. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads the target view model and sibling tests to match local naming and fixture patterns. | Review neighboring tests. | New tests align with repository conventions. |
| 2 | Agent maps observable behaviors: property changes, command paths, validation, messaging, navigation, and async state. | Review the test map. | Each changed behavior has a test target. |
| 3 | Agent selects the smallest isolation boundary for each collaborator. | Review fakes and mocks. | Setup stays concise and behavior-focused. |
| 4 | Agent writes MSTest v4 tests for happy, edge, and failure paths through the public surface. | Run targeted tests. | Public behavior coverage stays balanced. |
| 5 | Agent runs targeted coverage collection when the repository already uses coverage tooling. | Collect coverage for the target project. | View-model coverage runs complete successfully. |

## Test Strategy Matrix

| Behavior | Agent pattern | Pass |
|---|---|---|
| Property change notification | Subscribe to `PropertyChanged` and capture changed member names. | Expected property names arrive in order. |
| Command execution | Invoke the public command surface instead of private methods. | Expected state and collaborator effects occur. |
| `CanExecute` guard | Toggle state around the guard condition and reevaluate commands. | Guard transitions match observable state. |
| Async command | Await `ExecuteAsync`, inspect busy flags, and verify failure recovery. | Async state resets and outcomes stay deterministic. |
| Validation logic | Drive invalid and valid inputs through the view-model surface. | Errors appear and clear predictably. |
| Messaging and navigation | Use deterministic recipients and fake navigation seams. | Message interactions and route requests match expectation. |

## Isolation Matrix

| Dependency shape | Agent default | Agent avoids |
|---|---|---|
| Remote services | Mocks or deterministic fakes | Live infrastructure in unit tests |
| Navigation | Fake route and payload recorder | Shell or frame globals |
| Messenger | `WeakReferenceMessenger` test setup or a focused fake bus | Cross-test shared singleton state |
| DI container | Real `ServiceCollection` only for composition-sensitive checks | Container bootstrapping in simple unit tests |

## Reference Files

| File | Purpose |
|---|---|
| [references/testing-examples.md](references/testing-examples.md) | Example tests, coverage details, integration hooks, MCP hooks, pitfalls, and outputs. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Notification coverage | Run notification-focused tests. | Expected property names raise correctly. |
| Command coverage | Execute sync and async command tests. | State and collaborator effects match intent. |
| Validation coverage | Run invalid and valid state tests. | Errors and command gates stay synchronized. |
| Messaging coverage | Publish or receive through the configured messenger path. | Message interactions fire exactly once per scenario. |
| Isolation quality | Review test doubles and cleanup. | Tests stay independent and order-insensitive. |
| Coverage command | Run the targeted coverage command when applicable. | Coverage collection completes successfully. |
