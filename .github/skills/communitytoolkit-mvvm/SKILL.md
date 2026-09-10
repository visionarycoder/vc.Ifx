---
name: communitytoolkit-mvvm
title: CommunityToolkit.Mvvm Patterns
description: Apply CommunityToolkit.Mvvm attributes, messaging, validation, and DI patterns when .NET MVVM work needs generated observable state and commands.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1423
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - wpf-mvvm-implementation
  - maui-patterns
  - winui3-patterns
  - dependency-injection-patterns
  - dotnet-unit-testing
appliesTo: '**/*.{cs,csproj,xaml,axaml}'
tags:
  - mvvm
  - communitytoolkit
  - source-generators
  - messenger
  - validation
---
# CommunityToolkit.Mvvm Patterns

Agent implements CommunityToolkit.Mvvm with source-generated properties, commands, messaging, validation, and constructor-injected services so views stay thin and view models stay explicit.

## When to Use

| Condition | Use |
|---|---|
| Agent adds or modernizes MVVM view models in WPF, MAUI, or WinUI 3. | Agent uses this skill. |
| Agent replaces manual `INotifyPropertyChanged` or `ICommand` boilerplate. | Agent uses this skill. |
| Agent needs weak-reference messaging between loosely coupled view models or services. | Agent uses this skill. |
| Agent needs validation with observable error state. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent needs large-scale modular navigation, region composition, or module catalogs. | Agent uses `prism-framework-patterns`. |
| Agent authors a custom generator instead of consuming toolkit generators. | Agent uses `mvvm-source-generators`. |
| Agent edits only XAML layout or styling. | Agent uses the matching platform skill. |
| Agent works in a codebase that already standardizes on Prism or another MVVM framework. | Agent follows the established framework. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Platform scope | Yes | Identify WPF, MAUI, WinUI 3, or mixed desktop targets. |
| View-model responsibilities | Yes | Identify mutable state, derived state, and user actions. |
| Service boundaries | Yes | Identify navigation, dialogs, data access, and background work. |
| Message flows | No | Identify sender, recipient, and message lifetime. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps state, commands, validation rules, and message boundaries before edits. | Review the view-model contract. | Mutable state and user actions have one owner. |
| 2 | Agent converts touched types to `partial` toolkit-backed view models. | Build the target project. | Source generators run without missing-type errors. |
| 3 | Agent replaces manual property and command boilerplate with toolkit attributes. | Run targeted UI or unit tests. | Touched behavior matches the previous contract or target behavior. |
| 4 | Agent adds validation and messaging features where the feature scope needs them. | Trigger invalid data and one message path. | Validation and messaging remain explicit and bounded. |
| 5 | Agent inspects generated output during troubleshooting. | Open generated files after build. | Generated members align with source attributes and naming. |

## Toolkit Pattern Matrix

| Concern | Pattern | Pass |
|---|---|---|
| Observable state | `ObservableObject` or `ObservableRecipient` base type | Property notifications raise once per change |
| Generated properties | `[ObservableProperty]` on backing fields or supported partial properties | Public properties and change hooks appear |
| Commands | `[RelayCommand]` on sync or async methods | Command instances generate and enablement stays correct |
| Validation | `ObservableValidator` for editable forms | Validation errors surface and unsafe actions stay blocked |
| Messaging | `WeakReferenceMessenger` or injected `IMessenger` | Intended recipients update once and registration lifetime stays bounded |
| Dependency injection | Constructor injection for services and `IMessenger` | View models resolve without service locators or static globals |

## Migration Matrix

| Legacy pattern | Toolkit migration action | Pass |
|---|---|---|
| Manual `INotifyPropertyChanged` | Convert to `ObservableObject` or `ObservableValidator` plus `[ObservableProperty]`. | Property notifications remain equivalent. |
| Manual `ICommand` classes | Replace with `[RelayCommand]` methods. | Command behavior matches prior flow. |
| Static event hubs | Replace with injected `IMessenger` and explicit message contracts. | Sender and recipient stay decoupled. |
| Hand-authored validation plumbing | Replace with `ObservableValidator` and data annotations. | Error state stays observable and consistent. |

## Reference Files

| File | Purpose |
|---|---|
| [references/generator-examples.md](references/generator-examples.md) | Setup details, manual-versus-generated comparisons, code examples, platform hooks, MCP hooks, pitfalls, and outputs. |

## Verification Checklist

| Item | Test | Pass |
|---|---|---|
| Toolkit package reference exists. | Inspect the project file and build output. | `CommunityToolkit.Mvvm` resolves. |
| Generator-backed types are `partial`. | Inspect changed types and build output. | Zero missing-partial errors remain. |
| Commands and properties generate as expected. | Inspect `obj\generated` and invoke one command path. | Generated APIs match the source attributes. |
| Validation stays observable. | Trigger one invalid input path. | Errors surface through the view-model contract. |
| Messaging stays loosely coupled. | Send one message path and inspect recipients. | No direct sender-to-recipient dependency appears. |
