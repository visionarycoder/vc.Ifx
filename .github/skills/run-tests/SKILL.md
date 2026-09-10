---
name: run-tests
title: Run .NET Tests
description: Detect the .NET test platform and generate or run the narrowest correct dotnet test command.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1014
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - platform-detection
  - filter-syntax
  - mtp-hot-reload
appliesTo: '**/*.{cs,csproj,sln,slnx,slnf,json,props,md}'
tags:
  - dotnet
  - tests
  - dotnet-test
  - mtp
  - vstest
---
# Run .NET Tests

Agent detects the test platform and generates or runs the correct `dotnet test` command.

## When to Use

| User prompt | Use |
|---|---|
| User asks for an exact `dotnet test` command | Use this skill |
| User asks for MTP or VSTest filter syntax | Use this skill |
| User asks why a test flag fails | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to write tests | Use a test-writing skill |
| User asks for framework migration | Use the matching migration skill |
| User asks to debug test logic | Read the failing test code directly |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Test target | No | Project, solution, or current directory |
| Requested subset | No | Class, trait, category, method, or framework |
| Platform evidence | Yes | Read `global.json`, `.csproj`, and shared props |
| Extra flags | No | TRX, coverage, blame, verbosity, or `--no-build` |

## Platform Matrix

| Signal | Platform | Command shape |
|---|---|---|
| `global.json` sets `runner` to `Microsoft.Testing.Platform` | MTP on SDK 10+ | `dotnet test --project <path> <mtp args>` |
| `TestingPlatformDotnetTestSupport=true` | MTP on SDK 8 or 9 | `dotnet test <target> -- <mtp args>` |
| No MTP signal | VSTest | `dotnet test <target> [--filter ...]` |

## Filter Matrix

| Framework | Filter |
|---|---|
| MSTest or NUnit on VSTest | `--filter <expression>` |
| MSTest or NUnit on MTP | `--filter <expression>` |
| xUnit v3 on MTP | `--filter-class`, `--filter-method`, `--filter-trait`, or `--filter-query` |
| TUnit on MTP | `--treenode-filter` |

## Flag Matrix

| Need | VSTest | MTP |
|---|---|---|
| TRX | `--logger trx` | `--report-trx` when the extension exists |
| Coverage | `--collect "Code Coverage"` or repo standard | `--coverage` when the extension exists |
| Modern target path | Positional target is allowed | Use `--project` or `--solution` on SDK 10+ |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads `global.json`, target `.csproj`, and shared props. | Read those files. | One platform classification is possible. |
| 2 | Agent classifies VSTest or MTP. | Compare repo signals to the platform matrix. | Classification matches repo evidence. |
| 3 | Agent reads the framework and requested subset. | Read package references or test SDK settings. | Filter syntax matches the framework. |
| 4 | Agent generates the narrowest command. | Read the final command. | Command scope matches the user prompt. |
| 5 | Agent places optional flags on the correct side of `--`. | Read the command. | MTP on SDK 8 or 9 uses `--` once. |
| 6 | Agent runs the command when the user asked for execution. | Run `dotnet test ...`. | Command exits without syntax errors. |

## Verification Checklist

- [ ] Agent read platform evidence before writing the command.
- [ ] Agent matched filter syntax to the framework.
- [ ] Agent used `--project` or `--solution` for SDK 10+ MTP.
- [ ] Agent used optional flags supported by the platform.
- [ ] Agent kept the command scope narrow.
- [ ] Agent ran the command only when the user asked for execution.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent guesses the platform | Agent reads repo signals first. |
| Agent uses VSTest flags on MTP | Agent matches each flag to the platform matrix. |
| Agent runs the whole suite for one test | Agent adds the narrowest filter. |
| Agent passes a bare path to SDK 10+ MTP | Agent uses `--project` or `--solution`. |
