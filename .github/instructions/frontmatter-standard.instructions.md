---
title: Front Matter Standard
description: Apply a compact, predictable front matter contract to markdown files.
doc_type: policy
status: active
last_updated: 2026-08-29
target_audience: both
complexity: low
estimated_tokens: 555
prerequisites: []
related_skills:
  - documentation-frontmatter
  - documentation-governance
  - skill-suite-optimization
appliesTo: '**/*.md'
tags:
  - instructions
  - frontmatter
---
# Front Matter Standard

Agent applies one predictable YAML front matter contract to markdown files.

## Required Properties

| Property | Value Rule |
|---|---|
| `title` | Human-readable document title |
| `doc_type` | One allowed classifier |
| `status` | One allowed lifecycle value |
| `last_updated` | Date in `YYYY-MM-DD` format |

## Allowed Values

| Property | Allowed Values |
|---|---|
| `doc_type` | `readme`, `guide`, `reference`, `runbook`, `plan`, `report`, `policy`, `skill`, `prompt`, `instruction`, `archive` |
| `status` | `draft`, `active`, `deprecated`, `archived` |

## Optional Properties

| Property | Use |
|---|---|
| `summary` | Brief content summary |
| `owner` | Responsible team or person |
| `tags` | Classification list |
| `target_audience` | `ai`, `developer`, `ops`, or `both` |
| `related_docs` | Supporting document paths or URLs |
| `source_paths` | Related source-code paths |

## Procedure

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent adds the four required properties to every markdown file. | Inspect front matter. | All required properties exist. |
| 2 | Agent selects allowed `doc_type` and `status` values only. | Compare values to the allowed table. | No unknown values exist. |
| 3 | Agent infers missing values from the file title, location, and role. | Review inferred values. | Inferred values match the file purpose. |
| 4 | Agent validates the final file set. | Run `npm run frontmatter:validate`. | Exit code `0`. |

## Inference Rules

| Missing Property | Inference Rule |
|---|---|
| `title` | Use the first H1. Use the filename if the H1 is absent. |
| `doc_type` | Use the file path and document role. |
| `status` | Use `active` unless the file state says otherwise. |
| `last_updated` | Use the current edit date. |
