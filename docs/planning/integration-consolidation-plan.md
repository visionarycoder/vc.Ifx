---
title: Integration project consolidation
doc_type: plan
status: in_progress
last_updated: 2026-11-23
---

# Integration project consolidation

## Objective

Consolidate `Ifx.Integrations`, `Ifx.Integrations.Azure`, and
`Ifx.Integrations.Windows` into the existing `Ifx` project and package.
Keep provider source groups organized in their current source folders so they
can be separated into reusable slices in a later change.

## Implementation

- Compile general and Azure integration sources into the `Ifx` assembly.
- Multi-target `Ifx` for `net10.0` and `net10.0-windows`; include MSMQ sources
  and its package dependency only in the Windows target.
- Keep public namespaces and provider APIs unchanged.
- Retarget tests and integration benchmarks to the `Ifx` project.
- Remove the three integration projects from the solution and retire their
  package outputs. Keep Roslyn projects separate.
- Update package validation, release inventory, hierarchy checks, README files,
  architecture ownership docs, and package migration notes.
- Bump the package version to `0.4.0` for the pre-1.0 package layout change.

## Tradeoffs

The consolidated `Ifx` package carries the general and Azure integration
dependencies. Windows messaging is conditional on the Windows target. Consumers
that previously used a small provider package will receive the larger `Ifx`
dependency graph. Later reusable slices can restore smaller package boundaries
once the slice ownership and package contracts are defined.

## Status

Owner: Orchestrator; Updated: 2026-11-23; Notes: Implementation is in progress on the integration consolidation branch.

- [x] Scope confirmed with repository owner on 2026-11-23.
- [ ] Project and source compilation changes.
- [ ] Test and benchmark project reference updates.
- [ ] Package, solution, and architecture documentation updates.
- [ ] Remote quality and release validation.
