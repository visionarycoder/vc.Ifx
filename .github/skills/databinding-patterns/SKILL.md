---
name: databinding-patterns
title: Data Binding Patterns
description: Apply binding expressions, modes, converters, validation, and platform-specific binding optimizations when agent implements .NET UI data flow.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1357
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mvvm-patterns-dotnet
  - wpf-mvvm-implementation
  - maui-patterns
  - winui3-patterns
  - uwp-patterns
appliesTo: '**/*.{cs,csproj,xaml,axaml}'
tags:
  - databinding
  - xaml
  - ui
  - mvvm
  - performance
---
# Data Binding Patterns

Agent defines predictable data flow between XAML views and observable .NET state so bindings stay correct, diagnosable, and performant.

## When to Use

| Condition | Use |
|---|---|
| Work wires view state to a view-model property, collection, or command parameter | Use this skill |
| Work selects a binding mode or update trigger for editable UI | Use this skill |
| Work adds converters, validation, `StringFormat`, or `TargetNullValue` | Use this skill |
| Work tunes WinUI 3 or UWP binding performance with compiled bindings | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work restructures overall MVVM boundaries | Use `mvvm-patterns-dotnet` |
| Work focuses on command implementations and behavior mapping | Use `commands-behaviors-patterns` |
| Work edits domain services with no UI binding impact | Use the matching service skill |
| Work runs in a UI stack without XAML-style binding | Follow the platform-native pattern |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Bound properties and collections | Yes | Identify sources, targets, and null behavior. |
| Edit flow | Yes | Identify when source updates occur. |
| Target platforms | Yes | Identify WPF, MAUI, WinUI 3, or UWP rules. |
| Validation rules | No | Identify user-facing errors and blocking conditions. |
| Performance sensitivity | No | Identify high-frequency lists, forms, or startup paths. |

## Pattern Matrix

| Concern | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Read-only display | `OneWay` or `OneTime` as appropriate | Avoid `TwoWay` on labels and passive text | Target reflects the intended source timing |
| Editable form field | `TwoWay` plus deliberate update trigger | Match trigger cost to the user experience | Source updates at the intended time |
| View formatting | `StringFormat`, `TargetNullValue`, or small converters | Keep domain rules out of converters | Presentation transforms stay view-focused |
| High-traffic WinUI 3 or UWP path | `x:Bind` or compiled binding where appropriate | Keep update direction explicit | Binding compiles and stays responsive |
| Nested templates and extracted controls | Explicit data-context forwarding | Avoid accidental context loss | Child content resolves the intended source |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps each binding source, target, mode, and null path. | Review the binding map. | Every bound control has an explicit data-flow choice. |
| 2 | Agent selects binding mode and update trigger based on edit timing and cost. | Exercise one edit path per mode in scope. | Update timing matches the intended user experience. |
| 3 | Agent adds converters, formatting, and null handling only where presentation logic belongs in the view. | Search XAML and view-model code for duplicated formatting logic. | Transform logic lives in one place. |
| 4 | Agent wires validation and error visuals through platform-standard mechanisms. | Trigger one invalid path and one recovery path. | Error state appears and clears correctly. |
| 5 | Agent applies compiled binding or path simplification on performance-sensitive screens. | Build and exercise one list or form path. | Binding stays responsive. |
| 6 | Agent verifies data-context propagation and local overrides. | Open nested controls or templates in scope. | Child views receive the intended context. |

## Rules

| Topic | Rule |
|---|---|
| Mode selection | Match `OneWay`, `TwoWay`, `OneTime`, or `OneWayToSource` to the control intent. |
| Update timing | Use `PropertyChanged`, `LostFocus`, explicit update, or compiled-binding defaults deliberately. |
| Converters | Keep converters small and presentation-focused. |
| Null display | Add `TargetNullValue` or view-model defaults intentionally. |
| Diagnostics | Build and exercise touched views after binding-path changes. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Binding mode | Review editable and read-only controls. | No control uses an incorrect mode. |
| Update trigger | Type through one form path. | Source updates occur at the intended time. |
| Formatting ownership | Inspect XAML and view-model responsibilities. | Presentation transforms stay outside business logic. |
| Validation visuals | Trigger invalid and corrected inputs. | Error feedback follows state changes. |
| Data-context propagation | Render nested bound content. | Child content resolves the intended source. |

## Outputs

- Binding map with modes, update triggers, and null behavior
- Converter and formatting plan
- Validation and error-visual guidance
- Performance notes for compiled bindings and context propagation
- Verification steps for binding correctness and responsiveness
