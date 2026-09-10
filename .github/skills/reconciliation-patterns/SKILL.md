---
name: reconciliation-patterns
title: Reconciliation Patterns
description: Apply reconciliation workflows, discrepancy controls, and .NET matching patterns when ledger agreement or settlement validation is in scope.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1692
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - double-entry-accounting
  - ledger-balance-anomaly
  - repository-unitofwork-efcore
  - cqrs-patterns-dotnet
appliesTo: '**/*.{cs,csproj,sql,md}'
tags:
  - accounting
  - reconciliation
  - matching
  - finance
  - dotnet
---
# Reconciliation Patterns

Apply reconciliation workflows that align external evidence, subsidiary detail, and general-ledger balances.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Bank statements need comparison to cash ledger activity | Use this skill |
| Subsidiary-ledger, intercompany, or three-way match defects are in scope | Use this skill |
| Work item needs discrepancy classification and resolution workflow | Use this skill |
| .NET services need automated and manual reconciliation orchestration | Use this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Work item covers journal approval and posting lifecycle only | Use `journal-entry-patterns` |
| Work item covers close procedures and year-end sequencing only | Use `closing-cycle-patterns` |
| Work item covers audit evidence and segregation only | Use `audit-trail-compliance` |
| Work item covers raw double-entry fundamentals only | Use `double-entry-accounting` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source systems | Yes | Record bank, subledger, AP, AR, inventory, PO, receipt, and invoice sources in scope. |
| Match keys | Yes | Record amount, date, reference, vendor, entity, and tolerance rules. |
| Reconciliation period | Yes | Record statement period or fiscal period boundary. |
| Variance policy | Yes | Record materiality threshold and escalation path. |
| Ownership model | No | Record preparer, reviewer, and resolver roles when workflow controls exist. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Normalize source rows into shared match keys and period scope. | Every row carries comparable keys and period data. |
| 2 | Run exact matching before tolerance matching. | Exact matches settle before approximate rules apply. |
| 3 | Classify unmatched rows into timing, data-quality, duplicate, missing, or amount-variance cases. | Every open item has one primary category. |
| 4 | Route cases to manual review or automated resolution with owners and due state. | Each discrepancy has ownership and next action. |
| 5 | Publish matched, unmatched, aging, and adjusted totals and verify deterministic reruns. | Reports tie to the underlying populations and reruns reproduce the same result. |


## Reconciliation Scope Table

| Type | Evidence | Pass |
|---|---|---|
| Bank reconciliation | Statement lines, cash ledger, prior outstanding items | Adjusted book and adjusted bank balances tie. |
| Subsidiary to general ledger | Subsidiary detail and control-account balance | Detail equals the control-account ending balance. |
| Intercompany reconciliation | Entity-pair activity and translation basis | Due-to and due-from balances net to zero after approved rules. |
| Three-way match | Purchase order, receipt, and invoice | Payment approval occurs only for matched or approved-exception records. |
| Manual reconciliation | Case record, evidence, owner, and notes | Open items keep owner, age, and next action. |
| Automated reconciliation | Rule set, run log, and result queue | The same input set produces the same output. |


## Discrepancy Pattern Table

| Category | Resolution pattern | Pass |
|---|---|---|
| Outstanding check or deposit in transit | Carry forward until clearance, void, or aging escalation. | The item clears later or escalates by policy. |
| Timing difference | Preserve period note and close after crossover. | No correction entry is needed for valid adjacent-period timing. |
| Amount variance | Route for correction, dispute, or approved write-off. | Resolution code matches tolerance policy. |
| Missing transaction | Open investigation or journal action. | Missing items gain owner and root-cause code. |
| Duplicate activity | Reverse the duplicate or suppress the duplicate import. | Net activity reflects one valid event. |


## .NET Implementation Guidance

| Concern | Pattern |
|---|---|
| Domain model | `ReconciliationRun`, `MatchCandidate`, `DiscrepancyCase`, and `ResolutionAction` records |
| Matching engine | Deterministic exact, tolerance, and manual-review stages |
| Persistence | Normalized source snapshots, match results, evidence links, and resolver notes |
| Reporting | Matched totals, unresolved aging, and category counts through read-side DTOs |
| Idempotency | Import and run keys based on source period and statement or file reference |
| Observability | Rule counts, exception counts, and aging buckets per run |


## Integration Hooks

| Hook | Pattern |
|---|---|
| EF Core | Reconciliation runs, source rows, cases, and notes with concurrency control |
| Domain events | `ReconciliationCompleted`, `DiscrepancyOpened`, and `DiscrepancyResolved` |
| CQRS | Commands for import, run, assign, resolve, and reopen; queries for reports and aging |
| Scheduling | Automated runs tied to scheduler or close-calendar cadence |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter and STE wording | Run front-matter validation and the STE scan. | Validation passes and banned wording stays absent. |
| Match determinism and tie-out | Re-run the same source set and recalculate bank or subledger tie-outs. | Results repeat exactly and balances tie within configured precision. |
| Case ownership | Review unresolved discrepancy cases. | Every open case has owner, category, and age. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Tolerance match runs before exact match | Agent orders match stages from exact to approximate. |
| Unmatched rows lack one primary category | Enforce category assignment at case creation. |
| Reconciliation report omits carried timing items | Keep open-item aging and carry-forward detail. |
| Three-way match ignores quantity variance | Agent validates quantity, price, and term fields separately. |
| Import replay duplicates source evidence | Enforce idempotent import keys and source hashes. |

## Outputs

- Reconciliation workflow and scope matrices
- Discrepancy classification and resolution guidance
- Bank, subledger, and three-way-match tie-out patterns
- .NET persistence, CQRS, and scheduling hooks

