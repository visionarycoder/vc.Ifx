---
name: roslyn-source-generator-authoring
description: Author Roslyn source generators with deterministic emission, syntax-first discovery, C# 8-safe implementation, and GeneratorDriver validation for src/ifx projects.
title: Roslyn Source Generator Authoring
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1410
appliesTo: '**/*.{cs,csproj,props,targets}'
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - roslyn-analyzer-authoring
  - roslyn-codefix-authoring
  - mcp-csharp-test
tags:
  - roslyn
  - source-generators
  - incremental
  - ifx
---
# Roslyn Source Generator Authoring

Agent authors source generators for `src/ifx` packages. Agent keeps discovery cheap, output deterministic, and generator code compatible with `netstandard2.0` Roslyn projects.

## When to Use

| User prompt | Use |
|---|---|
| User asks for compile-time code generation | Use this skill |
| User asks to replace repeated boilerplate from attributes or interfaces | Use this skill |
| User asks for GeneratorDriver or incremental generator tests | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for diagnostics only | Use `roslyn-analyzer-authoring` |
| User asks for IDE edits instead of generated files | Use `roslyn-codefix-authoring` |
| User asks for runtime code generation or external service data | Use a build or runtime workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Trigger surface | Yes | Attribute, interface, syntax, or naming contract. |
| Generated artifact | Yes | State the file or type shape. |
| Generator model | Yes | Pick `IIncrementalGenerator` unless the scope is trivial. |
| Diagnostic needs | No | Add generator diagnostics for invalid input contracts. |
| Packaging target | No | State project or NuGet consumption needs. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent chooses generator model. | Inspect interface type. | New nontrivial work uses `IIncrementalGenerator`. |
| 2 | Agent defines the trigger contract. | Review attributes or syntax rules. | Positive and negative trigger cases are explicit. |
| 3 | Agent discovers candidates with syntax-first filters. | Inspect syntax provider or receiver. | Cheap predicates remove non-candidates early. |
| 4 | Agent projects syntax into small immutable models. | Inspect transform output. | Projection data contains only facts needed for emission. |
| 5 | Agent emits deterministic source. | Compare repeated output. | Hint names, ordering, and formatting stay stable. |
| 6 | Agent reports generator diagnostics for invalid inputs. | Run diagnostic tests. | Invalid contracts fail with actionable messages. |
| 7 | Agent validates output with GeneratorDriver tests. | Run generator tests. | Generated source exists and updated compilation succeeds. |
| 8 | Agent verifies packaging. | Inspect `.csproj`. | Package assets load as analyzer/generator assets where required. |

## Pattern Matrix

| Concern | Preferred pattern |
|---|---|
| New generator work | `IIncrementalGenerator` |
| Candidate discovery | `CreateSyntaxProvider` with cheap predicate |
| Projection | Immutable records or compact models |
| Emission | Stable hint names plus deterministic ordering |
| Invalid input | Roslyn diagnostics instead of thrown exceptions |
| Packaging | `analyzers/dotnet/cs` assets in `netstandard2.0` projects |

## Test Matrix

| Test | Agent verifies |
|---|---|
| Trigger positive | Valid annotated or matched input emits source. |
| Trigger negative | Non-target input emits nothing. |
| Compilation | Post-generation compilation succeeds. |
| Diagnostic | Invalid input raises the intended generator diagnostic. |
| Stability | Two runs over the same input produce identical output. |
| C# 8 | Generator implementation code stays C# 8-safe in `src/ifx`. |

## Validation Checklist

- [ ] Agent kept generator implementation code compatible with `netstandard2.0` and C# 8.
- [ ] Agent used syntax-first discovery before semantic projection.
- [ ] Agent kept projection models compact and immutable.
- [ ] Agent emitted stable hint names and stable member ordering.
- [ ] Agent reported diagnostics instead of throwing for invalid contracts.
- [ ] Agent added GeneratorDriver or equivalent tests.

## References

Agent reads `references/generator-patterns.md` for incremental pipeline snippets, deterministic emission examples, diagnostic patterns, and packaging examples.
