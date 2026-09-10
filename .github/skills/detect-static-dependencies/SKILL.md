---
name: detect-static-dependencies
description: Scan C# code for hard-to-test static dependencies and generate a ranked report of call sites by category and file.
license: MIT
title: Detect Static Dependencies
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 951
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - generate-testability-wrappers
  - migrate-static-to-wrapper
appliesTo: '**/*'
tags:
  - detect
  - static
  - dependencies
---
# Detect Static Dependencies

Agent scans C# code for hard-to-test static dependencies and ranks the results.

## When to Use

| User prompt | Use |
|---|---|
| User asks for static coupling audit | Use this skill |
| User asks where `DateTime`, `File`, or `Environment` block testing | Use this skill |
| User asks which wrappers add the most value first | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to generate wrappers | Use `generate-testability-wrappers` |
| User asks for bulk migration | Use `migrate-static-to-wrapper` |
| Scope is not C# | Use a language-specific workflow |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Scan scope | Yes | File, directory, project, or solution |
| Exclusions | No | Extra ignore patterns |
| Category filter | No | Time, file system, environment, network, console, or process |

## Category Matrix

| Category | Common patterns | Replacement |
|---|---|---|
| Time | `DateTime.Now`, `DateTime.UtcNow`, `Task.Delay` | `TimeProvider` |
| File system | `File.*`, `Directory.*`, `Path.*` | `IFileSystem` or file wrapper |
| Environment | `Environment.*` | `IEnvironmentProvider` |
| Network | `new HttpClient`, `HttpClient.*` | `IHttpClientFactory` |
| Console | `Console.*` | `IConsole` or `ILogger` |
| Process | `Process.*` | `IProcessRunner` |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent resolves the scope to production `.cs` files only. | Read the file list. | Scope excludes `bin`, `obj`, generated files, and test projects. |
| 2 | Agent scans for category patterns. | Run `rg` for the selected patterns. | Matches exist or the report states zero matches. |
| 3 | Agent groups matches by category, pattern, and file. | Read the report data. | Each match appears in one category. |
| 4 | Agent ranks the results by call-site count. | Read the summary table. | Categories and files are sorted descending by count. |
| 5 | Agent maps each category to a replacement. | Read the final report. | Each category row includes one replacement. |
| 6 | Agent generates next steps for wrapper generation or migration. | Read recommendations. | Next steps point to the correct follow-up skill. |

## Required Report Sections

| Section | Requirement |
|---|---|
| Scope summary | File count and total call sites |
| Category summary | Counts and replacements |
| Top patterns | Highest-frequency static calls |
| Most affected files | File path and call-site count |
| Next steps | Wrapper or migration route |

## Verification Checklist

- [ ] Agent scanned production `.cs` files only.
- [ ] Agent excluded generated and build output files.
- [ ] Agent grouped results by category and file.
- [ ] Agent ranked results by count.
- [ ] Agent listed one replacement per category.
- [ ] Agent routed follow-up work to the matching skill.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent scans test code | Agent limits work to production code. |
| Agent counts wrapped calls | Agent removes calls already behind injected abstractions. |
| Agent misses generated files | Agent excludes generated patterns up front. |
| Agent reports counts without replacement paths | Agent adds the replacement matrix. |
