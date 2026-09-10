---
name: dotnet-naming-standards
title: .NET Naming Standards
description: Govern identifier, file, project, and API naming across the repository's .NET code.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1200
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-code-quality-standards
  - dotnet-code-review
  - dotnet-namespace-standards
appliesTo: "**/*.{cs,csproj,sln,slnx}"
tags:
  - dotnet
  - naming
  - conventions
  - code-quality
  - api-design
  - analyzers
---

# .NET Naming Standards

Agent keeps names predictable, analyzable, and repository-aligned. Naming drift signals design drift.

## When to Use

| Condition | Use |
|---|---|
| Agent adds or renames types, methods, properties, files, or projects | Use this skill |
| Agent reviews public API shape or repository naming drift | Use this skill |
| Agent writes analyzers or code fixes for naming rules | Use this skill |
| Agent refactors legacy code toward repository naming rules | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Code is vendor-generated or framework-generated | Do not use this skill |
| External contract requires exact symbol names | Preserve external names |
| Audit scope is read-only | Record defects without renaming |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target artifact | Yes | Identifier, file, project, namespace, or API signature |
| Project type | Yes | Application, library, test project, or Roslyn tooling |
| Layer | Yes | Client, Manager, Engine, Access, Ifx, or shared component |
| Exception list | No | Generated or contract-bound names that stay unchanged |

## Workflow

Agent follows these steps:

1. Agent classifies artifact kind, ownership, and contract constraints.
2. Agent applies the rule matrix to casing, suffixes, file names, and project names.
3. Agent writes all linked references, docs, and tests to match the renamed artifact.
4. Agent verifies build output and analyzer output after each rename set.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero naming analyzer warnings in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| NAME-001 | Public types and public members use PascalCase | Mixed casing or acronym drift | Use PascalCase and repository acronym style |
| NAME-002 | Parameters, locals, and private fields use camelCase | PascalCase locals or underscore-prefixed new fields | Use camelCase |
| NAME-003 | Async methods end with `Async` | `Task` or `ValueTask` methods without async suffix | Rename method and callers |
| NAME-004 | Boolean members read as state or decision | `Get` or vague verbs on `bool` members | Use `Is`, `Has`, `Supports`, or decision verb |
| NAME-005 | File names match primary type or owned role | File name and primary type drift | Align file name with primary type |
| NAME-006 | Project and namespace names show layer and domain role | Generic names such as `Common2` or `Helpers` | Use layer and domain name |
| NAME-007 | Names describe responsibility, not trivia | `Helper`, `Data`, `Util`, or numbered suffixes | Use contract or responsibility name |
| NAME-008 | External or generated exceptions stay explicit | Contract-bound names changed by local convention | Preserve name and document exception |

## Convention Matrix

| Artifact | Pattern | Example |
|---|---|---|
| Public type | PascalCase | `InvoiceScheduler` |
| Private field or parameter | camelCase | `cancellationToken` |
| Async method | Verb or noun plus `Async` | `LoadInvoicesAsync` |
| Test project | Source name plus test suffix | `Client.Portal.WebApi.UnitTests` |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| API rename verification | `dotnet test [test-project].csproj` | Zero failing tests in changed scope |
| Text scan verification | Search changed files for old symbol names | Zero stale names outside documented exceptions |

## Verification Checklist

Agent verifies:
- [ ] Names map to real responsibility
- [ ] Casing matches repository convention
- [ ] Async suffix rules stay consistent
- [ ] File, type, project, and namespace names stay aligned
- [ ] Exceptions stay documented and intentional

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Generic names such as `Helper` hide responsibility | Use contract or domain role |
| Rename scope stops at one file | Write callers, docs, and tests too |
| New code copies legacy underscore fields | Use current camelCase field style |
| Generated or external names get normalized by habit | Preserve required external names |
