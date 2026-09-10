---
name: ledger-balance-anomaly
title: Ledger Balance Anomaly Detection
description: Detect ledger balance defects in file extracts and report counted findings with severity, row scope, and next actions.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1380
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - transaction-duplication-detection
appliesTo: '**/*.{csv,tsv,xlsx,parquet,md}'
tags:
  - ledger
  - anomaly
  - finance
  - balance
---
# Ledger Balance Anomaly Detection

Agent uses this skill to detect ledger balance defects in trial balance and journal export files.

## When to Use

| User prompt | Use |
|---|---|
| User asks to find ledger balance defects | Agent uses this skill |
| User asks to review debit and credit totals by period | Agent uses this skill |
| User asks to find running balance breaks or sign flips | Agent uses this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to find duplicate payments or invoices | Agent routes to `transaction-duplication-detection` |
| User asks to infer XML structure | Agent routes to `xml-schema-inference` |
| User asks accounting policy questions without file review | Agent answers directly |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Ledger file | Yes | Agent reads `.csv`, `.tsv`, `.xlsx`, or `.parquet`. |
| Period column | No | Agent treats one file as one period when the column is absent. |
| Currency column | No | Agent groups by currency when the column exists. |
| Tolerance | No | Agent uses `0.01` when the user omits a value. |

## Anomaly Pattern Table

| Pattern | Trigger | Severity | Test | Pass |
|---|---|---|---|---|
| Period imbalance | `abs(sum(debit) - sum(credit)) > tolerance` | High | Run grouped total scan by period. | Every flagged period has non-zero imbalance. |
| Running balance break | `abs(expected_balance - actual_balance) > tolerance` | High | Run row comparison by account and period. | Every flagged row has expected and actual balances. |
| Negative amount defect | `debit < 0` or `credit < 0` and `reversal_flag != true` | High | Run sign scan on debit and credit columns. | Every flagged row is not a reversal row. |
| Unexpected zero balance | `balance = 0` and prior period balance != `0` | Moderate | Run prior-period comparison by account. | Every flagged row has a non-zero prior balance. |
| Sign flip | `sign(balance)` changes and `reversal_flag != true` | Moderate | Run sign comparison by account. | Every flagged row shows a sign change. |
| Balance outlier | `z_score >= 3` | Moderate | Run period-end z-score scan by account. | Every flagged row has `z_score >= 3`. |
| Early outlier | `2 <= z_score < 3` | Info | Run period-end z-score scan by account. | Every flagged row has `2 <= z_score < 3`. |

## Column Mapping Table

| Standard column | Accepted aliases |
|---|---|
| `account_code` | `account`, `acct`, `gl_account`, `account_number` |
| `period` | `period`, `fiscal_period`, `month`, `posting_period` |
| `debit` | `debit`, `dr`, `amt_dr`, `debit_amount` |
| `credit` | `credit`, `cr`, `amt_cr`, `credit_amount` |
| `balance` | `balance`, `running_balance`, `closing_balance` |
| `reversal_flag` | `reversal`, `is_reversal`, `rev_flag` |
| `currency` | `currency`, `currency_code` |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads the file and maps columns with the column mapping table. | Read the header row. | Agent maps `debit` and `credit`, or Agent stops with a missing-column note. |
| 2 | Agent groups rows by period and currency. | Run grouped totals. | Every group has debit total, credit total, and imbalance value. |
| 3 | Agent flags period imbalance rows. | Run `abs(sum(debit) - sum(credit)) > tolerance`. | Every flagged group matches the pattern table. |
| 4 | Agent sorts rows by account and period, then computes expected balance. | Run running balance formula on each row. | Every flagged row includes prior balance, debit, credit, expected, and actual. |
| 5 | Agent scans for negative amounts, zero balances, and sign flips. | Run row-level pattern scans. | Every flagged row maps to one pattern. |
| 6 | Agent scans period-end balances for z-score outliers. | Run z-score scan by account. | Every flagged row includes z-score and severity. |
| 7 | Agent generates a result table with counts by severity and pattern. | Read the final report. | Report lists file name, row count, pattern counts, and row scope. |

## Output Contract

| Field | Agent action |
|---|---|
| Summary | Agent reports total rows, periods, accounts, and anomaly counts by severity. |
| Detail rows | Agent reports `account_code`, `period`, `pattern`, `severity`, and row identifier. |
| Metrics | Agent reports debit total, credit total, imbalance, expected balance, actual balance, and z-score when present. |
| Next action | Agent reports one finance action per high-severity pattern. |

## Verification

| Scope | Test | Pass |
|---|---|---|
| Modal verbs | Run modal-verb scan command. | Scan returns zero banned-term matches. |
| Front matter | Run `npm run frontmatter:validate`. | Command returns zero validation errors. |
| Pattern coverage | Read the anomaly pattern table and workflow table. | Every workflow step maps to one pattern or one output field. |
| Output clarity | Read the output contract. | Every output row includes severity and row scope. |
