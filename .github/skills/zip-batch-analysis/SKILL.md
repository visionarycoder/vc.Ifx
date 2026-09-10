---
name: zip-batch-analysis
title: ZIP Batch Analysis
description: Inventory archive contents, verify archive integrity, and route files to downstream skills.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 887
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - csv-ingestion-diagnostics
  - data-quality-reporting
appliesTo: '**/*.{zip,gz,tar.gz,md}'
tags:
  - data
  - archive
  - zip
  - ingestion
---
# ZIP Batch Analysis

Agent inventories archive contents, verifies integrity, and routes each file to the right downstream skill.

## When to Use

| User prompt | Use |
|---|---|
| User uploads `.zip`, `.gz`, or `.tar.gz` for batch review | Use this skill |
| User asks for file manifest before extraction | Use this skill |
| User asks whether an archive contains corrupt or nested archives | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for one CSV defect review | Use `csv-ingestion-diagnostics` |
| User asks for dataset rule scoring after extraction | Use `data-quality-reporting` |
| User asks for contract authoring | Use `data-contract-frontmatter` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Archive file | Yes | `.zip`, `.gz`, or `.tar.gz` file |
| Extraction intent | No | Manifest only or extract after review |
| Depth limit | No | Maximum nested archive depth |
| Route preference | No | Downstream skills or report only |

## Routing Table

| File type | Agent route |
|---|---|
| CSV or TSV | `csv-ingestion-diagnostics` |
| JSON, Parquet, or tabular dataset | `data-quality-reporting` |
| YAML contract input | `data-contract-frontmatter` |
| Nested archive | `zip-batch-analysis` with reduced depth |
| Unknown binary | Report only |

## Workflow

| Step | Agent step | Test | Pass |
|---|---|---|---|
| 1 | Agent reads archive metadata and entry list. | Read entry count and compressed size. | Archive reads with zero read defects. |
| 2 | Agent verifies integrity without full extraction. | Run CRC or archive test command. | Corrupt entry count equals zero or report lists exact entries. |
| 3 | Agent verifies file types and duplicates. | Read magic bytes and entry paths. | Each entry has one type label and duplicate flag state. |
| 4 | Agent verifies nested archives and protected entries. | Scan entry types and flags. | Nested depth and protection state are reported for each entry. |
| 5 | Agent generates manifest and route plan. | Read manifest tables. | Manifest lists path, type, size, status, and route. |
| 6 | Agent runs optional extraction to a new scope. | Re-read extracted file count. | Extracted file count matches manifest count for extracted entries. |

## Status Table

| Status | Meaning |
|---|---|
| Pass | Entry opens and type detection succeeds |
| Warning | Entry opens but needs downstream review |
| Fail | Entry is corrupt, protected, or unreadable |

## Outputs

| Output | Format |
|---|---|
| Archive manifest | Markdown |
| Entry inventory | CSV or Markdown table |
| Route plan | Markdown table |
| User summary | Short chat summary |

## Verification Checklist

- [ ] Agent reports archive read result.
- [ ] Agent reports corrupt, protected, and nested entries.
- [ ] Agent reports file type per entry.
- [ ] Agent writes extraction output to a new scope only.
- [ ] Agent includes downstream route decisions.
