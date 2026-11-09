# Repository Instructions

- Follow `docs/architecture/root-library-consolidation.md` for current project
  ownership. Core runtime code belongs in Ifx; general, Azure, and Windows
  integrations have separate dependency boundaries. Roslyn support, analyzers,
  generators, and code fixes are separate projects; Reporting owns file I/O.
- Nested projects reference their nearest compatible retained parent, except
  Abstractions projects. Parents must never reference children except Abstractions
  projects. Verify changes with `python scripts/Test-ProjectHierarchy.py`.
- Place shared artifacts in their immediate owning parent namespace and folder.
  Do not introduce Abstractions folders or namespace segments in production source.

- Apply Juval Lowy's Volatility-based Decomposition (VBD), modern software practices,
  and concise communication. Document deliberate compatibility exceptions.
- Target the latest stable .NET and its current stable C# features. The verified
  shared baseline is SDK 10.0.401 / C# 14.0. Do not enable preview implicitly;
  honor any newer explicit user decision and keep shared settings and docs consistent.
- Do not use underscore-prefixed identifiers.
- Read `docs/planning/framework-upgrade-parallel-plan.md` first. It is authoritative
  for assignment boundaries, claim rules and completion criteria. Claim only the
  assigned section's Status/Owner/Updated/Notes before implementation; do not edit
  another worker's claimed section without an explicit handoff.
- Preserve concurrent/user changes. Use `apply_patch` for manual edits. Record
  actual commands, coverage and remaining gates; do not mark unproven work Complete.
- Serialize builds/tests through `scripts/Invoke-FrameworkTests.ps1`; use single-node
  builds (`-m:1 -p:BuildInParallel=false`) and coordinate full-suite checks.
