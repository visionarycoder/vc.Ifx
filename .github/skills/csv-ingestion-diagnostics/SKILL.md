---
name: csv-ingestion-diagnostics
title: CSV Ingestion Diagnostics
description: Verify delimited files before ingestion and fix parse defects in a new file.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1030
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - zip-batch-analysis
  - data-quality-reporting
appliesTo: '**/*.{csv,tsv,txt,md}'
tags:
  - data
  - csv
  - ingestion
  - diagnostics
---
# CSV Ingestion Diagnostics

Agent verifies delimited files before ingestion. Agent writes fixes to a new file only.

## When to Use

| User prompt | Use |
|---|---|
| User uploads `.csv`, `.tsv`, or delimited `.txt` for ingestion | Use this skill |
| User reports a parse defect, row drift, or header defect | Use this skill |
| User asks for encoding, delimiter, or null analysis | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for archive inventory across many files | Use `zip-batch-analysis` |
| User asks for business rule scoring after parse succeeds | Use `data-quality-reporting` |
| User asks for schema contract authoring | Use `data-contract-frontmatter` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Source file | Yes | Delimited file in scope |
| Ingestion target | No | Fabric, ADF, SQL, or other target |
| Known delimiter | No | User supplies when already known |
| Fix intent | No | User states whether Agent writes a fixed file |

## Decision Table

| Condition | Agent step |
|---|---|
| Delimiter unknown | Agent detects delimiter from sampled rows |
| Encoding unknown | Agent detects encoding before parse |
| Header missing | Agent reports header defect and asks User for intent |
| File exceeds 50 MB | Agent reads chunks and aggregates findings |
| User declines fixes | Agent generates report only |

## Workflow

| Step | Agent step | Test | Pass |
|---|---|---|---|
| 1 | Agent reads file metadata and samples raw rows. | Read first 100 raw lines. | Sample rows load without read errors. |
| 2 | Agent verifies encoding and delimiter. | Parse three row samples with detected settings. | Parsed column count stays stable across samples. |
| 3 | Agent verifies structure. | Compare raw line count, parsed row count, and field counts. | Row drift equals zero or report lists exact drift rows. |
| 4 | Agent verifies headers and column patterns. | Read header row and column samples. | Duplicate headers equal zero or report lists duplicates. |
| 5 | Agent verifies null and type defects. | Profile each column on sampled or full data. | Each column has one dominant type and null rate. |
| 6 | Agent writes optional fixes to a new file. | Re-read fixed file with detected settings. | Fixed file parses with zero new structural defects. |
| 7 | Agent generates final report. | Read report sections. | Report includes metadata, defects, fixes, and next steps. |

## Defect Rules

| Defect | Agent verifies | Pass |
|---|---|---|
| Encoding defect | Text decodes with one consistent encoding | Zero decode exceptions |
| Delimiter drift | Row field counts match header width | At least 99% rows match header width |
| Duplicate headers | Header names stay unique after normalization | Zero duplicates |
| Null markers | Null markers map to one approved value | Zero unmapped null markers after fix |
| Mixed types | Numeric and text values do not mix unexpectedly | Mixed-type rate stays below 1% or report flags column |

## Outputs

| Output | Format |
|---|---|
| Diagnostic report | Markdown |
| Column findings | CSV or Markdown table |
| Fixed file | CSV when User requests fixes |
| User summary | Short chat summary |

## Verification Checklist

- [ ] Agent reads the source file before analysis.
- [ ] Agent verifies encoding before full parse.
- [ ] Agent reports row drift, header defects, and type defects.
- [ ] Agent writes fixes to a new file only.
- [ ] Agent includes test and pass results in the report.
