---
name: double-entry-accounting
title: Double-Entry Accounting
description: Apply double-entry bookkeeping rules and .NET ledger design when journal posting, ledger modeling, or trial balance workflows are in scope.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1650
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ledger-balance-anomaly
  - cqrs-patterns-dotnet
  - domain-events-dotnet
  - repository-unitofwork-efcore
appliesTo: '**/*.{cs,csproj,sql,md}'
tags:
  - accounting
  - ledger
  - journal
  - finance
  - dotnet
---
# Double-Entry Accounting

Agent applies foundational double-entry rules, ledger structure, and posting controls for accounting-domain work.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Ledger entries need balanced debit and credit lines | Agent uses this skill |
| Domain models need chart-of-accounts, journal line, or general-ledger structure | Agent uses this skill |
| Trial balance or posting validation rules are in scope | Agent uses this skill |
| .NET services need deterministic debit-credit posting behavior | Agent uses this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Work item covers approval states or posting workflow only | Agent uses `journal-entry-patterns` |
| Work item covers reconciliation investigation only | Agent uses `reconciliation-patterns` |
| Work item covers period close orchestration only | Agent uses `closing-cycle-patterns` |
| Work item covers audit evidence or segregation controls only | Agent uses `audit-trail-compliance` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Accounting equation policy | Yes | Agent confirms assets equal liabilities plus equity after posting. |
| Account catalog | Yes | Agent maps account code, account type, and normal balance. |
| Posting grain | Yes | Agent records header, line, currency, and effective date grain. |
| Precision rule | Yes | Agent records decimal precision and rounding boundary. |
| Posting source | No | Agent records subledger, import, or manual entry source when present. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps every account to one account type and one normal balance. | Read account master data. | Every active account has one type and one normal balance. |
| 2 | Agent validates each journal entry for equal debit and credit totals. | Sum debits and credits by entry. | Every accepted entry balances to zero net amount. |
| 3 | Agent posts balanced lines into account ledgers in effective-date order. | Read posting sequence and ledger rows. | Ledger rows preserve source order and source reference. |
| 4 | Agent calculates trial balance totals by account and by period. | Sum debit and credit columns in the trial balance. | Total debits equal total credits. |
| 5 | Agent verifies the accounting equation after posting. | Recalculate assets, liabilities, and equity. | Assets equal liabilities plus equity. |
| 6 | Agent runs targeted build or validation for posting services. | Run targeted build or ledger tests. | Build succeeds and posting assertions pass. |

## Account Type Matrix

| Account Type | Normal Balance | Debit Effect | Credit Effect | Trial Balance Section |
|---|---|---|---|---|
| Assets | Debit | Increases asset balance | Decreases asset balance | Assets |
| Liabilities | Credit | Decreases liability balance | Increases liability balance | Liabilities |
| Equity | Credit | Decreases equity balance | Increases equity balance | Equity |
| Revenue | Credit | Decreases revenue balance | Increases revenue balance | Income statement |
| Expenses | Debit | Increases expense balance | Decreases expense balance | Income statement |

## Ledger Structure Patterns

| Area | Pattern | Test | Pass |
|---|---|---|---|
| Chart of accounts | Agent groups accounts by major number ranges such as `1000` assets, `2000` liabilities, `3000` equity, `4000` revenue, and `5000` expenses. | Read account codes and type map. | Number range and account type align for every controlled account. |
| T-account view | Agent represents each account with left-side debits and right-side credits plus running balance. | Read ledger projection. | Projection shows debit, credit, and ending balance for each account. |
| General ledger | Agent stores immutable posted lines with entry id, line id, account id, amount, direction, period, source, and posting timestamp. | Read ledger schema or model. | Ledger row contains all posting trace fields. |
| Trial balance | Agent summarizes opening, activity, and ending balance by account and period. | Read trial balance output. | Output supports subtotal by account type and grand totals. |
| Journal validation | Agent rejects zero-line entries, mixed currencies without policy, duplicate line ids, and unbalanced amounts. | Run entry validation tests. | Invalid entries stop before posting. |

## .NET Implementation Guidance

| Concern | Agent Action |
|---|---|
| Aggregate shape | Agent models `JournalEntry` as the posting aggregate and `JournalLine` as owned lines with immutable debit-credit direction. |
| Value objects | Agent uses value objects for `AccountNumber`, `Money`, `FiscalPeriod`, and `PostingReference`. |
| Validation | Agent centralizes balance, currency, and account-state checks in domain methods before persistence. |
| Persistence | Agent stores posted lines in EF Core entities with explicit decimal precision and unique source references. |
| Query side | Agent projects trial balance, ledger detail, and T-account views through read-side DTOs only. |
| Idempotency | Agent keys imports and posting commands by source reference to prevent duplicate ledger impact. |

## Integration Hooks

| Hook | Agent Action |
|---|---|
| EF Core | Agent maps entry headers and lines with one-to-many ownership, precision configuration, and account foreign keys. |
| Domain events | Agent raises events such as `JournalEntryPosted` and `TrialBalancePrepared` after durable state change. |
| CQRS patterns | Agent uses commands for create and post flows, and queries for general-ledger, T-account, and trial-balance projections. |
| Repository flow | Agent keeps posting repositories aggregate-focused and keeps read models on direct query projections. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate`. | Command returns zero validation errors. |
| STE wording | Run the repository STE violation grep on the skill file. | Scan returns zero matches. |
| Balance rule | Run targeted tests for balanced and unbalanced entries. | Balanced entries post and unbalanced entries fail. |
| Trial balance | Recalculate total debits and credits. | Totals match exactly within configured precision. |
| Accounting equation | Recalculate assets, liabilities, and equity after posting. | Equation remains true for each verified period. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Account type lacks a normal balance | Agent adds explicit normal balance metadata to the account master. |
| Ledger stores signed amounts without debit-credit direction | Agent stores direction and amount separately, then derives signed views. |
| Trial balance skips inactive accounts with prior balances | Agent keeps zero-activity carry-forward rows where period reporting needs them. |
| Posting mutates posted lines | Agent keeps posted ledger rows immutable and writes correcting entries instead. |
| Import replay creates duplicate postings | Agent enforces unique source reference or idempotency key checks. |

## Outputs

- Double-entry rules matrix
- Chart-of-accounts guidance
- General-ledger structure guidance
- Trial-balance validation checklist
- .NET posting integration hooks
