---
name: ca-design-quality-fixes
title: CA Design Quality Fixes
description: Remediate Roslyn CA design/API-surface diagnostics (CA1051, CA1062, CA1303, CA1413) via in-repo code-fix providers plus manual guidance
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: low
estimated_tokens: 1550
prerequisites:
  - .NET SDK with Roslyn analyzers enabled
  - src/ifx/Ifx.CodeFixes wired via Directory.Build.props
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ca-code-quality-fixes
  - ca-perf-reliability-fixes
  - cs8794-fix
appliesTo: '**/*.cs'
tags:
  - roslyn
  - code-analysis
  - ca
  - api-design
---

# CA Design Quality Fixes

Agent uses this bundle for design, API-surface CA diagnostics. When new CA rules in this category are discovered, agent adds them to Rules Coverage matrix. Agent does not create separate skills for individual CA diagnostics in this category.

## Activation

**USE FOR:** CA1051, CA1062, CA1303, CA1413 warnings
**DO NOT USE FOR:** Other CA rules → `ca-code-quality-fixes`, `ca-perf-reliability-fixes`. CS errors with dedicated skill → `cs8794-fix`. Security → `security-controller`.

## Overview

Agent handles:
- Public API shape, argument validation, localization-facing literals, platform/interop usage
- Extensible design-category CA guidance in one bundle
- Future design-category CA diagnostics extend this file (not new standalone skills)

## Critical Rules

1. **API changes require owner review.** CA1051, CA1303 alter public contracts.
2. **Never blanket-suppress.** Every `#pragma warning disable` has inline justification comment.
3. **Build + test after each rule.** `dotnet build` clean. `dotnet test` green.
4. **One commit per rule** for reviewable diffs.

## Rules Coverage

| Rule ID | Severity | Description | Detection Pattern | Fix Strategy | Code Fix Available |
|------|----------|-------------|-------------------|--------------|--------------------|
| CA1051 | Warning | Do not declare visible instance fields | Public or protected instance fields on externally visible types | Convert field to property. When serializer-required, suppress with justification. | Yes |
| CA1062 | Warning | Validate public method arguments for null | Public method dereferences nullable input before guard | Add `ArgumentNullException.ThrowIfNull(param);` near method entry. | No |
| CA1303 | Warning | Do not pass literals as localized parameters | String literal passed where localized resource expected | Move literal to `.resx`. Replace with resource accessor. Suppress only for intentional infrastructure literals. | No |
| CA1413 | Warning | Platform/interop safety guidance for API usage | Platform-specific or interop-sensitive API usage without correct guard/annotation | Add `OperatingSystem.IsWindows()`-style guards OR `[SupportedOSPlatform(...)]` annotations. | No |
| **Future design-category CA rules** | - | Add new rules in this category here | Extend this matrix instead of creating new skill | Document detection + fix path in this bundle | - |

## Workflow

### 1. Inventory

Agent runs:
```powershell
dotnet build <target> /nologo 2>&1 |
  Select-String -Pattern 'warning (CA1051|CA1062|CA1303|CA1413)' |
  Group-Object { ($_ -split 'warning ')[1].Substring(0,6) } |
  Sort-Object Count -Descending
```
Test: Command completes
Pass: List of CA rules with occurrence counts

### 2. Auto-Fix CA1051

Agent runs:
```powershell
dotnet format analyzers <project> --diagnostics CA1051 --severity warn
```
Test: Run command
Pass: CA1051 count drops to zero OR remaining instances are legitimate (serializer contracts) → suppress with justification

### 3. Apply Rule-Specific Remediation

Agent uses Rules Coverage matrix as source of truth. Agent groups fixes by file when safe. Agent verifies per rule.

### 4. Verify

Agent verifies:
- [ ] `dotnet build` — zero new warnings
Test: Run `dotnet build`
Pass: Zero build errors. Zero new warnings.
- [ ] `dotnet test` — green for touched projects
Test: Run `dotnet test [project]`
Pass: All tests pass.
- [ ] Suppressions carry `// CAxxxx: <reason>` comments
- [ ] Public API changes reviewed with owner
- [ ] Commit per rule

## Adding New Rules

When new design-category CA rule is introduced:

Agent performs:
1. Agent verifies rule belongs to API surface, argument validation, localization, or platform/interop guidance
2. Agent adds new row to **Rules Coverage** with rule ID, severity, description, detection pattern, fix strategy, code-fix availability
3. Agent extends **Activation**, **Inventory** commands when new rule is included in default scans
4. Agent adds rule-specific guardrails or validation notes when fix breaks API behavior
5. Agent keeps this bundle as only design-category CA entry point. Agent does not create separate skill.

## Verification Checklist

Agent verifies:
- [ ] Build output shows targeted design-category CA warnings removed or intentionally suppressed
- [ ] Tests pass for touched projects
- [ ] Public API, localization changes reviewed where required
- [ ] Inline suppressions explain why warning remains

## Common Pitfalls

| Rule or Area | Pitfall | Fix |
|---|---|---|
| CA1051 | Convert serializer-bound fields without checking contract requirements | Preserve required field shape. Suppress with justification when conversion is unsafe. |
| CA1062 | Add null guards after first dereference | Place `ThrowIfNull` at top of public entry point |
| CA1303 | Move infrastructure-only messages into resources unnecessarily | Suppress intentional non-localized literals with clear reason |
| CA1413 | Add guards in one path but leave other platform calls unguarded | Audit every call site in affected API path |
| Future rules | Add new micro-skill for one diagnostic | Extend this bundle instead |

## Inputs

| Input | Required | Default |
|-------|----------|---------|
| Project/solution path | Yes | - |
| Rule subset | No | All 4 |

## Outputs

Agent produces:
- Reduced diagnostic count for CA1051/1062/1303/1413
- Justified suppressions where auto-fix is unsafe

## References

- Fix providers: `src/ifx/Ifx.CodeFixes/Providers/CA/Ca{1051,1062,1303,1413}CodeFixProvider.cs`
- Rule docs: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca<id>
