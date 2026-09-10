---
name: wpf-fluent-ui
title: WPF Fluent UI
description: Implement WPF desktop UI with Fluent styling, ModernWpf or WPF UI libraries, MVVM boundaries, shared resources, and accessible interaction states.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1740
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fluent-design-system
  - wpf-mvvm-implementation
  - ui-testing-patterns
  - ui-accessibility-standards
related_docs:
  - references/fluent2-implementation-patterns.md
appliesTo: '**/*.{cs,csproj,xaml}'
tags:
  - wpf
  - fluent
  - fluent2
  - modernwpf
  - wpf-ui
  - mvvm
---
# WPF Fluent UI

Agent implements Fluent-styled WPF screens with thin views, reusable resources, template-driven visuals, and library choices that fit the current desktop stack.

## When to Use

| Condition | Use |
|---|---|
| Agent builds or refreshes a WPF window, page, dialog, or reusable control. | Agent uses this skill. |
| Agent introduces or extends ModernWpf or WPF UI styling. | Agent uses this skill. |
| Agent standardizes brushes, typography, icons, spacing, and control states across WPF surfaces. | Agent uses this skill. |
| Agent adds bindings, commands, triggers, or control templates with Fluent-aligned visuals. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent works on WinUI 3 or UWP XAML. | Agent uses the matching Windows UI skill. |
| Agent changes only view-model logic with no visual impact. | Agent uses `wpf-mvvm-implementation`. |
| Agent focuses on automation only. | Agent uses `ui-testing-patterns`. |
| Agent builds browser-based Fluent UI. | Agent uses the matching web UI skill. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Screen inventory | Yes | Inventory identifies windows, pages, dialogs, and custom controls in scope. |
| Library baseline | Yes | Baseline identifies ModernWpf, WPF UI, or existing custom Fluent assets. |
| Theme tokens | Yes | Tokens identify brushes, spacing, radius, typography, and icon usage. |
| Interaction rules | Yes | Rules identify commands, validation, selection, and state changes. |
| Accessibility scope | No | Scope identifies keyboard, contrast, and screen-reader-adjacent requirements. |

## Library Decision Matrix

| Area | Preferred Pattern | Avoid | Pass |
|---|---|---|---|
| Fluent library | Keep the existing library that owns the shell | Mixed ModernWpf and WPF UI controls in one visual tree | One library drives the touched surface. |
| Theme resources | Application or feature dictionaries for brushes, type, and spacing | Repeated hard-coded values | Shared tokens render consistently. |
| State visuals | Data triggers, visual states, and template bindings | Event-driven style mutation | Visual state follows bound state predictably. |
| Commands | Relay commands or `ICommand` properties | Business logic in button click handlers | Input routes through the view model. |
| Dialog flow | Service or coordinator abstraction | Dialog creation inside many view models | Dialog behavior stays testable. |
| Templates | Reusable control templates for repeated surfaces | Copy-paste XAML blocks across screens | Shared visuals stay centralized. |

## Surface Pattern Matrix

| Concern | Pattern | Test | Pass |
|---|---|---|---|
| Navigation shell | Use the shell control and navigation model already present in the app. | Open primary navigation paths. | Selection and content state remain aligned. |
| Validation | Use the established WPF validation path with bound error visuals. | Enter invalid input. | Error surfaces appear next to the failing field. |
| Density | Use spacing and typography tokens for comfortable information grouping. | Review one dense screen in scope. | Content stays readable without clutter. |
| Focus | Keep visible focus treatment and logical tab order. | Tab through the touched surface. | Focus location remains obvious. |
| Themes | Verify light, dark, and high-contrast states where the app supports them. | Switch theme or inspect alternate dictionaries. | Meaning persists across themes. |
| Async state | Bind loading and disable states to view-model properties. | Trigger one long-running action. | Users see progress and blocked duplicate actions. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent identifies the existing Fluent library, shell pattern, and resource structure. | Inspect packages, `App.xaml`, and touched views. | One stack owns the touched scope. |
| 2 | Agent centralizes brushes, styles, templates, and typography in shared resources. | Compare touched XAML files. | Duplicate visual constants drop into shared dictionaries. |
| 3 | Agent maps actions, validation, and state into view models. | Review code-behind and commands. | View logic stays declarative and thin. |
| 4 | Agent applies data triggers or visual states for busy, selected, focus, and error states. | Toggle each state once. | Visual response stays consistent. |
| 5 | Agent verifies keyboard flow, contrast, and theme parity. | Run one keyboard path and theme switch. | Interaction and readability remain intact. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Find WPF resources, templates, and command bindings | `github-mcp-server-search_code` | Locates `ResourceDictionary`, styles, converters, and command wiring. |
| Inspect checked-in XAML and project setup | `github-mcp-server-get_file_contents` | Reads `App.xaml`, package references, and shared style files. |
| Review WPF UI pull request diffs | `github-mcp-server-pull_request_read` | Surfaces changed views, templates, and resource dictionaries. |
| Diagnose desktop build or UI test failures | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes Windows build, packaging, and regression-test issues. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Library ownership | Inspect touched screens and references. | One Fluent library owns the visual tree. |
| MVVM boundary | Review code-behind files. | Business state and commands stay in view models. |
| Resource reuse | Compare styling values across touched files. | Shared resources replace repeated literals. |
| State visuals | Toggle busy, selected, error, and disabled states. | Bound state drives visual response. |
| Keyboard flow | Tab through the screen. | Focus order and visibility remain clear. |
| Theme parity | Review supported themes. | Surfaces remain readable and coherent. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Mixed Fluent libraries appear in one surface. | Agent standardizes the touched scope on the existing shell library. |
| Brush and spacing literals spread through many views. | Agent moves values into shared dictionaries. |
| Templates carry business rules. | Agent keeps templates presentational and binds them to view-model state. |
| Event handlers drive persistent UI state. | Agent replaces event-driven state with bindings, commands, and triggers. |
| Validation appears only as message boxes. | Agent surfaces field-level errors in the view. |

## Outputs

- WPF Fluent library fit assessment
- Shared resource and template plan
- MVVM command and validation pattern
- Keyboard and theme validation summary
- Concrete remediation list for non-reusable or non-native surfaces
