---
name: fabric-kql-diagnostics
title: Fabric KQL Diagnostics
description: Diagnose Microsoft Fabric KQL query failures, ingestion visibility gaps, and common performance regressions with evidence-backed rewrites.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1420
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-lakehouse-ingestion
  - fabric-pipeline-diagnostics
appliesTo: '**/*.{kql,txt,json,md}'
tags:
  - fabric
  - kql
  - diagnostics
  - eventhouse
  - performance
---
# Fabric KQL Diagnostics

Agent diagnoses Microsoft Fabric KQL queries from pasted text, saved `.kql` files, or schema evidence.

## Use When

Agent uses this skill when:
- User reports a KQL syntax or semantic error
- User needs performance triage for a slow query
- User needs ingestion visibility commands for a KQL database or Eventhouse
- User needs an evidence-backed rewrite of an existing query

## Do Not Use When

Agent does not use this skill when:
- User needs Fabric pipeline failure analysis
- User needs Lakehouse file ingestion guidance
- User needs Power BI semantic model design
- User needs broad Kusto training without a diagnostic task

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Query text or file | Yes | Agent reads the exact query first |
| Error text | No | Agent uses the exact engine message when present |
| Table schema | No | Agent verifies column names and types when available |
| Performance symptom | No | Agent uses duration, timeout, or throttle details |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Capture evidence | Agent extracts tables, columns, operators, filters, and error text. | Agent verifies that every referenced table and operator appears in the intake summary. | Intake summary matches the user query with no missing clauses. |
| 2. Isolate failure type | Agent classifies the issue as syntax, semantic, ingestion, or performance. | Agent verifies one primary failure category from query text or error text. | Agent names one primary category with supporting evidence. |
| 3. Verify high-risk patterns | Agent verifies time filtering, filter pushdown, projection scope, join scope, and parse usage. | Agent verifies each pattern against the query pipeline. | Agent produces a bounded findings list with exact line or clause references. |
| 4. Produce minimal rewrite | Agent rewrites only the failing or high-cost clauses. | Agent verifies that the rewrite preserves the user intent and removes the named defect. | Rewritten query removes the primary defect without unrelated restructuring. |
| 5. Produce verification commands | Agent adds schema or ingestion visibility commands when they reduce uncertainty. | Agent verifies that each command maps to one open question. | Command set answers the remaining unknowns with no redundant steps. |

## Common Findings

| Finding | Signal | Minimal correction |
|---|---|---|
| Missing time filter | Query scans a time-series table without `ago(...)` or bounded timestamps | Add a bounded `where Timestamp >= ago(...)` clause early |
| Late filter pushdown | `where` appears after `summarize`, `join`, or `mv-expand` | Move the selective `where` clause before the expensive operator |
| Wide projection | Query returns all columns or omits `project` on a wide table | Add `project` for only required columns |
| Join overreach | `join` uses large inputs without selective pre-filters | Filter both sides first and reduce join columns |
| Schema mismatch | Error reports missing columns or type mismatch | Verify schema and cast or rename the failing column |
| Ingestion uncertainty | User cannot confirm whether data landed | Add `.show` commands for failures, schema, and extents |

## Verification Commands

| Purpose | Command |
|---|---|
| Verify schema | `.show table <TableName> schema as json` |
| Verify recent ingestion failures | `.show ingestion failures | where FailedOn >= ago(1d)` |
| Verify data freshness | `.show table <TableName> extents | summarize max(MaxCreatedOn)` |
| Verify bounded row count | `<TableName> | where Timestamp >= ago(1d) | count` |

## Output Contract

| Output | Contents |
|---|---|
| Findings summary | Primary failure type, exact query defects, and evidence |
| Rewritten query | Minimal corrected or optimized KQL |
| Verification plan | `.show` or query commands that close remaining gaps |

## Verification

| Check | Test | Pass |
|---|---|---|
| Failure isolation | Agent verifies one primary defect before rewriting | Output names one primary defect with evidence |
| Rewrite quality | Agent verifies that the rewritten query preserves intent | Rewrite changes only failing or expensive clauses |
| Diagnostic closure | Agent verifies that every open question has one command or note | No unresolved gap lacks a next action |
| Measurable outcome | Agent verifies a concrete success target | Query executes without the original error or returns bounded results in the expected time window |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent rewrites the whole query first | Agent isolates one primary defect before broader tuning |
| Agent guesses schema | Agent requests or generates schema verification commands |
| Agent uses `contains` on token searches without need | Agent prefers `has` when token matching fits |
| Agent treats ingestion and query failures as the same problem | Agent separates landing verification from query semantics |
