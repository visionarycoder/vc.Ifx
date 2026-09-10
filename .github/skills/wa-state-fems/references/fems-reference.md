---
title: Washington State FEMS Reference
description: Detailed fleet scope, lifecycle, chargeback, and file-exchange reference tables for the Washington State FEMS skill.
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 642
prerequisites:
  - wa-state-fems
related_skills:
  - wa-state-fems
appliesTo: '.github/skills/wa-state-fems/**/*'
tags:
  - washington-state
  - fems
  - reference
---
# Washington State FEMS Reference

Agent uses this file for deep fleet detail after the main skill selects the operational path.

## Scope Matrix

| Need | Route |
|---|---|
| Fleet inventory, work orders, fuel, or utilization | FEMS |
| Non-fleet capital-asset registration | CAMS |
| Fleet policy interpretation | SAAM |
| Fleet ERP procurement source flow | CGI Advantage |
| Fleet operational balances to statewide reporting | AFRS |

## Lifecycle Matrix

| Stage | Required Data |
|---|---|
| Acquisition | Acquisition date, vendor, cost, funding source, and equipment category |
| In service | Assignment, meter baseline, and depreciation-support start basis |
| Maintenance | Work-order ID, labor, parts, downtime, and completion date |
| Fuel | Transaction date, gallons, amount, meter reading, and fuel site |
| Transfer | From-unit, to-unit, effective date, and reason |
| Disposal | Disposal date, proceeds, reason, and approval evidence |
| Replacement planning | Age, mileage or hours, maintenance trend, and ranking inputs |

## Chargeback Matrix

| Driver | Use |
|---|---|
| Miles driven | Passenger and road fleet |
| Engine or equipment hours | Heavy equipment |
| Days assigned | Pool or reserve fleet |
| Work-order consumption | Internal maintenance support |
| Hybrid rate | Mixed fixed-readiness and variable-use recovery |

## File Matrix

| Surface | Validation Focus |
|---|---|
| Equipment master extract | Identifier uniqueness, status codes, dates, and opening meters |
| Maintenance feed | Header-detail totals, duplicate closures, and valid equipment keys |
| Fuel import | Duplicate transactions, outlier gallons, negative amounts, and meter rollback |
| Utilization snapshot | Period boundaries, nonnegative measures, and category-specific units |
| Chargeback export | Rate basis, quantity totals, and receiving-code alignment |

## TRAINS Matrix

| Term | Meaning |
|---|---|
| TRAINS | Transportation Reporting and Accounting Information System |
| Trains 4 | Team nickname for CGI Advantage 4 |
| Legacy Trains | Previous system in migration or reconciliation context |
