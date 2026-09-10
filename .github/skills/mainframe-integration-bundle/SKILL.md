---
name: mainframe-integration-bundle
title: Mainframe Integration Bundle
description: Bundle router for COBOL copybook parsing, C# model generation, and mainframe data conversion work tied to legacy file integration.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 1090
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mainframe-copybook-parsing
  - copybook-to-dotnet
  - copybook-data-conversion
appliesTo: '**/*.{cs,csproj,json,md,cbl,cpy,txt,dat}'
tags:
  - mainframe
  - copybook
  - cobol
  - ebcdic
  - comp-3
  - routing
---
# Mainframe Integration Bundle

Agent uses this bundle for legacy mainframe file integration spanning copybook structure, .NET model generation, and data-format conversion.

## When to Use

| Prompt Pattern | Use This Bundle |
|---|---|
| COBOL copybook plus .NET ingestion request | Yes |
| Mainframe file decode, unpack, or migration request | Yes |
| Large fixed-width batch integration request | Yes |
| Single narrow copybook parse task with no routing need | No |

## When Not to Use

| Prompt Pattern | Agent Route |
|---|---|
| Field layout extraction only | `mainframe-copybook-parsing` |
| C# type generation only | `copybook-to-dotnet` |
| EBCDIC, packed decimal, or zoned decimal conversion only | `copybook-data-conversion` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source artifact | Yes | Copybook, sample data file, or record specification |
| Record strategy | Yes | Single record, multi-record, or REDEFINES-heavy layout |
| Target integration | Yes | C# classes, parser pipeline, ETL, or API ingestion |
| File scale | No | Daily batch size, record length, and throughput target |

## Activation

| Request Signal | Use Bundle | Route Detail |
|---|---|---|
| Parse PIC clauses, OCCURS, REDEFINES, or level hierarchy | Yes | Agent routes to `mainframe-copybook-parsing` |
| Generate C# classes, enums, or typed readers | Yes | Agent routes to `copybook-to-dotnet` |
| Decode EBCDIC, COMP-3, binary, signed, or date formats | Yes | Agent routes to `copybook-data-conversion` |

## Coverage Matrix

| Track | Specialist Skill | Primary Outcome |
|---|---|---|
| Structural parse | `mainframe-copybook-parsing` | Deterministic record layout and field metadata |
| Model generation | `copybook-to-dotnet` | C# types aligned to field shape and nullability |
| Value conversion | `copybook-data-conversion` | Accurate byte-to-value translation for legacy encodings |

## Decision Matrix: Parse Copybooks vs Manual Mapping

| Condition | Preferred Path | Decision Signal |
|---|---|---|
| Stable copybook exists and record count is large | `mainframe-copybook-parsing` | Layout authority already exists |
| REDEFINES, OCCURS, and signed fields drive parsing logic | `mainframe-copybook-parsing` + `copybook-data-conversion` | Structural metadata drives conversion |
| One-off file with short lifespan and few fields | Manual mapping | Parse investment outweighs reuse value |
| Target domain model differs from physical record | `mainframe-copybook-parsing` then manual projection | Physical parse stays separate from business model |
| C# ingestion pipeline is the output | `copybook-to-dotnet` after parsing | Typed model generation reduces drift |

## Data Format Conversion Decision Table

| Source Format | Route | Conversion Focus |
|---|---|---|
| EBCDIC text | `copybook-data-conversion` | Code page decode and trimming rules |
| COMP-3 packed decimal | `copybook-data-conversion` | Nibble unpack, sign handling, scale placement |
| COMP or binary fields | `copybook-data-conversion` | Endianness, signed range, field width |
| Zoned decimal | `copybook-data-conversion` | Sign nibble, display precision, validation |
| Julian or packed date fields | `copybook-data-conversion` | Epoch rule, century handling, invalid-date rejection |

## Performance Considerations for Large Mainframe Files

| Concern | Agent Focus | Pass Signal |
|---|---|---|
| File size | Stream sequential reads instead of whole-file buffering. | Working set stays bounded to parser buffers. |
| Record count | Use record-by-record parsing and batch writes. | Throughput stays stable across long runs. |
| Conversion cost | Reuse field metadata and avoid per-record reflection. | Parser cost stays dominated by I/O and decode work. |
| Error handling | Capture record offsets and continue according to policy. | Bad records isolate without losing file position. |

## Integration with Modern .NET Patterns

| Concern | Pattern |
|---|---|
| Parsing pipeline | Use streaming readers, bounded buffers, and explicit record contracts |
| Domain boundary | Keep generated transport types separate from business aggregates |
| Validation | Validate decoded records before persistence or API publication |
| Observability | Log file name, record offset, record count, and reject count with structured logging |
| Persistence | Batch database writes and keep idempotent import keys per record source |

## Decision Order

| Decision | Agent Action |
|---|---|
| Copybook exists | Agent routes first to `mainframe-copybook-parsing`. |
| Generated .NET types are the target | Agent routes next to `copybook-to-dotnet`. |
| Legacy encodings or numeric compression appear | Agent routes to `copybook-data-conversion` before validation logic. |
| Manual mapping remains simpler | Agent records manual mapping rationale and skips generation. |
| Large-file throughput risk appears | Agent applies streaming and metadata-caching patterns before implementation. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inspect source | Agent identifies copybook authority, sample file, and record strategy. | Agent records source artifacts. | One authoritative layout source exists. |
| 2. Parse structure | Agent maps levels, fields, repeats, and redefinitions. | Agent records structural metadata. | Every byte range maps to one active interpretation rule. |
| 3. Select target model | Agent chooses generated transport types or manual projection. | Agent records target model path. | One target-model path exists. |
| 4. Apply conversions | Agent decodes text, numeric, and date formats. | Inspect sample record outputs. | Decoded values match sample expectations. |
| 5. Verify throughput path | Agent checks streaming, buffering, and rejection flow. | Inspect parser design and existing tests. | Parser path supports bounded-memory execution. |

## Verification Matrix

| Area | Test | Pass |
|---|---|---|
| Copybook parse | Compare parsed metadata to copybook clauses. | Every PIC, OCCURS, and REDEFINES rule maps correctly. |
| C# generation | Inspect generated types against field definitions. | Field widths, nullability, and names stay aligned. |
| Data conversion | Run sample records through decode path. | Text, numeric, and date values match expected outputs. |
| Performance | Inspect large-file processing path. | Design uses streaming and avoids whole-file materialization. |
| Integration | Inspect decoded-record handoff to .NET services. | Transport model and domain model stay separated. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Business model generation starts before physical layout is stable | Agent parses copybook first. |
| Packed decimal logic treats bytes as text | Agent routes numeric decode through `copybook-data-conversion`. |
| Generated classes leak mainframe field names into domain code | Agent keeps generated transport types at the integration boundary. |
| Large file import loads entire file into memory | Agent uses streaming readers and batch persistence. |
| REDEFINES branches lack discriminator logic | Agent records branch-selection rules during parsing. |

## Verification Checklist

- [ ] Agent created `.github/skills/mainframe-integration-bundle/SKILL.md`.
- [ ] Agent listed all three specialist skills in coverage and routing tables.
- [ ] Agent included parse-vs-manual, conversion, performance, and .NET integration guidance.
- [ ] Agent kept `estimated_tokens` below 1200.
