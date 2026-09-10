---
name: commands-behaviors-patterns
title: Commands and Behaviors Patterns
description: Implement command surfaces, async actions, and event-to-command behavior wiring when agent builds .NET MVVM interaction flows.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1360
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mvvm-patterns-dotnet
  - databinding-patterns
  - wpf-mvvm-implementation
  - maui-patterns
  - winui3-patterns
appliesTo: '**/*.{cs,csproj,xaml,axaml}'
tags:
  - commands
  - behaviors
  - mvvm
  - async
  - ui
---
# Commands and Behaviors Patterns

Agent models user interaction through `ICommand`, relay commands, and behavior-based event mapping so views stay declarative and view models stay testable.

## When to Use

| Condition | Use |
|---|---|
| Work replaces click handlers or selection events with command bindings | Use this skill |
| Work adds parameterized commands or async UI actions | Use this skill |
| Work needs `CanExecute` state that reacts to view-model changes | Use this skill |
| Work maps UI events to commands through behaviors or attached properties | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work defines overall MVVM architecture only | Use `mvvm-patterns-dotnet` |
| Work focuses on binding syntax, update triggers, or converters | Use `databinding-patterns` |
| Work edits background services with no UI interaction | Use the matching service skill |
| Work follows a deliberate framework pattern that keeps view-only event handlers | Follow the existing pattern |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| User actions | Yes | Identify triggers, parameters, and enabled state. |
| Async behavior | Yes | Identify loading, cancellation, and duplicate-invocation rules. |
| Target platforms | Yes | Identify WPF, MAUI, WinUI 3, or UWP behavior options. |
| Validation and guards | No | Identify `CanExecute` conditions. |
| Navigation or messaging hooks | No | Identify downstream coordination paths. |

## Pattern Matrix

| Scenario | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Simple synchronous action | `RelayCommand` or focused `ICommand` | Avoid async work in a synchronous command | Action stays readable and testable |
| Parameterized action | `RelayCommand<T>` or equivalent typed command | Validate null or type drift early | Command receives the intended parameter shape |
| Slow or remote action | `AsyncRelayCommand` or explicit async command wrapper | Block duplicate invocation while work is active | One operation runs and UI state resets predictably |
| Unsupported direct command surface | Behavior or attached property | Keep business logic in the view model | Event reaches the intended command without code-behind drift |
| Shell-level gesture or routed input | Platform command binding where the framework already uses it | Keep scope narrow and consistent | Keyboard and menu flows stay aligned |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent lists actions, parameters, and enablement rules. | Review the interaction map. | Each action has one command owner and guard rule. |
| 2 | Agent selects the command type that matches async and parameter needs. | Review one command per action. | Command type matches the workflow. |
| 3 | Agent wires `CanExecute` dependencies to observable state changes. | Toggle one guard property or selection path. | Enabled state updates without stale UI. |
| 4 | Agent maps events through behaviors or attached properties when direct command binding is absent. | Trigger one event-to-command path. | Event flow reaches the intended command. |
| 5 | Agent binds parameters deliberately and validates null or type drift. | Invoke one parameterized path. | Parameter arrives with the expected type and value. |
| 6 | Agent verifies async reentry, error recovery, and cancellation handling. | Run one slow path and one failure path. | UI blocks duplicates and resets predictably. |

## Rules

| Topic | Rule |
|---|---|
| Async commands | Use async-capable command types for async work. |
| Enablement | Raise `CanExecuteChanged` from every dependent state change path. |
| Behaviors | Route business changes back through the view-model command. |
| Parameters | Bind typed objects or stable identifiers instead of fragile strings. |
| Reentry | Guard against duplicate taps or clicks during in-flight work. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Action mapping | Review touched controls and view models. | No business action stays trapped in code-behind. |
| `CanExecute` reactivity | Change one guard property and one selection path. | Enabled state updates immediately. |
| Async reentry | Trigger rapid repeated input on one async path. | One operation runs and UI state resets at completion. |
| Event-to-command wiring | Inspect XAML and behavior wiring. | Event mapping stays declarative. |
| Parameter shape | Trigger one parameterized path. | Command receives the intended parameter shape. |

## Outputs

- Interaction map with commands, parameters, and guards
- Command type selection for synchronous and asynchronous actions
- Behavior or attached-property plan for unsupported direct command surfaces
- Verification steps for enablement, parameters, and async reentry
