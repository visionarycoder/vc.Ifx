---
name: test-modernization-controller
title: Test Modernization Controller
description: >
  Manages test modernization with modes for scan, fix, modernize, and verify. Executes MSTest analyzer remediation, version migration (v3→v4), and test quality analysis.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 2200
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - run-tests
  - test-fixes
  - test-anti-patterns
  - assertion-quality
appliesTo: '**/*.{cs,csproj}'
tags:
  - mstest
  - testing
  - controller
  - modernization
  - analyzers
---
# Test Modernization Controller

Agent uses this controller for MSTest warning remediation, version migration, and test-quality verification.

## When to Use

| User prompt | Use |
|---|---|
| User asks to scan for MSTest warnings | Use this skill |
| User asks to fix MSTest analyzer warnings | Use this skill |
| User asks to upgrade MSTest packages or patterns | Use this skill |
| User asks to verify test quality after modernization | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to write brand-new tests with no modernization scope | Use `test-fixes` or a test-authoring skill |
| User asks for framework-agnostic smell taxonomy only | Use `test-anti-patterns` |
| User asks for assertion analysis only | Use `assertion-quality` |
| User asks for test execution only | Use `run-tests` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Scope path | Yes | Test project, solution, solution filter, or focused directory. |
| Existing validation command | Yes | Repository build and test path already in use. |
| Requested outcome | Yes | `scan`, `fix`, `modernize`, or `verify`. |
| Current MSTest package state | No | Agent infers versions from `.csproj` files when needed. |

## Mode Selection Matrix

| User Intent | Mode | What Controller Does |
|-------------|------|---------------------|
| "Scan for test warnings" | `scan` | Detects `MSTEST0017`, `MSTEST0042`, `MSTEST0044`, `MSTEST0052`, and `MSTEST0065`. |
| "Fix test warnings" | `fix` | Applies analyzer-guided fixes in priority order. |
| "Upgrade to MSTest v4" | `modernize` | Migrates `v2 → v3 → v4`, aligns packages, and updates source patterns. |
| "Verify test quality" | `verify` | Runs tests, checks assertion quality, and detects test smells. |

## Workflow

| Step | Agent Action | Test | Pass |
|------|--------------|------|------|
| 1. Detect mode | Agent maps user intent to `scan`, `fix`, `modernize`, or `verify`. | Review the mapped mode. | Mode matches the request. |
| 2. Execute mode | Agent runs the selected workflow. | Review mode output. | Mode evidence exists. |
| 3. Run verification | Agent runs build and test checks for touched scope. | Run the repo validation command. | Zero failing tests. |
| 4. Generate report | Agent writes the mode summary. | Review the report. | Required sections exist. |

## Rules Coverage

| Rule | Defect | Agent Writes | Test | Pass |
|---|---|---|---|---|
| `MSTEST0017` | Outdated MSTest usage | Agent applies the approved MSTest pattern at the smallest scope. | Run `dotnet build <target> /nologo`. | Zero `MSTEST0017` warnings. |
| `MSTEST0042` | Non-compliant modern usage | Agent refactors to the current expected pattern. | Run `dotnet build <target> /nologo`. | Zero `MSTEST0042` warnings. |
| `MSTEST0044` | Non-compliant test structure | Agent replaces the flagged construct with the compliant form. | Run `dotnet build <target> /nologo`. | Zero `MSTEST0044` warnings. |
| `MSTEST0052` | Static `TestContext` storage | Agent keeps `TestContext` instance-scoped and passes it to helpers. | Run `rg "static\\s+TestContext" Tests AzureAPI src`. | Zero matches. |
| `MSTEST0065` | Obsolete `[DataTestMethod]` usage | Agent replaces `[DataTestMethod]` with `[TestMethod]` and explicit scenarios. | Run `rg "\\[DataTestMethod\\]" Tests AzureAPI src`. | Zero matches. |

## Scan Mode

### Scan Execution

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent runs the analyzer build filter. | Run `dotnet build <solution> /nologo 2>&1 \| Select-String -Pattern 'MSTEST(0017\|0042\|0044\|0052\|0065)'`. | Violations list exists. |
| 2 | Agent categorizes warnings by analyzer and file. | Review the grouped list. | Every warning maps to one bucket. |
| 3 | Agent ranks work by priority. | Review the priority table. | High-priority warnings lead. |
| 4 | Agent generates the scan report. | Review the report body. | Counts, top files, and analyzer IDs appear. |

### Scan Priority Table

| Priority | Rules | Reason |
|---|---|---|
| High | `MSTEST0052`, `MSTEST0065` | Static state and obsolete attribute usage. |
| Medium | `MSTEST0017`, `MSTEST0042`, `MSTEST0044` | Local source fixes. |

### Scan Report Template

```text
MSTest Analyzer Violations:
- MSTEST0017: X warnings in Y files
- MSTEST0042: X warnings in Y files
- MSTEST0044: X warnings in Y files
- MSTEST0052: X warnings in Y files
- MSTEST0065: X warnings in Y files

Top 5 Files by Warning Count:
[file path] - [warning count] - [analyzer IDs]
```

## Fix Mode

### Fix Workflow

| Step | Agent Action | Test | Pass |
|------|--------------|------|------|
| 1. Baseline warnings | Agent runs a build and captures target warnings. | Scan completes. | Warning inventory captured. |
| 2. Group by project | Agent groups warnings by project and shared helpers. | Review grouping. | Every warning maps to one project. |
| 3. Write smallest fixes | Agent applies minimal fixes per analyzer. | Build after fixes. | Target analyzer warnings drop to zero. |
| 4. Run tests | Agent runs tests for touched projects. | Tests execute. | Zero failing tests. |

### Rule-Specific Write Matrix

| Rule | Agent Writes | Guardrail |
|---|---|---|
| `MSTEST0017` | Agent uses the analyzer-approved MSTest pattern. | Agent does not suppress the warning. |
| `MSTEST0042` | Agent writes the current MSTest pattern expected by the analyzer. | Agent keeps expected and actual ordering readable. |
| `MSTEST0044` | Agent writes the compliant attribute or structure shape. | Agent keeps tests deterministic. |
| `MSTEST0052` | Agent writes `public TestContext TestContext { get; set; } = null!;` and passes `TestContext` to helpers. | Agent does not write replacement global mutable state. |
| `MSTEST0065` | Agent writes `[TestMethod]` tests with explicit scenario names. | Agent keeps scenario coverage equal or larger. |

## Modernize Mode

### Modernize Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent detects versions from `<PackageReference Include="MSTest.*" Version="..." />`. | Review the package inventory. | Each project has a recorded version. |
| 2 | Agent generates the version path plan. | Review the migration plan. | Plan states `v2 → v3`, `v3 → v4`, or existing `v4`. |
| 3 | Agent updates package references and source patterns. | Run restore and build for scope. | Packages align and source compiles. |
| 4 | Agent modernizes source usage. | Review touched tests and helpers. | Assert patterns, init signatures, and test context flow align. |
| 5 | Agent validates the full suite. | Run `dotnet test <solution-or-project>`. | Zero test failures. |
| 6 | Agent writes the migration summary. | Review summary content. | Summary lists version changes and verification outcome. |

### Modernize Change Matrix

| Version Path | Agent Action | Verification |
|---|---|---|
| `v2 → v3` | Agent updates packages, removes legacy patterns, and aligns Assert usage. | Build and targeted tests pass. |
| `v3 → v4` | Agent adopts current init patterns, updates attributes, and aligns source. | Build, discovery, and tests pass. |
| `v2 → v4` | Agent stages the work as `v2 → v3 → v4` even when edits land in one PR. | Summary documents both transitions. |

### Modernize Source Focus

| Area | Agent change |
|---|---|
| Package references | Agent updates MSTest package references to the intended version set. |
| Assert patterns | Agent migrates `Assert.AreEqual` to `Assert.That` where the version path requires it. |
| Initialization hooks | Agent updates `[ClassInitialize]` and `[ClassCleanup]` signatures. |
| Test context flow | Agent removes static storage and keeps `TestContext` instance-scoped. |
| Data-driven tests | Agent replaces obsolete forms and keeps scenario intent explicit. |

## Verify Mode

### Verify Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent runs all tests in scope. | Run `dotnet test <solution-or-project>`. | Zero failing tests. |
| 2 | Agent checks assertion quality. | Read `assertion-quality` guidance and inspect touched tests. | Assertion strengths and gaps are reported. |
| 3 | Agent detects test anti-patterns. | Read `test-anti-patterns` guidance and inspect touched tests. | High-confidence smells and fixes are reported. |
| 4 | Agent generates the quality report. | Review report content. | Report includes execution, assertions, smells, and modernization notes. |

### Verify Report Sections

| Section | Content |
|---|---|
| Test execution | Suite, project, or target command with pass/fail result |
| Assertion quality | Strong patterns, weak patterns, and missing assertion categories |
| Test smells | High-confidence anti-pattern findings with locations |
| Modernization health | Remaining MSTest warnings, migration residue, and next fix candidates |

## Preferred Pattern

```csharp
public sealed class PayrollTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void CalculateTotals_WithWeekendShift_ReturnsExpectedAmount()
    {
        var result = CalculateTotalsForShift(dayOfWeek: DayOfWeek.Saturday);
        Assert.AreEqual(expected: 125m, actual: result.TotalPay);
    }

    private PayrollResult CalculateTotalsForShift(DayOfWeek dayOfWeek)
    {
        return new PayrollResult(totalPay: dayOfWeek == DayOfWeek.Saturday ? 125m : 100m);
    }
}
```

## References

| File | Use |
|---|---|
| `references\mstest-migration-guide.md` | Version progression, breaking changes, and staged migration patterns |
| `references\analyzer-remediation-patterns.md` | Before/after analyzer fixes, pitfalls, and test strategy |

## Verification Checklist

- [ ] Agent selected the correct mode for the user intent.
- [ ] Agent covered all five MSTest analyzer families in scan or fix output.
- [ ] Agent preserved analyzer-driven fixes instead of warning suppression.
- [ ] Agent ran build and test validation for the touched scope.
- [ ] Agent included assertion-quality and anti-pattern analysis in `verify` mode.
- [ ] Agent wrote a mode-specific summary with counts, changes, and residual risk.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Treating `modernize` as package-only work | Agent updates source patterns and verification evidence in the same workflow. |
| Mixing broad refactors into analyzer remediation | Agent keeps `fix` mode minimal and warning-scoped. |
| Upgrading to `v4` without a staged version inventory | Agent records the incoming package state before edits. |
| Replacing one global test helper pattern with another | Agent keeps `TestContext` instance-scoped and passes data explicitly. |
| Stopping after build success | Agent runs tests and quality checks before closing the workflow. |
