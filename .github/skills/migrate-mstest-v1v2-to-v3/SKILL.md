---
name: migrate-mstest-v1v2-to-v3
description: Upgrade MSTest v1 or v2 projects to MSTest v3 with explicit package changes, source fixes, and verification.
title: MSTest v1/v2 → v3 Migration
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1450
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - migrate-mstest-v3-to-v4
  - writing-mstest-tests
appliesTo: '**/*.{cs,csproj,props,targets,runsettings,md}'
tags:
  - mstest
  - migration
  - v3
---
# MSTest v1/v2 → v3 Migration

Agent upgrades MSTest v1 or v2 projects to MSTest v3 by replacing obsolete references, fixing source breaks, and proving test discovery still works.

## When to Use

| Condition | Use |
|---|---|
| Agent finds `Microsoft.VisualStudio.QualityTools.UnitTestFramework` references | Use this skill |
| Agent finds `MSTest.TestFramework` or `MSTest.TestAdapter` 1.x or 2.x packages | Use this skill |
| Agent fixes build or discovery issues after a v3 package update | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Project already runs cleanly on MSTest v3 | Use v4 migration only if needed |
| User wants MSTest v4 | Use `migrate-mstest-v3-to-v4` |
| User migrates to xUnit or NUnit | Use a framework-conversion workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Test project or solution path | Yes | `.csproj`, `.sln`, or `.slnx` |
| Existing build/test commands | Yes | Preserve repo test entry points |
| Current MSTest version evidence | Yes | Assembly reference or NuGet version |
| Test settings path | No | Needed when `.testsettings` or `.runsettings` exists |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent identifies whether the project uses MSTest v1 assembly references or MSTest v2 NuGet packages and records the exact version | Version baseline |
| 2 | Agent removes QualityTools assembly references when v1 is present and updates packages to MSTest v3 or MSTest.Sdk | Updated project file |
| 3 | Agent updates dropped target frameworks, `.testsettings`, and test SDK settings that block discovery | Compatible test host configuration |
| 4 | Agent fixes source breaks such as generic assert overloads, strict `DataRow` typing, timeout assumptions, and other relevant v3 changes | Clean build |
| 5 | Agent runs tests and compares discovered/passing counts to the baseline | Verification evidence |

Test: Agent runs the existing build and test commands for the migrated test project.
Pass: Agent observes zero build errors. Agent observes zero discovery failures. Agent sees test counts that match the baseline except for intentional removals.

## Change Matrix

| Area | Agent reviews |
|---|---|
| References | QualityTools assembly removal, MSTest metapackage or MSTest.Sdk adoption |
| TFM support | .NET 6+, .NET Framework 4.6.2+, supported UWP/WinUI levels |
| Source compatibility | `Assert.AreEqual<T>`, `AreNotEqual<T>`, `AreSame<T>`, strict `DataRow` types |
| Configuration | `.testsettings` removal, `.runsettings` migration, test-host compatibility |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Version verification | Inspect project references | Exact pre-migration version identified |
| Compile verification | `dotnet build [entry]` | Zero build errors |
| Discovery verification | `dotnet test [entry] --list-tests` when available | Tests are discovered |
| Test verification | `dotnet test [entry]` | Zero new failures and stable count |

## Verification Checklist

Agent verifies:
- [ ] Current MSTest version was identified before edits
- [ ] v1 assembly references were removed when present
- [ ] MSTest v3 packages or MSTest.Sdk are configured consistently
- [ ] `.testsettings` was replaced when legacy settings existed
- [ ] Tests build, discover, and run after migration

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Upgrading packages without identifying v1 vs v2 | Record the starting model first |
| Leaving legacy settings in place | Replace `.testsettings` with `.runsettings` |
| Forgetting `Microsoft.NET.Test.Sdk` or VSTest compatibility needs | Add the required host configuration intentionally |
| Ignoring discovery-count regressions | Compare list-test or run counts before and after |
