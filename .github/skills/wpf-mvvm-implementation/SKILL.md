---
name: wpf-mvvm-implementation
title: WPF MVVM Implementation
description: Implement WPF MVVM with observable state, command-based behavior, injected services, and validation that remains testable and binding-friendly.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1175
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - wpf-project-setup
  - wpf-testing-patterns
  - dependency-injection-patterns
appliesTo: '**/*.{cs,csproj,xaml}'
tags:
  - wpf
  - mvvm
  - bindings
  - desktop
---
# WPF MVVM Implementation

Agent implements MVVM so that WPF views stay thin, view models expose user-facing state, and commands orchestrate services without code-behind business logic.

## When to Use

| Condition | Use |
|---|---|
| Agent adds screen behavior, commands, or validation to a WPF feature. | Agent uses this skill. |
| Agent replaces code-behind state with observable view-model state. | Agent uses this skill. |
| Agent needs fast tests around desktop behavior. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent changes only XAML layout or styling. | Agent uses `wpf-screen-generation`. |
| Agent works in a project that intentionally uses a different presentation pattern. | Agent follows the existing pattern. |
| Agent focuses on HTTP transport or token flow. | Agent uses `wpf-rest-client-integration`. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Screen workflow | Yes | Workflow identifies the user task and state transitions. |
| Model contracts | Yes | Contracts identify the data the view model exposes or transforms. |
| Service dependencies | Yes | Dependencies identify dialogs, navigation, storage, or API boundaries. |
| Validation rules | No | Rules identify field and command guardrails. |
| Async requirements | No | Requirements identify loading, cancellation, and retry expectations. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Model the screen contract | Agent lists editable state, derived state, and commands before editing bindings. | View-model contract | Agent reviews the contract. | The contract matches user-facing language. |
| 2. Define observable state | Agent uses `ObservableObject`, `ObservableValidator`, or equivalent generated properties for mutable state. | Observable properties | Agent changes properties in a test or running view. | Property changes raise notifications predictably. |
| 3. Define command behavior | Agent exposes user actions through command properties with explicit guard rules. | Command surface | Agent toggles command preconditions. | Command enablement follows view-model state. |
| 4. Inject collaborators | Agent constructor-injects services for transport, dialogs, storage, or navigation. | Dependency graph | Agent instantiates the view model in tests. | The view model resolves without WPF globals. |
| 5. Bind intentionally | Agent applies binding modes and update triggers deliberately for editable and read-only fields. | Binding map | Agent edits the UI or binding tests. | Edited values reach the view model at the intended time. |
| 6. Add validation and async state | Agent keeps validation near state and exposes busy or error flags for long-running work. | Validation and loading model | Agent triggers invalid and slow paths. | Invalid state blocks unsafe actions and long work stays observable. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Behavior verification | Existing unit tests or targeted `dotnet test [test-project].csproj` | Commands and property notifications pass |
| Binding verification | Manual screen pass | Bound edits and loading flags appear as expected |

## Verification Checklist

- [ ] Agent keeps business logic out of code-behind.
- [ ] Agent exposes commands instead of click-handler logic.
- [ ] Agent injects collaborators through the constructor.
- [ ] Agent surfaces validation through standard WPF mechanisms.
- [ ] Agent keeps async work observable and cancelable.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent writes repetitive property-change plumbing by hand. | Agent uses generator-backed observable properties. |
| Agent hides dependencies behind a service locator. | Agent constructor-injects explicit collaborators. |
| Agent binds collections without explicit selection state. | Agent models selection and derived flags in the view model. |
| Agent blocks the dispatcher with `.Result` or `.Wait()`. | Agent uses awaited commands and async services. |
