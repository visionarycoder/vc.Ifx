---
title: Washington State CAMS Reference
description: Detailed threshold, lifecycle, depreciation, and reporting reference tables for the Washington State CAMS skill.
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 666
prerequisites:
  - wa-state-cams
related_skills:
  - wa-state-cams
appliesTo: '.github/skills/wa-state-cams/**/*'
tags:
  - washington-state
  - cams
  - reference
---
# Washington State CAMS Reference

Agent uses this file for deep CAMS detail after the main skill selects the lifecycle and integration path.

## Threshold Matrix

| Asset Category | Threshold Pattern | Note |
|---|---|---|
| Land | All | Register for reporting and custody tracking. |
| Highway systems | All | Preserve infrastructure reporting lineage. |
| Infrastructure | Greater than $100,000 | Preserve project traceability and lifecycle history. |
| Buildings and improvements | Greater than $100,000 | Preserve long-life depreciation support. |
| Intangible assets | Greater than $1,000,000 | Preserve valuation and impairment detail. |
| Other capital assets | Unit cost greater than $10,000 | Apply standard capital-asset accounting. |
| Small and attractive assets | Agency-defined plus all weapons and firearms | Track custody outside standard depreciation when required. |

## Depreciation Matrix

| Condition | Treatment |
|---|---|
| Standard capital asset | Straight-line depreciation from governed useful life. |
| Early in-service reality | Initial catch-up plus ongoing straight-line depreciation. |
| Cost or useful-life change | Recalculate remaining depreciation with preserved change history. |
| Disposal | Stop future depreciation and preserve final tie-out data. |
| Inventory-only or nondepreciable asset | Keep accountability record without standard depreciation. |

## Lifecycle Matrix

| Stage | Required Data |
|---|---|
| Acquisition | Acquisition date, fund, document number, total cost, and class. |
| In service | Useful life, location, ownership, and trace fields. |
| Change | Revised field values plus lineage note. |
| Transfer | Location, organization, or custody change metadata. |
| Disposal | Disposal date, authority, and reason context. |
| Impairment review | Condition, valuation note, and supporting evidence. |

## Reporting Matrix

| Need | Output |
|---|---|
| Annual-report support | Capital-asset rollforward set |
| Infrastructure reporting | Category-aligned lifecycle support |
| CAMS-to-AFRS reconciliation | Reconciliation workbook inputs |
| Capital project tracking | Project-to-asset crosswalk |
| Impairment review | Valuation support package |
