---
name: extension-points
description: Applies MSBuild extension-point patterns for hooks, imports, and buildTransitive package layout.
license: MIT
title: MSBuild Extension Points
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1150
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - directory-build-organization
  - target-authoring
appliesTo: '**/*.{csproj,props,targets,sln,slnx}'
tags:
  - msbuild
  - extension
  - imports
  - ste
---
# MSBuild Extension Points

This skill applies extensible MSBuild hook patterns without breaking fresh clones or package consumers.
This skill keeps imports explicit, ordered, and gated.

## Pattern Table

| Goal | Preferred pattern | Avoid |
|---|---|---|
| Pre or post hook | `BeforeTargets` or `AfterTargets` on a well-known target | Editing SDK targets directly |
| Optional import | `Import ... Condition="Exists('path')"` | Unconditional imports to generated or machine-local files |
| User override slot | `CustomBefore...` or `CustomAfter...` pattern | Hardcoded logic with no extension slot |
| Package extension | `build` or `buildTransitive` layout | Content that depends on manual consumer edits |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Inspect | Agent maps the current import chain and existing hook surfaces. | Import graph |
| 2. Choose | Agent selects the narrowest hook or import point that satisfies the scenario. | Hook plan |
| 3. Gate | Agent adds existence checks and control properties for optional behavior. | Safe extension surface |
| 4. Validate | Agent evaluates a clean checkout build and the intended extension scenario. | Verified hook behavior |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Import safety | Review every changed `Import`. | Optional imports include `Exists(...)` or equivalent gating. |
| Extensibility | Review changed targets. | Consumers extend behavior without editing the shipped file. |
| Package propagation | Review package layout when packaging is involved. | The selected surface matches the intended propagation scope. |
| Validation | Run the smallest build or evaluation command. | The build works in both default and extension-enabled states. |
