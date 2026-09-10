---
title: AFRS Transaction Codes Reference
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1540
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - wa-state-saam
related_docs:
  - https://ofm.wa.gov/tech-support/agency-financial-reporting-system/outbound-interface-server/
  - https://ofm.wa.gov/tech-support/agency-financial-reporting-system/transaction-codes/
  - https://ofm.wa.gov/tech-support/agency-financial-reporting-system/error-code-manual/
  - https://ofm.wa.gov/wp-content/uploads/sites/default/files/public/itsystems/afrs/Transaction_Code_table.xlsx
  - https://ofm.wa.gov/wp-content/uploads/sites/default/files/public/itsystems/afrs/errorCodeManual/appendixA.pdf
tags:
  - afrs
  - transaction-codes
  - error-codes
  - outbound-interface
  - washington-state
---
# AFRS Transaction Codes Reference

Agent uses this reference with `wa-state-afrs` when work needs outbound server patterns, transaction-code workbook cues, D51 Appendix A error handling, or source-system reconciliation detail.

## Decision Matrix: Interface Options

| Decision Topic | Option | Use | Team Pattern |
|---|---|---|---|
| Timing model | Near-real-time outbound query | Use when the team refreshes AFRSTitles and Agency Descriptor data from the AFRS Outbound Interface SGN Server during business-day processing. | Query reference tables, stamp extraction time, and hydrate agency staging tables for validation or lookup services. |
| Timing model | Scheduled batch export | Use when the team submits GL, AP, AR, or budget data to AFRS through governed files and control totals. | Build immutable batch snapshots, validate, transmit, acknowledge, and reconcile by batch ID. |
| Submission model | File-based AFRS submission | Use when the team sends data into AFRS-facing workflows governed by fixed-width or delimited layouts. | Keep file artifacts, trailers, counts, and totals as first-class records. |
| Submission model | Agency-internal API wrapper | Use when upstream applications need modern integration without changing the AFRS-facing file contract. | Accept JSON or queue payloads internally, validate, stage, and emit AFRS-compatible files downstream. |
| Reference-data access | Outbound SQL server | Use when the team needs Statewide Titles or Agency Descriptor data inside the SGN. | Query `AFRSTitles` through controlled jobs and avoid documented refresh or patch windows. |

## AFRS Outbound Interface Server Patterns

| Topic | OFM Pattern | Team Implementation |
|---|---|---|
| Access boundary | Access is limited to agencies within the SGN. | Place extraction jobs on approved SGN-connected infrastructure. |
| Security setup | Agency Active Directory group membership gates access. | Treat server access as environment configuration, not application business logic. |
| Server and database | DNS entry is `AFRSOutbound.ofm.wa.gov`; extraction uses the `AFRSTitles` database. | Externalize server and database names into environment-backed options. |
| Data scope | Server exposes Statewide Titles and Agency Descriptor tables for agency query workloads. | Materialize reference-data snapshots into local validation or crosswalk stores. |
| Update pattern | Data receives near-real-time updates during the day. | Track extraction timestamp and refresh provenance on every snapshot. |
| Refresh window | Full refresh runs Tuesday through Saturday at 1:00 a.m. and lasts about 15 minutes. | Block scheduled extraction during the documented refresh window. |
| Patch window | Server patching occurs monthly and repeated reboots occur during the patching window. | Add retry, alerting, and maintenance-calendar awareness to extraction jobs. |

## Transaction Code Workbook Structure

| Workbook Asset | Purpose | Team Use |
|---|---|---|
| `TC Lookup` | Direct code lookup with title and core debit or credit GL combinations. | Drive analyst lookup screens and golden-file unit tests. |
| `TC Listing` | Broader transaction-code catalog with titles and GL relationships. | Build searchable reference-data views for accountants and support staff. |
| `TC Variable GLs` | Lists allowable variable GL account substitutions for selected transaction codes. | Validate code and GL combinations before file build. |
| `TC Wraps` | Maps base transaction codes to current, prior, warrant-cancellation, and ACH-return wrap codes. | Derive correction and reversal logic without hard-coded switch statements. |
| `TC Reference Guide` | Companion worksheet for workbook usage. | Preserve as analyst documentation source when building support tooling. |

## Selected Transaction Code Cues

| Transaction Code | Workbook Title | Accounting Cue | Team Validation Focus |
|---|---|---|---|
| `001` | RCPT-RCRD REVENUE (TREA) | Treasury receipt records revenue. | Validate revenue source, cash, and fiscal-period alignment. |
| `003` | RECORD NSF OR ADJUST REVENUE (TRE) | Revenue reversal or NSF handling. | Validate reversal lineage and negative-impact balancing. |
| `012` | RCRD CURRENT ACCTS REC | Current accounts receivable setup. | Validate customer, receivable GL, and revenue crosswalks. |
| `025` | INTERFUND GL TRANSFER | Interfund transfer credit-side pattern. | Validate paired transfer legs and fund compatibility. |
| `026` | INTERFUND GL TRANSFER | Interfund transfer debit-side pattern. | Validate reciprocal posting and batch balancing. |
| `042` | RCRD RECEIPT-BENEFITS PYMT RETURNED | Returned-benefit receipt activity. | Validate payment-reference linkage and correction routing. |
| `260` | JV TRSF-AMT DUE-AGCY REIM (TREA) | Treasury reimbursement transfer with multiple GL legs. | Validate multi-leg balancing and due-to or due-from crosswalks. |
| `261` | BILL (NOT RCVD) AGCY REIM EXPEND | Agency reimbursement expenditure billing. | Validate receivable timing and reimbursement support data. |
| `290` | REVERSE ACCT/VOU PAY TC 210(TREA) | Payable reversal pattern. | Validate original transaction linkage and duplicate-prevention rules. |
| `345` | LCL TRSF INCR AGCY EXPEN | Local transfer increases agency expenditure. | Validate local-to-state crosswalk and expenditure authority context. |
| `669` | INTERFUND EXPENDITURE TRANSFER-IN | Expenditure transfer-in pattern. | Validate inbound fund, object, and offsetting transfer entries. |
| `670` | INTERFUND EXPENDITURE TRANSFER-DE | Expenditure transfer-out pattern. | Validate outbound fund, object, and reciprocal batch totals. |

## D51 Appendix A Error Code Handling

| Error Family | Example Codes | Failure Shape | Team Handling Pattern |
|---|---|---|---|
| `A` | `A01`, `A02`, `A10`, `A20` | Record existence, lookup, biennium, and fund-reference failures. | Route to master-data or period-setup correction queues. |
| `B` | `B01`-`B17` | Batch agency, batch date, type, number, count, and amount failures. | Fail the batch before resubmission and rebuild control totals from source snapshots. |
| `C` | `C01`-`C07` | Banking and payment-field failures such as routing, account, name, and transaction code mismatches. | Route to vendor or payment-master remediation before retransmit. |
| `D` | `D01`-`D76` | Core code-table failures for agency, object, subobject, fund, appropriation, and GL fields. | Resolve crosswalk or reference-data gaps, then rerun validation. |
| `DS` | `DS1`-`DS6` | Program index, allocation, schedule, organization index, and appropriation index failures. | Reconcile source-system dimension values to AFRSTitles before file build. |
| `DD` | `DD1`-`DD4` | Fiscal-year and month-of-service format or biennium-context failures. | Correct fiscal-dimension mapping and re-stage the batch. |

## Common Validation and Error Patterns

| Validation Area | Common Error Shape | Example Code Cue | Team Fix |
|---|---|---|---|
| Batch controls | Batch totals or counts disagree with detail rows. | `B07`, `B08`, `B14` | Recompute totals from immutable detail rows and regenerate the trailer. |
| Period mapping | Biennium or fiscal month does not match the source posting context. | `A10`, `B05`, `B06`, `DD1`, `DD2` | Resolve posting date, fiscal month, and biennium derivation before export. |
| Code crosswalks | Object, fund, appropriation, GL, or organization values are missing or invalid. | `D22`, `D30`, `D54`, `D56`, `D62`, `DS5`, `DS6` | Refresh AFRSTitles snapshots and correct source-to-state crosswalk tables. |
| Payment data | Bank routing, account, or transaction code values are inconsistent. | `C01`-`C07` | Revalidate vendor payment master data and outbound payment formatting. |
| Duplicate submissions | Batch key or duplicate indicators collide with prior runs. | `B10`, `B11`, `B12`, `B16` | Tie resubmissions to original batch lineage and generate governed replacement batch identifiers. |

## Reconciliation with CGI Advantage or TRAINS 4

| Reconciliation Topic | Team Pattern | Pass Target |
|---|---|---|
| Source journal to AFRS batch | Persist source document ID, journal ID, and batch ID on every export row. | One-to-one lineage exists from source entry to AFRS response. |
| Vendor and customer crosswalks | Resolve local vendor, customer, or payee identifiers to statewide codes before file build. | No unresolved crosswalk remains in a submitted batch. |
| Budget and organization mapping | Map local budget units, organizations, and appropriation structures to AFRS-coded dimensions through effective-dated tables. | Source totals and AFRS-coded totals agree by period and organizational grain. |
| Transaction-code assignment | Store selected AFRS transaction code, workbook source, and selection rationale in validation output. | Reviewers trace every generated line to one governed transaction-code choice. |
| Reject replay | Keep original payload, corrected payload, acknowledgement, and resubmission links together. | Corrected replay does not lose prior rejection evidence. |

## .NET Integration Patterns

| Concern | Team Pattern | Output |
|---|---|---|
| Reference-data ingestion | Use background jobs that read outbound SQL snapshots into EF Core tables with extraction timestamp and source version. | Effective-dated AFRS title cache |
| File generation | Represent each record type with deterministic field maps and invariant formatting services. | Repeatable fixed-width or delimited files |
| Validation pipeline | Split structural, fiscal, code, and balance validation into composable services. | Machine-readable validation result set |
| Error-code normalization | Map D51 code, title, family, element number, and severity into strongly typed application results. | Queryable rejection and correction history |
| Submission orchestration | Use commands for build, validate, submit, acknowledge, reject, correct, and replay stages. | Traceable batch state machine |
