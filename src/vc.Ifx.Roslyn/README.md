# vc.Ifx.Roslyn

Shared support library for vc.Ifx Roslyn analyzers, code fixes, and generators.

This project contains dependency-light constants and helpers that must be consistent across Roslyn packages. Runtime framework code should not depend on this package.

## Contents

- `DiagnosticIdentifiers` defines analyzer IDs emitted by vc.Ifx Roslyn components.
- `DiagnosticIdPattern` recognizes C# compiler diagnostics (`CS####`) and .NET code-analysis diagnostics (`CA####`).

## Design Rules

- Keep this project small and stable.
- Do not place framework runtime abstractions here.
- Do not add workspace APIs unless both analyzers and code fixes need them.
