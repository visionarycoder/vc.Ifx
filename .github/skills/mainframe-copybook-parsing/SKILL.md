---
name: mainframe-copybook-parsing
title: Mainframe Copybook Parsing
description: Parse COBOL copybooks into validated structural models when work needs field layout, hierarchy, or parser strategy selection.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1491
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - copybook-to-dotnet
  - copybook-data-conversion
  - fabric-lakehouse-ingestion
  - xml-schema-inference
appliesTo: '**/*.{cpy,cob,txt,cs,md}'
tags:
  - mainframe
  - copybook
  - cobol
  - parsing
  - fixed-width
---
# Mainframe Copybook Parsing

Agent parses COBOL copybooks into hierarchical field models with offsets, lengths, data semantics, and validation results.

## When to Use

| Condition | Use |
|---|---|
| Work starts with a COBOL copybook and no trusted structural model exists | Use this skill |
| Work needs field offsets, lengths, OCCURS counts, or REDEFINES overlays | Use this skill |
| Work needs parser design guidance for .NET ingestion components | Use this skill |
| Work needs copybook validation before data conversion or class generation | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work already has a validated parsed model and only needs C# emission | Use `copybook-to-dotnet` |
| Work focuses on byte decoding from EBCDIC or packed fields | Use `copybook-data-conversion` |
| Work asks for XML or delimited schema inference instead of copybooks | Use `xml-schema-inference` or `csv-ingestion-diagnostics` |
| Work edits COBOL business logic outside copybook structure | Use a COBOL code workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Copybook source | Yes | Record file path, encoding, and line-continuation style. |
| Dialect facts | No | Record IBM Enterprise COBOL, Micro Focus, or local extensions when known. |
| Record root name | No | Derive the 01 group when the caller omits it. |
| Validation strictness | No | Record fail-fast or warning-first behavior. |
| Downstream target | No | Record C# generation, Fabric ingestion, EF Core staging, or diagnostics. |

## Pattern Matrix

| Concern | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Source normalization | Strip comments, continuation markers, and sequence-area noise into one normalized stream | Preserve source locations | Every data definition line maps to one normalized statement |
| Hierarchy | Build the tree from level numbers | Avoid indentation-only parsing | Each item has one valid parent except the root |
| Clause parsing | Parse PIC, USAGE, SIGN, OCCURS, REDEFINES, VALUE, and FILLER into explicit metadata | Flag unsupported dialect constructs | Each supported clause lands in the model with source location |
| Storage model | Compute offsets, lengths, overlay scope, and record length | Keep `REDEFINES` as shared storage, not duplicate storage | Offsets and overlay spans reconcile |
| Strategy selection | Choose custom parser, third-party adapter, or staged preprocessor plus parser | Match strategy to dialect breadth and delivery needs | One parser strategy fits the workload |
| Output contract | Emit hierarchy, offsets, types, and findings for downstream consumers | Keep parse and conversion concerns separate | Output model is ready for generation or ingestion |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent normalizes source lines, comments, continuation markers, and sequence areas. | Read the normalized token stream. | Every statement is parse-ready. |
| 2 | Agent builds the level-number hierarchy and distinguishes group from elementary items. | Inspect parent-child relationships. | Each item has the intended parent and level. |
| 3 | Agent parses PIC, USAGE, SIGN, OCCURS, REDEFINES, VALUE, and FILLER clauses. | Compare parsed clauses to source lines. | Supported clauses appear in the field model. |
| 4 | Agent computes storage length, relative offset, and overlay scope. | Inspect offset and length totals. | Record length and overlay boundaries are internally consistent. |
| 5 | Agent validates clause combinations, level jumps, duplicate names, and unsupported dialect features. | Run validation rules. | Findings list every error or warning with line scope. |
| 6 | Agent emits the structural model for downstream generation or ingestion. | Review the output model. | Output includes hierarchy, offsets, types, and findings. |

## Rules

| Topic | Rule |
|---|---|
| Hierarchy | Build parentage from numeric level semantics. |
| `REDEFINES` | Keep shared offsets and overlay references instead of duplicate storage. |
| `FILLER` | Preserve filler spans so downstream offsets remain correct. |
| Numeric metadata | Carry digit count, scale, sign, usage, and binary-width facts forward. |
| Dialect handling | Record unresolved dialect-dependent width or syntax as a validation finding. |
| Separation of concerns | Keep parsing distinct from EBCDIC decoding and .NET type emission. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Hierarchy accuracy | Compare the tree to copybook levels. | Every item has the intended parent and level. |
| Storage accuracy | Sum spans and overlay spans. | Record length and offsets reconcile. |
| Clause coverage | Inspect metadata for PIC, USAGE, OCCURS, REDEFINES, and SIGN. | Every clause in scope appears in output. |
| Validation output | Review errors and warnings. | Unsupported constructs and structural defects stay explicit. |
| Integration readiness | Compare the model to the downstream contract. | Output contains names, offsets, lengths, and validation facts. |

## Outputs

- Structural model with record name, hierarchy, offsets, lengths, and clause metadata
- Validation report with errors, warnings, and source locations
- Parser-strategy selection record
- Integration metadata for generation, staging, or ingestion
