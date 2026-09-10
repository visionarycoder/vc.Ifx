---
title: Skill Lifecycle Drift Detectors
description: Version, API, and pattern drift catalog for lifecycle audits and migration passes across repository skills.
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1947
prerequisites:
  - skill-lifecycle-controller
related_skills:
  - skill-lifecycle-controller
  - skill-suite-optimization
appliesTo: '.github/**/*.{md,prompt.md,instructions.md}'
tags:
  - drift
  - versions
  - migration
  - audit
---
# Skill Lifecycle Drift Detectors

Use this reference during `audit` and `migrate` mode when the controller needs a compact drift catalog instead of verbose inline guidance.

## .NET Version Progression

| From | To | Repository Guidance Focus | Review Targets |
|---|---|---|---|
| .NET 8 | .NET 9 | Refresh baseline runtime wording, package references, and test guidance | Skills that still describe .NET 8 as current |
| .NET 9 | .NET 10 | Update default runtime language, analyzer expectations, and framework examples | Skills that still promote .NET 9 or older patterns |
| .NET 10 | .NET 11 | Update default platform claims, C# pairing, SDK wording, and compatibility notes | Skills that mention preview assumptions or interim migration steps |

## C# Version Progression

| From | To | Guidance Focus | Review Targets |
|---|---|---|---|
| C# 8 | C# 12 | Remove legacy syntax examples when the project target allows newer syntax | Old examples in non-Roslyn projects |
| C# 12 | C# 13 | Refresh current-language statements and syntax examples | Skills that describe C# 12 as current |
| C# 13 | C# 14 | Update forward guidance for new repository defaults after platform adoption | Migration playbooks and language-standard references |

## Version Drift Markers

| Marker Type | Detect | Interpretation | Recommended Action |
|---|---|---|---|
| Explicit version text | `\.NET\s+[0-9]+`, `C#\s+[0-9]+`, `MSTest v[0-9]+` | Direct version pin or current-state claim | Compare to repository baseline and update if stale |
| Relative phrasing | `latest`, `current`, `newest`, `preview` | Potential stale wording after platform change | Replace with explicit version text when precision matters |
| Package-era terms | `net8.0`, `net9.0`, `LangVersion`, `preview features` | Build or language guidance tied to an old era | Confirm whether the repository still uses that era |
| Test-stack references | `MSTest v3`, `v2`, `legacy MSTest` | Test guidance drift | Align with the current test stack |

## Deprecated API Catalog

| API or Pattern | Drift Signal | Preferred Direction | Audit Notes |
|---|---|---|---|
| `DateTime.UtcNow` | Time acquisition appears in guidance without testability context | Prefer `TimeProvider` for injectable time | Flag when the guidance concerns application code or testable services |
| `DateTime.Now` | Local time appears in persistence or logic examples | Prefer `TimeProvider` or explicit zone handling | Flag as stronger risk than `UtcNow` |
| `ILogger.Log` | Generic logging examples dominate severity-specific methods | Prefer `LogTrace`, `LogDebug`, `LogInformation`, `LogWarning`, `LogError`, `LogCritical` | Flag when guidance omits structured logging examples |
| `Task.Result` | Blocking wait appears in async guidance | Prefer `await` end-to-end | Flag as superseded async pattern |
| `Task.Wait()` | Blocking wait appears in workflow examples | Prefer `await` or async orchestration | Flag as superseded async pattern |
| `Thread.Sleep` | Blocking delay appears in examples | Prefer `Task.Delay` or scheduler control | Flag unless the context is low-level threading diagnostics |
| `BinaryFormatter` | Obsolete serialization guidance exists | Prefer supported serializers | Treat as urgent drift if present |
| `WebRequest` or `HttpWebRequest` | Legacy HTTP stack appears | Prefer `HttpClient` patterns | Flag unless historical context is explicit |
| `IHostBuilder` setup examples that omit modern host defaults | Host guidance uses outdated bootstrap shape | Prefer current hosting patterns used by the repository | Flag when the guidance claims a default pattern |

## Superseded Pattern Rules

| Old Pattern | New Pattern | Detection Cue | Migration Recommendation |
|---|---|---|---|
| Blocking `.Result` or `.Wait()` | Full `async` and `await` flow | Search for `.Result`, `.Wait(`, sync-over-async wording | Rewrite examples and workflow steps to remain async |
| `Task.Run` for ordinary CPU advice | Direct synchronous execution, pipeline parallelism, or purpose-built background services | Search for `Task.Run` in general guidance | Keep `Task.Run` only when isolation or scheduler handoff is the explicit goal |
| `Task.Run` for server request handling | Native async I/O or queued background service | Search for `Task.Run` in Web API or service guidance | Remove as a scaling pattern |
| String-concatenated logging | Structured logging with templates | Search for `+` around log messages or interpolation-only logging guidance | Replace with template-based logging examples |
| Direct system time in tests | `TimeProvider` or injected clock abstraction | Search for `UtcNow`, `Now`, `DateOnly.FromDateTime` in test guidance | Shift to deterministic time sources |
| Broad `catch (Exception)` with no classification | Focused exception handling or result patterns | Search for catch-all guidance in workflow tables | Narrow the guidance to known failure classes |

## Category Grouping Rules

| Category | Include | Exclude |
|---|---|---|
| Versions | Runtime, language, package-era, and test-stack version statements | Pure dates in front matter |
| APIs | Named framework APIs and library surface recommendations | File paths or package IDs with similar text only |
| Patterns | Async, logging, time, scheduling, and exception-handling guidance shapes | One-off code snippets with historical labels |

## Bulk Migration Heuristics

| Heuristic | Use | Benefit |
|---|---|---|
| Group by exact text | Many skills contain the same version phrase | Produces deterministic replacements |
| Group by category | Versions, APIs, patterns need different review language | Keeps reports readable |
| Update references first | Deep catalogs often drive repeated main-file wording | Reduces duplicate edits |
| Recalculate tokens after each batch | Version text length changes can shift budgets | Keeps estimates accurate |
| Re-run modal scan after replacements | Naive replacements can reintroduce prohibited wording | Preserves STE compliance |

## Review Queries

| Query Goal | Suggested Search Shape |
|---|---|
| Find stale runtime statements | Search for `\.NET 8|\.NET 9|\.NET 10` |
| Find stale language statements | Search for `C# 8|C# 12|C# 13` |
| Find stale test references | Search for `MSTest v2|MSTest v3` |
| Find deprecated time access | Search for `DateTime.UtcNow|DateTime.Now` |
| Find blocking async patterns | Search for `\.Result|\.Wait\(|Thread.Sleep` |
| Find generic logging | Search for `ILogger\.Log|logger\.Log\(` |

## Reporting Pattern

| Report Section | Required Fields |
|---|---|
| Versions | File path, stale text, target version, recommended replacement |
| APIs | File path, API name, preferred direction, urgency |
| Patterns | File path, pattern cue, migration recommendation, confidence |

## Residual Risk Rules

| Situation | Report Rule |
|---|---|
| Historical reference file intentionally preserves old guidance | Record as accepted historical context |
| Roslyn `netstandard2.0` skill guidance references C# 8 intentionally | Record as valid exception |
| User-requested migration stops before edits | Return the plan and pending file count |
