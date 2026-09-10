---
name: winui3-patterns
title: WinUI 3 Patterns
description: >
  Implement WinUI 3 desktop apps with Windows App SDK, Fluent-native controls, MVVM, x:Bind, 
  windowing APIs, and deployment-fit packaging choices.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1790
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fluent2-design-comprehensive
  - fluent-design-system
  - ui-testing-patterns
  - ui-accessibility-standards
  - dependency-injection-patterns
related_docs:
  - references/fluent2-implementation-patterns.md
  - https://learn.microsoft.com/windows/apps/winui/winui3/
  - https://github.com/microsoft/WinUI-Gallery
appliesTo: '**/*.{cs,csproj,xaml,json,xml}'
tags:
  - winui3
  - fluent
  - fluent2
  - windowsappsdk
  - windows11
  - desktop
---
# WinUI 3 Patterns

Agent implements WinUI 3 desktop features with Windows App SDK, native Fluent controls, observable view models, performant binding choices, and explicit packaged or unpackaged deployment decisions.

## When to Use

| Condition | Use |
|---|---|
| Agent builds a new Windows desktop workflow or shell. | Agent uses this skill. |
| Agent modernizes a desktop client toward native Windows 11 controls and Fluent surfaces. | Agent uses this skill. |
| Agent adds `NavigationView`, `AppWindow`, title-bar customization, or Windows lifecycle integration. | Agent uses this skill. |
| Agent evaluates packaged versus unpackaged desktop deployment. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent maintains legacy UWP code. | Agent uses `uwp-patterns`. |
| Agent builds cross-platform mobile or desktop UI. | Agent uses `maui-patterns`. |
| Agent updates WPF views or templates. | Agent uses `wpf-fluent-ui`. |
| Agent focuses on automation coverage only. | Agent uses `ui-testing-patterns`. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Shell map | Yes | Map identifies windows, pages, dialogs, and navigation regions. |
| Deployment constraints | Yes | Constraints identify Store, enterprise, installer, identity, and elevation needs. |
| Data and command flows | Yes | Flows identify bindings, commands, and async load points. |
| Windows integration scope | No | Scope identifies file activation, share, notifications, and app lifecycle hooks. |
| Accessibility scope | No | Scope identifies keyboard, narration, contrast, and high-contrast requirements. |

## Platform Decision Matrix

| Area | Preferred Pattern | Avoid | Pass |
|---|---|---|---|
| Shell | `NavigationView` plus a frame or navigation service | Manual page swapping scattered through code-behind | Selection, header, and content stay synchronized. |
| Binding | `x:Bind` for strongly typed page-local bindings; `Binding` for dynamic data-context scenarios | One binding style used everywhere with no reason | Binding choice matches update needs and performance profile. |
| State | Observable view models and command objects | Page code-behind owns mutable business state | Pages stay compositional. |
| Controls | Native WinUI 3 controls first | Custom reimplementation of standard controls | Touched UI reads as native Windows UI. |
| Windowing | `AppWindow`, title-bar APIs, and app infrastructure services | Window logic mixed into page event handlers | Multi-window behavior stays predictable. |
| Materials | Mica on root surfaces, Acrylic on transient surfaces | Material use with no surface-role logic | Surface treatment aligns with Fluent roles. |

## Deployment Matrix

| Constraint | Select | Pass |
|---|---|---|
| Store distribution, identity features, or MSIX-centered servicing | Packaged deployment | Packaging assets, identity, and install flow align with product goals. |
| Enterprise-controlled install path with lighter packaging requirements | Unpackaged deployment | Launch, update, and integration behavior align with enterprise distribution. |
| Elevated helper or external native dependency path | Explicit companion strategy outside page code | UI project stays clear about process and install boundaries. |
| Secondary window or multi-instance workflow | Windowing infrastructure with explicit activation routing | Window ownership remains deterministic. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps shell, windows, dialogs, and deployment constraints before edits. | Review app startup, packaging files, and XAML shell. | Shell ownership and deployment path stay explicit. |
| 2 | Agent implements observable state, commands, and navigation services. | Build the touched project and run targeted checks. | State flow compiles and stays testable. |
| 3 | Agent applies `x:Bind` or `Binding` deliberately per scenario. | Build XAML and exercise one update path. | Binding diagnostics stay clean. |
| 4 | Agent wires windowing, title bar, and lifecycle behavior in app infrastructure code. | Open, resize, and reactivate touched windows. | Window behavior remains stable. |
| 5 | Agent validates Fluent materials, theme parity, and Windows 11 control fit. | Review light, dark, and high-contrast themes. | Native look and accessibility remain intact. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Find `NavigationView`, `AppWindow`, `x:Bind`, and title-bar usage | `github-mcp-server-search_code` | Locates shell composition, binding paths, and windowing infrastructure. |
| Inspect project files, package manifests, and publish assets | `github-mcp-server-get_file_contents` | Reads `.csproj`, packaging config, assets, and app startup files. |
| Review WinUI pull request diffs and discussion | `github-mcp-server-pull_request_read` | Surfaces XAML, packaging, and shell changes in one view. |
| Diagnose Windows runner failures in CI | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes packaging, signing, or UI test failures. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Navigation shell | Navigate across root pages. | Selected item, header, and frame content stay aligned. |
| Binding health | Build the project and inspect diagnostics. | No binding path or compile-time XAML errors appear. |
| Windowing behavior | Open and reactivate touched windows. | Window state and title-bar behavior remain consistent. |
| Deployment fit | Review package or publish configuration. | Deployment model matches stated constraints. |
| Fluent parity | Review theme and material output. | Touched surfaces feel native and readable. |
| Accessibility | Traverse one keyboard path and high-contrast path. | Focus and semantics remain intact. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Page code-behind owns load and mutation logic. | Agent moves state and commands into view models or coordinators. |
| `x:Bind` and `Binding` mix with no intent. | Agent assigns each binding style to a clear scenario. |
| Navigation passes view instances or heavy service objects. | Agent passes typed records or compact identifiers. |
| Package assumptions leak into unpackaged flows. | Agent selects one deployment model per scope and configures it explicitly. |
| Custom visuals replace native controls too early. | Agent starts with platform controls and extends only where value exists. |

## Outputs

- Shell and window ownership map
- Binding strategy table for `x:Bind` and `Binding`
- Deployment decision summary
- Fluent-native control and material fit review
- Focused runtime verification steps for Windows 10 and 11 targets in scope
