---
name: public-sector-accounting
title: Public Sector Accounting
description: Apply governmental accounting standards and reporting patterns when work needs GASB-aligned fund, budget, asset, debt, or annual reporting logic.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1542
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fund-accounting-patterns
  - wa-state-saam
  - budget-to-actual-tracking
  - financial-reporting-gaap
  - fabric-powerbi-integration
  - efcore-dbcontext-design
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx}'
tags:
  - accounting
  - public-sector
  - gasb
  - fund-accounting
  - budgeting
  - reporting
---
# Public Sector Accounting

Agent applies governmental accounting patterns with explicit fund, budget, statement, asset, debt, and disclosure rules.

## When to Use

| Condition | Use |
|---|---|
| Work needs GASB-aligned accounting treatment | Use this skill |
| Work compares fund accounting to commercial accounting | Use this skill |
| Data model needs modified accrual and full accrual separation | Use this skill |
| Reporting feature needs fund-level and government-wide mapping | Use this skill |
| Capital asset, infrastructure, or long-term debt logic needs public sector treatment | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on Washington State SAAM policy detail | Use `wa-state-saam` |
| Work focuses on fund type setup and interfund flows | Use `fund-accounting-patterns` |
| Work focuses on budget execution, encumbrances, or variance monitoring | Use `budget-to-actual-tracking` |
| Work focuses on general-purpose GAAP statement assembly only | Use `financial-reporting-gaap` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entity type | Yes | Record state agency, local government, component unit, or public enterprise context. |
| Reporting objective | Yes | Record operational posting, budget control, external reporting, or audit support. |
| Measurement basis | Yes | Record modified accrual, full accrual, or mixed scope. |
| Fund scope | Yes | Record affected funds, fund groups, and elimination boundaries. |
| Capital and debt scope | No | Record capitalization thresholds, depreciation rules, and debt-service requirements. |

## Pattern Matrix

| Reporting View | Basis | Focus | Implementation Shape |
|---|---|---|---|
| Governmental fund statements | Modified accrual | Current financial resources | Keep revenue availability, expenditure timing, fund context, and reconciliation support explicit. |
| Proprietary and fiduciary statements | Full accrual | Economic resources or resources held for others | Track assets, liabilities, depreciation, and long-term obligations directly. |
| Government-wide statements | Full accrual | Entity-wide economic resources | Derive adjustment and elimination layers outside operational fund ledgers. |
| Budget control | Budgetary basis plus actuals | Spending authority and variance | Keep original budget, revised budget, encumbrance, allotment, and actual measures separate. |
| Annual report package | Mixed statement and note support | Statements, notes, MD&A, and schedules | Publish immutable close snapshots and disclosure-support datasets. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies the transaction or feature by fund, reporting objective, and basis. | Review the intake matrix. | Each item maps to one fund view and one basis. |
| 2 | Agent selects recognition and measurement rules aligned to the chosen view. | Compare the design to the pattern matrix. | Recognition timing and measurement basis stay aligned. |
| 3 | Agent separates budgetary, fund-level, and government-wide representations. | Review the model and posting flow. | Representations stay distinct and reconcilable. |
| 4 | Agent applies capital asset, infrastructure, and long-term debt treatment where required. | Review asset and liability workflows. | Lifecycle events produce statement-ready support. |
| 5 | Agent maps outputs to annual report sections, notes, and schedules. | Review the reporting map. | Statements and note-support datasets match the objective. |
| 6 | Agent validates operational, reporting, and analytics layers. | Execute the verification checklist. | Transaction grain and reporting extracts stay consistent. |

## Rules

| Topic | Rule |
|---|---|
| Standards | Treat GASB as the primary accounting framework for governmental entities in scope. |
| Fund accountability | Carry fund, restriction, and reporting-period context on each posting. |
| Basis separation | Keep modified accrual, full accrual, and adjustment layers separate. |
| Budget control | Store budget, allotment, encumbrance, and actual facts in separate measures. |
| Asset treatment | Capitalize and depreciate in the full-accrual layer when required. |
| Debt treatment | Persist debt schedules, service timing, and disclosure support as first-class data. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Basis separation | Review ledger and reporting views. | Modified accrual and full accrual representations stay distinct and reconcilable. |
| Fund accountability | Inspect schema and sample postings. | Each posting carries fund and reporting-period context. |
| Budget traceability | Compare budget, encumbrance, and actual datasets. | Spending-control measures remain separate and reconcilable. |
| Asset treatment | Review capitalization and depreciation flow. | Asset lifecycle events produce consistent statement support. |
| Debt treatment | Review issuance and service schedules. | Debt balances and disclosure support reconcile to the ledger. |
| Reporting readiness | Review annual report mapping. | Statement, note, and schedule datasets cover the required scope. |

## Outputs

- Standards mapping for the feature or dataset in scope
- Basis-of-accounting matrix
- Fund, budget, asset, debt, and reconciliation design guidance
- Operational-to-reporting integration plan
- Verification plan for statement-ready behavior
