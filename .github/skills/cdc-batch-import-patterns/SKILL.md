---
name: cdc-batch-import-patterns
title: CDC Batch Import Patterns
description: Import CDC rows in efficient batches with bounded transactions, duplicate-aware progress tracking, and resilient failure accumulation.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1760
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cdc-file-validation
  - cdc-uniqueness-enforcement
  - efcore-dbcontext-design
  - repository-unitofwork-efcore
related_docs:
  - https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server
source_paths:
  - tools/Journal.Loader/src/component/engine/materializing/Engine.Materializing.Service/MaterializingEngine.cs
  - tools/Journal.Loader/src/component/engine/materializing/Engine.Materializing.Service/Simulators/MaterializingEngineSimulator.cs
  - tools/Journal.Loader/src/component/manager/import/Manager.Import.Service/ImportManager.cs
  - tools/Journal.Loader/src/component/manager/import/Manager.Import.Service/Simulators/ImportManagerSimulator.cs
  - tools/Journal.Loader/src/component/access/cost-accounting/Access.CostAccounting.Service/Simulators/CostAccountingAccessSimulator.cs
appliesTo: '**/*.{cs,csproj,md}'
tags:
  - cdc
  - batching
  - efcore
  - bulk-insert
  - performance
  - cancellation
  - journal-loader
---
# CDC Batch Import Patterns

Agent imports CDC rows in batches that preserve idempotency, bound memory growth, and keep partial-failure evidence.

## When to Use

| Condition | Use |
|---|---|
| Agent imports large CDC tables into SQL Server | Use this skill |
| Agent adds batch commit logic or throughput tuning to Journal.Loader import flows | Use this skill |
| Agent implements progress reporting, correlation, or failed-record accumulation during imports | Use this skill |
| Agent optimizes EF Core insert paths for CDC rows | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Agent validates CDC package structure or header contracts | Use `cdc-file-validation` |
| Agent enforces duplicate detection and idempotent key rules | Use `cdc-uniqueness-enforcement` |
| Agent tunes read-side EF queries instead of write-side imports | Use `optimizing-ef-core-queries` |
| Agent designs one-row administrative inserts with no batching need | Use direct repository patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| CDC row set | Yes | Agent receives normalized rows or entities for one table at a time. |
| Batch size | Yes | Agent uses the requested size or defaults to `1000`. |
| Duplicate policy | Yes | Agent aligns batch logic with idempotent skip or reject behavior. |
| Persistence path | Yes | Agent selects existing EF Core insert path, repository path, or approved bulk path. |
| Correlation scope | Yes | Agent assigns one correlation value per import execution or package. |
| Cancellation token | Yes | Agent propagates the token through every I/O and insert boundary. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent resolves the target table, target entity type, duplicate policy, and correlation identifier before inserts start. | Review the import context object. | Table name, entity type, duplicate policy, and correlation ID are all present. |
| 2 | Agent partitions rows into bounded batches. Agent uses the requested size or the repository default of `1000`. | Count rows per partition. | Every partition size is `> 0` and `<= configured batch size`. |
| 3 | Agent pre-filters duplicates before the write path when existing-key lookup is available. | Compare input-row count to candidate-row count. | Candidate rows exclude known duplicates and duplicate counts are preserved in progress metrics. |
| 4 | Agent configures the EF Core insert scope for throughput. | Inspect the `DbContext` setup. | `AutoDetectChangesEnabled` is `false` during batch inserts, change tracking stays bounded, and the insert scope restores defaults after completion. |
| 5 | Agent executes one transaction per batch or one explicit bounded transaction scope that matches the failure-isolation rule. | Review transaction boundaries for one import run. | One failed batch does not roll back unrelated completed batches. |
| 6 | Agent uses the approved high-throughput insert path. | Inspect the chosen persistence call. | Agent uses existing bulk-insert support when the repository already provides it. Agent uses `AddRange` plus `SaveChanges` batching when no approved bulk path exists. |
| 7 | Agent accumulates per-record failures without stopping unrelated batches. | Inject one bad record into a multi-batch import. | Failed-record count increases, succeeding batches continue, and the final status becomes partial success when loaded count is `> 0`. |
| 8 | Agent logs progress after each batch with loaded, skipped, failed, and remaining counts plus the correlation identifier. | Review progress logs from one import run. | Every completed batch emits one progress event with numeric counters. |
| 9 | Agent honors cancellation between batches and within long-running I/O loops. | Cancel the token during extraction, conversion, or insert processing. | Import exits promptly, no new batch starts after cancellation, and completed-batch evidence stays intact. |

## Pattern Matrix

| Concern | Pattern |
|---|---|
| Default batch size | Agent starts from `1000` rows per batch and adjusts only from measured pressure such as lock time, memory growth, or command duration. |
| Partitioning | Agent partitions one table at a time so entity resolution, transaction scope, and failed-record reporting stay coherent. |
| Bulk insert path | Agent uses repository-approved bulk insert support when it already exists. Agent avoids adding new bulk packages during pattern work alone. |
| EF Core fallback | Agent uses `AddRange`, `SaveChanges`, and periodic `ChangeTracker.Clear()` when no bulk path exists. |
| Change tracking | Agent disables automatic change detection during batch inserts and keeps query tracking off for read-only key lookups. |
| Transaction scoping | Agent commits one batch at a time for predictable retry and failure isolation. |
| Failure isolation | Agent follows the binary-search isolation pattern from `MaterializingEngine` when one batch fails and the importer needs the exact bad row set. |
| Error accumulation | Agent records failed rows with serialized record content, error message, table name, composite key, and failure time. |
| Progress reporting | Agent emits batch ordinal, batch size, inserted count, skipped count, failed count, and elapsed duration per batch. |
| Cancellation | Agent passes the same `CancellationToken` into extraction, conversion, lookups, bulk insert, and transaction commit calls. |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Batch partitioning | Import a known row count with a fixed batch size. | Batch count and per-batch row counts match the partition formula. |
| Duplicate-aware totals | Replay input with known duplicate keys. | `Inserted + Skipped + Failed` equals total input rows and duplicate keys appear in `Skipped` or `Failed` according to policy. |
| EF Core insert configuration | Inspect runtime configuration or targeted tests. | Automatic change detection stays disabled during batch inserts and is restored after the scope ends. |
| Transaction isolation | Fail one batch in the middle of a multi-batch import. | Completed earlier batches remain committed and later batches continue only when policy allows. |
| Failure accumulation | Insert a mixed-validity batch set. | Failed-record log contains one entry per failed row with correlation metadata. |
| Progress logging | Review batch logs for one execution. | Every batch produces one progress event with counters and correlation ID. |
| Cancellation | Cancel a large import during execution. | Import stops before the next batch starts and no orphaned transaction remains open. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Agent pushes the full file into one `SaveChanges` call | Agent partitions the import into bounded batches. |
| Agent leaves EF Core auto-detect changes enabled during large inserts | Agent disables automatic change detection for the batch scope. |
| Agent stops the entire import on the first bad row | Agent accumulates the row failure, records correlation data, and continues unaffected batches. |
| Agent opens one transaction across the full package | Agent limits the transaction to one batch or another bounded retry unit. |
| Agent logs batch progress without duplicate and failure counters | Agent emits inserted, skipped, failed, remaining, and elapsed values per batch. |
| Agent checks cancellation only before the first batch | Agent checks cancellation before each batch, inside row loops, and before commit boundaries. |
