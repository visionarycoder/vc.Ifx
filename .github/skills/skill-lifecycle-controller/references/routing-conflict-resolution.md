---
title: Skill Routing Conflict Resolution
description: Overlap analysis and narrowing patterns for appliesTo collisions across repository skills.
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 977
prerequisites:
  - skill-lifecycle-controller
related_skills:
  - skill-lifecycle-controller
  - skill-discovery-guide
appliesTo: '.github/**/*.{md,prompt.md,instructions.md}'
tags:
  - routing
  - conflicts
  - appliesTo
---
# Skill Routing Conflict Resolution

Use this reference when the controller detects overlapping `appliesTo` patterns or duplicate activation conditions.

## Overlap Types

| Type | Example | Risk |
|---|---|---|
| Broad plus narrow glob | `**/*.cs` and `**/*.{cs,csproj}` | The broader pattern captures work intended for the narrower skill |
| Identical glob | Two skills use the same `appliesTo` value | Route ambiguity |
| Near-identical wording | Two skills activate on the same user-intent phrases | User intent maps to many skills |
| Bundle plus specialist shadowing | A bundle and a specialist describe the same direct entry | The bundle hides the specialist or duplicates it |

## Narrowing Patterns

| Situation | Narrowing Action |
|---|---|
| Broad file-type overlap | Restrict by directory, file suffix, or artifact family |
| Same directory, different task | Narrow by trigger words in the "When to Use" table |
| Same trigger family | Keep one controller or bundle, move specialists behind explicit routes |
| One skill owns migration and another owns validation | State the phase boundary in both skills |

## Acceptable Overlap Rules

| Overlap Shape | Acceptable State |
|---|---|
| Controller plus specialist | Acceptable when the controller delegates and the specialist owns execution detail |
| Bundle plus specialist | Acceptable when the bundle stays at routing depth and the specialist handles implementation |
| Shared markdown scope | Acceptable when intent words clearly separate the skills |
| Shared ecosystem audit scope | Acceptable when one skill audits and another edits |

## Conflict Resolution Workflow

| Step | Agent Action | Pass |
|---|---|---|
| 1 | Group skills by identical or overlapping `appliesTo` values. | Each conflict group has a stable key. |
| 2 | Compare "When to Use" tables for duplicate intent phrases. | Duplicate activation text is visible. |
| 3 | Select one primary owner for each direct-entry scenario. | One owner exists per scenario. |
| 4 | Rewrite secondary skills to route, delegate, or narrow scope. | Direct-entry ambiguity drops. |
| 5 | Re-run overlap analysis. | Conflict count decreases or a justified exception is recorded. |

## Bundle Versus Specialist Boundaries

| Pattern | Primary Owner |
|---|---|
| User asks for a broad family outcome | Bundle or controller |
| User asks for one concrete implementation task | Specialist |
| User asks for ecosystem review across many skills | Controller |
| User asks for scaffold creation only | `create-skill` |

## Recommendation Patterns

| Conflict | Recommendation |
|---|---|
| Identical `appliesTo` and identical intent | Merge or convert one skill into a reference target |
| Identical `appliesTo` and different phases | State phase ownership in activation tables |
| Broad controller overlap | Keep overlap, document delegation, and narrow direct specialist entry text |
| Bundle shadowing specialist | Reduce bundle detail and point to the specialist |

## Report Fields

| Field | Use |
|---|---|
| Conflict group | Stable identifier for related overlaps |
| Skills | List of impacted skill names |
| Overlap pattern | Shared or conflicting `appliesTo` value |
| Intent collision | Repeated trigger text, if present |
| Recommendation | Narrow, delegate, merge, or accept with justification |
