---
title: AFRS File Specifications
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 5600
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - wa-state-afrs
related_skills:
  - wa-state-afrs
  - wa-state-saam
  - copybook-to-dotnet
related_docs:
  - .github/skills/wa-state-afrs/references/afrs-file-formats.md
  - .github/skills/wa-state-afrs/references/afrs-transaction-codes.md
  - docs/references/AFRS-Appendix-A-D51-Error-Code-Message.pdf
  - docs/references/Transaction_Code_table.xlsx
tags:
  - afrs
  - washington-state
  - file-formats
  - fixed-width
  - integration
---
# AFRS File Specifications

Agent uses this reference with `wa-state-afrs` when work needs one governed AFRS batch-file contract, one outbound AFO record map, one D51 reject catalog, or one .NET implementation pattern for deterministic fixed-width processing.

## Source Hierarchy and Scope

| Source | Role | Team Use |
|---|---|---|
| `references/afrs-file-formats.md` | Core file-shape guidance | Establishes fixed-width, delimiter, padding, and control-total rules. |
| `references/afrs-transaction-codes.md` | Transaction-code and reject-handling guidance | Establishes workbook usage, wraps, variable GL references, and reject lineage patterns. |
| `docs/references/AFRS-Appendix-A-D51-Error-Code-Message.pdf` | D51 error title catalog | Supplies reject codes, titles, element references, and severity cues. |
| `docs/references/Transaction_Code_table.xlsx` | Transaction-code workbook | Supplies code titles, GL combinations, variable GL references, wrap relationships, and field-presence cues. |
| OFM AFRS Batch Interface guide, October 2025 | Batch header and transaction layout authority | Supplies 950-byte layout, processing window, batch sequencing, and outbound transaction-type indicators. |

## Interface Control Matrix

| Topic | AFRS Rule | Observable Pass |
|---|---|---|
| Record size | Every header and detail record is 950 characters. | Every emitted line length equals `950`. |
| File type | Batch interface uses fixed-width records with one optional header per batch and one or more detail records. | Header appears first when present and no detail precedes it. |
| Null handling | Fields marked optional or not used stay blank-filled. No low values or null characters enter the file. | Serialized bytes contain spaces in unused positions and contain zero null bytes. |
| Batch identity | Batch Agency, Batch Date, Batch Type, and Batch Number identify one batch. | One file scan finds one consistent key per batch. |
| Detail identity | Detail rows share the batch key and differ by sequence number. | Sequence numbers are unique inside the batch. |
| Resubmission | Replacement batches use a new batch identifier and keep lineage to the rejected batch. | Audit data links original batch ID, reject report, corrected batch ID, and retransmit time. |

## Fixed-Width Structural Rules

| Rule Area | Rule | Validation Action |
|---|---|---|
| Text alignment | Code and reference fields preserve leading zeros and left-justified text rules. | Validate exact string width before write. |
| Numeric alignment | Amount and count fields use right alignment with left zero fill and implied decimals. | Reject decimal points, commas, currency symbols, and mis-sized numeric text. |
| Date shapes | Header and transaction dates use `YYMMDD`. Invoice date uses `CCYYMMDD`. | Parse dates by field contract, not by a generic date parser. |
| Filler ranges | Reserved and internal-use ranges remain blank-filled. | Compare reserved slices to space-filled constants. |
| Header sequence | Header `BATCH-SEQ-NO` equals `00000`. | Reject any non-zero header sequence. |
| Detail sequence | Detail `BATCH-SEQ-NO` starts at `00001` and increments without repeats. | Compare actual sequence set to expected contiguous values. |

## Batch Header Record Layout

The batch header occupies bytes `001-050`. Bytes `051-950` stay blank-filled.

| Positions | Field | Size | Use | Validation Cue |
|---|---|---:|---|---|
| `001-004` | `BATCH-AGENCY` | 4 | Submitting agency | Validate numeric or governed agency text exactly as OFM assigned. |
| `005-010` | `BATCH-DATE` (`YYMMDD`) | 6 | Batch creation date | Validate one real calendar date. |
| `011-012` | `BATCH-TYPE` | 2 | Source document and routing type | Validate against agency-approved type list. |
| `013-015` | `BATCH-NO` | 3 | Daily batch sequence | Start with `001` and increment for successive same-day batches. |
| `016` | `DUP-RECORD-IND` | 1 | System-assigned duplicate overflow indicator | Leave blank on inbound files. |
| `017` | `TRANSACTION-TYPE` | 1 | Interface record type | Inbound header value is `A`. |
| `018-022` | `BATCH-SEQ-NO` | 5 | Header sequence | Set to `00000`. |
| `023-024` | `BIENNIUM` | 2 | Ending year of the posting biennium | Validate odd-year biennium convention. |
| `025-026` | `FISCAL-MONTH` | 2 | Input fiscal month | Accept `01`-`24`, plus `99` and `25` for continuation use. |
| `027-032` | `BATCH-DUE-DATE` | 6 | Default warrant date for payment-producing rows | Leave blank when no payment timing applies. |
| `033-037` | `BATCH-COUNT` | 5 | Detail row count | Count excludes the header. |
| `038-050` | `BATCH-AMOUNT` | 13 | Total of detail `TRANS-AMT` values | Accept penny value `0000000000001` only when the team intends AFRS to calculate the amount. |

## Transaction Record Layout Map

The transaction record uses one common 950-byte envelope. Teams model the layout in sections so one serializer and one parser validate position rules deterministically.

### Section 1: Batch Key and Posting Identity

| Positions | Field | Size | Use |
|---|---|---:|---|
| `001-022` | Batch key and sequence area | 22 | Same fields as header except detail sequence varies per row. |
| `023-025` | `TRANS-CODE` | 3 | AFRS transaction code from the workbook. |
| `026` | `MODIFIER` | 1 | Transaction-code-controlled encumbrance liquidation indicator. |
| `027` | `REVERSE` | 1 | Reverses posting sign, not the GL code. |
| `028-031` | `BIENNIUM` and batch `FM` | 4 | Posting biennium and input fiscal month. |
| `032-033` | `OPS-POST-FM` | 2 | Optional posting fiscal month override. |
| `034-037` | `ORG` and `SUB-ORG` | 4 | Posting agency and sub-agency. |

### Section 2: Coding Dimensions

| Positions | Field | Size | Use |
|---|---|---:|---|
| `038-045` | `MASTER-INDEX` | 8 | Optional reduction key for predefined coding combinations. |
| `046-048` | `APPN-INDEX` | 3 | Expenditure authority key. |
| `049-051` | `FUND` | 3 | Accounting fund. |
| `054-058` | `PROG-INDEX` | 5 | Program index. |
| `059-062` | `ORGN INDEX` | 4 | Organization index. |
| `063-070` | `PROJ-NO`, `SUB-PROJ`, `PROJ-PHASE` | 8 | Project or grant dimensions. |
| `071-086` | `SUB-OBJECT`, `SUB-SUB-OBJECT`, `MAJOR-GROUP`, `MAJOR-SOURCE`, `SUB-SOURCE` | 16 | Expenditure and revenue classification. |
| `087-104` | `GL-ACCT-NO`, `SUBSID-ACCT-NO-A`, `SUBSID-ACCT-NO-B` | 18 | Variable GL and subsidiary detail. |
| `107-128` | Workclass, budget unit, geography, MOS, allocation | 22 | Operational and fiscal refinement fields. |

### Section 3: Payment and Document Tracking

| Positions | Field | Size | Use |
|---|---|---:|---|
| `129-146` | `P1 PAYMENT ID`, warrant override, prompt-pay date | 18 | Specialized payment-routing area. |
| `147-152` | `DOC-DATE` | 6 | Accounting event date. |
| `153-162` | `CUR-DOC-NO` and suffix | 10 | Current document tracking key. |
| `163-172` | `REF-DOC-NO` and suffix | 10 | Reference document key for reversals, liquidations, and related events. |
| `173-178` | `DUE-DATE` | 6 | Warrant date for payment-producing transactions. |
| `180-192` | `TRANS-AMT` | 13 | Signed or reversed transaction amount with implied decimals. |
| `224-228` | Payment category and ACH addenda type | 5 | Bank-facing ACH classification fields. |
| `234` | `US-FOREIGN INDICATOR` | 1 | Address-country cue for payment edits. |

### Section 4: Vendor and Extended Payables

| Positions | Field | Size | Use |
|---|---|---:|---|
| `248-287` | Vendor number, suffix, UBI, TIN, IRS box, tax type | 40 | Statewide vendor match and IRS reporting controls. |
| `288-312` | `VENDOR-TRAILER` | 25 | Remittance advice text. |
| `313-470` | Vendor name and address lines | 158 | Populated by vendor master or exception-code payment entry. |
| `609-646` | Expanded invoice number and invoice date | 38 | Payables traceability. |
| `647-710` | Account, provider, agreement, order | 64 | Extended source-system linkage. |
| `711-712` | `PACKET PURPOSE TYPE` | 2 | TALS budget-submittal use. |
| `801-802` | `PAYMENT EXCEPTION CODE` | 2 | Nonstandard payment processing cue. |

## Agency Financial Output (AFO) Record Layouts

The OFM batch-interface guide identifies outbound transaction records through `TRANSACTION-TYPE`. Teams treat these as AFO-facing record families that reuse the same 950-byte transaction envelope and vary by business meaning and downstream handling.

| Transaction Type | AFO Record Family | Source Meaning | Team Handling Pattern |
|---|---|---|---|
| `A` | Original transaction | Originating outbound transaction row | Parse with the common transaction layout and reconcile to the source batch. |
| `B` | Payment wrap | Payment wrap transaction | Preserve original source linkage and wrap lineage together. |
| `G` | Payment cancellation | Automated payment cancellation and payment indicator `C` or `D` transactions | Route to payment-reversal reconciliation queues. |
| `H` | SOL, non-AFRS cancellation, ACH return | Automated SOL AFRS or non-AFRS cancellation and ACH return activity | Preserve bank return identifiers and resubmission decisions. |
| `K` | Monthly GL balance | Enterprise-reporting monthly GL balance output | Parse as outbound-only reporting detail and preserve raw copies for audit replay. |
| `L` | Monthly project balance | Enterprise-reporting monthly project balance output | Parse as outbound-only reporting detail and preserve raw copies for audit replay. |

### AFO Parsing Rule Set

| Rule | Test | Pass |
|---|---|---|
| Parser reads one 950-character line into one layout object. | Run a parser test with one sanitized AFO line. | The parser returns one populated object and zero overflow bytes. |
| Parser preserves `TRANSACTION-TYPE` exactly. | Parse one `A`, `B`, `G`, `H`, `K`, and `L` row. | Output records retain the same one-character type. |
| Reconciliation store keeps source file name, line number, and raw text. | Review parsed AFO audit rows. | Every parsed row links back to one immutable raw record. |

## Batch Control and Sequencing Rules

| Control Topic | Rule | Pass Target |
|---|---|---|
| Batch count | Header count equals the number of detail rows. | Header `BATCH-COUNT` equals computed detail count. |
| Batch amount | Header amount equals the sum of detail `TRANS-AMT` values unless the team intentionally uses the penny auto-calculate pattern. | Header `BATCH-AMOUNT` equals computed amount or equals the penny sentinel with documented intent. |
| Multi-batch file | One file contains one or more batches. Each batch uses a distinct batch key. | Batch-key scan finds no duplicate `(Agency, Date, Type, Number)` combination. |
| Same-day rerun | Additional same-day submissions increment `BATCH-NO`. | Later reruns do not reuse prior numbers. |
| Sequence numbers | Detail sequence begins at `00001` and increments by one. | Sorted detail rows show one contiguous sequence set. |
| Processing window | Teams avoid the documented 8:00 p.m. to 10:00 p.m. processing window for batch submission. | Job schedule excludes the blocked window. |
| File naming for MFT | Flat-file name stays within 15 characters including automated extensions. | Generated base file name length stays within the agreed agency limit. |

## D51 Error Catalog

The Appendix A PDF contains a large statewide catalog. The table below consolidates the families that drive most interface triage activity.

| Family | Example Codes | Failure Theme | First Correction Step |
|---|---|---|---|
| `A` | `A01`, `A02`, `A10`, `A20`, `A36` | Existence, biennium, fund, and table-reference failures | Refresh effective-dated reference data and re-check fiscal context. |
| `B` | `B01`-`B17` | Batch agency, date, type, number, count, amount, and duplicate-batch failures | Rebuild the batch header from immutable staged detail rows. |
| `C` | `C01`-`C07` | Banking and payment-field failures | Re-check vendor routing, account, and ACH transaction data. |
| `D` | `D01`-`D99` | Core code-table and element-definition failures | Re-check agency crosswalks, field lengths, and title-table mappings. |
| `DS` | `DS1`-`DS6` | Program, allocation, schedule, organization index, and appropriation index failures | Reconcile source dimensions to AFRS title tables. |
| `DD` | `DD1`-`DD4` | Fiscal-year and month-of-service rule failures | Recompute fiscal derivations from the source posting date. |

### Selected D51 Codes for Batch Intake

| Code | Title | Triage Cue | Observable Resolution |
|---|---|---|---|
| `B01` | Batch Agency Invalid | Header agency does not map to an authorized submitting unit. | Corrected header passes agency validation. |
| `B02` | Batch Date Invalid | Header date fails `YYMMDD` parsing or calendar validation. | Header date parses and matches the source run date. |
| `B03` | Batch Type Invalid | Batch type is not approved for the interface. | Batch type matches one agency-approved value. |
| `B04` | Batch No. Invalid | Daily batch number is blank, malformed, or outside the agency sequence. | Batch number is three characters and unique for the day. |
| `B07` | Batch Amount Error | Header amount differs from computed detail total. | Computed amount equals header amount. |
| `B08` | Batch Count Error | Header count differs from actual detail count. | Computed count equals header count. |
| `B10` | Batch Sequence No. Invalid | Header or detail sequence violates the sequence contract. | Header uses `00000` and details use unique non-zero values. |
| `B12` | Duplicate Batches | Batch key collides with a prior submission. | Replacement file uses a new batch key. |
| `D22` | OBJ NOT IN D10 | Object code is invalid or inactive. | Object code resolves to one active D10 row for the posting date. |
| `D30` | ACCT FUND NOT IN D22 | Fund code does not exist in the title table. | Fund code resolves to one active D22 row. |
| `D33` | G/L NOT IN D31 | GL account is invalid or not permitted for the transaction. | GL account resolves to one valid D31 row. |
| `DS1` | Program Index Invalid | Program index does not resolve. | Program index resolves to one active title row. |
| `DS5` | Orgn Index Not Found on OI Table | Organization index is missing or inactive. | Organization index resolves to one active OI row. |
| `DS6` | Appn Index Not Found on AI Table | Appropriation index is missing or inactive. | Appropriation index resolves to one active AI row. |
| `DD2` | MOS Date Must Be in Format YYMM | Month-of-service fields are malformed. | MOS year and month serialize as four numeric characters. |

## Transaction Code Workbook Cues

The workbook contains `932` transaction-code rows across lookup and listing views, `81` wrap mappings, and dedicated variable-GL groupings. Teams use the workbook as governed reference data, not as application logic embedded in code.

| Workbook Sheet | Content | Team Use |
|---|---|---|
| `TC Lookup` | Quick lookup of transaction code, title, and GL combinations | Supports analyst lookup and unit-test fixtures. |
| `TC Listing` | Broad catalog with field-presence cues | Drives code selection, required-field validation, and support tooling. |
| `TC Variable GLs` | Allowable variable GL sets for referenced transaction codes | Validates `GL-ACCT-NO` values before export. |
| `TC Wraps` | Current, prior, warrant-cancellation, and ACH-return wrap mappings | Drives correction, reversal, and payment-return automation. |

### Selected Transaction Codes

| Code | Title | GL Cue | Implementation Cue |
|---|---|---|---|
| `001` | RCPT-RCRD REVENUE (TREA) | `7110 / 3210` | Use for treasury receipt revenue recording with revenue-source validation. |
| `003` | RECORD NSF OR ADJUST REVENUE (TRE) | `3210 / 7110` | Preserve reference-document linkage for reversal lineage. |
| `012` | RCRD CURRENT ACCTS REC | `1312 / 3205` | Validate customer, receivable, and revenue context together. |
| `025` | INTERFUND GL TRANSFER | `var / 7140` | Validate the variable GL against the workbook group before export. |
| `026` | INTERFUND GL TRANSFER | `7140 / var` | Validate reciprocal inbound transfer logic. |
| `042` | RCRD RECEIPT-BENEFITS PYMT RETURNED | `7110 / 5118` | Route returned-payment reconciliation with original payment linkage. |
| `058` | RCRD ADV TO OTH FNDS (TREA) | `1353 / 5111` | Use wrap mappings for current, prior, and cancellation follow-up. |
| `137` | RCRD OTHER AGENCY/FUND PAYABLE | `6505 / var` | Validate payable cross-agency support and wrap eligibility. |
| `138` | APPLY CREDIT MEMO/DISCOUNT-OTHERS | `var / 6505` | Preserve payable reduction references. |
| `210` | RCRD ACCT/VOU PAY-NO ENCUMB(TREA) | `6505 / 5111` | Validate payable creation without encumbrance lineage. |
| `260` | JV TRSF-AMT DUE-AGCY REIM (TREA) | `7140 / 1354 / 6505 / 6510` | Validate four-leg balancing before file emission. |
| `261` | BILL (NOT RCVD) AGCY REIM EXPEND | `1354 / 6505` | Preserve reimbursement billing references. |
| `290` | REVERSE ACCT/VOU PAY TC 210(TREA) | `5111 / 6505` | Reference the original payable document and reversal batch. |
| `345` | LCL TRSF INCR AGCY EXPEN | `6510 / 9920` | Validate local-transfer policy mapping and expenditure authority. |
| `669` | INTERFUND EXPENDITURE TRANSFER-IN | `6510 / 7140` | Pair with reciprocal transfer-out logic. |
| `670` | INTERFUND EXPENDITURE TRANSFER-DE | `7140 / 6510` | Pair with reciprocal transfer-in logic. |

### Wrap and Variable-GL Rules

| Pattern | Example | Team Rule |
|---|---|---|
| Wrap mapping | Base code `210` maps to current wrap `398`, prior wrap `818`, warrant-cancel wrap `451`, and ACH-return wrap `255`. | Teams derive wrap codes from the workbook instead of hard-coded switch statements. |
| Variable GL group | Code `025` references variable GL group `16`; code `026` references group `22`. | Teams validate the supplied GL against the referenced workbook group before serialization. |
| Multi-leg transaction | Code `260` contains four GL legs. | Teams validate balancing across every leg before write. |

## .NET Serialization and Deserialization Patterns

```csharp
public sealed record FixedWidthField(string Name, int Start, int Length);

public sealed record AfrsBatchHeader(
    string BatchAgency,
    string BatchDateYyMmDd,
    string BatchType,
    string BatchNumber,
    string Biennium,
    string FiscalMonth,
    string BatchDueDate,
    int BatchCount,
    long BatchAmount);

public static class AfrsAmountFormatter
{
    public static string Format(long cents) => cents.ToString("0000000000000", CultureInfo.InvariantCulture);
}
```

```csharp
public static string SerializeHeader(AfrsBatchHeader header)
{
    Span<char> buffer = stackalloc char[950];
    buffer.Fill(' ');

    Write(buffer, 1, 4, header.BatchAgency);
    Write(buffer, 5, 6, header.BatchDateYyMmDd);
    Write(buffer, 11, 2, header.BatchType);
    Write(buffer, 13, 3, header.BatchNumber);
    Write(buffer, 17, 1, "A");
    Write(buffer, 18, 5, "00000");
    Write(buffer, 23, 2, header.Biennium);
    Write(buffer, 25, 2, header.FiscalMonth);
    Write(buffer, 27, 6, header.BatchDueDate);
    Write(buffer, 33, 5, header.BatchCount.ToString("00000", CultureInfo.InvariantCulture));
    Write(buffer, 38, 13, AfrsAmountFormatter.Format(header.BatchAmount));

    return new string(buffer);
}
```

| Pattern | Test | Pass |
|---|---|---|
| Serializer writes blank-filled 950-character records. | Serialize one header and one detail object. | Output length equals `950` for both records. |
| Parser reads by absolute position, not delimiter splitting. | Parse one known-good sample line. | Every parsed field matches the expected substring. |
| Layout metadata stays versioned. | Review layout registry. | One effective date and one source citation exist per layout. |
| Validation runs before file write and after file read. | Execute one round-trip test. | Input object equals parsed output object for governed fields. |

## Validation Rules and Examples

| Validation Layer | Rule | Example Failure |
|---|---|---|
| Structural | Reserved bytes contain spaces only. | Internal-use bytes contain non-space text. |
| Structural | Header count equals detail count. | Header states `00002` and file contains three detail rows. |
| Fiscal | Biennium and fiscal month align with the posting window. | Header biennium points to the wrong ending odd year. |
| Transaction code | Transaction code, field presence, and GL pattern align with workbook cues. | Code `025` uses an unapproved variable GL account. |
| Reference data | Agency, fund, object, program, and index values resolve for the posting date. | `DS5` organization index failure. |
| Payment | Exception-code payments include required address data for US payees. | Exception payment omits city, state, or ZIP. |

## Sanitized Sample File Snippets

```text
Header
4610260830BA001 A000002601070000200000000012345[900 trailing spaces omitted]

Detail
4610260830BA001 A0000100122601070100100MI000001AI1F010PI001OI01PRJ1A1PHSO0001SS010001311AA000001BB000002DOC10001AAREF10001AA26083000000000012345[remaining governed fields and trailing spaces omitted]
```

```text
AFO Outbound Sample
4610260830BA001 B0000102102601070100100MI000001AI1F010PI001OI01....[type B payment wrap record, sanitized]
4610260830BA001 G0000202902601070100100MI000001AI1F010PI001OI01....[type G cancellation record, sanitized]
```

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Layout version recorded | Specification cites the 2025 OFM batch-interface layout plus local reference files. |
| Header verified | Header bytes `001-050` serialize correctly and bytes `051-950` stay blank-filled. |
| Detail verified | Detail rows serialize to `950` characters with contiguous sequence numbers. |
| AFO parsing verified | Parser preserves outbound transaction type and raw-line lineage. |
| D51 handling verified | Reject pipeline stores code, title, family, source line, owner, and correction status. |
| Workbook integration verified | Transaction-code validation uses workbook-derived metadata, wrap mappings, and variable-GL groups. |
