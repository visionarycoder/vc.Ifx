---
name: wa-state-afrs
title: Washington State AFRS
description: Apply Washington State Agency Financial Reporting System patterns for governed interfaces, file layouts, validation, reconciliation, and .NET batch integration.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1571
prerequisites:
  - wa-state-saam
related_skills:
  - wa-state-saam
  - public-sector-accounting
  - budget-to-actual-tracking
  - reconciliation-patterns
  - efcore-dbcontext-design
  - csv-ingestion-diagnostics
appliesTo: '**/*.{cs,sql,json,md,csv,txt,xlsx,xml}'
tags:
  - accounting
  - washington-state
  - afrs
  - gl
  - ap
  - ar
  - reconciliation
  - batch-integration
related_docs:
  - https://ofm.wa.gov/tech-support/agency-financial-reporting-system/
  - https://ofm.wa.gov/accounting/saam/
---
# Washington State AFRS

Agent applies AFRS patterns with explicit statewide coding, controlled file handling, and traceable reconciliation.

## When to Use

| Condition | Use |
|---|---|
| Work needs AFRS-facing file generation or file intake | Use this skill |
| Work needs GL, AP, AR, budget, or report extract mapping | Use this skill |
| Work needs statewide validation or agency-to-AFRS reconciliation | Use this skill |
| Work needs `.NET` batch services for AFRS flows | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on policy interpretation only | Use `wa-state-saam` |
| Work focuses on broad governmental accounting only | Use `public-sector-accounting` |
| Work focuses on budget controls without AFRS interfaces | Use `budget-to-actual-tracking` |
| Work focuses on generic reconciliation only | Use `reconciliation-patterns` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Agency context | Yes | Agent records agency, business area, and interface owner. |
| Interface scope | Yes | Agent records GL, AP, AR, budget, report extract, or mixed scope. |
| Layout source | Yes | Agent records the governing OFM layout or reference source. |
| Fiscal period | Yes | Agent records fiscal year, accounting period, and close window. |
| Reconciliation target | No | Agent records the tie-out view and exception owner. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects one interface path, one period context, and one statewide code set. | Review intake row. | One primary path is selected. |
| 2 | Agent records the authoritative layout, record types, control totals, and effective-dated references. | Review specification set. | Layout source and effective date stay explicit. |
| 3 | Agent models deterministic generation or intake for ordering, width, delimiter, padding, and trailer controls. | Review parser or serializer design. | File structure matches the selected layout. |
| 4 | Agent applies structural, fiscal, coding, and balancing validation rules. | Review validation results. | Validation covers structural and business rules. |
| 5 | Agent maps acknowledgements, D51-style errors, and resubmission flows to agency reconciliation. | Review exception workflow. | Every reject links to one owner and one correction path. |

## Rule Matrix

| Concern | Rule | Reference |
|---|---|---|
| System role | Agent treats AFRS as a governed statewide accounting authority. | `references/afrs-file-formats.md` |
| Layouts | Agent binds each run to one explicit layout version and effective date. | `references/afrs-file-formats.md` |
| Codes | Agent validates effective-dated statewide codes before posting or export. | `references/afrs-transaction-codes.md` |
| Batch controls | Agent persists counts, totals, source snapshot, and submission status per batch. | `references/afrs-file-formats.md` |
| Correction cycle | Agent stores acknowledgement, reject, correction, and resubmission lineage. | `references/afrs-transaction-codes.md` |
| Policy linkage | Agent links interface validations to SAAM-backed rules where they apply. | Related docs |

## Pattern Matrix

| Pattern | Use | Output |
|---|---|---|
| Fixed-width file | OFM layout uses positions and widths | Deterministic serializer or parser with golden-file tests |
| Delimited file | OFM layout uses declared order and delimiter rules | Schema-driven serializer or parser |
| Header and trailer control | Batch carries metadata and totals | First-class batch records and control-total validation |
| Reference-data refresh | Titles, descriptors, and code tables drive validation | Versioned reference snapshot before processing |
| Internal wrapper | Agency app stages or validates AFRS-compatible files | Modern boundary without changing AFRS file compatibility |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Layout compliance | Run parser or serializer tests against approved samples. | Output matches the selected AFRS layout exactly. |
| Code validity | Test active, inactive, and misdated codes. | Only valid codes for the submission date pass. |
| Control totals | Test counts, amounts, and balancing logic. | Header, detail, and trailer totals agree. |
| Period control | Test open, close, and reject-window scenarios. | Submission logic honors the selected fiscal window. |
| Reconciliation | Compare agency totals to AFRS-facing totals at each control point. | Differences resolve to zero or one approved exception record. |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Path selected | One interface, one period, and one code set lead the design. |
| Layout versioned | Every run records the governing layout version. |
| Batch trace preserved | Source, transmit, acknowledge, and correction states stay linked. |
| Reference detail preserved | File and transaction reference tables remain in sibling reference files. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Local layouts drift from OFM specifications | Agent versions every layout and binds each run to it. |
| Leading zeros or sign placement disappear | Agent centralizes field formatting rules. |
| Batch lineage lives outside the application record | Agent persists acknowledgement and correction history in the workflow store. |
| Reconciliation starts only after close | Agent adds batch-level control points before close. |
