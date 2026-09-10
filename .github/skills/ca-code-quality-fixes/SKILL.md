---
name: ca-code-quality-fixes
title: CA Code Quality Fixes
description: Remediate Roslyn CA code-quality diagnostics (CA1704, CA1707, CA1801, CA1806, CA1812, CA1819, CA1822, CA1823) via in-repo code fix providers plus manual guidance
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: low
estimated_tokens: 1850
prerequisites:
  - .NET SDK with Roslyn analyzers enabled
  - src/ifx/Ifx.CodeFixes wired via Directory.Build.props
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ca-design-quality-fixes
  - ca-perf-reliability-fixes
  - cs8794-fix
appliesTo: '**/*.cs'
tags:
  - roslyn
  - code-analysis
  - ca
  - maintainability
---

# CA Code Quality Fixes

Agent uses this bundle for code-quality, maintainability CA diagnostics. When new CA rules in this category are discovered, agent adds them to Rules Coverage matrix. Agent does not create separate skills for individual CA diagnostics in this category.

## Activation

**USE FOR:** CA1704, CA1707, CA1801, CA1806, CA1812, CA1819, CA1822, CA1823 warnings
**DO NOT USE FOR:** API-surface/interop rules → `ca-design-quality-fixes`. Performance rules → `ca-perf-reliability-fixes`. CS errors with dedicated skill → `cs8794-fix`.

## Overview

Agent handles:
- Naming hygiene, unused members, discarded results, static correctness, collection/API maintainability
- Extensible code-quality CA guidance in one bundle
- Future maintainability-focused CA rules extend this bundle (not new micro-skills)

## Critical Rules

1. **Renames of public identifiers (CA1704, CA1707) are breaking.** Add `[Obsolete]` shim for existing callers.
2. **CA1819 refactor changes public API.** Verify with owner before switching `T[]` return to `IReadOnlyList<T>`.
3. **CA1812/CA1823 removals are semantic.** Verify zero reflection/serializer/DI activation before deleting.
4. **Never blanket-suppress.** Every `#pragma warning disable` has inline justification comment.
5. **Build + test after each rule. One commit per rule.**

## Rules Coverage

| Rule ID | Severity | Description | Detection Pattern | Fix Strategy | Code Fix Available |
|------|----------|-------------|-------------------|--------------|--------------------|
| CA1704 | Warning | Spell public identifiers correctly | Externally visible names contain misspellings | Rename. Add compatibility shim for public APIs when needed. | No |
| CA1707 | Warning | Remove underscores from externally visible identifiers | Public symbol names contain underscores | Rename to public-compatible .NET naming. Add compatibility shim when required. | No |
| CA1801 | Warning | Review unused parameters | Parameter unused in method body | Remove parameter when possible. Otherwise use discard only if signature cannot change. | Yes |
| CA1806 | Warning | Do not ignore method results | Return value discarded or ignored | Capture and use value OR explicitly discard only when side effects are intended. | Yes |
| CA1812 | Warning | Avoid uninstantiated internal classes | Internal type appears unused to analyzer | Delete dead type OR annotate/justify reflection or DI activation. | No |
| CA1819 | Warning | Do not expose arrays from properties | Public property returns array directly | Replace with `IReadOnlyList<T>`, immutable collection, or read-only wrapper. | No |
| CA1822 | Warning | Mark members static when they do not access instance state | Instance member uses zero instance data | Add `static` unless interface/override requirements prevent it. | Yes |
| CA1823 | Warning | Remove unused private fields | Private field never read | Delete field after verifying reflection/serializer activation. | Yes |
| **Future code-quality CA rules** | - | Add new maintainability rules here | Extend this matrix instead of creating new skill | Document detection + fix path in this bundle | - |

## Workflow

### 1. Inventory

Agent runs:
```powershell
$rules = 'CA1704|CA1707|CA1801|CA1806|CA1812|CA1819|CA1822|CA1823'
dotnet build <target> /nologo 2>&1 |
  Select-String -Pattern "warning ($rules)" |
  Group-Object { ($_ -split 'warning ')[1].Substring(0,6) } |
  Sort-Object Count -Descending
```
Test: Command completes
Pass: List of CA rules with occurrence counts

### 2. Apply Auto-Fixes

Agent runs:
```powershell
dotnet format analyzers <project> --diagnostics CA1801,CA1806,CA1822,CA1823 --severity warn
```
Test: Run command
Pass: Auto-fixable rule counts drop to zero

### 3. Manual Remediation

Agent performs in order:
1. **CA1823** — Agent deletes unused private fields (verify reflection first)
2. **CA1822** — Agent applies `static` where analyzer cannot (e.g., overrides — skip)
3. **CA1812** — Agent verifies activation path. Agent deletes or annotates.
4. **CA1801/1806** — Agent replaces analyzer discards with real usage where sensible
5. **CA1819** — Agent refactors property to `IReadOnlyList<T>`/`ImmutableArray<T>` per audit with owner
6. **CA1704/1707** — Agent renames + adds shim (public API only)

### 4. Verify

Agent verifies:
- [ ] `dotnet build` clean, zero new warnings
Test: Run `dotnet build`
Pass: Zero build errors. Zero new warnings.
- [ ] `dotnet test` green for touched projects
Test: Run `dotnet test [project]`
Pass: All tests pass.
- [ ] Suppressions carry `// CAxxxx: <reason>` comments
- [ ] Public renames have `[Obsolete]` shims
- [ ] Commit per rule

## Adding New Rules

When new code-quality CA diagnostic appears:

Agent performs:
1. Agent verifies rule belongs to naming, dead code, static correctness, result usage, or maintainability
2. Agent adds row to **Rules Coverage** with severity, detection pattern, fix strategy, code-fix availability
3. Agent updates `Inventory` regex, `dotnet format analyzers` examples when new rule is auto-fixable
4. Agent adds breaking-change notes to **Critical Rules** or **Common Pitfalls**
5. Agent keeps this bundle as sole code-quality CA entry point. Agent does not add separate diagnostic skill.

## Verification Checklist

Agent verifies:
- [ ] Targeted code-quality CA warnings removed or intentionally suppressed with justification
- [ ] Public renames keep compatibility requirements explicit
- [ ] Reflection, serializer, DI activation paths work after removals
- [ ] Touched projects build, tests pass

## Common Pitfalls

| Rule or Area | Pitfall | Fix |
|---|---|---|
| CA1704 / CA1707 | Rename public APIs without migration plan | Use shims or coordinated API updates when compatibility matters |
| CA1801 | Add `_ = param;` everywhere | Remove parameter when signature changes safely |
| CA1806 | Discard results that carry behavior | Review API contract before using `_ =` |
| CA1812 / CA1823 | Delete members activated through reflection or DI | Verify runtime activation before removal |
| CA1819 | Replace arrays without considering callers | Use owner review for public API changes |
| Future rules | Spin up another micro-skill | Extend this bundle instead |

## Inputs

| Input | Required | Default |
|-------|----------|---------|
| Project/solution path | Yes | - |
| Rule subset | No | All 8 |

## Outputs

Agent produces:
- Reduced diagnostic count for 8 rules above
- Cleaner naming, fewer dead members, correct static usage
- Justified suppressions where auto-fix is unsafe

## References

- Fix providers: `src/ifx/Ifx.CodeFixes/Providers/CA/Ca{1704,1707,1801,1806,1812,1819,1822,1823}CodeFixProvider.cs`
- Rule docs: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca<id>
