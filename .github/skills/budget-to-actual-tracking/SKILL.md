---
name: budget-to-actual-tracking
title: Budget to Actual Tracking
description: Build budget execution, encumbrance, and variance monitoring patterns when work needs appropriation-aware budget versus actual controls and reporting.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1900
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - public-sector-accounting
  - fund-accounting-patterns
  - wa-state-saam
  - financial-reporting-gaap
  - fabric-powerbi-integration
  - efcore-dbcontext-design
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx}'
tags:
  - accounting
  - budgeting
  - encumbrance
  - variance
  - public-sector
---
# Budget to Actual Tracking

Agent designs budget execution workflows with explicit authority, allotment, encumbrance, actual, amendment, and lapse-year controls.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs budget preparation, approval, or budget-load patterns | Use this skill |
| Work item needs appropriation, allotment, or spending-limit control | Use this skill |
| Procurement flow needs pre-encumbrance or encumbrance tracking | Use this skill |
| Reporting feature needs budget versus actual variance analysis | Use this skill |
| Fiscal logic needs budget amendments, transfers, or lapse-year treatment | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item focuses on broad governmental accounting standards | Use `public-sector-accounting` |
| Work item focuses on fund setup and interfund flows | Use `fund-accounting-patterns` |
| Work item focuses on Washington SAAM policy detail | Use `wa-state-saam` |
| Work item focuses on annual financial statement and note assembly | Use `financial-reporting-gaap` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Budget structure | Yes | Record fiscal year, organization, fund, program, account, and object dimensions. |
| Authority source | Yes | Record appropriation, grant, contract, or internal allocation source. |
| Control points | Yes | Record requisition, purchase order, contract, invoice, payroll, and journal events in scope. |
| Period rules | Yes | Record open period, close period, and lapse-year behavior. |
| Variance thresholds | No | Record tolerance rules and escalation thresholds for reporting or workflow alerts. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Map authority from original budget through revisions and allotments. | Original, revised, and available authority stay distinct. |
| 2 | Define pre-encumbrance, encumbrance, and actual states for each control point. | Every source event maps to one budget state. |
| 3 | Enforce availability checks before controlled spending events. | Under-budget, exact-budget, and over-budget paths follow policy. |
| 4 | Apply amendment, transfer, and lapse-year rules with immutable history. | Fiscal constraints and history remain intact. |
| 5 | Publish variance and trend outputs and reconcile them to transactional data. | Operational and reporting layers agree by period and dimension. |


## Budget Execution Reference

| Topic | Pattern | Focus |
|---|---|---|
| Preparation and approval | Budget moves from proposal to adoption to governed revision. | Store events with approval metadata. |
| Authority | Legal or delegated authority stays separate from actuals. | Track source, dates, expiration, and restrictions. |
| Allotments | Spending limits refine broader authority. | Keep allotments as their own control layer. |
| Encumbrances | Purchase orders and contracts reserve budget before actuals. | Track reservation, release, and liquidation links. |
| Pre-encumbrances | Requisitions create soft reservations. | Store approval and aging data. |
| Variance analysis | Compare budget, encumbrance, actual, and available balances. | Publish shared measures and drill paths. |


## Budget State Matrix

| State | Trigger | Balance Effect | Reversal or Transition |
|---|---|---|---|
| Original budget | Adopted budget load | Establishes baseline authority | Revised through approved amendment events |
| Revised budget | Amendment or transfer approval | Updates current authorized plan | Further revision or fiscal close |
| Allotted budget | Spending limit allocation | Reduces broad authority into controlled availability | Additional allotment, deallotment, or close |
| Pre-encumbered | Requisition approval | Soft-reserves availability | Release, convert to encumbrance, or cancel |
| Encumbered | Purchase order or contract execution | Firm-reserves availability | Liquidate to actual, partial release, or cancel |
| Actual | Invoice, payroll, journal, or accrual event | Consumes budget with recognized spending | Adjustment, accrual reversal, or correction entry |
| Lapsed | Fiscal period close with lapse rule | Removes expired authority from available use | Carryforward or reappropriation path only when authorized |

## Public Sector Specific Patterns

| Pattern | Implementation |
|---|---|
| Layered authority | Separate budget, allotment, encumbrance, and actual facts with reconciliation logic. |
| Source-document traceability | Link each event to requisition, PO, contract, invoice, or payroll workflow state. |
| Amendment history | Store immutable delta events with approval identity and effective date. |
| Lapse-year awareness | Add fiscal-year and lapse rules to availability calculations. |
| Variance responsibility view | Publish manager-facing fund, org, program, and object dimensions. |


## Integration Hooks

| Surface | Output |
|---|---|
| EF Core | Budgets, amendments, allotments, reservations, actuals, and lapse rules with period-aware indexes |
| Microsoft Fabric | Original, revised, allotted, encumbered, actual, and available measures in semantic models |
| Workflow and procurement services | Availability checks at requisition, PO, contract, invoice, and payroll checkpoints |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Authority traceability | Review budget and amendment history. | Original, revised, and available balances remain reconstructable. |
| State transitions and controls | Test requisition, PO, invoice, payroll, cancellation, and over-budget scenarios. | Pre-encumbrance, encumbrance, actual, and exception paths follow policy. |
| Lapse and variance behavior | Test year-end or carryforward cases and compare analytics to operations. | Expired authority exits available balance unless authorized and reported measures reconcile. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Actuals replace encumbrances with no liquidation trail | Store liquidation links and release history. |
| Allotment logic disappears inside one available-balance formula | Expose allotment as a distinct control layer and measure. |
| Requisitions bypass budget control entirely | Add pre-encumbrance or planned-use checkpoints where the business flow needs them. |
| Budget transfers rewrite original history | Record approved delta events and preserves the baseline load. |
| Lapse-year rules depend on ad hoc report filters | Agent embeds lapse behavior in the core availability and close logic. |

## Outputs

| Output | Description |
|---|---|
| Budget state model | Authority, reservation, and consumption lifecycle |
| Control-point matrix | Budget checks by requisition, PO, contract, invoice, payroll, and journal event |
| Analytics design | Fabric-ready measures and drill dimensions |
| Verification plan | Control, transition, variance, and lapse tests |

