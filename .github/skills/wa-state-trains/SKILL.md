---
name: wa-state-trains
title: Washington State TRAINS
description: >
  Apply Transportation Reporting and Accounting Information System patterns when work 
  needs TRAINS-aligned FEMS integration, General Ledger reporting, reconciliation, or 
  statewide transportation accounting flows.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1676
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - wa-state-saam
  - wa-state-afrs
related_skills:
  - wa-state-systems-bundle
  - wa-state-fems
  - wa-state-afrs
  - wa-state-saam
  - cgi-advantage-integration
  - cgi-advantage-reporting
  - public-sector-accounting
  - legacy-trains-migration
appliesTo: '**/*.{cs,csproj,json,xml,csv,txt,md,sql,xlsx}'
tags:
  - washington-state
  - trains
  - transportation
  - gl
  - fems
  - reconciliation
  - reporting
---
# Washington State TRAINS

Agent applies Transportation Reporting and Accounting Information System patterns with explicit FEMS intake, transportation accounting classification, GL reporting, and statewide reconciliation alignment.

## When to Use

| Condition | Use |
|---|---|
| Work needs TRAINS-aligned transportation accounting logic or reporting | Use this skill |
| Work maps FEMS fleet, maintenance, fuel, or chargeback data into GL reporting support | Use this skill |
| Work needs transportation cost aggregation, control totals, or report extracts | Use this skill |
| Work needs reconciliation across FEMS, TRAINS, GL, and AFRS checkpoints | Use this skill |
| .NET services need TRAINS staging, validation, or report-generation behavior | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on fleet operations with no TRAINS or GL behavior | Use `wa-state-fems` |
| Work focuses on statewide policy meaning with no transportation-system behavior | Use `wa-state-saam` |
| Work focuses on AFRS file layout and submission only | Use `wa-state-afrs` |
| Work focuses on legacy coexistence or migration into another platform | Use `legacy-trains-migration` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Agency transportation context | Yes | Record agency, division, business owner, and reporting consumers. |
| Source operational scope | Yes | Record FEMS inventory, maintenance, fuel, utilization, chargeback, or mixed input. |
| Accounting objective | Yes | Record GL reporting, close support, reconciliation, or management reporting. |
| Fiscal period | Yes | Record fiscal month, year-end, biennium context, and correction window. |
| Interface surface | No | Record file extract, staged table, batch job, internal API, or report output. |

## Pattern Matrix

| Stage or Concern | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Source intake | Preserve FEMS identifiers, operational dates, and source lineage on every transported cost record | Avoid GL-only starting points | Source events land with no lost identifiers |
| Classification | Classify fuel, maintenance, depreciation support, chargebacks, and transfer activity separately | Avoid one undifferentiated cost path | Every row has one classification and period owner |
| Allocation | Persist mileage, hours, rates, or other drivers beside published results | Avoid totals with no stored driver set | Recomputed amounts match published amounts |
| Validation | Enforce structural keys, coding validity, period ownership, and balancing before publish | Reject incomplete or misdated rows before publication | Detail totals and coding rules reconcile |
| Publication | Publish GL-ready outputs and agency review packs from approved period snapshots | Avoid ad hoc reruns with drifting totals | Published totals tie to one approved batch |
| Reconciliation | Trace FEMS to TRAINS to GL to AFRS checkpoints | Store difference sets and disposition status | One reproducible path exists from source to statewide review |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps the request to one accounting objective and one source-data path. | Review the intake matrix. | Each request maps to one primary objective and source path. |
| 2 | Agent identifies FEMS, TRAINS, GL, and AFRS checkpoints in scope. | Review the data-flow map. | Operational and financial checkpoints stay explicit. |
| 3 | Agent classifies cost elements, allocation drivers, and fiscal-period ownership. | Review staging and mapping rules. | Every cost line has one classification and period owner. |
| 4 | Agent applies validation, balancing, and reject handling before publication. | Review the validation matrix. | Structural, coding, and total controls exist before publish. |
| 5 | Agent maps outputs to GL reports, statewide reconciliation, and agency review packs. | Review the output map. | One traceable path exists from source event to published report. |
| 6 | Agent verifies staging, batch execution, and report reproducibility. | Run targeted validation or existing tests. | Repeated runs reproduce the same control totals. |

## Rules

| Topic | Rule |
|---|---|
| System role | Treat TRAINS as a governed transportation-accounting layer, not an ad hoc extract folder. |
| Lineage | Preserve operational lineage from FEMS through published output. |
| Coding | Keep fund, account, program, object, and period mappings explicit. |
| Allocation | Persist rate and driver inputs beside results. |
| Period control | Derive fiscal ownership from governed activity context, not report run date alone. |
| Terminology | Distinguish Washington State TRAINS from CGI Advantage Trains 4 migration scope. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Source lineage | Trace one fuel, maintenance, or chargeback record from FEMS into TRAINS output. | One reproducible path exists. |
| Coding validity | Review fund, account, and object mappings. | All required codes are valid for the period. |
| Allocation reproducibility | Recalculate one rate-based output from stored drivers. | Recomputed amount matches the published amount. |
| Report balancing | Compare classified detail totals to published GL totals. | Totals agree with no unexplained difference. |
| Period control | Test open and close-window scenarios. | Misdated activity is held or corrected. |
| AFRS-facing readiness | Review downstream reconciliation checkpoints. | TRAINS output ties to the statewide review path. |

## Outputs

- TRAINS data-flow and checkpoint map
- Transportation cost classification and allocation rules
- GL reporting and reconciliation design
- Batch and validation pipeline plan
- Verification pack for lineage, totals, and fiscal control
