---
name: msbuild-modernization
license: MIT
title: MSBuild Modernization: Legacy to SDK-style Migration
description: Modernize legacy MSBuild project files into SDK-style projects with explicit checkpoints and measurable validation.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1300
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - directory-build-organization
related_skills:
  - convert-to-cpm
  - directory-build-organization
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog,json,md}'
tags:
  - msbuild
  - modernization
  - sdk-style
---
# MSBuild Modernization: Legacy to SDK-style Migration

Agent migrates legacy MSBuild projects to SDK-style structure with explicit package strategy, centralized settings, and verified build parity.

## When to Use

| Condition | Use |
|---|---|
| Agent finds `ToolsVersion`, explicit SDK imports, or manual compile-item lists | Use this skill |
| Agent replaces `packages.config` with `PackageReference` or CPM | Use this skill |
| Agent centralizes repeated build properties after a project converts cleanly | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Project already uses SDK-style structure | Use targeted cleanup instead |
| Project type cannot move to SDK-style in current scope | Preserve the legacy format |
| Work only reorganizes shared props/targets in modern projects | Use `directory-build-organization` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Legacy project files | Yes | `.csproj`, `.vbproj`, props, targets |
| Target framework plan | Yes | Stay on .NET Framework or move to modern .NET |
| Package model | Yes | `packages.config`, `PackageReference`, or CPM destination |
| Build command | Yes | Existing repo build command for parity checks |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent confirms the project is legacy and captures a clean pre-change build baseline | Baseline status |
| 2 | Agent converts the root project structure to SDK-style and removes redundant imports and default item lists | Updated project file |
| 3 | Agent migrates package references and assembly metadata handling with explicit keep-or-remove decisions | Modernized dependency model |
| 4 | Agent rebuilds the standalone project before centralizing repeated properties or targets | Stable SDK-style project |
| 5 | Agent moves repeated settings into `Directory.Build.props`, `Directory.Build.targets`, or CPM only after the project builds cleanly | Centralized repo settings |
| 6 | Agent reruns build and affected tests to verify parity | Verification evidence |

Test: Agent runs the existing restore, build, and affected test commands before and after conversion.
Pass: Agent observes zero new restore errors. Agent observes zero new build errors. Agent sees the converted project produce the expected outputs with no missing source files.

## Conversion Matrix

| Legacy indicator | Agent converts to | Agent preserves when needed |
|---|---|---|
| `ToolsVersion` plus `Microsoft.CSharp.targets` import | `<Project Sdk="Microsoft.NET.Sdk">` | Custom targets with real behavior |
| Manual `<Compile Include>` lists | SDK implicit globs | Items with non-default metadata |
| `packages.config` | `PackageReference` or CPM | None, unless migration is explicitly deferred |
| `AssemblyInfo.cs` metadata duplication | Project properties or `GenerateAssemblyInfo=false` | Custom attributes that SDK cannot generate |
| Repeated per-project properties | Directory-level props/targets | Project-local settings with unique behavior |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Restore verification | `dotnet restore [entry]` | Zero restore errors |
| Compile verification | `dotnet build [entry]` | Zero build errors |
| Item verification | Inspect produced compile/content items or build output | No missing required files |
| Test verification | Existing affected test command | Zero new failures |

## Verification Checklist

Agent verifies:
- [ ] Project was confirmed to be legacy before conversion
- [ ] SDK-style root and imports are correct
- [ ] Manual items remain only when custom metadata requires them
- [ ] Package model is modernized intentionally
- [ ] Shared settings moved only after standalone build parity existed

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Converting every XML element blindly | Preserve only behavior-bearing items |
| Centralizing settings before project parity exists | Stabilize the single project first |
| Removing explicit items with metadata | Keep the explicit item when metadata matters |
| Mixing modernization with unrelated package churn | Separate package updates from structural conversion when possible |
