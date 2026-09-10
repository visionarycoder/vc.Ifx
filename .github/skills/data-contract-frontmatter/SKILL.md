---
name: data-contract-frontmatter
title: Data Contract Frontmatter
description: Generate and verify compact data contract frontmatter for datasets, tables, and APIs.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1021
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - data-lineage
  - data-quality-reporting
appliesTo: '**/*.{yml,yaml,json,csv,md}'
tags:
  - data
  - contract
  - frontmatter
  - governance
---
# Data Contract Frontmatter

Agent generates ODCS-aligned frontmatter and verifies required metadata, schema, quality, and lineage references.

## When to Use

| User prompt | Use |
|---|---|
| User asks for a data contract from schema or sample data | Use this skill |
| User asks to verify existing contract frontmatter | Use this skill |
| User asks to bump contract version and changelog | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for lineage mapping only | Use `data-lineage` |
| User asks for rule scoring only | Use `data-quality-reporting` |
| User asks for archive or file diagnostics | Use an ingestion skill |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Contract scope | Yes | Dataset, table, stream, or API |
| Metadata | Yes | Name, owner, domain, status, description |
| Schema source | No | CSV, JSON, YAML, or manual column list |
| Version intent | No | Draft, bump, or deprecate |

## Decision Table

| Condition | Agent step |
|---|---|
| Schema file exists | Agent infers columns, types, and nullability |
| Schema file missing | Agent reads User-supplied column list |
| Existing contract exists | Agent preserves stable fields and bumps version |
| Quality rules missing | Agent routes to `data-quality-reporting` for rule seeds |
| Lineage block missing | Agent routes to `data-lineage` for lineage seeds |

## Version Decision Table

| Change type | Agent step |
|---|---|
| Breaking schema change | Agent bumps major version |
| Backward-compatible column addition | Agent bumps minor version |
| Metadata-only edit | Agent bumps patch version |

## Workflow

| Step | Agent step | Test | Pass |
|---|---|---|---|
| 1 | Agent reads source metadata and schema inputs. | Read metadata fields and one schema source. | Required metadata fields exist or report lists missing fields. |
| 2 | Agent verifies schema shape. | Read all columns or properties. | Column list has at least one named field. |
| 3 | Agent generates frontmatter. | Read generated YAML. | YAML includes name, version, owner, schema, quality, and lineage references. |
| 4 | Agent verifies syntax and required fields. | Parse generated YAML. | Parse succeeds with zero syntax defects. |
| 5 | Agent verifies version semantics. | Compare prior and new version. | Version bump matches the decision table. |
| 6 | Agent writes contract output. | Re-read written file. | Written file matches generated YAML exactly. |

## Required Contract Fields

| Field | Agent verifies | Pass |
|---|---|---|
| `name` | Contract name exists | One non-empty value |
| `version` | Semantic version format exists | Format matches `major.minor.patch` |
| `owner` | Owner block exists | Name and email both exist |
| `schema` | Schema block exists | At least one column entry exists |
| `quality` | Quality reference exists | Rule source or related skill exists |
| `lineage` | Lineage reference exists | Source or related skill exists |

## Outputs

| Output | Format |
|---|---|
| Contract frontmatter | YAML |
| Field gap summary | Markdown table |
| Version bump summary | Markdown |
| User summary | Short chat summary |

## Verification Checklist

- [ ] Agent reads metadata before generation.
- [ ] Agent verifies schema fields and version semantics.
- [ ] Agent includes quality and lineage references.
- [ ] Agent parses generated YAML before write.
- [ ] Agent reports missing fields with exact names.
