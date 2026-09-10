---
name: subledger-reconciliation-patterns
title: Subledger Reconciliation Patterns
description: Reconcile subsidiary-ledger summaries to GL control accounts when AR, AP, payroll, or fixed-asset control validation is in scope.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1910
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ledger-balance-anomaly
  - double-entry-accounting
  - audit-trail-compliance
appliesTo: '**/*.{cs,csproj,sql,md}'
tags:
  - reconciliation
  - subledger
  - gl-control
  - audit
  - accounting
---
# Subledger Reconciliation Patterns

Agent reconciles subsidiary-ledger summaries to general-ledger control accounts and produces discrepancy evidence, ownership, and sign-off artifacts.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| AR customer balances need tie-out to one or more receivable control accounts | Agent uses this skill |
| AP vendor balances need tie-out to trade payables or accrued-liability controls | Agent uses this skill |
| Payroll liability and expense summaries need comparison to wage, tax, or benefit control accounts | Agent uses this skill |
| Fixed-asset subsidiary balances need comparison to cost, accumulated depreciation, or CIP controls | Agent uses this skill |
| Close workflow needs preparer-reviewer evidence for subledger-to-GL certification | Agent uses this skill |
| .NET services need deterministic reconciliation logic, break classification, and alerting | Agent uses this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Work item covers journal balancing fundamentals only | Agent uses `double-entry-accounting` |
| Work item covers generic bank or external-party reconciliation | Agent uses `reconciliation-patterns` |
| Work item covers anomaly detection in flat ledger extracts without control-account tie-out | Agent uses `ledger-balance-anomaly` |
| Work item covers audit evidence design without reconciliation logic | Agent uses `audit-trail-compliance` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Reconciliation period | Yes | Agent records fiscal year, fiscal period, close status, and cutoff timestamp. |
| Subledger source set | Yes | Agent records source tables, filters, posting states, and currency scope. |
| GL control account map | Yes | Agent records control-account numbers, entity scope, and normal-balance rules. |
| Aggregation grain | Yes | Agent records account, legal entity, fund, department, location, or business-unit grain. |
| Tolerance policy | Yes | Agent records absolute amount, percentage, and count-based thresholds. |
| Ownership model | No | Agent records preparer, reviewer, approver, and resolver identities when workflow controls exist. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Extract subledger summary | Agent extracts period-end subledger balances with status filters, currency rules, and grouping keys. | Compare source query totals to subledger control reports. | Extract totals agree with source-of-record totals for the same cutoff. |
| 2. Extract GL control account balances | Agent extracts ending balances for mapped control accounts at the same cutoff and grouping grain. | Compare query output to trial balance or ledger report totals. | GL extract agrees with the period-end control-account report. |
| 3. Compare and identify breaks | Agent compares subledger and GL totals by entity, fund, currency, and control-account map. | Calculate difference, difference percent, and status per grouping key. | Every grouping key lands in matched, tolerance-match, or break status. |
| 4. Investigate and resolve discrepancies | Agent classifies breaks into timing, mapping, posting, duplication, omission, currency, or valuation categories and records next action. | Review open break register. | Every break has one category, one owner, one evidence path, and one target resolution date. |
| 5. Document evidence and sign-off | Agent stores extracts, calculation inputs, reviewer notes, adjustment references, and certification outcome. | Review evidence package and sign-off record. | Package supports independent re-performance and sign-off state is traceable. |

## Subledger Type Decision Matrix

| Subledger Type | Primary Subledger Evidence | Primary GL Controls | Break Drivers | Resolution Focus | Pass |
|---|---|---|---|---|---|
| Accounts Receivable | Open invoices, credit memos, unapplied cash, aging buckets | Trade AR, allowance, unbilled receivables | Unposted cash, customer mapping defects, allowance journal timing, duplicate invoice imports | Repost cash, correct customer-account mapping, post allowance adjustment, reverse duplicates | Customer aging total and receivable control tie at cutoff. |
| Accounts Payable | Open vouchers, vendor credits, hold items, unmatched receipts | Trade AP, accrued liabilities, GRNI | Invoice holds, receipt accrual timing, vendor merge defects, duplicate voucher loads | Release or clear holds, post receipt accruals, correct vendor master links, reverse duplicate vouchers | Vendor liability total and payable control tie at cutoff. |
| Payroll | Payroll register, tax withholdings, benefit accruals, net-pay clearing | Wages payable, tax payable, benefit payable, payroll clearing | Off-cycle payroll timing, tax remittance lag, labor distribution defects, stale clearing items | Post distribution adjustments, clear remittances, correct earning-code maps, investigate stale checks | Payroll liability schedule ties to mapped control accounts and stale items have owners. |
| Fixed Assets | Asset register, depreciation run, disposal log, CIP detail | Asset cost, accumulated depreciation, depreciation expense clearing, CIP | Depreciation run gaps, disposal posting defects, capitalization timing, asset-class mapping defects | Re-run depreciation, post disposal journals, correct asset categories, transfer CIP to in-service assets | Asset register balances tie to cost and accumulated-depreciation controls. |

## Break Detection Pattern Table

| Pattern | Trigger | Investigation Focus | Test | Pass |
|---|---|---|---|---|
| Timing break | Activity posted in one ledger before the paired ledger at cutoff | Adjacent-period postings, batch timestamps, interface schedule | Review posting timestamps around cutoff. | Offset appears in the next valid period or gains an approved adjustment. |
| Missing-posting break | Source activity exists in one ledger and no paired impact exists in the other | Failed posting jobs, rejected batches, suspended transactions | Compare source document counts to posted document counts. | Missing population gains repost, correction, or approved exception. |
| Duplicate-impact break | Same business event hits one ledger twice or both ledgers inconsistently | Import replay, duplicate batch keys, manual re-entry | Group by source reference and amount. | Net impact reflects one valid event per source reference. |
| Mapping break | Valid activity lands in the wrong GL control account or omitted map | Customer, vendor, earning-code, asset-category, or entity map | Compare source segment map to control-account map. | Every source segment resolves to one approved control account. |
| Currency break | Subledger and GL use inconsistent rates, currencies, or translation dates | Functional currency, revaluation, FX journal sequence | Recalculate translated balances by currency and rate date. | Difference traces to approved FX logic or closes through adjustment. |
| Valuation break | Allowance, reserve, depreciation, or accrual basis differs between ledgers | Aging reserve model, depreciation method, accrual rule | Recalculate valuation schedule from detailed population. | Valuation method and posted balance agree for the same cutoff. |
| Cutoff break | Backdated, future-dated, or partially posted activity crosses period boundary | Posting date, effective date, service date, asset in-service date | Compare document dates to ledger dates. | Every variance traces to documented cutoff policy or correction. |
| Sign or polarity break | Debit-credit direction in one ledger disagrees with the control-account normal balance | Reversal logic, credit memo logic, disposal logic | Review signed balances and source transaction types. | Signed balances follow approved polarity rules. |

## Investigation and Resolution Matrix

| Break Category | Agent Action | Evidence | Pass |
|---|---|---|---|
| Timing | Agent carries forward the item with next-period trace and reviewer note. | Subsequent posting, batch id, and cutoff explanation. | Item clears in the expected subsequent period. |
| Posting failure | Agent opens a resolver case tied to failed batch or rejected document population. | Error log, batch id, source ids, and repost reference. | Repost or correction closes the population. |
| Mapping defect | Agent records the defective crosswalk and posts corrective reclassification when needed. | Old map, new map, impacted documents, and approval note. | Corrective posting and map update remove the break on rerun. |
| Duplicate activity | Agent identifies duplicate source keys and reverses the extra ledger impact. | Duplicate key evidence and reversal reference. | One valid financial impact remains. |
| Valuation difference | Agent recalculates reserve, depreciation, or accrual logic and records the approved basis. | Calculation workbook or query output plus approval note. | Valuation schedule matches posted balance within tolerance. |
| Unexplained break | Agent escalates to reviewer with open status, aging, and blocker note. | Investigation log and unresolved-item certification. | Reviewer accepts the open-item disposition or routes further action. |

## .NET Implementation Surface

| Concern | Pattern |
|---|---|
| Domain model | `ReconciliationRun`, `ReconciliationScope`, `BalanceSnapshot`, `BreakCase`, `BreakEvidence`, and `SignOffRecord` aggregates |
| Query layer | Separate subledger extract and GL extract query services with deterministic cutoff filters |
| Matching logic | Exact compare first, tolerance compare second, break classification third |
| Configuration | Typed options for control-account maps, tolerance thresholds, subledger filters, and alert routing |
| Audit trail | Immutable run ids, source hashes, generated timestamps, user identity, and reviewer disposition |
| Scheduling | Period-close jobs and ad hoc rerun commands keyed by reconciliation scope and cutoff |

## Outputs

| Output | Description |
|---|---|
| Reconciliation summary | One row per scope with subledger total, GL total, difference, and status |
| Break register | One row per unresolved or tolerance-exceeded break with category, owner, age, and evidence path |
| Evidence package | Stored extracts, filter definitions, configuration snapshot, reviewer notes, and sign-off outcome |
| Adjustment list | Journal references or corrective action items tied to break cases |
| Certification result | Prepared, reviewed, approved, or rejected status with named actors and timestamps |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate`. | Validation returns zero errors. |
| STE wording | Run the repository STE scan against the new skill and reference files. | Scan returns zero banned-term matches. |
| Required headings | Search for required sections in `SKILL.md`. | All required sections exist. |
| Workflow coverage | Compare workflow steps to output and break tables. | Each workflow step maps to at least one output or break pattern. |
| Scope coverage | Review the subledger type decision matrix. | AR, AP, payroll, and fixed assets each have evidence, controls, break drivers, and pass criteria. |
| Reference linkage | Read related reference front matter and `related_docs`. | Reference points to `../SKILL.md`. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Subledger extract and GL extract use different cutoff timestamps | Agent stores one cutoff definition and reuses it across both queries. |
| Reconciliation compares different grouping grains | Agent aligns entity, fund, currency, and control-account grain before compare logic. |
| Tolerance rules hide material count defects | Agent evaluates count variance and amount variance separately. |
| Break register lacks one primary owner | Agent assigns owner at case creation and records escalation path. |
| Manual adjustments lack tie-back to reconciliation evidence | Agent links each adjustment reference to the originating break case and run id. |
| Payroll clearing accounts stay open across periods without aging logic | Agent adds stale-item aging and reviewer certification. |
| Fixed-asset reconciliation skips disposals or CIP transfers | Agent includes disposal and CIP populations in the extract contract. |

## Reference Files

| File | Purpose |
|---|---|
| `references/reconciliation-implementation.md` | Detailed .NET implementation, EF Core query patterns, alerting, audit trail, and sample subledger code |
