---
title: CGI Advantage Integration Reference
description: Detailed module, transport, AFRS, and resilience reference tables for the CGI Advantage Integration skill.
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 776
prerequisites:
  - cgi-advantage-integration
related_skills:
  - cgi-advantage-integration
appliesTo: '.github/skills/cgi-advantage-integration/**/*'
tags:
  - cgi-advantage
  - trains-4
  - afrs
  - reference
---
# CGI Advantage Integration Reference

Agent uses this file for deep module detail after the main skill selects the integration path.

## Module Matrix

| Module | Focus | Notes |
|---|---|---|
| General Accounting | Journal, posting, and trial-balance activity | Preserve period, batch, and ledger lineage. |
| Accounts Payable | Vendor obligations and disbursement events | Validate vendor identity and payment status before posting. |
| Accounts Receivable | Receipts, invoices, and receivable adjustments | Keep customer activity separate from settlement mappings. |
| Vendor management | Vendor master and remit data | Keep duplicate and inactive-vendor controls explicit. |
| Purchase orders | Encumbrance and procurement linkage | Preserve requisition-to-disbursement lineage. |
| COA Crosswalk | Segment normalization | Use one governed source-to-target crosswalk. |
| Batch processing | Scheduled inbound and outbound movement | Separate chain jobs from low-latency calls. |

## Transport Matrix

| Pattern | Strength | Guardrail |
|---|---|---|
| Real-time API | Low latency and immediate validation | Reject bulk-history movement on this path. |
| Batch API | Shared contract plus resumable pagination | Persist checkpoint and watermark state. |
| File exchange | High throughput and operator visibility | Validate file naming, encoding, row counts, and control totals. |
| Hybrid flow | Best channel per entity | Record one authority rule per entity. |
| Bus relay | Internal fan-out | Emit immutable envelopes and duplicate keys. |

## AFRS Matrix

| Stage | Focus | Pass Target |
|---|---|---|
| Extract | Posted GL activity only | Draft activity remains excluded. |
| Transform | Segment and journal mapping | Required AFRS fields map once. |
| Validate | Counts, totals, and period controls | Control totals reconcile before release. |
| Submit | Automated, manual, or hybrid release | One traceable batch exists per run. |
| Acknowledge | Accepted, warning, or reject outcomes | Acknowledgement links to the source batch. |
| Reconcile | Source, transformed, submitted, and acknowledged totals | Differences resolve to zero or one approved exception record. |

## Resilience Matrix

| Concern | Rule |
|---|---|
| Secrets | Store credentials outside source files. |
| Retry | Retry idempotent reads and header-guided transient failures only. |
| Timeout | Set explicit connect, request, and processing budgets. |
| Replay | Store checkpoint, source key, and correlation identifiers. |
| Error shape | Normalize auth, validation, and posting failures into stable internal result types. |
