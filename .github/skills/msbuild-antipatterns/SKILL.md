---
name: msbuild-antipatterns
license: MIT
title: MSBuild Anti-Pattern Catalog
description: Review MSBuild files for high-value anti-patterns and replace them with native, maintainable, and diagnosable build patterns.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1260
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - target-authoring
  - directory-build-organization
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog,json,md}'
tags:
  - msbuild
  - antipatterns
---
# MSBuild Anti-Pattern Catalog

Agent audits build files for common anti-patterns and replaces them with safer MSBuild-native patterns.

## When to Use

Agent uses this skill when:
- User requests maintainability cleanup in `.csproj`, `.props`, or `.targets`
- Build logic looks shell-heavy, duplicated, or non-portable
- Agent prepares build files for modernization or diagnostics

## When Not to Use

Agent does not use this skill when:
- Build system is not MSBuild based
- Task is a full SDK-style migration
- Runtime code quality is the only concern

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| File scope | Yes | One or more build files |
| Project style | Yes | SDK-style, legacy, mixed, or migration stage |
| Cleanup goal | No | Portability, readability, incrementality, or restore hygiene |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Scan categories | Agent searches for shelling out, duplication, hardcoded paths, and default noise | Agent groups findings by anti-pattern | Agent has a short prioritized findings list |
| 2. Confirm project context | Agent verifies SDK-style versus legacy behavior | Agent checks file type and existing conventions | Agent avoids invalid blanket edits |
| 3. Replace with native patterns | Agent uses built-in tasks, shared props, and portable paths | Agent maps each smell to one fix | Replacement improves diagnosability |
| 4. Centralize repeated settings | Agent moves repeated settings into shared build files when appropriate | Agent compares sibling projects | Duplication drops measurably |
| 5. Verify | Agent rebuilds or re-evaluates the touched scope | Agent checks for build correctness and clearer structure | Cleanup preserves behavior |

## High-Value Matrix

| ID | Smell | Agent replacement |
|---|---|---|
| AP-01 | `<Exec>` for file operations | `MakeDir`, `Copy`, `Delete`, `Move`, `WriteLinesToFile` |
| AP-02 | Unquoted `Condition` comparisons | Single-quoted operands on both sides |
| AP-03 | Hardcoded absolute paths | `MSBuildThisFileDirectory`, `MSBuildProjectDirectory`, or repo-relative paths |
| AP-04 | Repeated SDK defaults | Remove explicit defaults and keep only intentional overrides |
| AP-05 | Manual source lists in SDK projects | Use SDK globs plus targeted `Remove` or `Exclude` |
| AP-06 | Legacy NuGet `HintPath` references | `PackageReference` |
| AP-07 | Analyzer or tool packages without `PrivateAssets="all"` | Private project-scoped packages |
| AP-08 | Copied property blocks across projects | `Directory.Build.props`, `Directory.Build.targets`, or `Directory.Packages.props` |

## Validation

| Check | Test | Pass |
|---|---|---|
| Anti-pattern quality | Agent proves each cleanup removes a real smell | Fixes map to a catalog entry |
| Native replacement | Agent replaces shell or legacy workarounds where possible | MSBuild owns more of the workflow |
| Portability | Agent removes machine-specific paths and brittle commands | Build files become environment-agnostic |
| Behavior preservation | Agent rebuilds the touched scope | Build result stays correct |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent rewrites unusual constructs without context | Agent verifies project style first |
| Agent removes F# compile ordering | Agent preserves explicit order in `.fsproj` files |
| Agent keeps shell commands because they already work | Agent prefers native MSBuild tasks that the engine understands |
| Agent fixes one duplicate block but leaves the pattern | Agent centralizes the shared setting once |
