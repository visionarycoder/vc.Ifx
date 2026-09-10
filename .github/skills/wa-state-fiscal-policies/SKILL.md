---
name: wa-state-fiscal-policies
title: Washington State Fiscal Policies
description: >
  Apply Washington State fiscal-year, biennium, appropriation, allotment, coding, 
  vendor-payment, closeout, and reference-data patterns when work needs statewide 
  budget and accounting policy alignment.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1460
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - wa-state-saam
related_skills:
  - wa-state-saam
  - public-sector-accounting
  - fund-accounting-patterns
  - budget-to-actual-tracking
  - efcore-dbcontext-design
  - reconciliation-patterns
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx,yml,yaml}'
tags:
  - accounting
  - washington-state
  - fiscal-policy
  - budget
  - appropriation
  - allotment
  - reference-data
related_docs:
  - https://ofm.wa.gov/budget/how-it-works/
  - https://ofm.wa.gov/accounting/saam/
  - https://ofm.wa.gov/accounting/fund-reference-manual/
---
# Washington State Fiscal Policies

Agent applies Washington State fiscal policy patterns with explicit fiscal calendar, biennium, appropriation, allotment, code governance, payment, lapse-year, and closeout rules.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs Washington State fiscal year or biennium logic | Use this skill |
| Work item needs appropriation, allotment, or authority tracking rules | Use this skill |
| Data models need Washington State object, revenue, fund, agency, or organization reference structures | Use this skill |
| Workflows need statewide vendor-payment, lapse-year, or budget-closeout treatment | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item focuses on AFRS batch files and interface layouts | Use `wa-state-afrs` |
| Work item focuses on SAAM policy detail with no fiscal calendar or code-governance design | Use `wa-state-saam` |
| Work item focuses on generic public-sector accounting outside Washington State rules | Use `public-sector-accounting` |
| Work item focuses on fund structure only | Use `fund-accounting-patterns` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Agency and program context | Yes | Record agency, organization, program, and statewide reporting obligations. |
| Fiscal objective | Yes | Record budget, allotment, payment, coding, closeout, or reporting scope. |
| Period scope | Yes | Record fiscal year, biennium, accounting period, and lapse-year status. |
| Code domains | Yes | Record object, revenue, fund, agency, organization, and vendor domains. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps the request to one fiscal calendar context, one biennium context, and one authority path. | Review intake tables. | Each request maps to one period model and one authority model. |
| 2 | Agent identifies the statewide code families and effective-dated reference tables in scope. | Review code inventory. | Every required code domain maps to one governed source. |
| 3 | Agent applies appropriation, allotment, and organization rules to transactions, reports, or workflows. | Review schema and business rules. | Authority and organizational context stay explicit and queryable. |
| 4 | Agent maps payment, lapse-year, and closeout logic to the selected fiscal window. | Review period-close workflow. | Payment and closeout paths respect fiscal timing and authority status. |
| 5 | Agent verifies reconciliation, reporting, and audit-trace outcomes against the fiscal model. | Review reports and exception paths. | Budget, coding, and closeout outputs reconcile to the governed fiscal context. |

## Fiscal Policy Decision Matrix

| Domain | Primary Rule | Deep Reference |
|---|---|---|
| Fiscal calendar | Fiscal years run July 1 through June 30 and biennia begin July 1 of odd-numbered years. | `references/policy-reference.md#fiscal-calendar-reference` |
| Authority and allotment | Appropriation, allotment, and actuals remain distinct control layers. | `references/policy-reference.md#authority-and-allotment-reference` |
| Code families | Object, revenue, fund, agency, and organization codes remain governed and effective-dated. | `references/policy-reference.md#code-family-reference` |
| Payment and closeout | Vendor payment, lapse-year activity, reconciliation, and closeout follow fiscal timing. | `references/policy-reference.md#payment-and-closeout-reference` |
| File formats | Reference extracts, crosswalks, and snapshots remain deterministic and versioned. | `references/policy-reference.md#file-format-specifications` |

## Reference Files

| File | Purpose |
|---|---|
| [references/policy-reference.md](references/policy-reference.md) | Fiscal calendar, authority, code-family, payment, closeout, file-format, integration, pitfalls, and output reference tables. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Fiscal calendar | Test July boundary, June close, and odd-year biennium rollover scenarios. | Fiscal year and biennium assignment stay correct for all sample dates. |
| Authority layering | Compare appropriation, allotment, and actual datasets. | Each layer remains distinct and reconcilable. |
| Code validity | Test active, inactive, future-dated, and expired codes. | Validation accepts only date-valid codes and mappings. |
| Organization history | Test historical hierarchy lookups across reorganizations. | Prior-period reports retain the correct historical organization path. |
| Lapse-year logic | Test current-year and lapse-year close scenarios. | Adjustments land in the correct fiscal bucket and remain separately reportable. |
| Audit traceability | Review reference-data provenance and snapshot lineage. | Each code value and close snapshot links to one governed source and date. |
