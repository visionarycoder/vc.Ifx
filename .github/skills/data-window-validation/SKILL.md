---
name: data-window-validation
title: Data Window Verification
description: Verify whether dated records fall inside approved time windows and fiscal periods.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 932
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - csv-ingestion-diagnostics
  - data-quality-reporting
appliesTo: '**/*.{csv,tsv,xlsx,parquet,md}'
tags:
  - data
  - dates
  - window
  - finance
---
# Data Window Verification

Agent verifies that dated records fit approved windows, lag limits, and fiscal periods. Agent reports anomalies without changing source values.

## When to Use

| User prompt | Use |
|---|---|
| User asks for out-of-period review | Use this skill |
| User asks for future-dated or back-dated record review | Use this skill |
| User uploads data with one or more date columns | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for delimiter or encoding defects | Use `csv-ingestion-diagnostics` |
| User asks for broad rule scoring | Use `data-quality-reporting` |
| User asks for archive inventory | Use `zip-batch-analysis` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Dataset | Yes | File in scope |
| Window start | No | Start date for review |
| Window end | No | End date for review |
| Allowed lag | No | Days allowed before window start |
| Fiscal logic | No | Calendar year or fiscal year mapping |

## Window Decision Table

| Condition | Agent step |
|---|---|
| User supplies start and end dates | Agent uses User dates |
| User omits dates | Agent uses stated default window and reports assumption |
| Fiscal column exists | Agent compares derived period to stored period |
| Holiday review enabled | Agent verifies weekend and holiday postings |
| Multiple date columns exist | Agent verifies each date column separately |

## Severity Table

| Anomaly | Severity |
|---|---|
| Future-dated record | High |
| Record exceeds allowed lag | High |
| Fiscal period mismatch | High |
| Missing required date | High |
| Record outside window within lag | Medium |
| Weekend or holiday posting | Low |
| Parseable format drift | Low |

## Workflow

| Step | Agent step | Test | Pass |
|---|---|---|---|
| 1 | Agent reads dataset and detects date columns. | Parse candidate date columns on sampled rows. | At least one date column parses or report lists missing date columns. |
| 2 | Agent verifies review window inputs. | Read start, end, lag, and fiscal logic. | Window start and end exist before anomaly scan starts. |
| 3 | Agent verifies date normalization. | Reformat parsed dates to one standard form. | All parsed dates use one output format. |
| 4 | Agent runs anomaly scans. | Compare each date to window, lag, and today. | Each anomaly row has one or more severity labels. |
| 5 | Agent verifies distribution patterns. | Count records by week or month. | Gaps and spikes list exact periods. |
| 6 | Agent generates summary and detail outputs. | Read final tables. | Output lists counts by column and anomaly type. |

## Outputs

| Output | Format |
|---|---|
| Window summary | Markdown |
| Anomaly detail | CSV or Markdown table |
| Distribution table | CSV or Markdown table |
| User summary | Short chat summary |

## Verification Checklist

- [ ] Agent reports detected date columns.
- [ ] Agent reports the active window and lag.
- [ ] Agent reports future, lag, missing, and fiscal defects.
- [ ] Agent keeps source dates unchanged.
- [ ] Agent includes exact anomaly counts by date column.
