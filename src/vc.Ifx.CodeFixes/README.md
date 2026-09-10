# vc.Ifx.CodeFixes

Code fixes for vc.Ifx Roslyn analyzers.

## Diagnostic Debt Fixes

`DiagnosticDebtCodeFixProvider` supports:

- `IFX1000`: appends a `Tracked-by:` disposition marker to a comment that references a `CA####` or `CS####` diagnostic.
- `IFX1001`: appends a `Justification:` marker to a `#pragma warning disable` suppression.

The generated text uses `TODO` deliberately. A generic code fix cannot know whether a diagnostic should be fixed, tracked, or intentionally suppressed, so it creates a clear review point instead of silently hiding the issue.

## Fix-All

The provider uses Roslyn's batch fixer so teams can apply the same documentation pattern across a file, project, or solution.
