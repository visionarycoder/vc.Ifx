---
name: fund-accounting-patterns
title: Fund Accounting Patterns
description: Design fund accounting structures, interfund flows, and isolation boundaries when work needs governmental, proprietary, or fiduciary fund behavior.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1352
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - public-sector-accounting
  - wa-state-saam
  - budget-to-actual-tracking
  - fabric-powerbi-integration
  - efcore-dbcontext-design
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx}'
tags:
  - accounting
  - fund-accounting
  - public-sector
  - gasb
  - multi-tenant
---
# Fund Accounting Patterns

Agent structures governmental, proprietary, and fiduciary funds with explicit fund type, interfund, balance, and tenant-isolation rules.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs fund type design or classification logic | Use this skill |
| Ledger or API design needs fund-level isolation | Use this skill |
| Work item needs interfund transfers, loans, or service billing patterns | Use this skill |
| Reporting model needs fund balance classification | Use this skill |
| .NET solution needs multi-tenant fund isolation with public sector controls | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item focuses on broad governmental accounting standards beyond fund structure | Use `public-sector-accounting` |
| Work item focuses on Washington SAAM policy and AFRS coding | Use `wa-state-saam` |
| Work item focuses on budget authority, encumbrances, and variance monitoring | Use `budget-to-actual-tracking` |
| Work item focuses on annual report assembly and notes | Use `financial-reporting-gaap` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Fund inventory | Yes | Record legal purpose, revenue sources, and reporting group. |
| Transaction categories | Yes | Record transfers, loans, reimbursements, and shared services. |
| Balance policy | Yes | Record restriction, commitment, assignment, and residual rules. |
| Tenant model | Yes | Record agency, program, or business-unit boundaries. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies each fund into governmental, proprietary, or fiduciary categories. | Review fund inventory. | Each fund maps to one category and one fund type. |
| 2 | Agent defines posting and validation rules for fund isolation. | Review schema and API contracts. | Posting paths require fund context and preserve isolation. |
| 3 | Agent selects interfund patterns and reciprocal entries. | Review the interfund matrix. | Transfers, loans, and service charges map to balanced paired entries. |
| 4 | Agent applies balance or net-position classification rules. | Review close rules. | Close logic derives the expected classification for each fund. |
| 5 | Agent validates rollups, reconciliation, and reporting behavior. | Run or review close scenarios. | Fund statements reconcile across detail and consolidated views. |

## Fund Category Matrix

| Category | Choose When | Detail Reference |
|---|---|---|
| Governmental | Activity uses modified accrual, budget control, or legally restricted public resources. | `references/state-and-balance-reference.md#detailed-fund-type-reference` |
| Proprietary | Activity operates on a fee-supported or internal-service full-accrual basis. | `references/state-and-balance-reference.md#detailed-fund-type-reference` |
| Fiduciary | Activity holds assets for participants, beneficiaries, or external parties. | `references/state-and-balance-reference.md#detailed-fund-type-reference` |

## Interfund Decision Matrix

| Transaction Type | Use | Accounting Shape |
|---|---|---|
| Interfund transfer | Nonreciprocal movement between funds | Transfer out and transfer in |
| Interfund loan | Temporary financing between funds | Due from and due to |
| Shared service charge | Reciprocal cost recovery | Revenue and expenditure or expense pair |
| Reimbursement | One fund repays another for eligible cost | Expense or expenditure reclassification |
| Residual equity transfer | Structural reorganization of resources | Equity movement outside routine operations |

## Reference Files

| File | Purpose |
|---|---|
| [references/state-and-balance-reference.md](references/state-and-balance-reference.md) | Detailed fund types, Washington State fund guidance, balance classifications, integration hooks, pitfalls, and outputs. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Fund classification | Review fund master records. | Every fund maps to one valid category and type. |
| Isolation | Test tenant and fund filters end to end. | No posting or query crosses an unauthorized fund or tenant boundary. |
| State fund-code validity | Test active, inactive, unknown, and misdated state fund codes. | Validation accepts only authorized fund codes valid for the transaction date. |
| Interfund balance | Review paired entries and settlement schedules. | Reciprocal balances agree by source document and period. |
| Balance classification | Run period-close classification logic. | Balances land in the expected class with traceable authority data. |
| Reporting rollup | Review major-fund and aggregate reports. | Detail, rollup, and elimination views reconcile. |
