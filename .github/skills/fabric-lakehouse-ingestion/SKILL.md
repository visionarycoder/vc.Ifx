---
name: fabric-lakehouse-ingestion
title: Fabric Lakehouse Ingestion
description: Diagnose Microsoft Fabric Lakehouse ingestion plans, schema drift, OneLake path defects, and post-load verification gaps.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1490
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-kql-diagnostics
  - fabric-pipeline-diagnostics
appliesTo: '**/*.{csv,parquet,json,xlsx,py,sql,md}'
tags:
  - fabric
  - lakehouse
  - ingestion
  - delta
  - diagnostics
---
# Fabric Lakehouse Ingestion

Agent diagnoses Microsoft Fabric Lakehouse ingestion paths, schemas, and write plans before or after a failed load.

## Use When

Agent uses this skill when:
- User needs a file-to-Lakehouse ingestion plan
- User reports a Lakehouse write failure or schema mismatch
- User needs OneLake path triage
- User needs append, overwrite, or merge guidance with verification steps

## Do Not Use When

Agent does not use this skill when:
- User needs Fabric pipeline orchestration triage
- User needs KQL query diagnostics
- User needs semantic model design from finished tables
- User needs generic Spark training without a Lakehouse task

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source format | Yes | Agent verifies CSV, Parquet, JSON, Excel, or Delta shape |
| Workspace and Lakehouse names | Yes | Agent uses both values to build the target path |
| Target table or folder | Yes | Agent verifies `Tables/` versus `Files/` destination |
| Write mode | Yes | Agent verifies append, overwrite, or merge semantics |
| Existing target schema | No | Agent verifies drift when appending or merging |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Capture source and target facts | Agent records source format, target object, write mode, and business grain. | Agent verifies that the intake includes source, destination, and write mode. | Intake summary contains all required ingestion facts. |
| 2. Verify OneLake path | Agent builds the canonical `abfss://` path and the notebook-relative path. | Agent verifies workspace, Lakehouse, and destination segment values. | Path resolves to one unambiguous target location. |
| 3. Verify schema and drift | Agent compares inbound columns to target columns when a target exists. | Agent verifies added, missing, renamed, and type-shifted columns. | Drift report names every incompatible column and one correction path. |
| 4. Diagnose storage strategy | Agent verifies partition keys, write mode, and optional optimization columns. | Agent verifies date keys, low-cardinality partitions, and high-cardinality anti-patterns. | Storage plan avoids known partition or file-layout defects. |
| 5. Produce ingestion plan | Agent outputs the minimal notebook or SQL plan plus post-load verification steps. | Agent verifies that every plan step maps to a diagnosed need. | Plan is executable and includes row-count and schema verification. |

## Common Findings

| Finding | Signal | Minimal correction |
|---|---|---|
| Wrong OneLake path | `FileNotFoundException` or ambiguous workspace or Lakehouse name | Rebuild the `abfss://<workspace>@onelake.dfs.fabric.microsoft.com/<lakehouse>.Lakehouse/...` path |
| Schema mismatch | Delta or Spark reports missing or incompatible columns | Cast inbound columns or align the target schema before write |
| Partition misuse | High-cardinality identifier proposed as partition key | Use date or low-cardinality business columns instead |
| Merge uncertainty | User cannot state natural key or dedupe logic | Stop at plan stage and require the match key |
| Post-load blind spot | User has no row-count or schema verification step | Add count, sample, and schema verification commands |

## Verification Commands

| Purpose | Command or query |
|---|---|
| Verify row count | `SELECT COUNT(*) FROM <table>` |
| Verify schema | `DESCRIBE TABLE <table>` |
| Verify partition distribution | `SELECT <partition_column>, COUNT(*) FROM <table> GROUP BY <partition_column>` |
| Verify latest load window | `SELECT MAX(<ingestion_or_business_timestamp>) FROM <table>` |

## Output Contract

| Output | Contents |
|---|---|
| Ingestion plan | Target path, write mode, schema actions, and execution steps |
| Drift report | Added, missing, renamed, and incompatible columns |
| Verification plan | SQL or notebook steps for row count, schema, and freshness |

## Verification

| Check | Test | Pass |
|---|---|---|
| Path quality | Agent verifies canonical and relative target paths | Output contains one canonical target path and one execution path |
| Drift quality | Agent verifies every incompatible column | Drift report lists each breaking difference with one correction |
| Storage quality | Agent verifies partition and write-mode choices against source facts | Plan avoids high-cardinality partitions and undefined merge keys |
| Measurable outcome | Agent verifies a concrete success target | Post-load row count, schema, and freshness queries confirm a successful load |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent jumps to notebook code without target facts | Agent records workspace, Lakehouse, target, and mode first |
| Agent treats append and merge as equivalent | Agent requires explicit key semantics for merge |
| Agent recommends partitioning on unique IDs | Agent prefers date or low-cardinality keys |
| Agent omits post-load verification | Agent adds row-count, schema, and freshness verification |
