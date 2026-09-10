---
name: copybook-data-conversion
title: Copybook Data Conversion
description: Convert EBCDIC and fixed-width mainframe data into .NET values when work needs packed decimal, sign, date, or streaming record decoding.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1868
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mainframe-copybook-parsing
  - copybook-to-dotnet
  - fabric-lakehouse-ingestion
  - efcore-dbcontext-design
appliesTo: '**/*.{cpy,cob,dat,txt,bin,cs,md}'
tags:
  - mainframe
  - conversion
  - ebcdic
  - packed-decimal
  - streaming
---
# Copybook Data Conversion

Agent converts raw mainframe bytes into validated .NET values by combining copybook layout metadata with encoding and numeric conversion rules.

## When to Use

| Condition | Use |
|---|---|
| Work reads EBCDIC or fixed-width mainframe files | Use this skill |
| Work decodes `COMP-3`, zoned decimal, or binary `COMP` fields | Use this skill |
| Work needs streaming conversion for multi-GB data files | Use this skill |
| Work needs date, sign, or null-space normalization from mainframe records | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work needs copybook structure extraction only | Use `mainframe-copybook-parsing` |
| Work needs generated C# models from a parsed copybook | Use `copybook-to-dotnet` |
| Work reads delimited or JSON payloads instead of fixed-width records | Use a format-specific ingestion skill |
| Work focuses on relational schema design after conversion | Use EF Core or data-modeling workflows |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Parsed copybook layout | Yes | Use validated offsets, lengths, usage, and sign metadata. |
| Source encoding | Yes | Record EBCDIC code page or ASCII-family encoding. |
| Record length | Yes | Verify fixed-width record span before conversion. |
| Reject policy | No | Record fail-fast, quarantine row, or warning-first handling. |
| Throughput target | No | Record batch size, streaming need, and memory ceiling. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Fix the encoding and record span from validated copybook metadata. | One encoding and one record length apply to the file. |
| 2 | Slice records with spans and copybook offsets. | Every field slice stays inside its declared byte range. |
| 3 | Decode alphanumeric, display numeric, packed, zoned, binary, date, `OCCURS`, `REDEFINES`, and blank-field rules. | Sign, scale, range, and alternate views remain correct. |
| 4 | Project typed values into DTOs, staging rows, or EF Core entities. | Converted rows land in one typed destination shape. |
| 5 | Choose buffered or streaming I/O and emit rejects plus diagnostics. | Large files stay memory-bounded and bad rows stay measurable. |


## COBOL Data Type Mapping

| Storage form | Conversion target | Rule | Notes |
|---|---|---|---|
| `PIC X(n)` | `string` | Decode bytes with chosen code page | Trim and null policy stays explicit |
| Display numeric `PIC 9(n)` | `int`, `long`, or `decimal` | Parse digits after sign normalization | Blank handling stays caller-defined |
| Implied decimal `PIC 9(n)V9(m)` | `decimal` | Insert decimal scale after digit parse | Scale equals `m` |
| Packed decimal `COMP-3` | `decimal` | Decode nibbles and final sign nibble | `C` and `F` sign nibbles map positive; `D` maps negative |
| Zoned decimal | `decimal` or integer type | Decode overpunch or zoned sign from last byte | Trailing overpunch appears often in financial feeds |
| Binary `COMP` | `short`, `int`, or `long` | Read endian-safe bytes by dialect width | Width comes from usage metadata |
| `COMP-1` | `float` | IEEE or dialect-specific single precision path | Rare in flat files |
| `COMP-2` | `double` | IEEE or dialect-specific double precision path | Rare in flat files |

## Mainframe Pattern Matrix

| Pattern | Rule | Diagnostic focus |
|---|---|---|
| EBCDIC text | Decode with an explicit code page such as 037 or 1047. | Wrong code page shows punctuation or currency drift. |
| `COMP-3` | Read packed digits plus the final sign nibble. | Invalid nibble or scale mismatch. |
| Zoned decimal | Decode the overpunch or zone bits from the last byte. | Non-numeric zone or sign conflict. |
| Binary `COMP` | Read by dialect-specific width and signedness. | Overflow or wrong-width mapping. |
| Fixed-width layout | Slice by copybook offset and length. | Record-length drift. |
| `OCCURS` and `REDEFINES` | Iterate repeated spans and decode overlays from shared bytes. | Element-length or overlay mismatch. |
| Null, sign, and date fields | Apply explicit blank, sign, and calendar rules. | Silent trimming or rollover defects. |


## Performance Matrix

| Condition | Pattern | Result |
|---|---|---|
| In-memory batch is enough | Buffered record batches | Simple implementation |
| Multi-GB file | Sequential streaming over `Stream` and `Memory<byte>` | Bounded memory |
| CPU-heavy packed or date parsing | Span-based parsing and pooled buffers | Lower allocation rate |
| Fabric landing next | One-pass raw-to-typed row streaming | Traceable ingest with reject counts |


## Integration Hooks

| Target | Result |
|---|---|
| Microsoft Fabric ingestion | Bronze and silver stages retain byte-to-column traceability. |
| EF Core storage | Staging entities preserve precision, scale, and raw-row lineage. |


## Before and After Example

```cobol
01 PAYMENT-REC.
   05 ACCOUNT-NUMBER PIC X(10).
   05 PAYMENT-AMOUNT PIC S9(5)V99 COMP-3.
   05 EFFECTIVE-DATE PIC 9(7).
```

```csharp
public static PaymentRec Parse(ReadOnlySpan<byte> record, Encoding ebcdic) => new(
    ebcdic.GetString(record[..10]).TrimEnd(),
    PackedDecimalConverter.ReadDecimal(record.Slice(10, 4), scale: 2),
    JulianDateConverter.ReadDate(record.Slice(14, 7)));
```


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter and STE wording | Run `npm run frontmatter:validate` and the modal-verb scan. | Front matter validates and banned modal verbs do not appear. |
| Encoding, numeric, and record accuracy | Decode known text, `COMP-3`, zoned, binary, and fixed-length samples. | Characters, sign, scale, range, and slice boundaries all match the trusted input. |
| Streaming bounds and reject traceability | Review large-file buffering and inspect reject output. | Memory stays bounded and reject rows include offset, field, and reason. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Agent treats EBCDIC input as UTF-8 | Select an explicit EBCDIC code page before text decoding. |
| Decode packed decimals by string conversion only | Read nibbles directly from bytes to preserve performance and diagnostics. |
| Agent ignores overpunch signs in zoned decimals | Decode zoned sign rules from the last byte. |
| Agent allocates a new string or buffer per field on multi-GB files | Use spans, pooled buffers, and sequential streaming. |
| Agent drops raw-row lineage during projection | Agent preserves record index, offset, or raw-span references for audit and replay. |

## Outputs

| Output | Contents |
|---|---|
| Conversion pipeline | Encoding, field converters, and projection target |
| Typed rows | Strings, numerics, dates, arrays, and overlay views |
| Reject artifacts | Record index, field name, byte span, and failure reason |
| Performance plan | Streaming, batching, and allocation control |

