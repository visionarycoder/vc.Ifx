---
name: mvvm-source-generators
title: MVVM Source Generator Authoring
description: Author C# 8-safe Roslyn incremental generators for MVVM properties, commands, validation, and diagnostics when repository work needs custom compile-time MVVM code generation.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1287
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - roslyn-source-generator-authoring
related_skills:
  - roslyn-source-generator-authoring
  - wpf-mvvm-implementation
  - maui-patterns
  - winui3-patterns
  - mcp-csharp-test
appliesTo: '**/*.{cs,csproj,props,targets}'
tags:
  - mvvm
  - source-generators
  - roslyn
  - incremental
  - netstandard2.0
---
# MVVM Source Generator Authoring

Agent authors MVVM-focused source generators with Roslyn incremental pipelines, deterministic output, explicit diagnostics, and repository-safe `netstandard2.0` plus C# 8 implementation constraints.

## When to Use

| Condition | Use |
|---|---|
| Agent creates a custom MVVM attribute or convention that repeats across many view models. | Agent uses this skill. |
| Agent generates `INotifyPropertyChanged`, `ICommand`, or validation code at compile time. | Agent uses this skill. |
| Agent needs generator diagnostics for missing `partial` types or invalid MVVM signatures. | Agent uses this skill. |
| Agent packages a generator as an analyzer NuGet for reuse across solutions. | Agent uses this skill. |

## When Not to Use

| Condition | Route |
|---|---|
| Agent consumes CommunityToolkit.Mvvm instead of authoring a new generator. | Agent uses `communitytoolkit-mvvm`. |
| Agent needs modular navigation, regions, or dialogs instead of source generation. | Agent uses `prism-framework-patterns`. |
| Agent adds runtime reflection or dynamic proxy behavior. | Agent uses a runtime pattern, not a source generator. |
| Agent works outside Roslyn-compatible project constraints. | Agent uses a non-generator implementation path. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Trigger contract | Yes | Attributes, interfaces, naming, or syntax shape. |
| Generated artifact | Yes | Property, command, validation, or helper output. |
| Partial-type contract | Yes | Type kinds, accessibility, and nesting rules. |
| Diagnostic set | Yes | Missing-partial, invalid-signature, and unsupported-shape coverage. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent defines triggers, generated API shape, and partial-type rules. | Review contract tables and target examples. | Positive and negative cases stay explicit. |
| 2 | Agent creates an incremental pipeline with syntax-first discovery and compact projection models. | Inspect the pipeline stages. | Candidate filtering stays cheap and deterministic. |
| 3 | Agent emits MVVM members with stable naming, ordering, and formatting. | Run the generator twice on the same input. | Output stays stable. |
| 4 | Agent reports diagnostics for missing `partial`, invalid signatures, and unsupported shapes. | Compile negative test cases. | Diagnostics appear instead of generator exceptions. |
| 5 | Agent validates packaging and consumer compilation under repository Roslyn constraints. | Build generator and consuming projects. | Analyzer assets load and C# 8-safe implementation remains intact. |

## Authoring Decision Matrix

| Concern | Pattern | Pass |
|---|---|---|
| Target framework | `netstandard2.0` generator projects | Roslyn host compatibility stays intact |
| Language ceiling | C# 8 syntax in generator implementation files | No prohibited syntax appears |
| Discovery | `CreateSyntaxProvider` with cheap syntax predicates | Non-candidate syntax drops early |
| MVVM members | Generated observable properties, commands, and validation hooks | Generated APIs match the intended notification and command contracts |
| Diagnostics | Compiler diagnostics for missing `partial` and invalid signatures | Invalid input stays actionable |
| Packaging | Analyzer assets under `analyzers/dotnet/cs` | Consumer projects load the generator automatically |

## Reference Files

| File | Purpose |
|---|---|
| [references/authoring-examples.md](references/authoring-examples.md) | Manual-versus-generated comparisons, boilerplate samples, detailed diagnostic rules, integration hooks, MCP hooks, pitfalls, and outputs. |

## Verification Checklist

| Item | Test | Pass |
|---|---|---|
| Generator project targets `netstandard2.0`. | Inspect the project file. | Roslyn host compatibility stays intact. |
| Generator implementation stays C# 8-safe. | Build and inspect touched generator files. | No prohibited syntax appears. |
| Missing-partial diagnostics exist. | Compile one invalid target. | Diagnostic ID, message, and location stay actionable. |
| GeneratorDriver tests cover positive and negative cases. | Run the existing generator test project. | Generated source and diagnostics stay verified. |
| Package layout exposes analyzer assets. | Inspect the packed NuGet. | Consumer projects load the generator without manual copy steps. |
