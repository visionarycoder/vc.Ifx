---
name: transaction-duplication-detection
title: Transaction Duplication Detection
description: Detect exact and near duplicate transaction defects in finance files and report counted matches with risk and row scope.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1385
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ledger-balance-anomaly
appliesTo: '**/*.{csv,tsv,xlsx,parquet,md}'
tags:
  - transaction
  - duplication
  - finance
  - anomaly
---
# Transaction Duplication Detection

Agent uses this skill to detect exact and near duplicate transaction defects in payment and ledger files.

## When to Use

| User prompt | Use |
|---|---|
| User asks to find duplicate payments, invoices, or journal rows | Agent uses this skill |
| User asks to compare two files for duplicate transactions | Agent uses this skill |
| User asks to find repeated transaction IDs with conflicting amounts | Agent uses this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to find debit and credit imbalance | Agent routes to `ledger-balance-anomaly` |
| User asks to infer XML structure or XSD | Agent routes to `xml-schema-inference` |
| User asks to delete duplicates from source files | Agent reports findings only |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Transaction file | Yes | Agent reads `.csv`, `.tsv`, `.xlsx`, or `.parquet`. |
| Second transaction file | No | Agent runs cross-file matching when a second file exists. |
| Amount tolerance | No | Agent uses `0.00` when the user omits a value. |
| Time window | No | Agent uses `7` days when the user omits a value. |

## Duplication Pattern Table

| Pattern | Trigger | Risk | Test | Pass |
|---|---|---|---|---|
| Exact duplicate | All key columns match | High | Run group-by on key columns. | Every flagged group has row count `> 1`. |
| Transaction ID conflict | Same `transaction_id` and different amount or date | High | Run group-by on `transaction_id`. | Every flagged group has at least one conflicting value. |
| Near duplicate | Same vendor, amount delta `<= tolerance`, and date delta `<= window` | Moderate | Run vendor, amount, and date comparison. | Every flagged pair meets all thresholds. |
| Amount-only repeat | Same amount and different vendor | Info | Run amount comparison after vendor mismatch filter. | Every flagged pair has vendor mismatch. |
| Cross-file exact duplicate | All key columns match across two files | High | Run exact match between file A and file B. | Every flagged pair includes row scope from both files. |
| Cross-file near duplicate | Same vendor, amount delta `<= tolerance`, and date delta `<= window` across two files | Moderate | Run near-match between file A and file B. | Every flagged pair includes thresholds and row scope from both files. |

## Key Column Table

| Standard column | Accepted aliases |
|---|---|
| `transaction_id` | `txn_id`, `trans_id`, `invoice_number`, `ref_no` |
| `date` | `date`, `transaction_date`, `posting_date`, `invoice_date` |
| `amount` | `amount`, `amt`, `total`, `gross_amount`, `net_amount` |
| `vendor` | `vendor`, `supplier`, `payee`, `vendor_name` |
| `account` | `account`, `gl_account`, `cost_center` |
| `currency` | `currency`, `currency_code` |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads file headers and maps key columns. | Read header rows. | Agent maps `amount` and one date or ID column, or Agent stops with a missing-column note. |
| 2 | Agent normalizes vendor text with lowercase, punctuation trim, and whitespace collapse. | Read normalized sample rows. | Equal vendor names have equal normalized values. |
| 3 | Agent runs exact duplicate grouping on present key columns. | Run group-by scan. | Every flagged group has a duplicate count and key values. |
| 4 | Agent runs transaction ID conflict scan when `transaction_id` exists. | Run `transaction_id` group comparison. | Every flagged group has amount or date conflict. |
| 5 | Agent runs near duplicate comparison with amount and date thresholds. | Run pair scan on normalized vendor, amount, and date. | Every flagged pair meets the threshold values. |
| 6 | Agent runs cross-file scans when file B exists. | Run file A to file B comparisons. | Every flagged pair includes file name and row scope from both files. |
| 7 | Agent generates a result table with counts by pattern and risk. | Read the final report. | Report lists total rows, pattern counts, and amount at risk. |

## Decision Table

| Condition | Agent action |
|---|---|
| `transaction_id` exists | Agent runs ID conflict scan. |
| Two files exist | Agent runs cross-file exact and near scans. |
| `currency` exists | Agent groups comparisons by currency. |
| Duplicate already matches exact pattern | Agent excludes that row pair from near duplicate counts. |

## Output Contract

| Field | Agent action |
|---|---|
| Summary | Agent reports total rows, duplicate groups, pair counts, and amount at risk. |
| Detail rows | Agent reports `pattern`, `risk`, row scope, file scope, and key values. |
| Thresholds | Agent reports time window and amount tolerance. |
| Next action | Agent reports one finance action per high-risk pattern. |

## Verification

| Scope | Test | Pass |
|---|---|---|
| Modal verbs | Run modal-verb scan command. | Scan returns zero banned-term matches. |
| Front matter | Run `npm run frontmatter:validate`. | Command returns zero validation errors. |
| Pattern coverage | Read the duplication pattern table and workflow table. | Every workflow step maps to one pattern or one output field. |
| Output clarity | Read the output contract. | Every detail row includes risk and row scope. |
