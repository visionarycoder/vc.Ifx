---
name: closing-cycle-patterns
title: Closing Cycle Patterns
description: Apply period-close and year-end close workflows, adjusting entry patterns, and .NET orchestration guidance when financial close processes are in scope.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1677
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - double-entry-accounting
  - journal-entry-patterns
  - quartz-scheduling-patterns
  - domain-events-dotnet
appliesTo: '**/*.{cs,csproj,sql,md}'
tags:
  - accounting
  - close
  - accruals
  - year-end
  - dotnet
---
# Closing Cycle Patterns

Apply period-close and year-end close controls, adjusting entry patterns, and close-calendar orchestration.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Period-close checklist, close calendar, or deadline workflow is in scope | Use this skill |
| Accruals, deferrals, depreciation, amortization, or closing entries are in scope | Use this skill |
| Year-end close and financial statement preparation are in scope | Use this skill |
| .NET workflow orchestration is needed for recurring close tasks and approvals | Use this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Work item covers journal entry lifecycle only | Use `journal-entry-patterns` |
| Work item covers reconciliation case handling only | Use `reconciliation-patterns` |
| Work item covers control evidence and access policy only | Use `audit-trail-compliance` |
| Work item covers account-type rules and ledger design only | Use `double-entry-accounting` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Close calendar | Yes | Record period open date, soft-close date, hard-close date, and report deadline. |
| Adjusting entry catalog | Yes | Record accrual, deferral, depreciation, amortization, reclass, and close-entry patterns. |
| Ownership matrix | Yes | Record preparer, reviewer, approver, and dependency owner for each close task. |
| Financial statement scope | Yes | Record balance sheet, income statement, cash flow, and supporting schedules in scope. |
| Year-end policy | No | Record retained-earnings transfer and audit support rules when year-end work is in scope. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Open the close run with task owners, dependencies, and deadlines. | Every task has ownership and dependency state. |
| 2 | Post standard accrual, deferral, depreciation, amortization, and reclass entries. | Generated entries map to approved close patterns. |
| 3 | Finish reconciliations and variance review before hard close. | Blocking reconciliations close before period lock. |
| 4 | Lock routine posting, prepare statements, and run year-end closing entries when in scope. | Final statements tie to ledger balances and temporary accounts zero at year end. |
| 5 | Record evidence, completion state, and controlled reopen paths. | The close run keeps timestamps, artifacts, and final status. |


## Close Procedure Table

| Area | Pattern | Pass |
|---|---|---|
| Soft close | Gather preliminary balances, open items, and early variance review. | Preliminary statements and unresolved-items lists exist. |
| Hard close | Restrict routine posting, complete final adjustments, and freeze period output. | No routine posting enters the closed period. |
| Checklist | Track dependencies, owners, blockers, and evidence. | Task status stays current. |
| Statement preparation | Derive statements and support schedules from final balances. | Statements tie to ledger and schedules. |
| Reopen governance | Record reason, approver, scope, and new close state. | Reopened periods carry explicit approval trace. |


## Adjusting and Closing Entry Table

| Entry pattern | Timing | Pass |
|---|---|---|
| Accrued expense or revenue | Cost or revenue is incurred before cash movement. | Recognition lands in the correct period. |
| Prepaid expense or deferred revenue | Cash timing differs from recognition timing. | Unexpired or unearned amounts remain on the balance sheet. |
| Depreciation or amortization | Period-end asset and intangible close step. | Expense and carrying-value schedules tie out. |
| Closing entry | Year-end close. | Temporary accounts zero and retained earnings updates. |


## .NET Implementation Guidance

| Concern | Pattern |
|---|---|
| Workflow model | `CloseRun`, `CloseTask`, `CloseDependency`, `CloseEvidence`, and `CloseException` records |
| Automation | Deterministic close services and adjusting-entry generators |
| Period control | `AccountingPeriod` states for open, soft-close, hard-close, and reopened |
| Reporting | Read-side DTOs sourced from final ledger balances |
| Scheduling | Triggers, reminders, and escalations tied to the close calendar |
| Recovery | Idempotent rerun keys and restart boundaries |


## Integration Hooks

| Hook | Pattern |
|---|---|
| EF Core | Close runs, period state, evidence, and generated entries with concurrency tokens |
| Domain events | `CloseRunStarted`, `CloseTaskCompleted`, `PeriodHardClosed`, and `YearEndClosed` |
| CQRS | Commands for close actions and queries for calendars, dashboards, and statements |
| Scheduler integration | Close milestones, reminders, and escalations |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter and STE wording | Run front-matter validation and the STE scan. | Validation passes and banned wording stays absent. |
| Adjustments and hard-close control | Compare generated entries to source schedules and attempt routine posting into a hard-closed period. | Entries tie to approved sources and hard-close blocks routine posting. |
| Year-end close | Recalculate temporary accounts and retained earnings. | Temporary accounts zero and retained earnings reflects net activity. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Hard close occurs before reconciliation completion | Agent marks reconciliation completion as a blocking dependency. |
| Adjusting entries lack source schedule linkage | Store evidence reference and schedule id on generated entries. |
| Deferred revenue and prepaid expense schedules share one rule path | Separate liability and asset recognition logic. |
| Year-end close edits prior statements in place | Agent preserves prior close artifacts and writes a controlled reopen or correction path. |
| Close calendar omits ownership and due markers | Store owner, due point, and escalation rule for each task. |

## Outputs

- Close procedure matrix
- Adjusting-entry and year-end-close guidance
- Close-calendar orchestration hooks
- Financial-statement preparation checks

