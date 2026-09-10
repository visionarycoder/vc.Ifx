---
title: AFRS File Formats Reference
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 880
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - wa-state-afrs
related_docs:
  - https://ofm.wa.gov/tech-support/agency-financial-reporting-system/documentation/
  - https://ofm.wa.gov/tech-support/agency-financial-reporting-system/outbound-interface-server/
tags:
  - afrs
  - file-formats
  - fixed-width
  - delimited
  - washington-state
---
# AFRS File Formats Reference

Agent uses this reference with `wa-state-afrs` when work needs AFRS file-shape rules, record layouts, field formatting, or batch-control validation.

## Format Decision Matrix

| Format Area | Use | Team Pattern |
|---|---|---|
| Fixed-width submission | OFM layout defines byte positions and exact field widths. | Model start, width, padding, sign, and implied-decimal rules as versioned metadata. |
| Delimited submission | OFM layout defines ordered fields with delimiter and quoting rules. | Model delimiter, quote, empty-field, and trailer behavior explicitly. |
| Header-detail-trailer batch | Interface requires batch metadata and control totals. | Persist header, detail, trailer, and acknowledgement lineage by batch ID. |
| Reference-data extract | AFRS outbound query result feeds local validation. | Stamp extraction time, source table, and effective period on every snapshot. |

## Record Layout Reference

| Layout Topic | AFRS Pattern | Validation Cue |
|---|---|---|
| Record type | Each row maps to one published record type. | Reject unknown or misordered record types. |
| Field position | Fixed-width layouts use exact start and length values. | Compare emitted length to the published layout on every row. |
| Field order | Delimited layouts use one governed column order. | Reject missing, extra, or shifted columns. |
| Filler and reserved fields | Reserved space stays present even when business content is blank. | Preserve filler width and default content exactly. |
| Leading zeros | Codes and identifiers preserve leading zeros. | Treat code fields as formatted text, not numeric values. |
| Signed numeric fields | Sign and implied decimals follow the published rule. | Validate sign location, precision, and rounding before write. |

## Common Field Definitions

| Field Type | Pattern | Team Rule |
|---|---|---|
| Agency, fund, appropriation, object, and revenue codes | Effective-dated code values with exact length rules | Validate active status and text length before serialization. |
| Amount fields | Scaled numeric values with declared sign handling | Centralize formatting in one reusable formatter. |
| Dates | Published state date shape for the interface | Validate the period context before format conversion. |
| Batch identifiers | Unique submission lineage value | Tie every emitted file to one immutable batch record. |
| Reference text | Published maximum length and allowed character set | Normalize disallowed characters before export. |

## Control Totals and Trailers

| Control | Purpose | Pass Target |
|---|---|---|
| Record count | Confirms header, detail, and trailer agreement | Stored detail count equals trailer count. |
| Amount total | Confirms financial completeness | Detail total equals trailer total. |
| Balance rule | Confirms paired debit and credit or equivalent balancing logic | Difference resolves to zero or an approved rule. |
| Batch lineage | Confirms resubmission traceability | Original and replacement batch links remain queryable. |

## Validation Cues

| Validation Area | Failure Shape | Team Handling |
|---|---|---|
| Structural | Wrong width, delimiter count, record type, or reserved-field content | Fail before transmission and store machine-readable errors. |
| Fiscal | Closed period, wrong fiscal year, or wrong accounting month | Hold the batch and route it to fiscal correction. |
| Code | Inactive, unknown, or incompatible statewide code | Refresh reference data and correct the source crosswalk. |
| Totals | Trailer, count, or balance mismatch | Recompute from immutable detail rows and rebuild the file. |
| Acknowledgement | Accept, warning, or reject response from AFRS | Persist outcome and tie it to resubmission lineage. |

## Common Failure Patterns

| Pitfall | Agent Fix |
|---|---|
| Serializer trims leading zeros | Agent formats governed codes as text. |
| Trailer math derives from mutable working rows | Agent computes totals from immutable staged detail rows. |
| One parser handles multiple layout versions with hidden branches | Agent versions the layout metadata explicitly. |
| Error handling drops the original file image | Agent stores the source payload beside the response lineage. |

