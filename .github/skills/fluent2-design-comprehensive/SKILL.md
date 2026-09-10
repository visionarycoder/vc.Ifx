---
name: fluent2-design-comprehensive
title: Fluent 2 Design System - Comprehensive Reference
description: >
  Complete cross-platform Fluent 2 design system reference covering design tokens, 
  components, typography, color, motion, accessibility, Windows/Web/Mobile patterns, 
  and Copilot/AI integration.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 5800
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fluent-design-system
  - wpf-fluent-ui
  - winui3-patterns
  - ui-design-principles
  - ui-accessibility-standards
related_docs:
  - https://fluent2.microsoft.design
  - https://learn.microsoft.com/windows/apps/design
  - https://github.com/microsoft/fluentui
appliesTo: '**/*.{xaml,cs,tsx,ts,jsx,js,css,scss,razor,html}'
tags:
  - fluent
  - fluent2
  - design-system
  - tokens
  - windows
  - web
  - copilot
  - accessibility
---
# Fluent 2 Design System — Consolidated Skill



> **Primary source of truth:** https://fluent2.microsoft.design  

> **Windows design docs:** https://learn.microsoft.com/windows/apps/design  

> **Open-source repo:** https://github.com/microsoft/fluentui  

> **Copilot component layer:** `@fluentui-copilot/react-copilot` on npm  

> **Last verified:** August 2026



---



## 1. System Overview



Fluent 2 (launched 2023) is Microsoft's current cross-platform design system. It succeeds Fluent 1 and is built around a token-driven architecture covering Web, iOS, Android, and Windows. No "Fluent 3" exists as of mid-2026; active investment is directed toward Copilot/AI experience extensions.



**Implementations:**

| Platform | Library |

|---|---|

| Web | Fluent UI React v9 (`@fluentui/react-components`) |

| Web Components | `@microsoft/fast-foundation` + Fluent UI Web Components |

| Windows native | WinUI 3 / Windows App SDK |

| iOS | Fluent UI iOS |

| Android | Fluent UI Android |

| Copilot/AI layer | `@fluentui-copilot/react-copilot` |



**Design files (Figma):**

- **Fluent 2 Design Language** — source of truth for global/alias tokens (color, stroke, corner radius, spacing, size)

- **Fluent 2 Core UI Kits** — Web, iOS, Android code-aligned components

- **Copilot UI Kits** — AI-focused components (chat, prompt, AI-response surfaces)

- **Labs UI Kits** — Partner-contributed experimental components



---



## 2. Design Language



### 2.1 Design Principles

Fluent 2's character: **productivity-grade pragmatism** — calm layered materials, tight spacing, deep keyboard \& accessibility support built for long working sessions and enterprise consistency, extended with a growing AI/Copilot pattern layer.



Five historical sensory concepts (Fluent 1 origin, still inform the system):

- **Light** — illumination for depth cues

- **Depth** — layered materials and elevation

- **Motion** — purposeful, quick, connected transitions

- **Material** — translucent surfaces (Mica, Acrylic)

- **Scale** — responsive across device types



### 2.2 Color

- Token-driven; never hardcode hex values in component code.

- Supports **light**, **dark**, **high-contrast**, and **branded** themes out of the box.

- High-contrast is a Microsoft accessibility signature — always test against it.

- Use alias color tokens (e.g., `colorNeutralBackground1`, `colorBrandForeground1`) rather than global raw values.

- Reference: `fluent2.microsoft.design/design-language/color`



**Contrast requirements (WCAG AA):**

- Standard text: ≥ 4.5:1

- Large text (>18.5 px Bold or >24 px Regular): ≥ 3:1

- Interactive \& non-text components (icons): ≥ 3:1



### 2.3 Typography

- Type ramp tokens: `typeRampMinus2` → `typeRampPlus6` (font size + line height pairs)

- Windows: use Segoe UI Variable

- Web: system font stack via tokens

- `typeRampPlus4` through `typeRampPlus6` = display sizes; use sparingly

- Reference: `fluent2.microsoft.design/design-language/typography`



### 2.4 Elevation

- Indicates depth and hierarchy through shadow values.

- Used for button states, drag-and-drop, dialogs, and layered surfaces.

- On Windows, complement elevation with Mica/Acrylic materials instead of heavy shadows.

- Reference: `fluent2.microsoft.design/design-language/elevation`



### 2.5 Iconography

- Use **Fluent Iconography** kit (separate Figma library).

- Available as `@fluentui/react-icons` (React) or SVG sprites.

- Icons must meet ≥ 3:1 contrast against adjacent colors.

- Use the **Icon Scaling Tool** Figma plugin for multi-size generation.

- Reference: `fluent2.microsoft.design/design-language/iconography`



### 2.6 Motion

**Four principles:** Functional, Natural, Consistent, Appealing.



**Key patterns:**

| Pattern | Use case |

|---|---|

| Fade | Top-level page transitions (large surfaces) |

| Slide | Drawers, panels entering from edges |

| Scale | Menus, tooltips, popovers appearing near trigger |

| Elevation | Depth changes — button press, drag states |

| Stagger | Lists of items animating in; preferred over synchronized |



**Duration guidance:** Keep durations short. Respect `prefers-reduced-motion`; always design a "no motion" fallback.  

**Accessible motion rules:**

- Avoid flashes or jarring movements (seizure risk)

- Constrain motion to the focused element

- Use ARIA live regions to communicate dynamic content to screen readers

- Reference: `fluent2.microsoft.design/design-language/motion`



### 2.7 Layout

- Grid-based layouts with defined spacing ramp.

- Design for 320 px breakpoint (supports 400% zoom without horizontal scroll).

- Support portrait/landscape reflow without content loss.

- Design for text zoom up to 200% without clipping.



### 2.8 Shapes

- `controlCornerRadius` — controls with backplates (buttons, inputs); default rounded

- `layerCornerRadius` — cards, flyouts, dialogs; can be increased (e.g., 20 px for round cards)

- Consistent corner geometry is a Fluent 2 visual signature; avoid mixing sharp and round radii.

- Reference: `fluent2.microsoft.design/design-language/shapes`



---



## 3. Design Tokens



> Reference: `fluent2.microsoft.design/ux-frameworks-and-guidelines/design-tokens`  

> Web token API: `learn.microsoft.com/fluent-ui/web-components/design-tokens`



### 3.1 Token Architecture (Two Layers)



```

Global Tokens  →  context-agnostic raw values (hex colors, px sizes, ms durations)

&#x20;    ↓

Alias Tokens   →  semantic meaning applied to roles (foreground, background, brand, neutral, danger…)

```



**Never consume global tokens directly in components.** Always go through alias tokens so theming works.



### 3.2 Token Categories

| Category | Examples |

|---|---|

| Color | `colorNeutralBackground1`, `colorBrandForeground1`, `colorPaletteRedBorder1` |

| Typography | `fontFamilyBase`, `fontSizeBase300`, `lineHeightBase300` |

| Spacing | `spacingHorizontalS`, `spacingVerticalL` |

| Border radius | `borderRadiusMedium`, `borderRadiusXLarge` |

| Stroke width | `strokeWidthThin`, `strokeWidthThick` |

| Shadow / Elevation | `shadow4`, `shadow16`, `shadow64` |

| Duration (motion) | `durationFast`, `durationNormal`, `durationSlow` |

| Easing | `curveEasyEase`, `curveDecelerateMid` |



### 3.3 Theming

Tokens support automatic theming for:

- **Light** / **Dark** — via `baseLayerLuminance` or Fluent Provider theme prop

- **High Contrast** — required for Windows accessibility

- **Branded** — product-specific color overrides using brand ramp tokens



**Figma:** The *Fluent 2 Design Language* file houses Figma Variables for all tokens. Use the Variables panel for light/dark toggle during design.



### 3.4 Key Web Token APIs

```ts

// React — apply theme via FluentProvider

import { FluentProvider, webLightTheme, webDarkTheme } from '@fluentui/react-components';



// Web Components — set token via DesignToken API

import { DesignToken } from '@microsoft/fast-foundation';

const myColor = DesignToken.create<string>('my-color');

myColor.setValueFor(element, '#0078d4');

```



### 3.5 Density Tokens

- `density`: modifier for `baseHeightMultiplier` and `baseHorizontalSpacingMultiplier`

- Set `density: 1` to increase control size; `-1` to decrease (compact mode)

- `designUnit`: base grid unit; all height/spacing derived from it



---



## 4. Components



> Web component reference: `fluent2.microsoft.design` → Components → Web  

> Source: `github.com/microsoft/fluentui`



### 4.1 Component Philosophy

- Always prefer Fluent components over custom implementations.

- Components map 1:1 with Figma kit assets — properties in design match code props.

- Organize by variants and component properties (not nested overrides).

- **Fluent UI React v9** is the current flagship; avoid v8/Northstar (deprecated).



### 4.2 Web Component Inventory (Fluent UI React v9)

**Layout \& Structure:** Accordion, Card, Divider, Drawer, List, Nav, Tree  

**Navigation:** Breadcrumb, Menu, Tablist, Toolbar  

**Inputs \& Forms:** Button, Checkbox, Combobox, Dropdown, Field, Input, Radio Group, Rating, Searchbox, Select, Slider, Spin Button, Switch, Tag Picker, Textarea  

**Feedback \& Status:** Badge, Dialog, Info Label, Message Bar, Progress Bar, Skeleton, Spinner, Toast  

**Data Display:** Avatar, Avatar Group, Carousel, Image, Persona, Tag  

**Overlays:** Popover, Tooltip  

**Text:** Label, Link, Text  

**Utility:** Fluent Provider, Icon  



### 4.3 Component Usage Principles

- Read each component's **Usage** tab for do/don't patterns.

- Check the **Accessibility** section per component for focus order, ARIA roles, and keyboard behavior.

- Use **Skeleton** while content loads — not spinners for large regions.

- Use **Toast** for non-blocking feedback; **Dialog** for required decisions only.

- Prefer **Drawer** over custom side panels.



### 4.4 Keyboard \& Focus Requirements

- Keyboard-first completeness is mandatory (Windows conventions: access keys, arrow-key navigation).

- Every interactive element must have a visible focus indicator.

- Use the **A11y – Focus Order** Figma plugin to annotate tab order in specs.

- Reference WAI-ARIA authoring practices for semantic ARIA roles.



---



## 5. Content Design



> Reference: `fluent2.microsoft.design/ux-frameworks-and-guidelines/content-design`



### 5.1 Core Writing Principles



| Principle | Guidance |

|---|---|

| Keep it simple | Plain language, short sentences, fragments OK for scanning |

| Get to the point | Make choices and next steps immediately obvious |

| Second person | Use "you/your" — supports friendly tone, avoids passive voice |

| Write simply | Eliminate jargon; define technical terms for broad audiences |

| Punctuate carefully | Question marks always; periods only after full sentences; avoid exclamation points except for genuinely celebratory moments |



### 5.2 Voice \& Tone

- **Voice** (stable): Warm, direct, helpful, human

- **Tone** (contextual): Friendly \& inviting on dashboards; just-the-facts in navigation/headers

- Match tone to the customer's intent and emotional state:

&#x20; - Anxious user → reassuring and calm

&#x20; - Task-focused user → concise and efficient

&#x20; - Celebrating completion → warmer, brief enthusiasm is OK



### 5.3 Capitalization

| Context | Rule |

|---|---|

| Windows / desktop UI | Sentence case for most UI text |

| iOS / macOS menu items | Title Case (capitalize each word except articles/conjunctions) |

| Buttons, labels | Sentence case |

| Headings | Sentence case |



### 5.4 Accessibility in Content

- Write **descriptive link text** — never "Click here" or "Learn more" without context.

- Avoid **directional terms** ("above," "below," "right") — they don't localize well and exclude screen reader users.

- Provide **alt text** for all illustrative elements (icons, images, charts).

- Use headings, tables, and lists to organize content logically.

- Design for **text zoom to 200%** without content clipping.



### 5.5 UI Element Text Do's and Don'ts

- **Headings:** No period; act as an outline — if skimmable headings don't explain the page, rewrite them.

- **Buttons:** Verb + noun ("Save file," "Delete account"); sentence case.

- **Error messages:** State what happened, why, and what to do next.

- **Empty states:** Explain the state and provide a clear call to action.

- **Tooltips:** Supplementary info only; don't duplicate the label.



---



## 6. Windows-Specific Guidance



> Reference: `learn.microsoft.com/windows/apps/design`  

> WinUI 3 docs: `learn.microsoft.com/windows/apps/winui`



### 6.1 Materials

| Material | Use |

|---|---|

| **Mica** | Primary app window backdrop; connects window to desktop wallpaper via translucency |

| **Acrylic** | Contextual surfaces (menus, flyouts, side panes); blur-based background material |



**Mica implementation (WinUI 3 XAML):**

```xml

<Window>

&#x20; <Window.SystemBackdrop>

&#x20;   <MicaBackdrop />

&#x20; </Window.SystemBackdrop>

</Window>

```

- Do **not** set a solid `Background` on Window, NavigationView, or page Grid elements — it covers the Mica effect.

- For section backgrounds, use semi-transparent brushes (e.g., `CardBackgroundFillColorDefaultBrush`).



### 6.2 Standard WinUI 3 App Shell Pattern

The canonical modern Windows desktop app uses:

1\. **Mica backdrop** — translucent material tied to wallpaper

2\. **Custom TitleBar** — app icon + title in draggable area, replacing default caption bar

3\. **NavigationView (Left pane)** — primary navigation shell with sidebar

4\. **InfoBar** — non-modal status/error/success/warning messages (inline, not dialogs)

5\. **Transparent page backgrounds** — pages don't set own background; Mica shows through



### 6.3 Windows Design Guidelines Summary

| Domain | Key guidance |

|---|---|

| Color | Establish hierarchy and meaning; use system accent color |

| Commanding | Present actions in clear, consistent patterns (AppBar, CommandBar, menus) |

| Elevation | Guide focus through layering; complement with Mica/Acrylic |

| Geometry | Rounded corners, consistent sizing; matches Fluent corner radius tokens |

| Haptics | Add touch feedback to reinforce input on touch-capable Windows devices |

| Iconography | Use Fluent icons; match icon size to control density |

| Layout | Grids, spacing, alignment; adaptive to window resize |

| Motion | Quick, purposeful; respects reduced-motion OS setting |

| Navigation | NavigationView as primary shell; BackStack for hierarchical navigation |

| Typography | Segoe UI Variable; use type ramp tokens consistently |

| Widgets | Glanceable, interactive surfaces on the Windows desktop |

| Writing | Clear, concise language following Content Design guidelines above |



### 6.4 WinUI 3 Preferred Controls (Fluent-styled by default)

Prefer standard WinUI controls — they get Fluent styling, rounded geometry, and Mica support automatically:

- `NavigationView`, `InfoBar`, `ContentDialog`, `TeachingTip`

- `TreeView`, `ListView`, `DataGrid`

- `AutoSuggestBox`, `NumberBox`, `RatingControl`

- `ProgressRing`, `ProgressBar`

- `MenuBar`, `CommandBarFlyout`



---



## 7. Copilot \& AI Patterns



> Reference: `fluent2.microsoft.design` → Working with AI  

> Copilot component package: `@fluentui-copilot/react-copilot`  

> M365 agent UX guidance: `learn.microsoft.com/microsoft-365/dev/agents`



### 7.1 Responsible AI Design

Before building AI experiences, ground work in:

- **Types of AI harm** — identify potential failure modes early

- **Responsible AI principles** — fairness, reliability, privacy, inclusiveness, transparency, accountability

- Reference: `fluent2.microsoft.design/ux-frameworks-and-guidelines/responsible-ai`



### 7.2 Agent Personality Types

| Type | When to use | Tone characteristics |

|---|---|---|

| **Digital Worker** (Engagement-oriented) | Collaboration, meeting flow, group coordination | Warm, conversational, calm, uses contractions, first person |

| **Assistive Agent** (Task-oriented) | Speed, precision, low social overhead | Concise, neutral-to-warm, no first-person narration, no filler |



**Disambiguation rule:** If role mode is unclear, ask one question: "Is this experience mostly coordinating people, or mostly completing tasks quickly?"



### 7.3 M365 Copilot Personality Principles (for aligned agents)

Apply to agents where cross-product tone consistency matters:

1\. **Trustworthy** — credible, context-relevant, steady

2\. **Empathetic** — supportive without removing user agency

3\. **Humble** — candid about uncertainty and limitations

4\. **Transparent** — clear about what is known, unknown, inferred

5\. **Explicitly digital** — never framed as a human identity

6\. **Supportive** — creative partner; users own outcomes

7\. **Concise by default** — easy to scan; expands when needed



### 7.4 Content Engineering \& System Prompt Structure

Structure system prompts into four components:



| Component | Purpose | Example |

|---|---|---|

| **Role** | AI persona, purpose, point of view | "You are an editorial assistant for a marketing team…" |

| **Task** | Specific action the AI performs | "Summarize in 3 bullet points. Keep each bullet under 15 words." |

| **Rules** | Guardrails — what to do and avoid | "Never include pricing. Always cite sources." |

| **Examples** | Few-shot patterns for quality calibration | Input/output pairs showing ideal responses |



**Do:** Encode voice, tone, format, and length *directly* in the system prompt.  

**Don't:** Rely on the model to infer tone from vague instructions like "Sound friendly and professional."



### 7.5 Tone Calibration Inputs (per agent/surface)

Calibrate with these explicit inputs:

- Agent name

- Product context or surface

- Primary audience

- High-stakes moments (irreversible actions, sensitive communications, time pressure)

- **Tone calibration phrase** — a short, concrete phrase that changes model output style



### 7.6 Anti-Patterns to Avoid in AI UX

- **Persona inflation** — assigning human traits, emotions, or a named identity beyond defined role

- **Over-social framing** — greetings, check-ins, social phrases unrelated to the task

- **Decorative openings/closings** — text at start/end of response that adds no task value

- **Overstating capability** — implying the AI knows more than it does; leads to broken trust

- **Exclamatory language** — makes the agent feel overenthusiastic and verbose



### 7.7 Copilot Surface Layout Patterns

**Inline mode** (default):

- Used for: previews, confirmations, simple actions, quick decision prompts

- Keep content within a single scroll

- Structure: Agent header → Inline widget → Model response



**Side-by-side mode** (optional, richer interactions):

- Use when: editing, reviewing, or managing structured content needs more space

- Structure: Conversation pane (primary) + Chiclet card (collapsed inline widget) + Side-by-side panel header + App workspace + Contextual controls

- The workspace is contextual — not a full application shell



### 7.8 Agent Design Principles (Human-Centered)

- **Adopt Fluent UI React v9** — accessibility, responsive behavior, better performance

- **Start from Fluent patterns** — toggle buttons, compound buttons, dialogs, menus, tabs before building custom components

- **Support iterative refinement** — users should be able to adjust prompts, narrow scope, request clarifications, or edit outputs directly

- **Surface contextual signals** — provide referenced data/assumptions so users understand why a response was generated

- **Provide clear correction pathways** — regenerate, revise, or manually edit without restarting

- **Preserve human control** — especially for actions affecting enterprise data



---



## 8. Accessibility



> Reference: `fluent2.microsoft.design/ux-frameworks-and-guidelines/accessibility`



### 8.1 Core Requirements

- **Color contrast:** 4.5:1 (text), 3:1 (large text, icons, interactive components)

- **Keyboard navigation:** Full keyboard access; visible focus indicators on all interactive elements

- **Screen reader support:** Semantic HTML / ARIA roles; alt text for all non-text elements

- **Responsive layout:** Reflow at 320 px viewport (400% zoom); text zoom to 200% without clipping

- **Motion:** Always provide a "no motion" option; respect OS `prefers-reduced-motion`



### 8.2 Design Specification Requirements

Document in design specs (not just color and padding):

- Focus order (use **A11y – Focus Order** Figma plugin)

- Screen reader annotations

- Semantic structure

- Intended interactions across devices and input modalities



### 8.3 Code Standards

- Follow WAI-ARIA authoring practices: `w3.org/WAI/ARIA/apg`

- Use semantic HTML elements before reaching for ARIA

- Fluent UI React v9 components ship with built-in accessibility support — don't override ARIA roles unnecessarily



---



## 9. Figma Resources \& Tooling



### 9.1 Core Figma Libraries

| Library | Purpose |

|---|---|

| Fluent 2 Design Language | Global + alias token Variables (color, spacing, type, corner radius) |

| Fluent 2 Core Web UI Kit | Code-aligned web components |

| Fluent 2 iOS UI Kit | iOS components |

| Fluent 2 Android UI Kit | Android components |

| Copilot UI Kit | AI/Copilot-specific components |

| Fluent Iconography | Icon set |

| Fluent Emoji | Emoji set |



### 9.2 Figma Plugins

| Plugin | Use |

|---|---|

| A11y – Focus Order | Annotate tab/focus order for specs |

| A11y – Color Contrast Checker | Verify WCAG contrast ratios |

| Content Reel | Pull text strings, images, icons from one palette |

| Icon Scaling Tool | Draw icon once, export to multiple sizes |



### 9.3 Enabling Libraries

1\. Open the **Assets panel** → **Libraries** dialog

2\. Enable desired Fluent 2 UI kits

3\. For always-available access: **Account Settings** → auto-enable in drafts



---



## 10. Quick Reference — Key URLs



| Topic | URL |

|---|---|

| Home | `fluent2.microsoft.design` |

| Design tokens (web) | `fluent2.microsoft.design/ux-frameworks-and-guidelines/design-tokens` |

| Web color tokens | `fluent2.microsoft.design/tokens/color-tokens/web` |

| Components (web) | `fluent2.microsoft.design/components/web` |

| Content design | `fluent2.microsoft.design/ux-frameworks-and-guidelines/content-design` |

| Content engineering (AI) | `fluent2.microsoft.design/ux-frameworks-and-guidelines/content-engineering` |

| Accessibility | `fluent2.microsoft.design/ux-frameworks-and-guidelines/accessibility` |

| Motion | `fluent2.microsoft.design/design-language/motion` |

| Windows design guidelines | `learn.microsoft.com/windows/apps/design` |

| WinUI 3 modern app structure | `learn.microsoft.com/windows/apps/winui/winui3` |

| Fluent UI React v9 (GitHub) | `github.com/microsoft/fluentui` |

| Copilot component package | `npmjs.com/package/@fluentui-copilot/react-copilot` |

| M365 agent human-centered design | `learn.microsoft.com/microsoft-365/dev/agents` |

| WAI-ARIA authoring practices | `w3.org/WAI/ARIA/apg` |



