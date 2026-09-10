---
name: wa-state-fems
title: Washington State FEMS
description: Apply Washington State Fleet Equipment Management System patterns for fleet inventory, lifecycle, maintenance, fuel, chargeback, reconciliation, and controlled integration.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1722
prerequisites:
  - wa-state-saam
  - wa-state-afrs
  - wa-state-cams
related_skills:
  - wa-state-systems-bundle
  - wa-state-saam
  - wa-state-afrs
  - wa-state-cams
  - cgi-advantage-integration
  - legacy-trains-migration
  - public-sector-accounting
appliesTo: '**/*.{cs,csproj,json,xml,csv,txt,md,sql,xlsx}'
tags:
  - washington-state
  - fems
  - trains
  - fleet
  - equipment
  - maintenance
  - fuel
  - chargebacks
---
# Washington State FEMS

Agent applies FEMS patterns with explicit fleet inventory, maintenance, fuel, utilization, replacement-planning, and financial-integration rules.

## When to Use

| Condition | Use |
|---|---|
| Work needs fleet or heavy-equipment inventory tracking | Use this skill |
| Work needs maintenance history, work orders, or service-cost accumulation | Use this skill |
| Work needs fuel, mileage, hours, or utilization reporting | Use this skill |
| Work needs chargebacks, replacement planning, or depreciation-support data | Use this skill |
| Work needs FEMS-oriented extraction, submission, or reconciliation interfaces | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on non-fleet capital-asset registration | Use `wa-state-cams` |
| Work focuses on policy interpretation only | Use `wa-state-saam` |
| Work focuses on AFRS batch layout without fleet context | Use `wa-state-afrs` |
| Work focuses on ERP procurement exchange only | Use `cgi-advantage-integration` |
| Work focuses on broad governmental accounting only | Use `public-sector-accounting` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Agency fleet context | Yes | Agent records agency, division, shop, and program owner. |
| Equipment scope | Yes | Agent records vehicle, heavy equipment, support equipment, or mixed scope. |
| Lifecycle event | Yes | Agent records acquisition, maintenance, fuel, transfer, disposal, or planning event. |
| Financial objective | Yes | Agent records valuation, allocation, reconciliation, or reporting objective. |
| Integration surface | Yes | Agent records screen, extract, staged table, API wrapper, or AFRS tie-out path. |
| Reporting period | No | Agent records fiscal month, year-end, biennium, or ad hoc window. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps the request to one equipment scope, one lifecycle stage, and one financial objective. | Review intake row. | One primary scope, stage, and objective are selected. |
| 2 | Agent records required master, maintenance, fuel, utilization, and financial data elements. | Review field matrix. | Required identifiers, dates, measures, and cost fields stay explicit. |
| 3 | Agent applies valuation, depreciation-support, and replacement-planning rules. | Review record design. | Value, useful-life, and planning factors align to one rule path. |
| 4 | Agent maps maintenance, work-order, and fuel events to utilization and cost-allocation outputs. | Review lineage tables. | Usage, service, and cost records tie to one equipment identifier path. |
| 5 | Agent validates file layouts, control totals, exception handling, and close-period traceability. | Run targeted tests. | Interface artifacts parse correctly and every exception has one owner path. |

## Rule Matrix

| Concern | Rule | Reference |
|---|---|---|
| System role | Agent treats FEMS as the operational system of record for state fleet equipment. | `references/fems-reference.md` |
| Scope split | Agent routes fleet operations to FEMS and non-fleet asset registration to CAMS. | `references/fems-reference.md` |
| Lifecycle detail | Agent records acquisition, maintenance, fuel, transfer, disposal, and planning events separately. | `references/fems-reference.md` |
| Financial support | Agent preserves valuation, depreciation-support, chargeback, and reconciliation data. | `references/fems-reference.md` |
| TRAINS linkage | Agent distinguishes `TRAINS`, `Trains 4`, and legacy Trains in mappings and notes. | `references/fems-reference.md` |
| File handling | Agent binds each exchange format to one schema, control-total rule, and replay path. | `references/fems-reference.md` |

## Pattern Matrix

| Pattern | Use | Output |
|---|---|---|
| Equipment master extract | Fleet identity, status, assignment, and cost data | Versioned equipment contract |
| Maintenance work-order feed | Labor, parts, vendor, and downtime capture | Header-detail workflow with lineage |
| Fuel transaction import | Gallons, amount, site, and meter data | Deduplicated fuel staging flow |
| Utilization snapshot | Miles, hours, and availability metrics | Period snapshot contract |
| Chargeback export | Driver, rate, quantity, and billed amount | Deterministic period batch |
| Replacement ranking | Age, usage, downtime, and trend scoring | Reproducible planning output |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Equipment scope alignment | Compare the request to the equipment categories. | Each unit maps to one governed category path. |
| Lifecycle traceability | Trace one unit from acquisition through latest status. | One reproducible history exists for each unit. |
| Maintenance and fuel integrity | Reconcile work-order and fuel totals to equipment history. | Cost and usage totals tie without orphan transactions. |
| Chargeback accuracy | Recalculate one period from stored allocation drivers. | Chargeback totals reproduce for the selected period. |
| Financial reconciliation | Compare FEMS totals to AFRS or CAMS targets in scope. | Differences resolve to zero or one approved exception record. |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Scope selected | One equipment scope and one lifecycle stage lead the design. |
| CAMS boundary preserved | Fleet operations stay separate from non-fleet asset registration. |
| TRAINS naming preserved | System names stay explicit in integration outputs. |
| Reference detail preserved | Deep fleet matrices and file notes live in the reference file. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| FEMS and CAMS scopes collapse into one workflow | Agent uses the scope split before entity design. |
| Routine maintenance inflates capital value | Agent separates maintenance expense from capital improvement events. |
| Fuel and utilization metrics use inconsistent equipment keys | Agent normalizes one governed equipment identifier path. |
| Chargeback logic hides its driver basis | Agent stores rate, driver, quantity, and period together. |
