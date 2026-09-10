---
name: wa-state-saam
title: Washington State SAAM
description: Apply Washington State Administrative and Accounting Manual patterns when work needs SAAM-aligned policy mapping, AFRS coding, appropriation control, or statewide compliance logic.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1377
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - public-sector-accounting
  - fund-accounting-patterns
  - budget-to-actual-tracking
  - financial-reporting-gaap
  - fabric-powerbi-integration
  - efcore-dbcontext-design
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx}'
tags:
  - accounting
  - washington-state
  - saam
  - afrs
  - compliance
related_docs:
  - https://ofm.wa.gov/accounting/saam/
---
# Washington State SAAM

Agent applies Washington State Administrative and Accounting Manual patterns with explicit policy, coding, fiscal period, appropriation, payment, and control mappings.

## When to Use

| Condition | Use |
|---|---|
| Prompts need Washington State accounting policy alignment | Use this skill |
| Work items need AFRS-oriented coding or statewide reporting structure | Use this skill |
| Budget, allotment, or appropriation tracking needs SAAM vocabulary | Use this skill |
| Vendor payment or internal control flow needs state-specific treatment | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work items need broad governmental accounting guidance with no Washington-specific policy | Use `public-sector-accounting` |
| Work items focus on generic fund structures and interfund patterns | Use `fund-accounting-patterns` |
| Work items focus on budget execution analytics outside SAAM policy mapping | Use `budget-to-actual-tracking` |
| Work items focus on general GAAP statement assembly only | Use `financial-reporting-gaap` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Agency context | Yes | Record agency, business area, and statewide reporting obligations. |
| Policy objective | Yes | Record transaction, close, reporting, control, or integration scope. |
| Coding scope | Yes | Record appropriation, allotment, object code, revenue source, and vendor data. |
| Fiscal period | Yes | Record fiscal year, accounting period, and close timing. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps the work item to SAAM policy sections, statewide coding elements, and the affected fiscal period. | Review intake tables. | Each requirement maps to one policy topic, one coding scope, and one period context. |
| 2 | Agent identifies AFRS-facing data elements and control points. | Review code and data contracts. | Required fields, edit logic, and control metadata appear in the design. |
| 3 | Agent applies appropriation, allotment, and object-code rules to the posting or reporting flow. | Review transaction and budget schemas. | Budget authority and coding rules stay explicit and traceable. |
| 4 | Agent aligns payment, reconciliation, and internal-control evidence paths. | Review payment workflow and audit fields. | Payment processing preserves approval, separation, and traceability data. |
| 5 | Agent validates persistence and reporting integration. | Review EF Core entities and reporting datasets. | Operational, compliance, and reporting layers share a consistent vocabulary and grain. |

## SAAM Policy Matrix

| Domain | Primary Rule | Deep Reference |
|---|---|---|
| Policy baseline | SAAM sets statewide minimum standards and agencies layer stricter detail without breaking the baseline. | `references/policy-reference.md#saam-chapter-matrix` |
| AFRS contracts | Agency transactions flow through statewide structures, mandatory codes, and validation rules. | `references/policy-reference.md#statewide-accounting-reference` |
| Appropriation control | Budget authority, allotment, reserve, and close workflows remain explicit. | `references/policy-reference.md#saam-integration` |
| Code governance | Mandatory statewide codes remain governed and effective-dated. | `references/policy-reference.md#saam-integration` |
| Control evidence | Internal control covers operations, reporting, and compliance objectives. | `references/policy-reference.md#public-sector-specific-patterns` |

## Reference Files

| File | Purpose |
|---|---|
| [references/policy-reference.md](references/policy-reference.md) | SAAM chapter mappings, statewide accounting reference tables, integration guidance, pitfalls, outputs, and further reading. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Policy traceability | Review rules and workflow definitions. | Each SAAM-specific behavior references a policy area and effective period. |
| AFRS readiness | Review import and export contracts. | Required statewide coding fields and validation rules exist for the target flow. |
| Fiscal period control | Test open, close, and lapse period scenarios. | Transactions honor the intended period state and effective-date rules. |
| Appropriation control | Compare transaction, allotment, and authority views. | Spending logic respects authority and preserves variance visibility. |
| Code validity | Test active, inactive, and misdated code scenarios. | Validation accepts only codes valid for the transaction date. |
| Payment and control evidence | Review payment approval and exception data. | Approval, separation, and audit evidence remain complete and queryable. |
