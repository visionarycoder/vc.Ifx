---
name: journal-entry-patterns
title: Journal Entry Patterns
description: Apply journal entry lifecycle, approval, reversal, and audit patterns when accounting posting workflows or entry-domain models are in scope.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1690
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - double-entry-accounting
  - audit-trail-compliance
  - cqrs-patterns-dotnet
  - domain-events-dotnet
appliesTo: '**/*.{cs,csproj,sql,md}'
tags:
  - accounting
  - journal-entry
  - workflow
  - approval
  - dotnet
---
# Journal Entry Patterns

Apply journal entry lifecycle controls, entry templates, and posting workflow rules for accounting-domain work.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Journal entries move through draft, review, approval, and posting states | Use this skill |
| Work item needs reversing, correcting, or adjusting entry behavior | Use this skill |
| Batch posting or template-driven entry creation is in scope | Use this skill |
| .NET services need journal entry aggregates and audit-friendly state transitions | Use this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Work item covers account-type rules and trial balance fundamentals only | Use `double-entry-accounting` |
| Work item covers reconciliation variance handling only | Use `reconciliation-patterns` |
| Work item covers close calendar and year-end sequencing only | Use `closing-cycle-patterns` |
| Work item covers access control evidence and segregation controls only | Use `audit-trail-compliance` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entry source policy | Yes | Record manual, import, template, accrual, and system-generated sources. |
| Approval policy | Yes | Record approver roles, thresholds, and separation rules. |
| Posting period policy | Yes | Record open-period and closed-period behavior. |
| Reversal policy | Yes | Record auto-reversal timing and reversal linkage fields. |
| Template catalog | No | Record template code, fixed lines, and variable dimensions when present. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Create draft entries with balanced lines, source reference, and preparer metadata. | Draft entries keep header, line, and preparer trace data. |
| 2 | Validate account status, period status, currency rules, and balance before review. | Invalid drafts stop before promotion. |
| 3 | Promote valid drafts to approval with immutable review snapshot data. | Pending state records reviewer context and timestamp. |
| 4 | Apply approval hierarchy and segregation checks, then post approved entries once. | Approval and posting follow policy and ledger impact is durable. |
| 5 | Handle reversal, correction, adjustment, recurrence, template, and batch flows through explicit entry types. | Each flow preserves traceability and balance. |


## Lifecycle Pattern Table

| State | Exit rule | Pass |
|---|---|---|
| Draft | Validation passes and the preparer submits. | Snapshot is complete before review. |
| Pending | Required approver accepts or rejects. | Financial lines stay unchanged while pending. |
| Approved | Posting service runs in an open period. | Approval evidence stays attached. |
| Posted | Only correction or reversal creates follow-up activity. | Original posted entry stays immutable. |
| Rejected | Preparer edits or clones into a new review cycle. | Rejection reason stays attached to the reviewed draft. |


## Entry Pattern Table

| Entry type | Trigger | Pass |
|---|---|---|
| Reversing entry | Accrual reversal date arrives. | The reversal offsets the source entry by account and amount. |
| Correcting entry | A posted entry contains an error. | The correction references the source and net balance matches intent. |
| Adjusting entry | Period-close event occurs. | Purpose and period controls stay explicit. |
| Recurring entry | Schedule reaches the next run date. | Generated drafts match template and schedule metadata. |
| Template entry | User or system selects a template. | Rendered drafts validate and balance after parameter fill. |
| Batch entry | Import or mass-entry run starts. | Batch totals reconcile to contained entries. |


## .NET Implementation Guidance

| Concern | Pattern |
|---|---|
| Aggregate shape | `JournalEntry` with status, source, posting, approval, and linked-entry metadata |
| State control | Domain methods such as `Submit`, `Approve`, `Reject`, `Post`, `Reverse`, and `Correct` |
| Batch processing | `JournalBatch` orchestration with separate entry aggregates |
| Scheduling | Recurrence rules beside template identity and next-run date |
| Persistence | Immutable posted-line snapshots and linked correction or reversal references |
| Auditability | Transition history with actor, timestamp, prior state, new state, and reason code |


## Integration Hooks

| Hook | Pattern |
|---|---|
| EF Core | Entry status, approval metadata, linked entries, and batch ownership with concurrency tokens |
| Domain events | `JournalEntrySubmitted`, `JournalEntryApproved`, `JournalEntryRejected`, and `JournalEntryPosted` |
| CQRS | Commands for create, submit, approve, reject, post, reverse, and correct; queries for status and history |
| Result modeling | Explicit validation and policy failure codes for rejected transitions |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter and STE wording | Run front-matter validation and the STE scan. | Validation passes and banned wording stays absent. |
| Lifecycle and immutability | Run status-transition tests and inspect posted-entry edit paths. | Invalid transitions fail and posted content changes only through linked corrective flows. |
| Batch integrity | Compare batch totals to contained entry totals. | Totals match within configured precision. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Approval occurs after posting | Enforce approval state before posting state. |
| Reversal overwrites the original accrual | Agent creates a new linked reversing entry and preserves the source entry. |
| Correction edits a posted row in place | Agent writes a new correcting entry with source linkage. |
| Batch failure hides partial posting outcome | Store entry-level posting result and batch-level summary separately. |
| Template catalog omits dimension validation | Agent validates cost center, entity, and account parameters during template rendering. |

## Outputs

- Journal entry lifecycle model
- Entry-type and approval-pattern guidance
- Batch, template, and recurrence orchestration hooks
- Verification steps for transitions, immutability, and totals

