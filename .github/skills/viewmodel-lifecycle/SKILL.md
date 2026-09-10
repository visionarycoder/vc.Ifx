---
name: viewmodel-lifecycle
title: View Model Lifecycle
description: Manage initialization, navigation state, cleanup, and memory-safety boundaries when agent implements or reviews .NET view-model lifecycles.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1510
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mvvm-patterns-dotnet
  - commands-behaviors-patterns
  - maui-patterns
  - winui3-patterns
  - uwp-patterns
appliesTo: '**/*.{cs,csproj,xaml,axaml}'
tags:
  - viewmodel
  - lifecycle
  - disposal
  - navigation
  - memory
---
# View Model Lifecycle

Agent manages view-model creation, activation, navigation data, state preservation, and cleanup so long-lived screens stay correct and leak-free.

## When to Use

| Condition | Use |
|---|---|
| Work initializes a view model from navigation, activation, or shell events | Use this skill |
| Work preserves or restores screen state across navigation or suspension | Use this skill |
| Work disposes subscriptions, messengers, timers, streams, or async resources | Use this skill |
| Work investigates stale-event or memory-retention defects | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on bindings, converters, or validation only | Use `databinding-patterns` or `validation-mvvm` |
| Work focuses on command wiring only | Use `commands-behaviors-patterns` |
| Work edits stateless views with no retained resources or navigation state | Use the platform UI skill |
| Work targets server-side request lifecycles | Use the matching service or API skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Creation and navigation path | Yes | Record when the view model starts and receives parameters. |
| State-preservation scope | Yes | Record which fields persist across navigation, suspension, or reload. |
| Resource inventory | Yes | Record events, messengers, timers, streams, and disposables. |
| Platform targets | Yes | Record WPF, MAUI, WinUI 3, or UWP lifecycle hooks. |
| Caching policy | No | Record reuse, eviction, and recreation rules. |

## Pattern Matrix

| Concern | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Initial load | Explicit `InitializeAsync`, activation callback, or navigation-aware method | Avoid constructor-driven async work | Data loads once per intended lifecycle phase |
| Navigation parameters | Compact records or identifiers | Avoid live service, control, or view references | Destination resolves the intended state without tight coupling |
| Reentry | Distinguish first activation from resumed activation | Align reload behavior to cache policy | Navigate away and back with predictable behavior |
| State restore | Restore only durable, user-meaningful state | Avoid restoring transient or unsafe state | Filters, selection, and form state return predictably |
| Cleanup | `IDisposable`, `IAsyncDisposable`, weak events, or explicit unregistration | Release subscriptions on deactivation or disposal | Inactive view models stay detached |
| Cached instances | Clear stale subscriptions and stale state on reuse or eviction | Avoid cached event roots | Reused screens stay correct and leak-free |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps creation, activation, navigation, suspension, and disposal points. | Review the lifecycle map. | Every entry and exit point stays explicit. |
| 2 | Agent implements initialization and parameter handling with compact payloads. | Open one route or activation path. | Initial state resolves without service-instance coupling. |
| 3 | Agent selects recreate, cache-per-route, or shell-singleton behavior. | Navigate away and back through the touched path. | Reload behavior matches the selected policy. |
| 4 | Agent preserves durable state and rejects unsafe restore paths. | Recreate or resume the screen in scope. | Only meaningful user state returns. |
| 5 | Agent adds disposal, messenger cleanup, weak events, and cancellation handling. | Close the screen and trigger late activity. | Inactive view models stop receiving work. |
| 6 | Agent validates memory safety through repeated lifecycle loops. | Repeat open-close or navigation cycles. | Resource counts and event activity stay stable. |

## Rules

| Topic | Rule |
|---|---|
| Startup | Move load work out of constructors and into explicit lifecycle methods. |
| Parameters | Pass stable identifiers or compact records. Resolve services through DI, not navigation payloads. |
| Cancellation | Link long-running initialization to screen lifetime and ignore late results after deactivation. |
| Cleanup | Unregister messengers and event subscriptions unless the registration is weak by default. |
| Cache policy | Record reuse, reset, and eviction behavior before implementing state restore. |
| Platform hooks | Use the native lifecycle surface for the target platform and keep ownership explicit. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Initialization timing | Open, reuse, or recreate the screen. | No duplicate initial load occurs outside the selected policy. |
| Parameter shape | Review payloads and navigate once. | Payloads avoid live control and service references. |
| Durable restore | Resume or recreate the touched screen. | User-meaningful state returns predictably. |
| Cleanup | Close the screen and trigger late events or messages. | Inactive view models stay detached. |
| Cache safety | Cycle repeated navigation or tab activation. | Cached behavior stays stable across cycles. |

## Outputs

- Lifecycle map for creation, activation, navigation, and disposal
- Cache-versus-recreate decision record
- State preservation and restoration plan
- Cleanup plan for events, messengers, disposables, and weak references
- Verification steps for lifecycle correctness and memory safety

## Reference Files

- [Platform hook reference](references/platform-hooks.md)
