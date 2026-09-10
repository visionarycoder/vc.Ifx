---
name: wpf-screen-generation
title: WPF Screen Generation
description: Generate WPF screens with stable layout patterns, reusable templates, explicit state handling, and binding-friendly control composition.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1146
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - wpf-project-setup
  - wpf-mvvm-implementation
  - wpf-testing-patterns
appliesTo: '**/*.{xaml,cs}'
tags:
  - wpf
  - xaml
  - ui
  - desktop
---
# WPF Screen Generation

Agent turns a view-model contract into a WPF screen that renders clearly, scales to realistic data, and remains easy to restyle and test.

## When to Use

| Condition | Use |
|---|---|
| Agent creates a new form, list, dashboard, or master-detail screen. | Agent uses this skill. |
| Agent replaces ad hoc XAML with reusable layout and template patterns. | Agent uses this skill. |
| Agent maps a web screen or sketch into desktop controls. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent focuses on view-model behavior or service orchestration. | Agent uses `wpf-mvvm-implementation`. |
| Agent focuses on project bootstrap or host startup. | Agent uses `wpf-project-setup`. |
| Agent builds a custom drawing surface or 3D visualization. | Agent uses a graphics-specific approach. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Screen purpose | Yes | Purpose identifies the user workflow. |
| Primary data model | Yes | Model identifies the bound state or item type. |
| Interaction style | Yes | Style identifies form, list, master-detail, or dashboard behavior. |
| State variations | No | Variations identify loading, empty, error, and validation states. |
| Styling requirements | No | Requirements identify density, accessibility, or theme needs. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Choose the screen pattern | Agent selects the dominant workflow pattern before placing controls. | Pattern choice | Agent reviews the pattern choice. | The layout reflects the real workflow instead of a generic page skeleton. |
| 2. Build a stable layout | Agent uses `Grid` for major structure and reserves lighter panels for local composition. | Layout skeleton | Agent resizes the screen. | Alignment remains stable across common sizes. |
| 3. Map controls to behavior | Agent selects form, list, or detail controls that match editing, selection, and density needs. | Control map | Agent exercises core interactions. | Control behavior matches the workflow. |
| 4. Extract repeated visuals | Agent moves repeated item rendering or chrome into templates and shared styles. | Templates and styles | Agent changes one repeated visual. | One resource change updates all repeated instances. |
| 5. Design alternate states | Agent designs loading, empty, error, and validation states in the first pass. | State plan | Agent triggers non-happy paths. | The screen remains understandable outside the happy path. |
| 6. Validate realistic rendering | Agent tests long strings, many rows, keyboard navigation, and validation feedback. | Rendering report | Agent performs a manual smoke pass. | The screen stays usable with representative data density. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Rendering verification | Manual screen pass with realistic data | Layout, bindings, and states render correctly |
| Keyboard verification | Manual tab and selection pass | Focus order and selection behavior remain usable |

## Verification Checklist

- [ ] Agent uses a layout pattern that matches the workflow.
- [ ] Agent keeps repeated visuals in templates or shared styles.
- [ ] Agent limits converters to presentation-only work.
- [ ] Agent designs loading, empty, and validation states explicitly.
- [ ] Agent validates keyboard and resize behavior with realistic data.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent uses one `StackPanel` for an entire business screen. | Agent uses `Grid` for page-level structure. |
| Agent chooses a light repeater for an interactive table. | Agent uses `DataGrid` or `ListView` when selection and keyboard behavior matter. |
| Agent duplicates repeated card or row chrome across views. | Agent extracts a shared template or style. |
| Agent ignores empty and error states until late testing. | Agent designs alternate states in the first draft. |
