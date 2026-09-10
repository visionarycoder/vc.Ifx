---
name: migrate-mstest-v3-to-v4
title: MSTest v3 → v4 Migration
description: Upgrade MSTest v3 projects to MSTest v4 with package alignment, source fixes, and discovery verification.
doc_type: skill
status: active
last_updated: 2026-07-29
license: MIT
target_audience: ai
complexity: medium
estimated_tokens: 1350
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - migrate-mstest-v1v2-to-v3
related_skills:
  - writing-mstest-tests
  - run-tests
appliesTo: '**/*.{cs,csproj,props,targets,runsettings,md}'
tags:
  - mstest
  - migration
  - v4
---
# MSTest v3 → v4 Migration

Agent upgrades MSTest v3 projects to MSTest v4 by aligning packages, fixing source-breaking APIs, and proving discovery and execution stay intact.

## When to Use

| Condition | Use |
|---|---|
| Agent upgrades MSTest packages from 3.x to 4.x | Use this skill |
| Agent fixes source or discovery issues after an MSTest v4 update | Use this skill |
| Agent adopts `MSTest.Sdk` 4.x or keeps explicit 4.x packages | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Project still uses MSTest v1 or v2 | Run the v1/v2 to v3 migration first |
| User migrates to another test framework | Use a framework-conversion workflow |
| Work is only general test cleanup | Use a testing skill instead |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Test project or solution path | Yes | `.csproj`, `.sln`, or `.slnx` |
| Existing build/test commands | Yes | Preserve repo validation commands |
| Current package model | Yes | `MSTest`, `MSTest.TestFramework`, `MSTest.TestAdapter`, or `MSTest.Sdk` |
| CI/discovery mode | No | `dotnet test`, VSTest, or Microsoft.Testing.Platform |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent records the current 3.x package or SDK model and captures baseline discovery/test counts | Baseline evidence |
| 2 | Agent updates packages or project SDK to 4.x and restores | Updated project metadata |
| 3 | Agent fixes source breaks such as `ExecuteAsync`, `DisplayName`, removed `ExpectedException`, `ThrowsExactly`, `ContainsKey`, timeout constants, and dropped TFMs | Clean build |
| 4 | Agent updates runsettings or CI behavior for discovery warnings, VSTest compatibility, and AppDomain needs when relevant | Compatible test-host configuration |
| 5 | Agent rebuilds, rediscovers tests, and reruns the suite | Verification evidence |

Test: Agent runs the existing build, discovery, and test commands after the package update.
Pass: Agent observes zero build errors. Agent observes zero discovery failures. Agent observes zero new failing tests on MSTest v4.

## Change Matrix

| Area | Agent reviews |
|---|---|
| Package alignment | `MSTest`, `MSTest.TestFramework`, `MSTest.TestAdapter`, `MSTest.Sdk` 4.x |
| Source compatibility | `TestMethodAttribute.ExecuteAsync`, `ExpectedException`, assert API renames, `ContainsKey`, timeout constants |
| Target frameworks | Removal of `net5.0`, `net6.0`, and `net7.0` support |
| Discovery behavior | MTP vs VSTest, discovery warnings, AppDomain differences |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Package verification | Inspect project references | All MSTest packages or SDK are 4.x |
| Compile verification | `dotnet build [entry]` | Zero build errors |
| Discovery verification | `dotnet test [entry] --list-tests` when available | Stable or intentionally changed test count |
| Test verification | `dotnet test [entry]` | Zero new failures |

## Verification Checklist

Agent verifies:
- [ ] Current 3.x package model was recorded before edits
- [ ] Project uses a consistent 4.x package or SDK model
- [ ] Source-breaking APIs were migrated intentionally
- [ ] Discovery path works in local and CI-relevant mode
- [ ] Tests pass after the upgrade

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Updating packages without checking discovery mode | Confirm MTP vs VSTest requirements first |
| Leaving `[ExpectedException]` in place | Replace with `Assert.ThrowsExactly` or async equivalent |
| Ignoring dropped target frameworks | Move to a supported TFM before verification |
| Treating discovery warnings as noise | Fix the warnings or document a temporary override |
