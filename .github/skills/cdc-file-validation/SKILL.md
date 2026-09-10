---
name: cdc-file-validation
title: CDC File Validation
description: Validate CGI 2137 CDC ZIP packages before import and classify package, file, header, and row defects.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1580
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - csv-ingestion-diagnostics
  - zip-batch-analysis
  - cdc-uniqueness-enforcement
  - cdc-batch-import-patterns
related_docs:
  - https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server
source_paths:
  - tools/Journal.Loader/docs/specifications/file-format.md
  - tools/Journal.Loader/src/component/engine/extracting/Engine.Extracting.Service/ExtractingEngine.cs
  - tools/Journal.Loader/src/component/engine/extracting/Engine.Extracting.Service/Simulators/ExtractingEngineSimulator.cs
  - tools/Journal.Loader/src/component/engine/converting/Engine.Converting.Service/ConvertingEngine.cs
  - tools/Journal.Loader/src/component/access/cost-accounting/Access.CostAccounting.Orm/Models/RActv.cs
appliesTo: '**/*.{zip,csv,cs,md}'
tags:
  - cdc
  - validation
  - zip
  - csv
  - sql-server
  - journal-loader
---
# CDC File Validation

Agent validates CGI 2137 CDC packages before import. Agent verifies archive structure, file naming, control-file counts, CDC headers, and CDC value types.

## When to Use

| Condition | Use |
|---|---|
| Agent validates a `2137-IDL_SWEEP_YYYYMMDD-HHMMSS.ZIP` package before import | Use this skill |
| Agent investigates a non-compliant CDC package | Use this skill |
| Agent verifies control-file counts against extracted CSV rows | Use this skill |
| Agent verifies CDC metadata columns before conversion or materialization | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Agent enforces idempotent duplicate handling for `(__$start_lsn, __$seqval)` | Use `cdc-uniqueness-enforcement` |
| Agent tunes import throughput or batch commit behavior | Use `cdc-batch-import-patterns` |
| Agent reviews one generic CSV file outside the CGI 2137 CDC contract | Use `csv-ingestion-diagnostics` |
| Agent inventories mixed archive formats without CDC semantics | Use `zip-batch-analysis` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| ZIP package | Yes | Agent reads the archive bytes or file path. |
| CDC file-format contract | Yes | Agent uses `tools/Journal.Loader/docs/specifications/file-format.md`. |
| Expected table list | Yes | Agent verifies one control file and twenty `R_*` data files. |
| Entity schema source | Yes | Agent maps business columns from the ORM model or resolved entity type. |
| Validation sink | No | Agent writes defects to logs, queue storage, or a failed-record store. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent opens the archive with ZIP read semantics and records the package name and package timestamp. | Read the archive entry list. | Archive opens with zero extraction exceptions. |
| 2 | Agent verifies package structure. Agent expects exactly `21` files: `1` control file and `20` CDC data files. | Count extracted entries after filtering empty directory entries. | Entry count equals `21`. Control-file count equals `1`. Data-file count equals `20`. |
| 3 | Agent verifies file naming patterns and timestamp consistency. | Match every entry name against the CDC regex set and compare suffix timestamps. | Every entry name matches the expected pattern and every entry shares one timestamp token. |
| 4 | Agent parses the control file as two-column CSV with no header row. Agent verifies table coverage, duplicate table rows, and integer record counts. | Parse every control row into `(TableName, RecordCount)`. | Control file yields `20` distinct table entries and every `RecordCount` parses as an integer `>= 0`. |
| 5 | Agent maps each control entry to one CDC data file and verifies the row count from the control file against the actual data row count. | Compare `RecordCount` to `data row count excluding header`. | Every data file is present and every count comparison result is recorded. |
| 6 | Agent reads every CDC header row and verifies the required CDC metadata columns in the required leading order. | Compare the first seven header names to the CDC contract. | Header starts with `tran_begin_time`, `__$start_lsn`, `__$end_lsn`, `__$seqval`, `__$operation`, `__$update_mask`, `__$command_id`. |
| 7 | Agent verifies CDC column types and null rules for every row. | Parse timestamps, hex values, and integers on sampled or full rows. | `tran_begin_time` parses as `DateTime`, `__$start_lsn` and `__$seqval` parse as binary hex, `__$operation` is `1` to `4`, `__$command_id` parses as `int`, and required CDC fields are present. |
| 8 | Agent verifies business-column coverage against the target model for the table under review. | Compare CSV business headers to the resolved entity properties or schema map. | Missing required business columns equal `0`, or Agent records the exact missing column set. |
| 9 | Agent classifies defects by scope and routes the outcome. | Review package, file, and row defect totals. | Package-level defects stop package import. File-level defects skip the affected file. Row-level defects enter failed-record handling without hiding the error count. |

## Pattern Matrix

| Concern | Pattern |
|---|---|
| ZIP structure | Agent follows the extraction pattern used in `Engine.Extracting.Service` and validates after extraction instead of trusting file extensions alone. |
| Control-file parsing | Agent parses the control file as headerless CSV with exactly two fields: `TableName` and `RecordCount`. |
| File-name validation | Agent validates `2137-IDL_SWEEP_YYYYMMDD-HHMMSS.ZIP`, `IDL_Crosswalk_CDC_CONTROL_YYYYMMDD-HHMMSS.csv`, and `R_{TABLE}_CDC_YYYYMMDD-HHMMSS.csv` separately. |
| Header validation | Agent treats the first seven columns as a strict ordered prefix instead of a loose contains check. |
| CDC type validation | Agent parses LSN and mask fields as `0x`-prefixed or normalized hex and validates binary length where the contract defines `binary(10)`. |
| Operation validation | Agent accepts only `1`, `2`, `3`, and `4` and records the offending row when a different value appears. |
| Missing-column detection | Agent records the exact missing headers and the owning file name in one defect record. |
| Count mismatch handling | Agent records expected and actual row counts from the control file and the data file in one warning entry. |
| Entity alignment | Agent maps business columns against the resolved model such as `RActv` so file validation stays aligned with import targets. |
| Error handling | Agent separates package reject, file skip, and row failure paths so one defect scope does not erase another scope. |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Archive readability | Open the ZIP and enumerate entries. | Zero archive read failures. |
| Package file count | Count control and data entries. | Exactly `1` control file and exactly `20` data files exist. |
| Timestamp consistency | Extract the timestamp token from every entry name. | Distinct timestamp token count equals `1`. |
| Control-file integrity | Parse all control rows and count distinct tables. | Distinct table count equals `20` and malformed rows equal `0`. |
| Header contract | Compare header prefix to the seven required CDC columns. | Missing CDC columns equal `0` and leading-order mismatches equal `0`. |
| CDC data types | Parse CDC fields on every row or the configured validation scope. | Invalid timestamp, invalid LSN, invalid operation, and invalid command-id counts equal `0`. |
| Missing business columns | Compare header to entity schema map. | Required business-column gaps equal `0` or the gap list is complete. |
| Error routing | Review defect output by scope. | Every defect includes package name, file name, scope, message, and disposition. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Agent trusts `.zip` extension without opening the archive | Agent opens the archive and fails validation on read exceptions. |
| Agent counts the header row as data rows | Agent subtracts the header row from the actual data-row count. |
| Agent accepts the CDC columns in any order | Agent enforces the required seven-column prefix order. |
| Agent omits `__$command_id` from validation because older code paths list six required columns | Agent validates all seven CDC metadata columns from the file-format contract. |
| Agent treats `__$end_lsn` or `__$update_mask` as always required | Agent accepts nullable values only where the contract allows null. |
| Agent rejects the entire package for one bad row | Agent records the row defect, routes it to failed-record handling, and preserves package-level status separately. |
