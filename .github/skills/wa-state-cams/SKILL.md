---
name: wa-state-cams
title: Washington State CAMS
description: Apply Washington State Capital Asset Management System patterns for capitalization, registration, depreciation, lifecycle tracking, reconciliation, and controlled integration.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1562
prerequisites:
  - wa-state-saam
  - wa-state-afrs
related_skills:
  - wa-state-systems-bundle
  - wa-state-saam
  - wa-state-afrs
  - cgi-advantage-integration
  - public-sector-accounting
appliesTo: '**/*.{cs,csproj,json,xml,csv,txt,md,sql,xlsx}'
tags:
  - washington-state
  - cams
  - capital-assets
  - depreciation
  - afrs
related_docs:
  - https://ofm.wa.gov/tech-support/capital-asset-management-system/
  - https://ofm.wa.gov/accounting/saam/
---
# Washington State CAMS

Agent applies CAMS patterns with explicit capitalization, asset-register, depreciation, inventory, and reconciliation rules.

## When to Use

| Condition | Use |
|---|---|
| Work needs CAMS-aligned asset setup, change, disposal, or reporting | Use this skill |
| Work needs capitalization thresholds or class-code useful life | Use this skill |
| Work needs AFRS-to-CAMS pending asset intake or reconciliation | Use this skill |
| Work needs `.NET` file generation or report extracts for CAMS flows | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on SAAM policy only | Use `wa-state-saam` |
| Work focuses on AFRS file balancing only | Use `wa-state-afrs` |
| Work focuses on broad governmental reporting only | Use `public-sector-accounting` |
| Work focuses on source ERP exchange only | Use `cgi-advantage-integration` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Agency asset context | Yes | Agent records agency, fund, organization, and reporting scope. |
| Lifecycle event | Yes | Agent records acquisition, change, transfer, disposal, or impairment. |
| Asset classification | Yes | Agent records class code, ownership code, and threshold path. |
| Interface scope | Yes | Agent records direct entry, AFRS pending flow, extract, or report flow. |
| Reporting period | Yes | Agent records fiscal month, biennium, year-end, or annual-report cycle. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps the work item to one lifecycle event, one capitalization path, and one reporting period. | Review intake row. | One event, threshold row, and period row are selected. |
| 2 | Agent records required fields, SAAM class rules, and interface touchpoints. | Review field matrix. | Required fields and source systems stay explicit. |
| 3 | Agent applies registration, useful-life, depreciation, and inventory rules. | Review record design. | Record design includes required fields and depreciation basis. |
| 4 | Agent maps AFRS, Advantage, and project-acquisition data into CAMS registration and reconciliation outputs. | Review crosswalk. | Source lineage stays traceable. |
| 5 | Agent validates file formats, control totals, and report tie-out paths. | Run targeted tests. | Artifacts parse correctly and reconcile to source totals. |

## Rule Matrix

| Concern | Rule | Reference |
|---|---|---|
| System role | Agent treats CAMS as the governed capital-asset register. | `references/cams-reference.md` |
| Thresholds | Agent applies the SAAM-backed capitalization threshold before registration design. | `references/cams-reference.md` |
| Depreciation | Agent keeps CAMS as the book-of-record straight-line path unless a separate analytical view exists. | `references/cams-reference.md` |
| Inventory control | Agent preserves tag, location, serial, and custody data for traceable assets. | `references/cams-reference.md` |
| Pending intake | Agent treats AFRS-routed pending assets as governed staging records. | `references/cams-reference.md` |
| Reporting | Agent preserves period-close snapshots for CAMS-to-AFRS and annual-report support. | `references/cams-reference.md` |

## Pattern Matrix

| Pattern | Use | Output |
|---|---|---|
| Direct CAMS entry | Asset registration or field maintenance | Record contract with class and ownership validation |
| AFRS pending feed | Skeleton asset creation from routed transactions | Batch-lineage-aware intake workflow |
| Master record management | Fixed-field asset record maintenance | Deterministic serializer and field-length guards |
| Report extract | CM-series reporting and reconciliation views | Stable export schema and query definition |
| Useful-life override | Approved used or degraded asset treatment | Override evidence linked to the asset record |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Threshold alignment | Compare asset class and amount to the threshold table. | Asset follows the correct threshold row. |
| Registration completeness | Review required fields for the selected category. | Required identifiers, ownership, location, and cost fields exist. |
| Depreciation accuracy | Recalculate straight-line or exception schedule. | Monthly and remaining-life values tie to CAMS rules. |
| Pending file integrity | Compare routed records to completed asset records. | Pending records convert without orphan fields. |
| Disposal and reporting tie-out | Compare disposal activity to report outputs. | Disposal dates, authority, and report totals align. |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Lifecycle selected | One lifecycle row leads the workflow. |
| Threshold applied | One capitalization row governs the record. |
| Book-of-record preserved | Straight-line CAMS values remain authoritative. |
| Reference detail preserved | Threshold, field, and report detail lives in the reference file. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Acquisition date replaces in-service intent | Agent records depreciation intent and reconciles initial timing. |
| Additions overwrite audit history | Agent uses lineage-preserving updates. |
| Small and attractive assets enter capital depreciation | Agent separates inventory-only treatment from capitalization. |
| Useful-life overrides lack approval evidence | Agent stores the override basis beside the asset. |
