---
name: mvvm-patterns-dotnet
title: .NET MVVM Patterns
description: Establish MVVM architecture, observable state, and platform-specific boundaries when agent implements or reviews .NET UI features.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1903
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - wpf-mvvm-implementation
  - maui-patterns
  - winui3-patterns
  - uwp-patterns
  - dependency-injection-patterns
appliesTo: '**/*.{cs,csproj,xaml,axaml}'
tags:
  - mvvm
  - dotnet
  - desktop
  - ui
  - architecture
---
# .NET MVVM Patterns

Agent structures .NET UI features so models hold domain data, views hold presentation markup, and view models expose observable state and commands without business logic in code-behind.

## When to Use

| Condition | Use |
|---|---|
| Agent creates a new WPF, MAUI, WinUI 3, UWP, or Avalonia screen. | Use this skill. |
| Agent moves business logic from code-behind into a testable presentation layer. | Use this skill. |
| Agent needs manual `INotifyPropertyChanged` or `ObservableCollection<T>` patterns. | Use this skill. |
| Compare MVVM against MVC or MVP for an existing UI architecture choice. | Use this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent edits backend services, APIs, or data access only. | Use the matching service skill. |
| Agent changes styling, layout, or control templates only. | Use the platform UI skill. |
| Agent works in a project with an established non-MVVM presentation pattern. | Follow the existing pattern. |
| Agent focuses on command wiring details only. | Use `commands-behaviors-patterns`. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Screen workflow | Yes | Workflow identifies state, user actions, and transitions. |
| Domain model boundaries | Yes | Boundaries identify model ownership and transformation points. |
| Target platforms | Yes | Scope identifies WPF, MAUI, WinUI 3, UWP, or Avalonia. |
| Validation scope | No | Scope identifies editable fields and error rules. |
| Navigation pattern | No | Pattern identifies page, window, or shell transitions. |

## Pattern Comparison

| Pattern | State Owner | View Responsibility | Interaction Style | Best Fit |
|---|---|---|---|---|
| MVVM | View model | Bindings, templates, visual states | Data binding and commands | XAML-first UI stacks |
| MVC | Controller | Passive rendering | Controller actions update model and view | Request-response UI or web apps |
| MVP | Presenter | Thin interface surface | Presenter pushes values into view contract | UI stacks with weak binding support |
| Code-behind driven UI | View | Layout and behavior mix | Event handlers mutate control state directly | Small throwaway screens only |

## Observable State Matrix

| Concern | Pattern | Pass |
|---|---|---|
| Scalar state | `INotifyPropertyChanged` or shared observable base type | One logical property change raises one notification. |
| Collection state | `ObservableCollection<T>` for mutable bound lists | Add, remove, and reset operations refresh the UI. |
| Derived state | Explicit dependent-property notifications | Derived values stay synchronized with source state. |
| Bulk refresh | Replace collection contents or the view-model instance, not control state | Data and selection stay coherent. |
| Thread affinity | Marshal UI-bound updates onto the UI thread | Async refresh paths avoid cross-thread UI exceptions. |


## Platform-Specific Guidance

| Platform | View and binding | View-model and hooks |
|---|---|---|
| WPF | `DataContext`, `Binding`, validation rules, `CollectionViewSource` | Manual `INotifyPropertyChanged`, `ObservableObject`, Prism regions, or MVVM Toolkit generators |
| .NET MAUI | `BindingContext`, `ObservableCollection<T>`, Shell or page navigation | Constructor-injected services plus MVVM Toolkit or MAUI Community Toolkit |
| WinUI 3 | `Page`, `NavigationView`, `x:Bind`, or `Binding` | Async initialization with CommunityToolkit.Mvvm or Prism for WinUI |
| UWP | Adaptive XAML with `x:Bind` or `Binding` | Activation-aware view models with Prism or MVVM Toolkit |
| Avalonia | `DataContext` and compiled bindings where enabled | Dispatcher-aware observable models with CommunityToolkit.Mvvm or ReactiveUI interop |


## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Map model, view, and view-model responsibilities before edits. | Each touched file has one clear owner. |
| 2 | Move business logic from code-behind into view models or injected services. | Code-behind keeps composition only. |
| 3 | Implement observable scalar, collection, derived, and validation state. | Bound UI updates match state changes. |
| 4 | Align binding and lifecycle choices with the target platform. | XAML and navigation follow platform conventions. |
| 5 | Compare the feature to MVVM, MVC, and MVP fit criteria. | The chosen pattern matches the UI stack and interaction style. |


## Integration Hooks

| Integration | Trigger | Pass |
|---|---|---|
| CommunityToolkit.Mvvm | Boilerplate properties or commands appear across files. | Generated members replace repeated notification plumbing. |
| Prism | Existing app conventions already use Prism navigation or regions. | New work matches the container and navigation style. |
| Source generators | .NET 10+ UI projects already allow MVVM Toolkit generators. | Generated members compile and reduce manual code. |
| Manual implementation | The project avoids extra helper packages. | Notification behavior stays explicit and testable. |


## MCP Hooks

| Task | MCP Tool | Assistance |
|---|---|---|
| Find view, view-model, and code-behind ownership drift | `github-mcp-server-search_code` | Locates bindings, commands, notification paths, and code-behind logic. |
| Inspect versioned screen files in branches or pull requests | `github-mcp-server-get_file_contents` | Reads XAML, view-model, and project files without local branch switches. |
| Review prior MVVM architecture decisions | `github-mcp-server-search_pull_requests` | Surfaces earlier refactors, naming patterns, and reviewer guidance. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Layer boundaries | Review touched files and search code-behind handlers. | Responsibilities stay separated and handlers delegate to commands or coordinators. |
| Notifications | Exercise direct, derived, and collection updates. | UI refreshes correctly with no manual control mutation. |
| Platform fit | Compare implementation to the platform guidance table. | Binding and lifecycle choices match the target stack. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| View model exposes control types or visual primitives that belong to the view. | Expose plain state and lets XAML handle presentation. |
| Property setter raises notifications for unchanged values. | Agent guards setters with equality checks. |
| Derived property notifications stay missing. | Raise explicit notifications for dependent properties. |
| Collections use `List<T>` in bound screens. | Switch bound mutable collections to `ObservableCollection<T>`. |
| Service location appears inside the view model. | Constructor-inject dependencies. |

## Outputs

| Output | Contents |
|---|---|
| Responsibility map | Model, view, and view-model ownership for the feature |
| State pattern | Observable property, collection, and validation choices |
| Platform notes | Binding, navigation, and lifecycle decisions |
| Verification plan | Focused UI update and architecture checks |

