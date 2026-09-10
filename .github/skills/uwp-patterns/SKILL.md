---
name: uwp-patterns
title: UWP Patterns
description: Maintain legacy UWP apps with adaptive UI, MVVM boundaries, Windows integration, and explicit migration decisions toward WinUI 3 when an agent changes user-facing features.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1710
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - winui3-patterns
  - ui-testing-patterns
  - ui-accessibility-standards
appliesTo: '**/*.{cs,csproj,xaml,appxmanifest,json,xml,md}'
tags:
  - uwp
  - adaptive-ui
  - legacy
  - windows
  - migration
---
# UWP Patterns

Agent maintains UWP applications with adaptive layouts, observable state, Windows contract integration, and explicit containment of legacy platform constraints while migration pressure stays visible.

## When to Use

| Condition | Use |
|---|---|
| Agent maintains an existing UWP application that remains in service. | Agent uses this skill. |
| Agent adds adaptive XAML, visual states, or Windows shell integration to UWP. | Agent uses this skill. |
| Agent stabilizes a UWP feature while migration planning continues. | Agent uses this skill. |
| Agent evaluates keep-versus-migrate direction for a touched UWP area. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent starts a new Windows desktop application. | Agent uses `winui3-patterns`. |
| Agent builds cross-platform UI. | Agent uses `maui-patterns`. |
| Agent modernizes WPF desktop UI. | Agent uses `wpf-fluent-ui`. |
| Agent works on testing only. | Agent uses `ui-testing-patterns`. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Current app footprint | Yes | Footprint identifies pages, controls, app model assumptions, and device-family dependencies. |
| Breakpoint map | Yes | Map identifies narrow, medium, and wide layouts. |
| Integration scope | Yes | Scope identifies notifications, file pickers, share, background tasks, or activation paths. |
| Migration pressure | No | Pressure identifies unsupported controls, packaging limits, or roadmap risks. |
| Distribution model | No | Model identifies Store, enterprise, or sideload assumptions. |

## Adaptive UI Matrix

| Area | Preferred Pattern | Avoid | Pass |
|---|---|---|---|
| Layout adaptation | `VisualStateManager`, `AdaptiveTrigger`, and flexible panels | Resize logic in page code-behind | Layout changes remain declarative and stable. |
| Binding | `x:Bind` for compile-time paths where page-local scope fits | Runtime string bindings everywhere | Binding health and performance remain strong. |
| State | View models own commands, validation, and selection state | Business logic inside view event handlers | Code-behind stays thin. |
| Platform contracts | Services wrap notifications, pickers, share, and activation behavior | Direct API calls across many pages | Legacy platform usage stays localized. |
| Data-heavy surfaces | List virtualization and bounded templates | Over-rendered item trees | Scrolling remains responsive. |
| Migration trace | Explicit keep, isolate, or migrate record per touched area | Silent scope growth in legacy code | Migration intent stays visible. |

## Migration Decision Matrix

| Condition | Decision | Pass |
|---|---|---|
| The touched feature depends on UWP-only contract behavior with no near-term replacement need | Keep and isolate | Legacy coupling stays localized and documented. |
| The touched feature uses standard navigation, forms, lists, and dialogs with rising maintenance cost | Prepare migration to WinUI 3 | Shared concepts and seams appear before deeper investment. |
| Packaging or Store assumptions block required product changes | Escalate migration priority | Constraints become explicit in task output. |
| Control gaps or unsupported roadmap areas raise recurring workarounds | Shift new investment to WinUI 3 | New code avoids deepening UWP-only debt. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent inventories adaptive states, platform contracts, and lifecycle entry points. | Review XAML, manifest, and service files. | Responsive and integration boundaries stay explicit. |
| 2 | Agent implements layout changes through visual states and flexible containers. | Resize across target widths. | Narrow, medium, and wide states remain usable. |
| 3 | Agent moves commands, validation, and selection logic into view models. | Exercise one form or list path. | Stateful logic stays outside the view. |
| 4 | Agent isolates Windows contracts behind services or coordinators. | Instantiate touched view models in tests or review constructors. | View models stay testable. |
| 5 | Agent records keep, isolate, or migrate direction for the touched scope. | Review task output. | Migration intent is explicit. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Find adaptive triggers, manifests, and activation code | `github-mcp-server-search_code` | Locates `VisualStateManager`, app lifecycle, and Windows contract usage. |
| Inspect checked-in UWP packaging and asset files | `github-mcp-server-get_file_contents` | Reads manifests, package assets, and startup configuration. |
| Review migration-related pull request context | `github-mcp-server-pull_request_read` | Surfaces legacy containment changes and WinUI 3 comparison notes. |
| Diagnose UWP build or packaging failures | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes package, restore, or Windows runner issues. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Breakpoint coverage | Resize across supported widths. | Adaptive layouts remain usable at each breakpoint. |
| MVVM boundary | Review changed files. | Commands and validation stay in view models. |
| Contract containment | Inspect notification, picker, share, or activation code. | Platform-specific APIs stay behind services. |
| Performance fit | Exercise one data-heavy screen in scope. | List interactions remain responsive. |
| Migration visibility | Review task output. | Touched scope includes a keep, isolate, or migrate decision. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| New work deepens UWP-only coupling without review. | Agent adds seams and records migration direction. |
| Resize behavior lives in imperative event code. | Agent moves responsive behavior into visual states. |
| View models call Windows contracts directly. | Agent wraps contracts behind interfaces. |
| Breakpoints appear without content-priority rules. | Agent ties each width state to explicit visibility and layout decisions. |
| Migration pressure stays implicit. | Agent writes the decision into the skill output for the touched area. |

## Outputs

- Adaptive-state map
- UWP contract containment plan
- Keep, isolate, or migrate decision summary
- Legacy risk notes for touched areas
- Focused validation steps for supported Windows targets
