---
name: accounting-systems-bundle
title: Accounting Systems Bundle
description: Routes accounting system implementation work to the correct accounting skill set for commercial, public sector, fund, close, control, reporting, and Washington State scenarios.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: low
estimated_tokens: 2402
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - double-entry-accounting
  - chart-of-accounts-design
  - journal-entry-patterns
  - adjustment-entry-patterns
  - reconciliation-patterns
  - subledger-reconciliation-patterns
  - closing-cycle-patterns
  - audit-trail-compliance
  - public-sector-accounting
  - fund-accounting-patterns
  - interfund-accounting-patterns
  - appropriation-lifecycle-patterns
  - multi-currency-accounting
  - tax-accounting-patterns
  - wa-state-saam
  - saam-ontology
  - budget-to-actual-tracking
  - financial-reporting-gaap
appliesTo: '**/*.{cs,csproj,sql,json,md,yml,yaml}'
tags:
  - accounting
  - finance
  - public-sector
  - fund-accounting
  - saam
  - fabric
  - routing
---
# Accounting Systems Bundle

Agent uses this bundle for accounting system work that spans posting rules, period workflows, controls, governmental accounting, budget enforcement, or external financial reporting.

## When to Use

| Prompt Pattern | Use This Bundle |
|---|---|
| New accounting platform or accounting module | Yes |
| Commercial and governmental accounting choice | Yes |
| Journal, reconciliation, close, controls, and reporting in one request | Yes |
| Washington State agency accounting requirements | Yes |
| Single accounting topic with no routing need | No |

## When Not to Use

| Prompt Pattern | Agent Route |
|---|---|
| Only debits, credits, or chart of accounts | `double-entry-accounting` |
| Only journal workflow design | `journal-entry-patterns` |
| Only GAAP statements or CAFR reporting | `financial-reporting-gaap` |
| Only Washington State SAAM compliance | `wa-state-saam` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entity type | Yes | Commercial, nonprofit, public sector, or Washington State agency |
| Ledger scope | Yes | General ledger only, subledgers, budget ledger, or fund ledger |
| Compliance basis | No | GAAP, GASB, SOX, SAAM, or mixed |
| Integration surface | No | EF Core, CQRS, domain events, Fabric, external banking, ERP |

## Activation

| Request Signal | Use Bundle | Route Detail |
|---|---|---|
| Debits, credits, posting rules, chart design | Yes | `double-entry-accounting` |
| Chart of accounts structure, segments, numbering | Yes | `chart-of-accounts-design` |
| Journal intake, approval, posting, reversal, correction | Yes | `journal-entry-patterns` |
| Adjustment entries: correcting, accrual, reclassification | Yes | `adjustment-entry-patterns` |
| Bank, subledger, or intercompany tie-out | Yes | `reconciliation-patterns` |
| Subledger-to-GL reconciliation, break detection | Yes | `subledger-reconciliation-patterns` |
| Interfund transactions, due to/from, eliminations | Yes | `interfund-accounting-patterns` |
| Multi-currency, FX translation, remeasurement | Yes | `multi-currency-accounting` |
| Sales tax, use tax, tax jurisdiction, 1099 reporting | Yes | `tax-accounting-patterns` |
| Month-end, year-end, or close checklist | Yes | `closing-cycle-patterns` |
| Controls, approvals, evidence, audit trail, segregation | Yes | `audit-trail-compliance` |
| Governmental, GASB, budgetary, or grant accounting | Yes | `public-sector-accounting` |
| Fund setup, restriction tracking, or fund balance logic | Yes | `fund-accounting-patterns` |
| Appropriation lifecycle, funds availability, encumbrance | Yes | `appropriation-lifecycle-patterns` |
| Washington State agency rules or SAAM mapping | Yes | `wa-state-saam` |
| SAAM ontology, semantic validation, RDF queries | Yes | `saam-ontology` |
| Appropriation, allotment, encumbrance, budget variance | Yes | `budget-to-actual-tracking` |
| Financial statements, note support, CAFR outputs | Yes | `financial-reporting-gaap` |

## Coverage Matrix

| Need | Specialist Skill | Primary Outcome |
|---|---|---|
| Ledger foundation | `double-entry-accounting` | Balanced posting model and chart of accounts |
| Chart structure | `chart-of-accounts-design` | Account numbering, segments, hierarchy, roll-up rules |
| Entry workflow | `journal-entry-patterns` | Draft-to-post lifecycle with reversals and corrections |
| Adjustment entries | `adjustment-entry-patterns` | Correcting, accrual, reclassification, prior period adjustments |
| Matching and tie-out | `reconciliation-patterns` | Deterministic reconciliation flow and exception handling |
| Subledger control | `subledger-reconciliation-patterns` | AR, AP, Payroll, Fixed Assets to GL reconciliation |
| Interfund transactions | `interfund-accounting-patterns` | Due to/from, transfers, eliminations for consolidated reporting |
| Multi-currency | `multi-currency-accounting` | FX translation, remeasurement, gain/loss recognition |
| Tax accounting | `tax-accounting-patterns` | Sales tax, use tax, jurisdiction-based calculation |
| Period governance | `closing-cycle-patterns` | Controlled close calendar and cutoff rules |
| Controls and evidence | `audit-trail-compliance` | Traceability, approvals, and segregation boundaries |
| Government model | `public-sector-accounting` | GASB-aligned accounting structure |
| Fund model | `fund-accounting-patterns` | Fund classification and restriction tracking |
| Appropriation control | `appropriation-lifecycle-patterns` | Authorization, allotment, encumbrance, expenditure lifecycle |
| State-specific policy | `wa-state-saam` | SAAM-aligned accounting decisions for Washington agencies |
| SAAM semantic layer | `saam-ontology` | RDF/OWL validation and semantic model integration |
| Budget control | `budget-to-actual-tracking` | Encumbrance and variance tracking against authority |
| External reporting | `financial-reporting-gaap` | Statements, schedules, and CAFR-ready outputs |

## Decision Matrix: Commercial vs Public Sector Accounting

| Signal | Commercial Path | Public Sector Path |
|---|---|---|
| Primary basis | Investor, lender, or management reporting | Accountability, legal compliance, and public stewardship |
| Core standards | GAAP | GASB plus agency policy |
| Budgetary control | Optional management control | Core accounting dimension |
| Fund separation | Rare | Frequent or required |
| State policy overlay | Low | High, especially for Washington agencies |

## Decision Matrix: When to Use Fund Accounting

| Condition | Use Fund Accounting | Route |
|---|---|---|
| Resources carry legal, grant, donor, or statutory restrictions | Yes | `fund-accounting-patterns` |
| Entity tracks independent self-balancing pools | Yes | `fund-accounting-patterns` |
| Governmental reporting requires fund statements | Yes | `public-sector-accounting` + `fund-accounting-patterns` |
| One unrestricted commercial ledger fits the model | No | `double-entry-accounting` |

## Decision Order

| Decision | Agent Action |
|---|---|
| Commercial entity with one unrestricted ledger | Agent starts with `double-entry-accounting`, then `journal-entry-patterns`, `reconciliation-patterns`, `closing-cycle-patterns`, and `financial-reporting-gaap`. |
| Public sector entity with GASB reporting | Agent starts with `public-sector-accounting`, then `fund-accounting-patterns`, `budget-to-actual-tracking`, `journal-entry-patterns`, `closing-cycle-patterns`, and `financial-reporting-gaap`. |
| Washington State agency | Agent inserts `wa-state-saam` after public-sector classification and before final model decisions. |
| Control-heavy implementation | Agent adds `audit-trail-compliance` before workflow finalization. |
| Analytics and reporting platform in scope | Agent maps operational ledger events to Fabric semantic, warehouse, or lakehouse outputs after accounting model selection. |

## Integration Patterns

| Concern | Pattern |
|---|---|
| EF Core | Use immutable journal headers and lines, posting batches, reconciliation snapshots, and close status aggregates with explicit concurrency tokens. |
| CQRS | Use commands for draft, approve, post, reverse, close, reopen, reconcile, and budget reserve actions; use read models for trial balance, fund balance, budget variance, and close dashboards. |
| Domain events | Emit `JournalPosted`, `ReconciliationCompleted`, `PeriodClosed`, `BudgetExceeded`, and `FundBalanceChanged` events for downstream controls and reporting. |
| Fabric | Land posted journal, budget, and reconciliation facts in Fabric lakehouse or warehouse; publish semantic models for statements, CAFR schedules, variance analysis, and audit evidence. |
| SAAM routing | Map state-specific appropriation, allotment, object, and fund rules through `wa-state-saam` before schema freeze or reporting design. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify entity | Agent identifies commercial, public sector, or Washington State context. | Agent records one entity path. | One primary accounting path exists. |
| 2. Select ledger model | Agent chooses unrestricted ledger, budget ledger, fund ledger, or mixed model. | Agent maps restrictions and reporting drivers. | Ledger model fits constraints. |
| 3. Add workflow skills | Agent attaches journal, reconciliation, close, and control skills in dependency order. | Agent lists routed skills. | Each operational need maps to one skill. |
| 4. Add reporting path | Agent maps GAAP, GASB, CAFR, and Fabric outputs. | Agent records statement and analytics consumers. | Reporting path covers all consumers. |
| 5. Verify policy fit | Agent checks SAAM, GASB, and audit evidence needs. | Agent compares controls to compliance basis. | Policy obligations map to one routed skill. |

## Verification Matrix

| Area | Agent Verifies | Test | Pass |
|---|---|---|---|
| Posting model | Debits equal credits and chart segments align to reporting needs. | Inspect entry rules and sample postings. | All sample postings balance. |
| Workflow | Draft, approval, posting, reversal, and close states are explicit. | Inspect command and state flow. | No hidden state transition exists. |
| Reconciliation | Exceptions, timing items, and adjustments are traceable. | Inspect matching and exception outputs. | Every break has an owner and status. |
| Public sector and funds | Funds, budgetary controls, and GASB outputs align. | Inspect fund model and statement mapping. | Government reporting dimensions reconcile. |
| Compliance and analytics | Audit evidence, SAAM mapping, and Fabric outputs stay consistent. | Inspect lineage from transaction to report. | One traceable path exists from entry to report. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Treating governmental accounting as commercial accounting with extra fields | Agent routes first to `public-sector-accounting` and `fund-accounting-patterns`. |
| Deferring budgetary control until reporting | Agent routes early to `budget-to-actual-tracking`. |
| Designing SAAM mapping after schema lock | Agent routes early to `wa-state-saam`. |
| Using reports as the source of reconciliation truth | Agent anchors reconciliation in operational accounting records. |
| Publishing Fabric models straight from unstable journal drafts | Agent publishes from posted and controlled accounting facts only. |
