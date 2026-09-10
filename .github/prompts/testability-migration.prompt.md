---
mode: agent
title: Testability Migration Agent
description: Route .NET testability migration through detect, generate, and migrate phases.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1200
invokes_skills:
  - detect-static-dependencies
  - generate-testability-wrappers
  - migrate-static-to-wrapper
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills: []
appliesTo: '**/*'
tags:
  - prompts
  - prompt
  - ste
---
# Testability Migration Agent

Agent routes .NET testability migration through Detect, Generate, and Migrate phases.

## Phase Workflow

| Phase | Agent Action | Test | Pass |
|---|---|---|---|
| Detect | Agent runs `detect-static-dependencies` for the selected scope. | Read skill output. | Ranked static dependency findings exist. |
| Generate | Agent runs `generate-testability-wrappers` or adopts built-in abstractions. | Read diff or instructions. | Abstraction choice is explicit. |
| Migrate | Agent runs `migrate-static-to-wrapper` for the agreed scope. | Review diff. | Static calls are replaced in scope. |

## Generate Decisions

| Condition | Agent Action | Test | Pass |
|---|---|---|---|
| `TimeProvider`, `IHttpClientFactory`, or `System.IO.Abstractions` already fits | Agent adopts the built-in abstraction. | Read design note. | Built-in type name exists. |
| Existing wrapper already covers the target static | Agent skips wrapper generation. | Read design note. | Existing wrapper name exists. |
| No existing abstraction fits | Agent generates interface and implementation. | Review diff. | Interface and implementation files exist. |
| Class is static or DI is absent | Agent uses ambient context. | Read design note. | Ambient-context choice is explicit. |
| Scope has fewer than 5 call sites | Agent uses ambient context. | Count call sites. | Call site count < 5. |

## Safety Gates

| Condition | Agent Action | Test | Pass |
|---|---|---|---|
| Generated code target | Agent skips the file. | Review file list. | No `*.Designer.cs`, `*.g.cs`, `obj/`, or `bin/` file changed. |
| `.NET Framework < 4.6` and `TimeProvider` requested | Agent stops and warns. | Read report. | Warning names compatibility limit. |
| Sealed class mock request | Agent wraps the sealed class. | Review diff. | Wrapper exists around sealed class API. |

## Workflow Steps

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent runs Detect for the selected scope. | Read skill output. | Findings are ranked by frequency. |
| 2 | Agent selects one category and one scope slice. | Read plan note. | Category and scope are explicit. |
| 3 | Agent runs Generate or records skip reason. | Review diff or note. | Abstraction path is explicit. |
| 4 | Agent runs `dotnet build`. | Run build. | Exit code = 0. |
| 5 | Agent runs Migrate for the selected slice. | Review diff. | In-scope static calls are replaced. |
| 6 | Agent updates existing tests with test doubles when files are in scope. | Review diff. | Test updates reference the new abstraction. |
| 7 | Agent runs `dotnet build` again. | Run build. | Exit code = 0. |
| 8 | Agent writes remaining scope and next slice. | Read report. | Report names remaining files or categories. |

## Report Contract

| Section | Test | Pass |
|---|---|---|
| Phase executed | Read report. | Detect, Generate, or Migrate is named. |
| Scope | Read report. | Project, namespace, or file slice is named. |
| Changes | Read report. | File-level change list exists. |
| Build result | Read report. | Build command and outcome exist. |
| Remaining scope | Read report. | Next slice or none exists. |
