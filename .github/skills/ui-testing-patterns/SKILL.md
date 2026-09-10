---
name: ui-testing-patterns
title: UI Testing Patterns
description: Implement UI automation and visual verification for WPF, .NET MAUI, and WinUI 3 with platform-fit tools, stable selectors, deterministic data, and observable waits.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1795
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - wpf-testing-patterns
  - writing-mstest-tests
  - ui-accessibility-standards
  - maui-patterns
appliesTo: '**/*.{cs,csproj,xaml,json,yml,xml}'
tags:
  - ui-testing
  - appium
  - flaui
  - winappdriver
  - visual-regression
---
# UI Testing Patterns

Agent implements UI automation around stable user journeys, platform-fit tooling, deterministic data, and observable waits so desktop and cross-platform UI stays verifiable without fragile screen-driving scripts.

## When to Use

| Condition | Use |
|---|---|
| Agent verifies end-to-end behavior across WPF, MAUI, or WinUI 3 surfaces. | Agent uses this skill. |
| Agent adds coverage for focus, keyboard flow, dialogs, navigation, or rendered state. | Agent uses this skill. |
| Agent selects Appium, FlaUI, WinAppDriver, or an existing visual-regression stack. | Agent uses this skill. |
| Agent needs assertions on real UI behavior that unit tests do not cover. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent tests pure view-model logic or service coordination. | Agent uses the matching unit-test or MVVM skill. |
| Agent changes only static documentation or non-UI code. | Agent uses the matching area skill. |
| Agent seeks snapshot-only coverage with no behavioral assertion. | Agent adds behavior assertions first. |
| Agent keeps a legacy coded UI suite alive with no new investment. | Agent contains work to maintenance-only scope. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Target platforms | Yes | Platforms identify WPF, MAUI, WinUI 3, or mixed scope. |
| User journeys | Yes | Journeys identify navigation, input, dialogs, and expected outcomes. |
| Test environment | Yes | Environment identifies emulator, simulator, desktop host, CI agent, and display constraints. |
| Selector strategy | Yes | Strategy identifies automation IDs, accessibility IDs, names, or semantic properties. |
| Visual scope | No | Scope identifies screenshot baselines, tolerance, and golden-path surfaces. |

## Tool Selection Matrix

| Platform | Preferred Tool | Avoid | Pass |
|---|---|---|---|
| WPF | FlaUI | Coordinate-based driving or fragile visual tree index selection | Controls resolve through stable UIA properties. |
| MAUI | Appium | Platform-specific tap coordinates with no semantic selector | Accessibility identifiers drive the journey. |
| WinUI 3 | WinAppDriver or Appium Windows path already present in the repo | Window discovery through timing guesses | Startup and control lookup remain deterministic. |
| Shared rendering | Existing visual-regression tool already in the repo | Full-suite screenshot sprawl | Only high-value surfaces capture baselines. |
| Legacy coded UI | Existing harness only for containment paths | New suite growth on deprecated tooling | Legacy flow stays stable with no new expansion. |

## Test Design Matrix

| Concern | Preferred Pattern | Test | Pass |
|---|---|---|---|
| Selectors | Stable automation IDs or accessibility identifiers bound to semantics | Inspect touched UI files and tests. | Selectors survive layout and style changes. |
| Data setup | Deterministic fixtures, launch arguments, or seeded services | Run the same journey twice. | Initial state stays the same across runs. |
| Waiting | Wait on visible state, enablement, or window readiness | Run under load once. | Tests stay stable without fixed sleeps. |
| Dialogs | Assert title, content, action result, and return state | Complete one dialog path. | Dialog behavior is verified beyond appearance. |
| Failure paths | Assert visible recovery state and blocked invalid actions | Trigger one negative case. | Error handling stays user-visible and testable. |
| Visual checks | Snapshot only layout- or brand-critical surfaces | Compare one approved baseline. | Diff scope stays reviewable and intentional. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects one automation stack per platform in scope and documents selector rules. | Review app type and existing test references. | Tool choice matches the framework. |
| 2 | Agent adds stable automation IDs or accessibility identifiers to critical controls. | Inspect changed UI files. | Critical controls expose deterministic selectors. |
| 3 | Agent scripts core journeys with explicit success and failure assertions. | Run targeted UI tests. | Journeys verify user-visible outcomes. |
| 4 | Agent replaces fixed delays with observable waits. | Search touched tests and rerun once. | Timing stays stable with no sleep-based gates. |
| 5 | Agent adds bounded visual checks where rendering fidelity matters. | Capture and compare one baseline path. | Snapshot scope stays small and valuable. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Find automation IDs, accessibility IDs, and existing UI tests | `github-mcp-server-search_code` | Locates selectors, driver setup, and test harness utilities. |
| Inspect checked-in test configs and baselines | `github-mcp-server-get_file_contents` | Reads test project files, run settings, and screenshot assets. |
| Review UI test pull request diffs | `github-mcp-server-pull_request_read` | Surfaces changed selectors, tests, and baseline files together. |
| Diagnose CI failures for UI automation | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes host startup, emulator, desktop runner, or screenshot failures. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Tool fit | Review references and run one test. | Automation tool matches the target platform. |
| Selector stability | Inspect touched UI files and tests. | Semantic identifiers replace coordinates and index lookups. |
| Outcome assertions | Review the targeted test set. | Tests assert visible success, failure, or recovery states. |
| Wait strategy | Search for fixed-delay calls. | Touched tests avoid sleep-based timing. |
| Visual scope | Review baseline count and coverage. | Snapshots cover only high-value surfaces. |
| Environment readiness | Start the app under the target environment. | Host and driver connect reliably. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Tests locate elements by order or coordinates. | Agent switches to semantic selectors. |
| UI tests duplicate unit-test coverage only. | Agent keeps rendered behavior and integration paths in the UI suite. |
| Fixed sleeps hide race conditions. | Agent waits on window state, element readiness, or observable app state. |
| Snapshot coverage grows without review value. | Agent limits visual checks to brand-critical or layout-sensitive paths. |
| Legacy coded UI expands into new investment. | Agent contains coded UI work to maintenance-only paths. |

## Outputs

- Platform-to-tool selection matrix
- Stable selector strategy
- Core user-journey automation plan
- Visual regression scope summary
- Host and CI verification steps
