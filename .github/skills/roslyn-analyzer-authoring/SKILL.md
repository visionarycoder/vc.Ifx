---
name: roslyn-analyzer-authoring
description: Author Roslyn analyzers with precise diagnostics, narrow registration, C# 8-safe implementation, and MSTest verification for src/ifx projects.
title: Roslyn Analyzer Authoring
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1320
appliesTo: '**/*.{cs,csproj,editorconfig,props,targets}'
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - roslyn-codefix-authoring
  - ca-code-quality-fixes
  - mcp-csharp-test
tags:
  - roslyn
  - analyzers
  - diagnostics
  - ifx
---
# Roslyn Analyzer Authoring

Agent authors analyzers for `src/ifx` packages. Agent keeps analyzer logic deterministic, narrow, and C# 8-safe for `netstandard2.0` Roslyn projects.

## When to Use

| User prompt | Use |
|---|---|
| User asks for a new diagnostic rule | Use this skill |
| User asks to enforce architecture or API patterns at compile time | Use this skill |
| User asks for analyzer tests or packaging updates | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for automatic source edits | Use `roslyn-codefix-authoring` |
| User asks for generated code instead of diagnostics | Use `roslyn-source-generator-authoring` |
| User asks for a one-time grep or script audit | Use direct tools |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Rule intent | Yes | State the exact invalid pattern. |
| Diagnostic metadata | Yes | Provide ID, title, message, category, severity. |
| Analysis surface | Yes | Pick syntax, symbol, operation, or compilation scope. |
| False-positive limits | Recommended | List exclusions, generated code rules, and valid exceptions. |
| Packaging target | No | State analyzer package or project target. |

## Guardrail Matrix

| Concern | Agent action |
|---|---|
| Target framework | Agent confirms `netstandard2.0` before using Roslyn project patterns. |
| Language level | Agent uses C# 8 syntax only inside `src/ifx`. |
| Generated code | Agent calls `ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)` unless the rule needs generated files. |
| Concurrency | Agent calls `EnableConcurrentExecution()`. |
| Registration scope | Agent picks the cheapest action that answers the rule. |
| Locations | Agent reports diagnostics on stable member or syntax locations. |
| Tests | Agent adds positive and negative MSTest cases. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent defines one rule contract. | Read descriptor draft. | ID, title, message, category, and severity are explicit. |
| 2 | Agent selects the narrowest registration surface. | Inspect `Initialize`. | Registration uses syntax, symbol, or operation callbacks only where needed. |
| 3 | Agent filters cheaply before semantic work. | Inspect analysis method. | Syntax or symbol shape removes obvious non-candidates. |
| 4 | Agent resolves semantic facts late. | Inspect semantic calls. | Symbol or compilation lookups appear only after cheap filters. |
| 5 | Agent reports one stable diagnostic. | Run analyzer tests. | Squiggle location and message match the rule contract. |
| 6 | Agent validates hot-path cost. | Review allocations and cached lookups. | No repeated expensive lookups appear in per-node paths. |
| 7 | Agent adds MSTest coverage. | Run analyzer test project. | Positive and negative tests pass. |
| 8 | Agent verifies package loading. | Inspect `.csproj` and package assets. | Analyzer output lands under `analyzers/dotnet/cs` when packing applies. |

## Analysis Pattern Matrix

| Need | Preferred pattern |
|---|---|
| Cheap shape check | `RegisterSyntaxNodeAction` plus syntax filtering |
| Member contract rule | `RegisterSymbolAction` on the exact symbol kind |
| Semantic expression rule | `RegisterOperationAction` on the exact operation kind |
| Shared type lookup | `RegisterCompilationStartAction` plus cached symbol lookup |
| Stable report site | `Diagnostic.Create(rule, syntax.GetLocation(), args)` or member location |

## Test Matrix

| Test | Agent verifies |
|---|---|
| Positive case | Invalid code raises the expected diagnostic ID and message. |
| Negative case | Valid code stays silent. |
| Boundary case | Exclusion paths stay silent. |
| Location case | Diagnostic lands on the intended span. |
| Framework case | `netstandard2.0` analyzer code compiles with C# 8-safe syntax. |

## Validation Checklist

- [ ] Agent kept Roslyn project code compatible with `netstandard2.0` and C# 8.
- [ ] Agent used the narrowest possible registration surface.
- [ ] Agent filtered candidates before expensive semantic work.
- [ ] Agent reported diagnostics at stable, developer-facing locations.
- [ ] Agent added MSTest positive and negative coverage.
- [ ] Agent verified packaging or project wiring when the scope included distribution.

## References

Agent reads `references/analyzer-patterns.md` for descriptor templates, registration examples, performance notes, and test snippets.
