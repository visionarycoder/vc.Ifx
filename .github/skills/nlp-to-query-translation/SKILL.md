---
name: nlp-to-query-translation
title: NLP to Query Translation
description: Translate natural-language questions into governed SQL, DAX, or KQL patterns when a prompt needs intent parsing, semantic search, or Copilot-ready query generation.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1785
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-kql-diagnostics
  - semantic-layer-fabric
  - semantic-modeling-standards
  - terminology-normalization
appliesTo: '**/*.{md,sql,dax,kql,json,yml,yaml}'
tags:
  - nlp
  - sql
  - dax
  - kql
  - copilot
---
# NLP to Query Translation

Agent translates natural-language requests into bounded query intents and validated SQL, DAX, or KQL output.

## When to Use

| Condition | Use |
|---|---|
| Prompt asks for SQL from a business question | Use this skill |
| Prompt asks for DAX from a KPI or report question | Use this skill |
| Prompt asks for KQL from a telemetry or operations question | Use this skill |
| Prompt needs semantic search or Copilot-style query interpretation | Use this skill |
| Prompt needs safe query generation with explicit assumptions and validation | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt already has the exact query and needs debugging only | Use the query-surface diagnostic skill |
| Prompt needs semantic-model design before DAX generation | Use `semantic-layer-fabric` or `semantic-modeling-standards` |
| Prompt needs term harmonization with no query output | Use `terminology-normalization` |
| Prompt needs unrestricted code generation outside governed query surfaces | Use the stack-specific implementation skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Natural-language request | Yes | Agent records metric, entities, filters, grain, and time scope. |
| Target surface | Yes | Agent records SQL, DAX, KQL, or semantic-search target. |
| Schema context | Yes | Agent records tables, measures, fields, or telemetry entities. |
| Safety policy | No | Agent records blocked verbs, row limits, and approval boundaries. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent parses the request into intent, entities, metrics, filters, and time scope. | Review intent frame. | Intent frame captures metric, grain, filters, and period. |
| 2 | Agent resolves prompt vocabulary to schema, semantic-model, or ontology terms. | Review term-resolution map. | Each referenced concept maps to a valid field or measure. |
| 3 | Agent selects the output pattern from the decision matrix. | Compare intent to matrix rows. | Query form matches the target engine and semantic grain. |
| 4 | Agent generates bounded query text with explicit projections, filters, grouping, and ordering. | Review generated query. | Query text uses only validated fields and operations. |
| 5 | Agent applies safety checks for destructive verbs, injection risk, and unbounded scans. | Review safety checklist. | Query stays read-oriented and scoped. |
| 6 | Agent emits assumptions, confidence notes, and a validation companion query. | Review output package. | Output contains one governed query plus one measurable validation path. |

## Decision Matrix

| Intent Shape | Preferred Surface | Pattern | Example |
|---|---|---|---|
| Transactional table lookup with joins and aggregates | SQL | `SELECT ... FROM ... WHERE ... GROUP BY ...` | Sales by region and month |
| KPI question over governed measures and dimensions | DAX | `EVALUATE SUMMARIZECOLUMNS (...)` | Monthly net revenue by fund |
| Telemetry, logs, or time-series diagnostics | KQL | `table | where | summarize` | Error count by hour |
| Similarity or retrieval over metadata and definitions | Semantic search | Embedding lookup plus metadata filter | Search glossary entries for appropriation terms |
| Low-confidence or ambiguous prompt | Intent frame plus clarification assumptions | No direct execution path | `Account` flagged as overloaded |

## Pattern Catalog

| Pattern | Use | Example |
|---|---|---|
| Intent frame | Normalize question parts before query generation | `metric=Net Revenue; grain=Month; filter=Region=West` |
| SQL aggregate | Structured facts and joins | `SELECT Region, SUM(NetAmount) ...` |
| DAX business query | Existing measures and dimensions | `EVALUATE SUMMARIZECOLUMNS ( DimDate[Month], "Net Revenue", [Net Revenue] )` |
| KQL diagnostic | Time-bounded grouped events | `traces | where Timestamp > ago(7d) | summarize count()` |
| Semantic-search request | Retrieval over governed metadata | `topK=10; domain='finance'` |
| Validation companion | Reconcile totals or row counts | `COUNT(*)`, `SUM(...)`, or `ROW(...)` |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Intent fidelity | Compare generated output to the original request. | Metric, grain, filters, and time scope match the request. |
| Schema validity | Review resolved fields or measures against the target context. | Query references only valid schema elements. |
| Safety | Scan generated text for mutations or unbounded scans. | Output stays read-oriented and scoped. |
| Vocabulary alignment | Compare resolved terms to glossary or ontology anchors. | Generated query uses governed names or mapped aliases. |
| Validation path | Review companion query and assumptions note. | Output includes one measurable verification path. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Prompt uses a synonym with no direct schema match | Agent resolves terminology before query generation. |
| Generated SQL selects every column | Agent projects only fields required for the intent. |
| DAX query bypasses governed measures | Agent binds to existing measures first. |
| KQL query omits an explicit time window | Agent adds bounded time scope. |
| Low-confidence prompt jumps straight to execution | Agent returns assumptions and validation instead of silent execution. |

## Reference Assets

| Resource | Role | Location |
|---|---|---|
| Guvi ontology article | Background for semantic meaning beyond raw fields | https://www.guvi.in/blog/ontologies-in-ai/ |
| SAAM JSON-LD | Real-world term anchors for intent resolution | `docs/references/SAAM/Ontology/saam-core-jsonld.json` |
| SAAM Fabric semantic model | Real-world DAX and semantic-model target vocabulary | `docs/references/SAAM/Ontology/saam-fabric-semantic-model.json` |
