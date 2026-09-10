---
name: data-quality-reporting
title: Data Quality Reporting
description: Run data quality rules, score results, and generate concise reports and framework exports.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1074
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - data-contract-frontmatter
  - data-lineage
appliesTo: '**/*.{csv,xlsx,parquet,json,yml,yaml,md}'
tags:
  - data
  - quality
  - reporting
  - scoring
---
# Data Quality Reporting

Agent runs rule-based data quality analysis. Agent generates scorecards, defect tables, and optional framework exports.

## When to Use

| User prompt | Use |
|---|---|
| User asks for completeness, uniqueness, or freshness review | Use this skill |
| User uploads a dataset with contract rules | Use this skill |
| User asks for Great Expectations or dbt rule export | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for parse or encoding defects before load | Use `csv-ingestion-diagnostics` |
| User asks for contract authoring only | Use `data-contract-frontmatter` |
| User asks for lineage impact only | Use `data-lineage` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Dataset | Yes | File or table in scope |
| Rule source | No | Contract rules, custom rules, or default rules |
| Primary keys | No | User-supplied or inferred keys |
| Freshness target | No | Age limit for most recent record |

## Rule Decision Table

| Condition | Agent step |
|---|---|
| Contract quality block exists | Agent reads contract rules |
| Contract rules missing | Agent applies default rules |
| Reference table exists | Agent runs referential integrity rules |
| Export requested | Agent generates matching framework output |
| PII columns exist | Agent masks sampled values in reports |

## Dimension Table

| Dimension | Agent verifies | Pass |
|---|---|---|
| Completeness | Required values are not null | Null rate stays within rule threshold |
| Uniqueness | Key values do not repeat | Duplicate count stays within threshold |
| Validity | Values match type, range, or regex | Pass rate stays within threshold |
| Consistency | Cross-column logic holds | Pass rate stays within threshold |
| Timeliness | Latest record age stays within target | Age stays within target |
| Referential integrity | Foreign keys exist in source table | Match rate stays within threshold |

## Workflow

| Step | Agent step | Test | Pass |
|---|---|---|---|
| 1 | Agent reads dataset and profile inputs. | Read row count and column list. | Dataset loads with zero read defects. |
| 2 | Agent verifies rule source. | Read contract rules or default table. | Each active rule has scope, threshold, and severity. |
| 3 | Agent runs rules by dimension. | Run rules on full data or sampled data. | Each rule records pass count, fail count, and rate. |
| 4 | Agent verifies score computation. | Recompute dimension and overall scores. | Score totals match weighted formula exactly. |
| 5 | Agent generates reports and optional exports. | Read report sections and export files. | Output includes summary, rule detail, and recommendations. |
| 6 | Agent verifies report safety. | Read sampled values in output. | PII samples stay masked in all outputs. |

## Score Table

| Score range | Status | Agent step |
|---|---|---|
| 90-100 | Pass | Agent marks dataset ready for downstream use |
| 75-89 | Warning | Agent flags review before downstream use |
| 50-74 | Degraded | Agent flags blocked scope and defect owners |
| 0-49 | Fail | Agent flags immediate stop for affected scope |

## Outputs

| Output | Format |
|---|---|
| Quality report | Markdown |
| Rule detail | CSV or Markdown table |
| Scorecard | CSV or Markdown table |
| Framework export | JSON or YAML when User requests it |
| User summary | Short chat summary |

## Verification Checklist

- [ ] Agent reads dataset and active rules.
- [ ] Agent reports pass counts and fail counts per rule.
- [ ] Agent verifies weighted score math.
- [ ] Agent masks PII samples.
- [ ] Agent reports blocked scope for degraded or fail results.
