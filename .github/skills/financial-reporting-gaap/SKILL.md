---
name: financial-reporting-gaap
title: Financial Reporting GAAP
description: Build GAAP-aligned financial statement, note, and annual report outputs when work needs statement assembly, disclosures, RSI, or statistical reporting patterns.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1864
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - public-sector-accounting
  - fund-accounting-patterns
  - wa-state-saam
  - budget-to-actual-tracking
  - fabric-powerbi-integration
  - efcore-dbcontext-design
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx}'
tags:
  - accounting
  - gaap
  - financial-reporting
  - cafr
  - statements
---
# Financial Reporting GAAP

Agent assembles GAAP-aligned financial statements, notes, MD&A support, RSI, and comprehensive annual reporting datasets with public sector awareness.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs statement preparation logic for balance sheet, operating results, cash flow, or equity or net position changes | Use this skill |
| Work item needs footnote or disclosure support data | Use this skill |
| Reporting package needs MD&A, RSI, or statistical section structure | Use this skill |
| Government reporting flow needs comprehensive annual report component mapping | Use this skill |
| .NET report generation needs statement-ready datasets and narrative support hooks | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item focuses on foundational governmental accounting treatment | Use `public-sector-accounting` |
| Work item focuses on fund structures and interfund logic | Use `fund-accounting-patterns` |
| Work item focuses on Washington SAAM policy mapping | Use `wa-state-saam` |
| Work item focuses on budget execution and encumbrance control | Use `budget-to-actual-tracking` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Reporting basis | Yes | Record governmental GAAP, proprietary accrual, or mixed package scope. |
| Statement package | Yes | Record statements, notes, MD&A, RSI, and statistical sections in scope. |
| Close status | Yes | Record preliminary, adjusted, final, or issued close stage. |
| Comparative periods | Yes | Record current year, prior year, and trend-year requirements. |
| Disclosure topics | No | Record debt, capital assets, leases, pensions, commitments, and contingencies in scope. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Map statements, notes, MD&A support, RSI, and statistical schedules. | Every requested section has named datasets and owners. |
| 2 | Align each section to the selected GAAP and public-sector view. | Basis, presentation, and disclosures match the reporting objective. |
| 3 | Trace statement lines and note support to closed-ledger and subledger sources. | Published amounts remain lineage-safe and reproducible. |
| 4 | Build narrative, comparative, and trend measures from governed datasets. | MD&A, RSI, and statistical sections avoid ad hoc spreadsheet-only values. |
| 5 | Validate cross-statement ties, disclosure completeness, and issue-state controls. | Statements, notes, and trend schedules reconcile across the package. |


## Accounting Standards Reference

| Topic | Pattern | Focus |
|---|---|---|
| Statements | Use closed and adjusted balances with consistent classification. | Maintain chart-to-line mappings and close-stage locks. |
| Footnotes | Publish structured support for policies, balances, commitments, and risks. | Prefer schedule datasets over prose-only evidence. |
| MD&A | Source comparative metrics and explanations from issued statements. | Govern ratios, variances, and trend inputs. |
| RSI | Preserve mandated multi-year schedules and source lineage. | Keep schedule grain stable across years. |
| Statistical section | Maintain long-horizon trends with consistent dimensions. | Record restatements and definition changes. |
| Annual report package | Version statements, notes, MD&A, RSI, and statistics together. | Track release state per section and issue set. |


## Statement Reference Table

| Section | Core sources | Output |
|---|---|---|
| Balance sheet or net position | General ledger, asset ledger, debt ledger, accrual adjustments | Statement-ready ending balances |
| Activities or operating results | Revenue, expenditure or expense, transfer, and adjustment balances | Operating results by classification |
| Cash flow | Cash ledger, AP, AR, debt, payroll, investing activity | Cash flow lines and reconciliation schedules |
| Equity or net-position rollforward | Beginning balance, activity, adjustments, ending balance | Rollforward by class |
| Notes | Structured schedules and narrative-support metrics | Disclosure schedules and checklists |
| MD&A, RSI, and statistical sections | Comparative measures, required schedules, and trend datasets | Narrative-support, supplementary, and long-horizon outputs |


## Public Sector Specific Patterns

| Pattern | Implementation |
|---|---|
| Package versioning | Store issuance state, period, and package version on report artifacts. |
| Note-support datasets | Persist disclosure schedules with line references. |
| Fund and government-wide ties | Maintain reconciliation tables between presentation layers. |
| RSI and statistical persistence | Snapshot issued-year trends by schedule definition. |
| Narrative support | Publish approved measures and definitions for authors. |


## Integration Hooks

| Surface | Output |
|---|---|
| EF Core | Close packages, statement mappings, note schedules, disclosure topics, and issuance states |
| Microsoft Fabric | Statement lines, note schedules, MD&A metrics, RSI schedules, and trend datasets |
| Report generation services | Deterministic close, assembly, narrative binding, and publication pipeline |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Lineage and reconciliation | Trace sample lines and compare statement-ending balances across the package. | Each line ties to governed sources and cross-statement totals agree. |
| Disclosure support | Review note topics, MD&A metrics, RSI inputs, and statistical definitions. | Material notes and supplementary sections have structured support with stable measures. |
| Publication control | Review package version and issue state. | Issued packages stay immutable and reproducible. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Statement lines pull directly from ad hoc report formulas | Agent creates governed mapping tables and close snapshots. |
| Footnotes rely on manual narrative with no structured evidence | Agent publishes note-support schedules and disclosure checklists. |
| MD&A metrics differ from published statements | Agent sources narrative support from the same governed model as the issued package. |
| RSI and statistical schedules change definitions silently | Agent versions schedule definitions and records restatement notes. |
| Issued reports stay editable in place | Apply package versioning and immutable issue states. |

## Outputs

| Output | Description |
|---|---|
| Package map | Statements, notes, MD&A, RSI, and statistical layout |
| Lineage model | Source-to-statement and source-to-note traceability |
| Integration plan | EF Core, Fabric, and report-build design |
| Verification plan | Reconciliation, completeness, and publication-control checks |

