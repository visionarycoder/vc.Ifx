---
name: maui-patterns
title: .NET MAUI Patterns
description: >
  Implement .NET MAUI cross-platform UI with MVVM, Shell, handlers, resource dictionaries, 
  and explicit platform seams when an agent builds or reviews app surfaces.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1760
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fluent2-design-comprehensive
  - communitytoolkit-mvvm
  - ui-design-principles
  - ui-testing-patterns
  - ui-accessibility-standards
related_docs:
  - references/fluent2-implementation-patterns.md
appliesTo: '**/*.{cs,csproj,xaml,resx,xml,json}'
tags:
  - maui
  - fluent
  - fluent2
  - mvvm
  - shell
  - handlers
  - cross-platform
---
# .NET MAUI Patterns

Agent implements .NET MAUI features with thin views, observable view models, Shell navigation, shared styling, and explicit seams for Android, iOS, Windows, and Mac Catalyst behavior.

## When to Use

| Condition | Use |
|---|---|
| Agent builds or updates a MAUI page, shell flow, or cross-platform feature. | Agent uses this skill. |
| Agent adds MVVM state, validation, or async command flow to a MAUI screen. | Agent uses this skill. |
| Agent adds platform-specific behavior through handlers, partial classes, or services. | Agent uses this skill. |
| Agent aligns one feature across Android, iOS, Windows, and Mac Catalyst. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent works on WinUI 3 desktop-only UI. | Agent uses `winui3-patterns`. |
| Agent works on WPF desktop UI. | Agent uses `wpf-fluent-ui`. |
| Agent works on backend APIs or domain logic only. | Agent uses the matching backend skill. |
| Agent focuses on UI automation only. | Agent uses `ui-testing-patterns`. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Feature workflow | Yes | Workflow identifies pages, dialogs, commands, and state transitions. |
| Navigation map | Yes | Map identifies Shell routes, query contracts, and back-stack rules. |
| Platform scope | Yes | Scope identifies Android, iOS, Windows, and Mac Catalyst differences. |
| Theme tokens | No | Tokens identify colors, spacing, typography, and imagery. |
| Device capabilities | No | Capabilities identify file pickers, permissions, sensors, and notifications. |

## Platform Decision Matrix

| Area | Preferred Pattern | Avoid | Pass |
|---|---|---|---|
| Navigation | Shell routes with compact parameters | Page construction through global singletons | Route transitions stay explicit and testable. |
| State | `ObservableObject` or `ObservableValidator` view models | Business state in code-behind | Views stay thin and reactive. |
| Commands | `RelayCommand` or `AsyncRelayCommand` with busy guards | Click handlers with inline async logic | Duplicate taps stay blocked and state stays visible. |
| Shared styling | App-level or feature-level resource dictionaries | Repeated inline colors and spacing | Styling stays consistent across pages. |
| Platform behavior | Handlers, partial classes, or injected services | `#if` blocks spread through view models | Platform differences stay localized. |
| Lifecycle | Shell or page lifecycle delegates that forward to view-model methods | Data loads in constructors with hidden side effects | Load and resume flows stay predictable. |

## Feature Pattern Matrix

| Concern | Pattern | Test | Pass |
|---|---|---|---|
| Validation | Use `ObservableValidator` for editable form state. | Submit invalid input. | Errors surface and unsafe commands stay blocked. |
| Collections | Update observable collections through view-model methods. | Refresh one list path. | UI updates without threading faults. |
| Theming | Use `AppThemeBinding` or shared theme tokens where the UI varies by theme. | Switch theme once. | Colors and emphasis remain readable. |
| Device idiom | Use layout adaptation for phone, tablet, and desktop density. | Review target idioms in scope. | Controls remain readable and tappable. |
| Native tuning | Extend handlers in `MauiProgram` for control-level native behavior. | Run the touched platform path. | Native behavior changes only on the intended platform. |
| Platform assets | Keep images, fonts, and splash assets in the single-project MAUI layout. | Build each target in scope. | Asset resolution stays consistent. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps pages, routes, commands, and service boundaries. | Review feature files and route registration. | Ownership for state, navigation, and services stays explicit. |
| 2 | Agent implements view-model state and command flow with Toolkit attributes or equivalent patterns. | Build the touched project. | Generated members and bindings compile cleanly. |
| 3 | Agent centralizes styles, templates, and theme tokens. | Open touched pages in each theme in scope. | Shared styling replaces per-page duplication. |
| 4 | Agent isolates platform behavior behind handlers, partial classes, or services. | Build and run each platform in scope. | Shared view models stay free of platform branches. |
| 5 | Agent validates navigation, busy state, failure state, and resume state. | Exercise one success path and one failure path. | User-visible state remains coherent. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Find Shell routes, handlers, platform folders, and XAML views | `github-mcp-server-search_code` | Locates route registration, partial classes, and platform-service seams. |
| Inspect checked-in MAUI assets and project files | `github-mcp-server-get_file_contents` | Reads `.csproj`, resource dictionaries, app manifests, and launch settings. |
| Review mobile or desktop build workflow runs | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes Android, iOS, Windows, or Mac build failures tied to UI changes. |
| Review cross-platform feature diffs | `github-mcp-server-pull_request_read` | Surfaces changed views, handlers, and platform-specific files together. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Route map | Navigate through each new or changed route. | Forward and back navigation resolve correctly. |
| MVVM boundary | Review changed pages and code-behind files. | Business logic stays out of code-behind. |
| Async state | Trigger one long-running command. | Busy state appears and duplicate actions stay blocked. |
| Platform seam | Inspect changed platform behavior. | Shared code stays free of scattered platform conditionals. |
| Theme parity | Review light and dark themes in scope. | Meaning and readability remain stable. |
| Build coverage | Build every targeted platform in scope. | Touched targets compile successfully. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Large objects travel through Shell query parameters. | Agent passes compact identifiers and reloads detail data in the destination view model. |
| Device services appear directly in view models. | Agent wraps device APIs behind interfaces and platform implementations. |
| Platform branches spread through shared XAML or state logic. | Agent isolates differences behind handlers, templates, or services. |
| Inline styling drifts across pages. | Agent moves tokens and styles into shared dictionaries. |
| Busy state resets only on success. | Agent resets state in unified completion paths. |

## Outputs

- Page and route ownership map
- View-model and command pattern selection
- Platform seam plan for Android, iOS, Windows, and Mac Catalyst
- Shared styling and theming plan
- Focused verification steps per platform in scope
