---
title: MSTest Migration Guide
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Version-by-version guidance for MSTest v2 to v3 to v4 modernization under the test-modernization-controller skill.
target_audience: ai
related_docs:
  - ..\SKILL.md
tags:
  - mstest
  - migration
  - reference
---
# MSTest Migration Guide

This guide supports the `modernize` mode in `test-modernization-controller`.

## Version Progression

| From | To | Focus |
|---|---|---|
| v2 | v3 | Package alignment, analyzer uptake, legacy pattern cleanup |
| v3 | v4 | Current API surface, initialization updates, discovery confidence |
| v2 | v4 | Two-stage reasoning with one consolidated validation pass |

## Migration Sequence

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent inventories all MSTest package references across the target scope. | Review package list. | Each test project has a recorded baseline. |
| 2 | Agent records current build and test health before package changes. | Run baseline build and test commands. | Baseline result exists for comparison. |
| 3 | Agent chooses the migration path per project. | Review the plan. | Each project maps to one version path. |
| 4 | Agent updates packages, then source patterns. | Build after each group of edits. | Package restore and compile both succeed. |
| 5 | Agent reruns discovery and tests for the full scope. | Run the repo validation command set. | Zero regression in test discovery or test results. |

## Detecting the Current Version

| Signal | Meaning | Agent action |
|---|---|---|
| `MSTest.TestFramework` and `MSTest.TestAdapter` on `2.x` | Legacy baseline | Plan `v2 → v3` first |
| `MSTest.TestFramework` and `MSTest.TestAdapter` on `3.x` | Intermediate baseline | Plan `v3 → v4` |
| `MSTest.Sdk` or packages on `4.x` | Current baseline | Run verification and residue cleanup |
| Mixed versions across projects | Partial migration state | Normalize the package graph before source cleanup |

## v2 → v3 Focus Areas

| Area | Agent change | Reason |
|---|---|---|
| Package references | Align framework and adapter versions across all MSTest projects. | Mixed versions create confusing discovery and analyzer drift. |
| Analyzer adoption | Enable the expected analyzer surface and capture the first warning inventory. | v3 modernization works best with current warning visibility. |
| Assertion patterns | Replace low-signal and ambiguous assertion shapes with explicit patterns. | Clear assertion form improves later v4 migration. |
| Data-driven patterns | Review `[DataTestMethod]`, `[DataRow]`, and dynamic data usage. | These patterns often surface again during v4 cleanup. |

### Typical v2 → v3 Edits

| Pattern | Before | After |
|---|---|---|
| Legacy package mix | One project uses `2.x`, another uses `3.x` | All scoped projects use one aligned `3.x` set |
| Warning debt ignored | Build output is not filtered for MSTEST warnings | Build output is filtered and tracked before edits |
| Ambiguous assertions | Positional `Assert.AreEqual(actual, expected)` | Named arguments or corrected ordering |

## v3 → v4 Focus Areas

| Area | Agent change | Reason |
|---|---|---|
| Package alignment | Move the full scoped set to `4.x`. | Partial upgrade creates host and API inconsistency. |
| Initialization hooks | Recheck `[ClassInitialize]` and `[ClassCleanup]` signatures. | Older patterns often fail or confuse discovery after upgrade. |
| Assert surface | Adopt the current preferred assertion surface, including `Assert.That` where the active package set uses it. | The suite benefits from the v4 assertion direction. |
| Discovery flow | Reconfirm `dotnet test` and any list-tests command used by the repo. | Version upgrades are incomplete without discovery proof. |

### Typical v3 → v4 Edits

| Pattern | Before | After |
|---|---|---|
| Version drift | Adapter and framework carry different major versions | All MSTest references share one `4.x` intent |
| Old initialization signature | Hook uses older parameter or static shape | Hook matches the current supported signature |
| Obsolete data-test form | `[DataTestMethod]` remains in place | `[TestMethod]` with explicit scenario data remains |

## Breaking-Change Review

| Area | Risk | Agent response |
|---|---|---|
| Package version leap | Build or discovery breaks after upgrade | Upgrade packages in one scoped batch, then build immediately |
| Assertion surface | Existing helpers hide outdated assertion patterns | Update helper wrappers before updating many tests |
| Initialization | Old lifecycle hooks compile but discover poorly | Run list-tests or equivalent discovery validation |
| Data-driven tests | Scenario names degrade during attribute replacement | Keep explicit scenario intent in test names or data rows |

## Migration Patterns

### Pattern 1: Package Alignment First

1. Agent updates package references in the scoped test projects.
2. Agent restores and builds the scope.
3. Agent fixes only the compile errors and analyzer issues caused by the version move.
4. Agent reruns tests before any elective cleanup.

This pattern keeps failure attribution clear.

### Pattern 2: Project-by-Project Upgrade

| Use | Benefit | Tradeoff |
|---|---|---|
| Large repo with separate test projects | Easier isolation and rollback | More repeated validation commands |
| Different product areas with separate owners | Cleaner evidence per team area | Shared helper projects still require coordination |

### Pattern 3: Shared Helper First

Agent upgrades common test helper projects before leaf test projects when helpers expose assertion wrappers, lifecycle helpers, or data-source helpers. This order limits duplicate edits across the suite.

## Validation Workflow

| Check | Command | Pass |
|---|---|---|
| Restore | `dotnet restore <entry>` | Restore succeeds |
| Build | `dotnet build <entry> /nologo` | Zero build errors |
| Discovery | `dotnet test <entry> --list-tests` when the repo uses it | Discovery succeeds with stable or explained count |
| Test run | `dotnet test <entry>` | Zero failing tests |
| Analyzer residue | `dotnet build <entry> /nologo 2>&1 \| Select-String -Pattern 'MSTEST(0017\|0042\|0044\|0052\|0065)'` | Zero targeted warnings or explicitly documented residue |

## Migration Summary Template

```text
MSTest Modernization Summary
- Scope: [solution or project]
- Baseline version state: [v2, v3, v4, or mixed]
- Final version state: [v3 or v4]
- Projects updated: [count]
- Analyzer warnings removed: [count by ID]
- Build result: [pass/fail]
- Test result: [pass/fail]
- Residual items: [list or none]
```

## Common Pitfalls

| Pitfall | Safer move |
|---|---|
| Upgrading packages without a baseline test result | Record build and test evidence first |
| Treating discovery as implied by compile success | Run discovery and test commands explicitly |
| Mixing package alignment with unrelated suite cleanup | Keep unrelated cleanup in a follow-on pass |
| Losing scenario intent during attribute migration | Preserve explicit names in methods or data rows |
