---
name: ui-accessibility-standards
title: UI Accessibility Standards
description: Enforce WCAG-aligned semantics, keyboard support, focus management, contrast, screen-reader support, and accessible naming when an agent builds or reviews UI.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1946
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ui-design-principles
  - ui-testing-patterns
  - fluent-design-system
appliesTo: '**/*.{html,css,scss,sass,less,ts,tsx,js,jsx,vue,razor,cshtml,xaml,md}'
tags:
  - accessibility
  - wcag
  - keyboard
  - screen-reader
  - aria
---
# UI Accessibility Standards

Agent enforces accessible structure, interaction, and presentation so interfaces remain perceivable, operable, understandable, and robust.

## When to Use

| Condition | Use |
|---|---|
| Agent designs, builds, or reviews a page, component, form, dialog, menu, grid, or navigation flow. | Use this skill. |
| Agent changes semantic markup, ARIA usage, focus movement, or keyboard interaction. | Use this skill. |
| Agent reviews screen-reader output, color contrast, scaling, or high-contrast behavior. | Use this skill. |
| Agent validates user-facing work against WCAG 2.1 or 2.2 expectations. | Use this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent works on visual polish with no accessibility scope. | Agent uses `ui-design-principles`. |
| Agent works on Fluent-specific material and motion behavior as the primary goal. | Agent uses `fluent-design-system`. |
| Agent changes only server-side logic or infrastructure. | Agent uses the matching backend skill. |
| Agent reviews test harness structure only. | Agent uses `ui-testing-patterns`. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| UI scope | Yes | Page, component, workflow, or changed files |
| Interaction model | Yes | Keyboard, pointer, touch, switch, and assistive-technology paths |
| Content structure | Yes | Headings, landmarks, forms, dialogs, alerts, navigation regions |
| Supported scaling targets | No | Zoom, text resize, OS scaling, high-contrast expectations |
| Known defects | No | Focus, contrast, labeling, or announcement issues |

## Requirement Matrix

| Requirement | Preferred Pattern | Avoid | Pass |
|---|---|---|---|
| Semantics | Native platform or markup semantics first | ARIA where native semantics already fit | Accessibility tree stays coherent. |
| Naming | Explicit labels, accessible names, descriptions, and error associations | Placeholder-only labels and unlabeled icon buttons | Interactive elements expose accurate names. |
| Keyboard | Full workflow completion through keyboard-only input | Pointer-only open, close, or submit paths | Every critical task completes without a pointer. |
| Focus | Visible, ordered, entry, contained, and return focus | Focus loss to background or hidden elements | Focus location stays predictable. |
| Screen reader support | Meaningful reading order and bounded announcements | Silent updates or noisy repeated live regions | Spoken output matches visible meaning. |
| Contrast | Verified text, icon, border, and state contrast | Text-only contrast checks | Content stays readable in every state. |
| Reflow and scaling | Responsive reflow with no clipped content | Fixed containers and overflow traps | Content stays usable at required scaling. |
| Cognitive clarity | Plain labels, grouped choices, and local recovery guidance | Vague errors and scattered instructions | Users identify recovery steps quickly. |

## Pattern Decision Matrix

| Area | Select | Select When | Pass |
|---|---|---|---|
| Form fields | Explicit label, help text, and field-level error link | Users enter or edit data | Purpose and recovery stay clear. |
| Dialogs | Labeled title, initial focus, contained focus, return focus | A transient surface blocks or requests a decision | Users enter and exit predictably. |
| Menus and popovers | Managed focus and expected arrow-key behavior | The pattern acts as a menu or listbox | Keyboard users reach every option. |
| Alerts and status | Programmatic announcement with concise text | The UI updates after an action or background work | Users hear the update at the right moment. |
| Data grids | Header association and explicit selection or sort state | The surface presents tabular data | Structure and state read accurately. |
| Color semantics | Text, icon, or pattern cue paired with color | The UI marks success, warning, or error states | Meaning survives color loss. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies semantic structure and accessible names before visual polish. | Inspect markup, control tree, or XAML properties. | Native structure covers the workflow cleanly. |
| 2 | Agent verifies keyboard order, focus entry, focus return, and visible focus treatment. | Traverse the workflow with keyboard only. | Focus never disappears and task flow remains complete. |
| 3 | Agent verifies live updates, errors, and status changes for screen-reader output. | Trigger one success update and one failure update. | Spoken feedback matches visible meaning. |
| 4 | Agent verifies contrast, scaling, zoom, and high-contrast behavior. | Review required themes and scaling targets. | Content stays readable and operable. |
| 5 | Agent verifies cognitive clarity of labels, grouping, and recovery guidance. | Review forms, dialogs, and errors in scope. | Recovery steps remain local and direct. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Find ARIA usage, focus logic, automation properties, and high-contrast styling | `github-mcp-server-search_code` | Locates labels, live regions, keyboard handlers, and accessibility props. |
| Inspect checked-in UI files and semantic structure | `github-mcp-server-get_file_contents` | Reads templates, markup, XAML, and style assets tied to accessibility behavior. |
| Review accessibility-focused diffs and comments | `github-mcp-server-pull_request_read` | Surfaces changed semantics, labels, and keyboard logic. |
| Diagnose accessibility test or lint failures | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes axe, snapshot, UI test, or Windows runner failures. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Semantic structure | Inspect landmarks, headings, controls, and relationships. | Structure exposes a clear document and interaction model. |
| Accessible names | Inspect labels, descriptions, and error associations. | Names and states stay accurate across dynamic changes. |
| Keyboard completion | Run the workflow with keyboard only. | Every critical task completes with no pointer input. |
| Focus management | Open dialogs, menus, validation errors, and dynamic updates. | Focus appears, moves, and returns predictably. |
| Screen-reader output | Read the workflow with the approved assistive path. | Announcements match visual timing and meaning. |
| Contrast and scaling | Review required states, themes, and scaling targets. | Content stays readable and operable. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| ARIA duplicates native semantics. | Agent removes redundant ARIA and keeps native semantics first. |
| Focus ring disappears for visual polish. | Agent restores a strong visible focus indicator. |
| Placeholder text acts as the only label. | Agent adds explicit labels and linked help text. |
| Dialog focus returns to an arbitrary element. | Agent records and restores the invoking element. |
| Contrast review checks normal text only. | Agent checks icons, borders, disabled-adjacent cues, and interactive states together. |
