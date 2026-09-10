---
name: copybook-to-dotnet
title: Copybook to .NET
description: Generate C# types and metadata from COBOL copybooks for typed contracts, source generation, and fixed-width serialization.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1999
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - roslyn-source-generator-authoring
related_skills:
  - mainframe-copybook-parsing
  - copybook-data-conversion
  - roslyn-source-generator-authoring
  - dotnet-naming-standards
  - dotnet-unit-testing
appliesTo: '**/*.{cpy,cob,cs,csproj,md}'
tags:
  - mainframe
  - copybook
  - dotnet
  - source-generator
  - fixed-width
---
# Copybook to .NET

Agent generates deterministic C# models, metadata, and tests from COBOL copybooks.

## When to Use

| Condition | Use |
|---|---|
| Work needs C# contracts generated from a copybook | Use this skill |
| Work needs Roslyn source generation for compile-time copybook models | Use this skill |
| Work needs fixed-width serialization metadata aligned to COBOL offsets | Use this skill |
| Work needs tests that lock field mapping and generated output | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work needs copybook parsing or validation only | Use `mainframe-copybook-parsing` |
| Work needs byte-to-value conversion for raw mainframe files | Use `copybook-data-conversion` |
| Work needs handwritten domain models with no generation path | Use direct C# authoring |
| Work targets non-.NET consumers only | Use platform-specific generation |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Parsed copybook model | Yes | Use a validated hierarchy with offsets and data types. |
| Namespace and output path | Yes | Record generated scope. |
| Emission model | Yes | Record class, record, or partial-type strategy. |
| Serialization attribute pattern | No | Record fixed-width library or repository-local attribute shape. |
| REDEFINES policy | No | Record alternate-view properties, union wrapper, or raw-span metadata. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Validate the parsed copybook model and reject blocking parse errors. | Input hierarchy has zero unresolved errors. |
| 2 | Map groups, elementary items, `OCCURS`, `REDEFINES`, and `FILLER` rules to deterministic C# shapes. | Every source element has one stable emission pattern. |
| 3 | Map PIC and USAGE metadata to C# types, offsets, lengths, scale, and converter metadata. | Every field keeps one stable type and layout contract. |
| 4 | Convert COBOL names and comments into PascalCase members and XML docs. | Public members follow naming and comment rules. |
| 5 | Choose Roslyn, CLI, or scripted generation and verify repeatable output plus targeted tests. | Generation is deterministic and happy, edge, and invalid-layout tests pass. |


## Hierarchy Matrix

| COBOL structure | C# emission pattern | Notes |
|---|---|---|
| Root `01` group | Top-level class or record | One file per root record |
| Nested group with business meaning | Nested type or sibling partial type | Pattern stays stable across regenerations |
| Elementary item | Property | Property carries length and offset metadata |
| `FILLER` item | No public property, optional private metadata row | Record length remains accurate |
| `OCCURS` group | Array or `List<T>` plus count metadata | Arrays fit fixed count layouts best |
| `REDEFINES` group | Alternate-view property set or overlay wrapper | Shared offset stays explicit |

## COBOL Data Type Mapping

| COBOL syntax | C# type | Metadata |
|---|---|---|
| `PIC X(n)` | `string` | Length attribute |
| `PIC 9(n)` | `int` or `long` | Length and numeric-style metadata |
| `PIC S9(n)V9(m)` | `decimal` | Scale metadata |
| `PIC S9(n)V9(m) COMP-3` | `decimal` | Packed-decimal converter attribute |
| `PIC 9(n) COMP` | `short`, `int`, or `long` | Binary converter attribute by dialect width |
| `PIC X(1) OCCURS n TIMES` | `string[]` or `char[]` | Element length and count metadata |
| Group item | Nested type | Group offset metadata |

## Mainframe Pattern Matrix

| Pattern | Emission rule | Generated artifact |
|---|---|---|
| `COMP-3` | Emit `decimal` property plus packed-decimal converter metadata | Property and converter attribute |
| EBCDIC source data | Keep encoding choice in reader layer, not model layer | Reader configuration hook |
| Fixed-width layout | Emit offset and length metadata on every elementary field | Attributes or generated descriptors |
| `OCCURS` | Emit arrays for fixed counts or `List<T>` for post-parse projection | Repeating group container |
| `REDEFINES` | Emit alternate view members with shared offset contract | Overlay descriptor or partial helper |
| COBOL comments | Emit XML doc comments on generated public members | `<summary>` blocks |

## Generation Matrix

| Condition | Pattern | Result |
|---|---|---|
| Build-time deterministic generation inside `src/ifx` or analyzer package | Roslyn source generator | Compile-time emitted models and diagnostics |
| Repository needs checked-in generated files | CLI or MSBuild task emission | Reviewable generated source |
| Mixed runtime and design-time consumers exist | Generate descriptors plus partial models | Shared metadata for readers and ORMs |
| One-off migration or discovery work | Scripted generator | Fast conversion with lower infrastructure cost |

## Integration Hooks

| Target | Result |
|---|---|
| Microsoft Fabric ingestion | Landing and silverization stages reuse one generated field contract. |
| EF Core storage | Staging entities preserve copybook lineage plus numeric precision and scale. |


## Before and After Example

```cobol
01 CUSTOMER-REC.
   05 CUSTOMER-ID     PIC 9(7).
   05 CUSTOMER-NAME   PIC X(30).
   05 CURRENT-BALANCE PIC S9(7)V99 COMP-3.
   05 STATUS-CODE     PIC X(1) OCCURS 3 TIMES.
```

```csharp
[FixedWidthRecord(Length = 45)]
public sealed partial class CustomerRec
{
    [FixedWidthField(Offset = 0, Length = 7)]
    public int CustomerId { get; init; }
    [FixedWidthField(Offset = 7, Length = 30)]
    public string CustomerName { get; init; } = string.Empty;
    [PackedDecimalField(Offset = 37, Length = 5, Scale = 2)]
    public decimal CurrentBalance { get; init; }
    [FixedWidthArrayField(Offset = 42, Count = 3, ElementLength = 1)]
    public string[] StatusCodes { get; init; } = [];
}
```


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter and STE wording | Run `npm run frontmatter:validate` and the modal-verb scan. | Front matter validates and banned modal verbs do not appear. |
| Naming, type, and layout fidelity | Compare COBOL names, PIC or USAGE metadata, and parsed spans to generated members. | Names stay deterministic and every emitted field preserves type, scale, offset, and length. |
| Repeatability and tests | Run the generator twice, then execute targeted generator or model tests. | Output diff is empty and happy, edge, and invalid-layout cases pass. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Map `COMP-3` to `double` | Map packed decimal to `decimal` and preserves scale metadata. |
| Agent hides `REDEFINES` overlays behind duplicated offsets with no metadata | Emit one overlay contract with shared-span descriptors. |
| Agent drops COBOL comments during generation | Promote comments into XML docs on public members. |
| Use `List<T>` for every `OCCURS` | Agent prefers arrays for fixed-count record layouts. |
| Agent mixes handwritten edits into generated files | Agent isolates generated output and keeps custom behavior in partial types. |

## Outputs

| Output | Contents |
|---|---|
| Generated models | Types, properties, XML docs, and layout metadata |
| Generation path | Roslyn generator, CLI generator, or build task choice |
| Mapping contract | PIC-to-C# map with offsets, lengths, and scale |
| Test suite | Generator or model tests that lock deterministic output |
