---
name: mvvm-toolkit-bundle
title: MVVM Toolkit Bundle
description: Routes MVVM implementation work to the correct MVVM skill set and selects manual, CommunityToolkit, Prism, generator, validation, testing, and platform guidance for the request.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 2016
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mvvm-patterns-dotnet
  - databinding-patterns
  - commands-behaviors-patterns
  - viewmodel-lifecycle
  - communitytoolkit-mvvm
  - prism-framework-patterns
  - mvvm-source-generators
  - validation-mvvm
  - mvvm-testing-patterns
  - wpf-fluent-ui
  - maui-patterns
  - winui3-patterns
  - uwp-patterns
appliesTo: '**/*.{cs,csproj,xaml,axaml,json,xml,md}'
tags:
  - mvvm
  - desktop
  - maui
  - winui3
  - wpf
  - uwp
  - routing
---
# MVVM Toolkit Bundle

Agent uses this bundle for MVVM requests that require routing across manual MVVM, CommunityToolkit.Mvvm, Prism, binding, commands, lifecycle, validation, source generators, tests, or XAML platform selection.

## When to Use

| Prompt Pattern | Use This Bundle |
|---|---|
| New MVVM feature or screen architecture | Yes |
| Manual MVVM versus CommunityToolkit versus Prism selection | Yes |
| Binding, commands, validation, lifecycle, and testing in one request | Yes |
| Single narrow MVVM topic with no routing need | No |

## When Not to Use

| Prompt Pattern | Agent Route |
|---|---|
| Only WPF Fluent UI composition or styling | `wpf-fluent-ui` |
| Only platform shell, deployment, or native UI structure | `maui-patterns`, `winui3-patterns`, or `uwp-patterns` |
| Only one MVVM testing task | `mvvm-testing-patterns` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Platform target | Yes | WPF, MAUI, WinUI 3, or UWP |
| Framework preference | No | Manual MVVM, CommunityToolkit, Prism, or undecided |
| Navigation and lifecycle needs | No | Region navigation, page activation, resume, or teardown |
| Validation and test scope | No | Forms, async commands, state transitions, or binding tests |

## Activation

| Request Signal | Use Bundle | Route Detail |
|---|---|---|
| View-model structure, state, and separation of concerns | Yes | `mvvm-patterns-dotnet` |
| Bindings, converters, templates, or update timing | Yes | `databinding-patterns` |
| Commands, behaviors, gesture handling, or async actions | Yes | `commands-behaviors-patterns` |
| Activation, navigation, disposal, or message cleanup | Yes | `viewmodel-lifecycle` |
| Source-generated observable properties and relay commands | Yes | `communitytoolkit-mvvm` |
| Region composition, event aggregation, or Prism navigation | Yes | `prism-framework-patterns` |
| Custom Roslyn generation for MVVM plumbing | Yes | `mvvm-source-generators` |
| Errors, editable forms, or validation summaries | Yes | `validation-mvvm` |
| View-model unit tests, command tests, or notification tests | Yes | `mvvm-testing-patterns` |

## Decision Matrix: Manual MVVM vs CommunityToolkit vs Prism

| Signal | Manual MVVM | CommunityToolkit | Prism |
|---|---|---|---|
| Small feature set, low framework surface | Best fit | Good fit | Heavy fit |
| Boilerplate reduction priority | Weak fit | Best fit | Good fit |
| Region composition and modular shell | Weak fit | Partial fit | Best fit |
| Existing team familiarity with raw `INotifyPropertyChanged` | Best fit | Good fit | Good fit |
| Large desktop shell with dialogs and navigation infrastructure | Partial fit | Partial fit | Best fit |

## Decision Matrix: When to Use Source Generators

| Condition | Use Generators | Route |
|---|---|---|
| Repeated property notification and command plumbing | Yes | `communitytoolkit-mvvm` |
| Team needs standard MVVM patterns with low custom infrastructure | Yes | `communitytoolkit-mvvm` |
| Project needs custom generated attributes, wrappers, or diagnostics | Yes | `mvvm-source-generators` |
| Project has a small view-model count and explicit handwritten code aids maintenance | No | `mvvm-patterns-dotnet` |
| Roslyn complexity outweighs boilerplate savings | No | `communitytoolkit-mvvm` or manual MVVM |

## Decision Matrix: Platform Selection

| Signal | WPF | MAUI | WinUI 3 | UWP |
|---|---|---|---|---|
| Mature Windows desktop line-of-business app | Best fit | Partial fit | Good fit | Legacy fit |
| Cross-platform mobile and desktop target | Weak fit | Best fit | Weak fit | Weak fit |
| Native current WindowsAppSDK desktop experience | Partial fit | Partial fit | Best fit | Weak fit |
| Existing shipped legacy Windows Store app | Weak fit | Partial fit | Migration target | Best fit |
| Route skill | `wpf-fluent-ui` | `maui-patterns` | `winui3-patterns` | `uwp-patterns` |

## Decision Order

| Decision | Agent Action |
|---|---|
| Platform selection | Agent selects `wpf-fluent-ui`, `maui-patterns`, `winui3-patterns`, or `uwp-patterns` first. |
| Framework selection | Agent chooses manual MVVM, CommunityToolkit, or Prism from app size, shell complexity, and boilerplate targets. |
| Implementation approach | Agent adds binding, commands, lifecycle, validation, and testing skills after framework selection. |
| Generator choice | Agent applies `communitytoolkit-mvvm` before `mvvm-source-generators` unless custom generation requirements exist. |
| Final verification | Agent verifies platform behavior and view-model tests before closing the task. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify platform | Agent maps the request to WPF, MAUI, WinUI 3, or UWP. | Agent records one primary UI platform. | Platform route is explicit. |
| 2. Select framework | Agent chooses manual MVVM, CommunityToolkit, or Prism. | Agent compares shell complexity, navigation, and boilerplate goals. | Framework choice matches constraints. |
| 3. Add specialist skills | Agent routes bindings, commands, lifecycle, validation, generators, and tests in dependency order. | Agent lists routed skills. | Every MVVM concern maps to one skill. |
| 4. Integrate platform skill | Agent aligns view-model guidance with the selected UI platform skill. | Agent reviews routed platform and MVVM skills together. | Platform and MVVM patterns do not conflict. |
| 5. Verify behavior | Agent checks observable state, commands, validation, navigation, and tests. | Run existing targeted build or test commands in scope. | MVVM flow stays stable. |

## Verification Matrix

| Area | Agent Verifies | Test | Pass |
|---|---|---|---|
| State | Observable properties raise notifications once per change. | Run targeted MVVM tests or inspect generated members. | Bound state updates predictably. |
| Commands | Guards, async flow, and duplicate-execution prevention are explicit. | Trigger enabled and disabled paths. | Command behavior matches state. |
| Lifecycle | Activation, teardown, and subscription cleanup are bounded. | Navigate in and out of the target view. | No stale subscriptions remain. |
| Validation | Errors block unsafe actions and surface through the selected platform. | Submit invalid input. | Validation output is visible and actionable. |
| Platform fit | Platform shell and MVVM pattern align. | Review routed UI platform skill outputs. | View-model pattern fits the target platform. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Mixing Prism navigation with ad hoc page state ownership | Agent chooses one navigation owner and routes lifecycle through `viewmodel-lifecycle`. |
| Handwriting repetitive property and command plumbing in generator-friendly projects | Agent routes to `communitytoolkit-mvvm`. |
| Introducing custom generators before standard toolkit patterns | Agent starts with `communitytoolkit-mvvm` and adds `mvvm-source-generators` only for custom gaps. |
| Letting code-behind own validation or command logic | Agent moves behavior into the view model and bindings. |
| Choosing a platform before checking deployment and device scope | Agent resolves platform fit before framework details. |

