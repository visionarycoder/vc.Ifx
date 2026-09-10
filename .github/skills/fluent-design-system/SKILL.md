---
name: fluent-design-system
title: Fluent Design System
description: >
  Apply Fluent Design System 2 patterns for materials, motion, depth, lighting, tokens, 
  and Windows-aligned interaction when an agent designs or reviews UI.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1780
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fluent2-design-comprehensive
  - winui3-patterns
  - wpf-fluent-ui
  - ui-design-principles
  - ui-accessibility-standards
appliesTo: '**/*.{xaml,cs,html,css,scss,ts,tsx,razor,md}'
tags:
  - fluent
  - fluent2
  - windows
  - motion
  - materials
  - theming
---
# Fluent Design System

Agent applies Fluent Design System 2 patterns so Windows-aligned interfaces feel calm, layered, readable, and native on current Windows surfaces.

> **For comprehensive Fluent 2 reference:** Agent uses `fluent2-design-comprehensive` when work needs detailed design tokens, cross-platform component guidance, Copilot/AI patterns, content engineering, or complete accessibility specifications.

## When to Use

| Condition | Use |
|---|---|
| Agent designs a Windows desktop surface, shell, dialog, menu, or page. | Agent uses this skill. |
| Agent updates Mica, Acrylic, Smoke, elevation, or reveal treatment. | Agent uses this skill. |
| Agent tunes motion, tokens, or control states to match Fluent 2. | Agent uses this skill. |
| Agent aligns a product surface with Windows 11 visual expectations. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent works on cross-platform layout with no Windows-specific design system. | Agent uses `ui-design-principles`. |
| Agent works on WCAG conformance, focus, narration, or keyboard support as the primary goal. | Agent uses `ui-accessibility-standards`. |
| Agent updates only backend services or non-visual code. | Agent uses the matching backend skill. |
| Agent targets Apple, Material, or web-brand design language over Fluent. | Agent uses the matching platform design skill. |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Surface inventory | Yes | Inventory identifies app shell, content surfaces, transient surfaces, and action surfaces. |
| Theme scope | Yes | Scope identifies light, dark, or high-contrast coverage. |
| Platform target | Yes | Target identifies WinUI 3, WPF, MAUI on Windows, or Windows-aligned web scope. |
| Motion scope | No | Scope identifies transitions, connected animations, and reduced-motion boundaries. |
| Brand constraints | No | Constraints identify accent, icon, or typography limits. |

## Foundation Matrix

| Principle | Preferred Pattern | Avoid | Pass |
|---|---|---|---|
| Material | Use Mica for window-level background context, Acrylic for transient light-dismiss surfaces, and Smoke for modal emphasis. | Acrylic behind dense text or Mica on transient menus. | Surface roles stay clear and readable. |
| Depth | Use a small elevation scale with consistent layer roles. | Many ad hoc shadow values. | Users distinguish base, active, and transient layers instantly. |
| Motion | Use short entrance, exit, and connected motion tied to task continuity. | Decorative motion that competes with content. | Motion explains change and ends quickly. |
| Lighting | Use focus, reveal, highlight, and contrast to guide attention. | Persistent glow on passive content. | Attention cues remain meaningful. |
| Tokens | Use shared color, radius, spacing, and typography tokens. | One-off visual constants in each view. | Sibling surfaces render as one system. |
| Theme parity | Verify light, dark, and high-contrast meaning. | Dark theme as an inverted light theme only. | Hierarchy and semantics persist across themes. |

## Component Decision Matrix

| Area | Select | Select When | Pass |
|---|---|---|---|
| Window background | Mica | The surface anchors the main app chrome or root container. | Root window feels native and stable. |
| Flyout or context menu | Acrylic | The surface is transient and overlays active content. | Overlay feels layered without harming legibility. |
| Modal interruption | Smoke | The background needs to recede under blocking content. | Modal focus stays clear. |
| Primary action | One prominent filled action | The layout has a single dominant next step. | Users identify the next step in one scan. |
| Secondary action set | Neutral, subtle, or outline treatment | Multiple low-priority actions share one region. | Action weight matches task priority. |
| Transition | Connected or content continuity motion | Navigation preserves object or place continuity. | Users track what changed. |
| Focus treatment | Strong ring or reveal tied to control state | Keyboard or assistive input path enters the control. | Focus remains visible in every theme. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps surface roles, layer roles, and action priority before edits. | Review changed views and shell structure. | Each region has one clear purpose. |
| 2 | Agent assigns Fluent 2 materials, elevation, and token usage by role. | Compare root, content, and transient surfaces. | Material use matches surface role. |
| 3 | Agent applies control states for rest, hover, focus, pressed, disabled, and selected behavior. | Trigger each state once. | States remain clear in every theme. |
| 4 | Agent tunes motion for entrance, exit, continuity, and reduced-motion support. | Exercise one navigation flow and one transient surface. | Motion stays brief and purposeful. |
| 5 | Agent validates theme parity, contrast, and visual calmness. | Review light, dark, and high-contrast output. | The same information hierarchy stays intact. |

## GitHub MCP Hooks

| Task | GitHub MCP Tool | Assistance |
|---|---|---|
| Locate XAML, CSS, token maps, and component templates | `github-mcp-server-search_code` | Finds theme dictionaries, brush keys, motion settings, and control-state definitions. |
| Inspect checked-in style and asset files | `github-mcp-server-get_file_contents` | Reads resource dictionaries, icon sets, and design-token files without a local clone of another repo. |
| Review UI-focused pull request diffs | `github-mcp-server-pull_request_read` | Surfaces changed visual assets, XAML, and review comments tied to Fluent updates. |
| Inspect build or UI test workflow failures | `github-mcp-server-actions_list` and `github-mcp-server-get_job_logs` | Exposes packaging, screenshot, or Windows runner failures. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Material fit | Review Mica, Acrylic, and Smoke usage in context. | Each material matches the correct surface role. |
| Depth clarity | Review dialogs, panes, menus, and overlays. | Layer order stays obvious. |
| Motion quality | Trigger navigation and transient interactions. | Motion explains change without distraction. |
| Theme parity | Review light, dark, and high-contrast output. | Meaning and hierarchy persist in each theme. |
| Token reuse | Inspect changed styling assets. | Shared tokens replace one-off values. |
| Primary action weight | Review action groups. | One dominant action stands above secondary actions. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Acrylic appears behind dense tables or long text. | Agent moves dense content to opaque or lightly tinted surfaces. |
| Elevation values drift across screens. | Agent reduces the layer scale to repeatable roles. |
| Reveal or glow appears as decoration. | Agent reserves lighting cues for actionable or focused elements. |
| Motion length exceeds task value. | Agent shortens duration and removes non-informational movement. |
| Brand overrides erase Fluent structure. | Agent keeps Fluent spacing, state, and hierarchy while applying brand accents selectively. |

## Outputs

- Fluent 2 surface-role map
- Material and elevation decision table
- Motion and state treatment plan
- Theme-parity validation summary
- Concrete remediation list for non-native or noisy surfaces
