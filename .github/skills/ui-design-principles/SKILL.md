---
name: ui-design-principles
title: UI Design Principles
description: Apply visual hierarchy, responsive layout, typography, color, feedback, and interaction-quality patterns when an agent designs or reviews user interfaces.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1680
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fluent-design-system
  - ui-accessibility-standards
  - ui-testing-patterns
appliesTo: '**/*.{html,css,scss,sass,less,ts,tsx,js,jsx,vue,razor,cshtml,xaml,md}'
tags:
  - ui
  - design
  - responsive
  - typography
  - hierarchy
---
# UI Design Principles

Agent applies durable UI quality patterns across hierarchy, spacing, responsiveness, feedback, color, and typography so interfaces stay clear and efficient.

## When to Use

| Condition | Use |
|---|---|
| Agent designs a new page, component, form, dashboard, or navigation flow. | Agent uses this skill. |
| Agent reviews layout clarity, visual consistency, or interaction quality. | Agent uses this skill. |
| Agent tunes responsive behavior for phone, tablet, desktop, or large-screen surfaces. | Agent uses this skill. |
| Agent restructures feedback, affordances, spacing, color, or type scale. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent works on platform-specific Fluent behavior as the primary goal. | Agent uses `fluent-design-system`. |
| Agent works on accessibility conformance and assistive-technology support as the primary goal. | Agent uses `ui-accessibility-standards`. |
| Agent works on backend logic or transport behavior only. | Agent uses the matching backend skill. |
| Agent focuses on automation strategy only. | Agent uses `ui-testing-patterns`. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| UI scope | Yes | Scope identifies the page, component, workflow, or feature under review. |
| Primary tasks | Yes | Tasks identify the main actions, reading order, and success outcomes. |
| Breakpoint targets | Yes | Targets identify supported narrow, medium, wide, and large layouts. |
| Brand constraints | No | Constraints identify tone, theme, component library, or product rules. |
| Current defects | No | Defects identify density, clarity, conversion, readability, or consistency issues. |

## Principle Matrix

| Principle | Preferred Pattern | Avoid | Pass |
|---|---|---|---|
| Visual hierarchy | Emphasize page purpose, primary action, and current state first | Equal visual weight across all content | Users identify the next step in one scan. |
| Consistency | Reuse component, spacing, and label patterns | One-off variants for similar actions | Equivalent controls look and behave alike. |
| Feedback | Show loading, success, error, and empty states near the source action | Global-only or delayed status messages | Users see what changed and what to do next. |
| Affordance | Make interactivity visible through placement, shape, and state cues | Flat passive-looking controls | Interactive elements read as interactive. |
| Responsiveness | Start with narrow layouts and scale upward | Desktop-first compression | Content stays usable at every breakpoint. |
| Typography | Use a deliberate type scale, readable line length, and emphasis order | Dense text blocks with weak hierarchy | Reading flow stays comfortable. |
| Color | Use color to reinforce state and structure with non-color backup cues | Color as the only signal | Meaning survives grayscale or reduced vision. |
| Spacing | Align to a repeatable spacing system | Random margins and gaps | Layout feels ordered and scannable. |

## Pattern Decision Matrix

| Area | Select | Select When | Pass |
|---|---|---|---|
| Primary action | One dominant action in each task region | The user flow has a clear next step | Decision friction stays low. |
| Form layout | Single-column mobile-first grouping | The user enters sequential data | Reading order and tab order stay aligned. |
| Dense data | Progressive disclosure and grouping | The surface contains many metrics or controls | Users isolate key data quickly. |
| Empty state | Context plus one clear next action | The feature has no items or results | Empty space still teaches the next step. |
| Error state | Inline cause and recovery action | A field or workflow fails | Recovery stays local and understandable. |
| Responsive navigation | Persistent current-location cue | The product has multiple sections | Users keep orientation while moving. |
| Touch targets | Larger hit areas and separated actions | The surface serves touch devices or mixed input | Taps remain accurate. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps primary tasks, secondary tasks, and content priority. | Review the UI map or changed files. | Each visible element ties to a user need. |
| 2 | Agent establishes hierarchy through layout, type, and action weight. | Review the first visible viewport. | Purpose and next step stand out immediately. |
| 3 | Agent applies a spacing, grid, and responsive strategy from the narrowest target upward. | Review each supported breakpoint. | Content reflows without clipping or overlap. |
| 4 | Agent aligns components, colors, labels, and interaction states across sibling surfaces. | Compare changed surfaces. | Repeated patterns stay consistent. |
| 5 | Agent validates empty, loading, success, and failure states. | Trigger each state once. | Every state communicates status and recovery clearly. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Find layout components, style tokens, and repeated UI patterns | `github-mcp-server-search_code` | Locates grids, cards, forms, type scales, and state treatments across the repo. |
| Inspect checked-in design token or component files | `github-mcp-server-get_file_contents` | Reads theme files, component markup, and shared style assets. |
| Review UX-related pull request diffs | `github-mcp-server-pull_request_read` | Surfaces layout, styling, and copy changes in one review view. |
| Diagnose UI build or snapshot workflow failures | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes lint, screenshot, or visual-regression failures. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Hierarchy clarity | Review the first-view screenshot or running UI. | Purpose and primary action stand out first. |
| Responsive behavior | Review each supported breakpoint. | No clipped text, hidden actions, or overlap appear. |
| Pattern consistency | Compare sibling surfaces. | Shared actions and content blocks follow one treatment. |
| State coverage | Trigger loading, empty, success, and error states. | Each state communicates status and next action. |
| Typography quality | Review heading order and line length. | Text remains readable and structured. |
| Color quality | Review semantic states with non-color cues. | Meaning persists without color alone. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Desktop layout assumptions drive every screen. | Agent starts with the narrowest supported layout and scales upward. |
| Several actions compete for primary emphasis. | Agent assigns one dominant action per task boundary. |
| Spacing values drift across components. | Agent aligns gaps and margins to the existing scale. |
| Feedback appears far from the source action. | Agent places status and recovery cues near the triggering control. |
| Typography scale lacks contrast. | Agent increases separation between headings, labels, and body text. |

## Outputs

- UI quality review for hierarchy, spacing, feedback, and responsiveness
- Pattern decision table for layout and interaction choices
- Breakpoint validation summary
- Concrete remediation list for clarity and consistency defects
