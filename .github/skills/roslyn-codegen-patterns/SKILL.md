---
name: roslyn-codegen-patterns
title: Roslyn Code Generation Patterns
description: Apply production Roslyn source-generator patterns for incremental pipelines, attributed discovery, multi-file emission, debugging, testing, and performance-focused generated code.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1243
appliesTo: '**/*.{cs,csproj,props,targets,editorconfig}'
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - roslyn-source-generator-authoring
related_skills:
  - roslyn-source-generator-authoring
  - roslyn-analyzer-authoring
  - writing-mstest-tests
  - mcp-csharp-test
tags:
  - roslyn
  - source-generators
  - incremental
  - performance
  - testing
  - ifx
---
# Roslyn Code Generation Patterns

Agent applies production Roslyn source-generator patterns that scale across large solutions, stable builds, and repeatable test pipelines.

## When to Use

| User prompt | Use |
|---|---|
| User asks for advanced source-generator architecture beyond basic authoring | Use this skill |
| User asks for incremental pipeline design, caching, or performance tuning | Use this skill |
| User asks for attributed type discovery, multi-file emission, or generator debugging | Use this skill |
| User asks for MSTest coverage around generator output or diagnostics | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for a first generator scaffold or a simple single-file generator | Use `roslyn-source-generator-authoring` |
| User asks for diagnostics without generated output | Use `roslyn-analyzer-authoring` |
| User asks for IDE edits after diagnostics | Use `roslyn-codefix-authoring` |
| User asks for runtime templating or reflection-based code creation | Use a runtime or build workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Trigger contract | Yes | Attribute, interface, base type, partial type, or syntax shape. |
| Output contract | Yes | File count, hint naming, namespace rules, and member shape. |
| Discovery model | Yes | Incremental provider, syntax receiver, or syntax-context receiver. |
| Test scope | Recommended | Positive, negative, diagnostic, and stability cases. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects the generator model and trigger contract. | Inspect generator registration. | Production work uses `IIncrementalGenerator` unless a legacy exception is documented. |
| 2 | Agent chooses the cheapest discovery surface. | Inspect predicate and transform stages. | Syntax filtering removes obvious non-candidates before semantic work. |
| 3 | Agent projects candidates into compact immutable data. | Inspect transform output types. | Projection keeps only emission facts and stable keys. |
| 4 | Agent emits deterministic files, diagnostics, and gated debug hooks. | Compare repeated runs and invalid-input behavior. | Output, diagnostics, and debugging remain stable and actionable. |
| 5 | Agent validates compilation, tests, and hot-path cost. | Run targeted generator tests. | Generated output compiles and repeated semantic work stays bounded. |

## Generator Decision Matrix

| Concern | Preferred Pattern | Pass |
|---|---|---|
| Generator model | `IIncrementalGenerator` for production generators | Incremental pipelines or documented legacy exceptions |
| Discovery | Syntax predicate first, semantic projection second | Semantic work appears only after filtering |
| Projection | Immutable, comparable models with no retained syntax trees | Incremental cache reuse stays stable |
| Output | Stable hint names and `StringComparer.Ordinal` ordering | Repeated runs emit identical files |
| Diagnostics | Roslyn diagnostics instead of exceptions | Invalid input yields actionable feedback |
| Debugging | `Debugger.Launch()` behind environment or build gating | CI and standard local builds stay non-interactive |

## Reference Files

| File | Purpose |
|---|---|
| [references/codegen-examples.md](references/codegen-examples.md) | Pipeline examples, receiver comparisons, attributed discovery, multi-file emission, debugging, testing, conventions, MCP hooks, pitfalls, and outputs. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Generator model | Inspect implementation type. | Production generator uses `IIncrementalGenerator` or documents the legacy exception. |
| Discovery cost | Review predicate and transform. | Semantic work appears only after syntax filtering. |
| Output determinism | Run the generator twice on identical input. | Hint names and text stay identical. |
| Diagnostic quality | Run invalid-input tests. | Diagnostic ID, title, and message stay actionable. |
| Compilation validity | Run updated compilation tests. | Generated output compiles cleanly. |
| Performance | Review allocations and repeated lookups. | No avoidable repeated symbol search remains in hot paths. |
