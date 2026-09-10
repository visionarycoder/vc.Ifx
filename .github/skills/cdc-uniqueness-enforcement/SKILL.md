---
name: cdc-uniqueness-enforcement
title: CDC Uniqueness Enforcement
description: Enforce idempotent CDC imports by normalizing and comparing the composite key built from start LSN and sequence value.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1510
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cdc-file-validation
  - cdc-batch-import-patterns
  - transaction-duplication-detection
related_docs:
  - https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server
source_paths:
  - tools/Journal.Loader/src/ifx/Ifx.JournalLoader/Simulators/CdcCompositeKey.cs
  - tools/Journal.Loader/src/component/engine/materializing/Engine.Materializing.Service/MaterializingEngine.cs
  - tools/Journal.Loader/src/component/engine/materializing/Engine.Materializing.Service/Simulators/MaterializingEngineSimulator.cs
  - tools/Journal.Loader/src/component/access/cost-accounting/Access.CostAccounting.Service/Simulators/CostAccountingAccessSimulator.cs
  - tools/Journal.Loader/src/component/access/cost-accounting/Access.CostAccounting.Orm/Models/RActv.cs
appliesTo: '**/*.{cs,sql,md}'
tags:
  - cdc
  - idempotency
  - uniqueness
  - lsn
  - duplicate-detection
  - journal-loader
---
# CDC Uniqueness Enforcement

Agent enforces CDC idempotency with the composite key `(StartLsn, SequenceValue)`. Agent normalizes key values, detects duplicates in memory, and preserves database-level uniqueness.

## When to Use

| Condition | Use |
|---|---|
| Agent imports CDC rows into cost-accounting tables | Use this skill |
| Agent implements duplicate detection for repeated CDC packages or replayed rows | Use this skill |
| Agent compares `byte[]` CDC keys from SQL entities and CSV hex values | Use this skill |
| Agent designs unique database constraints for CDC-backed tables | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Agent validates ZIP structure, control-file rows, or CSV headers | Use `cdc-file-validation` |
| Agent tunes batch size, transaction scope, or progress reporting | Use `cdc-batch-import-patterns` |
| Agent finds business duplicates that do not use CDC metadata | Use `transaction-duplication-detection` |
| Agent reviews one-off generic hash-key logic outside CDC import | Use direct code review or repository-specific patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Start LSN source | Yes | Agent reads `__$start_lsn` from CSV or `StartLsn` from entity bytes. |
| Sequence value source | Yes | Agent reads `__$seqval` from CSV or `SequenceValue` from entity bytes. |
| Duplicate policy | Yes | Agent records skip, reject, or overwrite behavior per call path. |
| In-memory store scope | Yes | Agent selects `ConcurrentDictionary` or equivalent concurrent key store. |
| Database model | Yes | Agent verifies the unique key at the table layer. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent normalizes CSV hex values and entity byte arrays into one stable representation. Agent uses `CdcCompositeKey.FromHex` and `CdcCompositeKey.FromBytes` patterns. | Convert the same logical key from CSV and from entity bytes. | Both paths produce one identical storage key string. |
| 2 | Agent validates key completeness before duplicate checks. | Read the normalized `StartLsn` and `SequenceValue` values. | `IsValid` equals `true` for import candidates and invalid-key count is recorded separately. |
| 3 | Agent constructs the composite uniqueness key from `StartLsn` and `SequenceValue` only. | Review key creation code. | Key includes exactly two parts and key order stays stable. |
| 4 | Agent preloads existing keys from the target table when the import path supports lookups. | Query existing rows and extract `StartLsn` plus `SequenceValue`. | Existing-key set contains every already imported CDC key in normalized form. |
| 5 | Agent detects in-batch duplicates with a concurrent key store. | Insert repeated keys through the in-memory detection path. | First `TryAdd` succeeds and repeated `TryAdd` calls fail for the same storage key. |
| 6 | Agent applies duplicate disposition rules. | Replay the same record across the configured policy modes. | Skip mode increments `RecordsSkipped`, reject mode emits a failure record, and overwrite mode appears only on an explicit replace path. |
| 7 | Agent preserves database uniqueness with a unique index or equivalent constraint on `(StartLsn, SequenceValue)`. | Inspect the ORM mapping or generated schema. | Database enforces one row per normalized key pair. |
| 8 | Agent logs duplicate outcomes with table, composite key, source file, and disposition. | Review duplicate log records. | Every duplicate entry includes enough data for replay analysis and idempotency auditing. |

## Pattern Matrix

| Concern | Pattern |
|---|---|
| Key helper | Agent uses the existing `CdcCompositeKey` helper in `Ifx.JournalLoader` instead of ad hoc string concatenation. |
| Hex normalization | Agent strips the `0x` prefix, uppercases hex, and preserves deterministic ordering as `StartLsn|SequenceValue`. |
| Byte-array equality | Agent avoids `byte[]` reference equality and converts bytes to normalized hex before comparison. |
| In-memory duplicate detection | Agent uses `ConcurrentDictionary<string, byte>` or `ConcurrentDictionary<string, object>` with `TryAdd` for thread-safe key admission. |
| Existing-row lookup | Agent loads existing keys once per import scope and compares normalized storage keys in a `HashSet<string>`. |
| Skip logic | Agent skips replays of already imported keys and records the skip as an idempotent outcome, not as a data-loss defect. |
| Reject logic | Agent rejects rows with missing or malformed CDC keys and writes a failed-record entry with the correlation context. |
| Database uniqueness | Agent aligns the ORM and migration model to a unique index on `StartLsn` and `SequenceValue`. |
| Logging | Agent logs duplicate count, table name, composite key, and disposition at information level for skips and at error level for malformed keys. |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Hex normalization parity | Build one key from CSV hex and one key from entity bytes. | Storage key strings match exactly. |
| In-batch duplicate detection | Feed two rows with the same `(__$start_lsn, __$seqval)` through the same batch. | First row admits once and duplicate row increments the skip or reject counter. |
| Cross-run idempotency | Replay an already imported file. | New insert count for repeated keys equals `0` and duplicate count matches the replayed key count. |
| Database constraint | Inspect schema or migration output for the unique key pair. | Unique key covers `StartLsn` and `SequenceValue` together. |
| Malformed-key handling | Feed rows with null, empty, or invalid key values. | Rows do not reach the insert path and failure output includes the composite-key defect. |
| Duplicate logging | Review logs or failed-record storage. | Every duplicate event includes table name, key, and disposition. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Agent compares `byte[]` values by reference | Agent converts bytes to normalized hex before equality checks. |
| Agent uses only `StartLsn` as the uniqueness key | Agent includes both `StartLsn` and `SequenceValue` in the composite key. |
| Agent treats duplicate replays as hard errors in the idempotent path | Agent records the event as a skip unless the configured policy is reject. |
| Agent loads existing rows and batch keys with different normalization rules | Agent routes both paths through `CdcCompositeKey`. |
| Agent enforces uniqueness only in memory | Agent adds a database unique constraint so parallel imports stay safe. |
| Agent overwrites an existing row on duplicate detection without explicit policy | Agent keeps overwrite behavior on explicit replace-only paths. |
