---
title: Multi-source selection local milestone
doc_type: report
status: active
last_updated: 2026-09-10
---

# Multi-source selection local milestone

Gate 1 bounded infrastructure handoff, 2026-09-10. Global acceptance remains with
Orchestrator. No package source or package tests were edited by this workstream.

## Implemented contract

`Invoke-FrameworkTests.ps1 -TestSourceScope 'Folder;Other/Nested;RootTests.cs'`
selects a canonical set of literal directories/root files, plus existing shared
test setup/helpers. Single-directory calls remain valid. Traversal, wildcards,
absolute paths, expression escapes, empty entries, missing paths, wrong casing,
generated output directories, and symlinks/junctions are rejected.

Versioned canonical selectors and the exact selected package reference produce a
SHA-256 fingerprint; 128 bits form the isolated restore/build output key. Different
reference selections cannot reuse the same runner output key. A fresh scoped build
is needed once after this upgrade. Full test/CI configuration remains unchanged.
FullCoverage rejects source scopes, reference scopes and test filters at parameter
binding, including the internal coverage entry point's filter parameter.

Run/build evidence records canonical selectors, key and fingerprint. Reporting
validates that metadata against top-level scope fields and rejects scoped evidence
for full reports. Package source-content fingerprints and report-v1 metric policy
are unchanged. Selection fingerprints do not prove freshness after source edits.

## Verification

- `./tests/infrastructure/coverage/Test-CoverageInfrastructure.ps1`: 48 checks
  passed, retaining the original 15. Exact selected Compile/ProjectReference sets
  and output/restore paths were compared for single, combined, unrestricted-reference,
  nested-folder and aggregator literal-root selections. The restored test SDK's
  own generated entry point is explicitly accounted for when InitialTargets runs.
  Symlink/junction, malformed selector, full-scope guard, hash/key tampering and
  provenance regressions passed. Artifacts:
  `TestResults/coverage-infrastructure/1ad49a43611c435e8ff7c1ac755be15e`.
- `./tests/infrastructure/reporting/Test-ReportingInfrastructure.ps1`: all eight
  existing host checks passed; zero build warnings. Artifacts:
  `TestResults/reporting-host-tests/a99db00925df413cab051d50bb0f44f8`.
- `./scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Filtering.EntityFrameworkCore -TestSourceScope 'Filtering/EntityFrameworkCore;Infrastructure' -TestPackage vc.Ifx.Filtering.EntityFrameworkCore -WarningsAsErrors`:
  five passed, zero failed/skipped, zero build warnings; exact 14/14 lines and 4/4
  branches. Fresh JSON/OpenCover/TRX/context:
  `TestResults/coverage/vc.Ifx.Filtering.EntityFrameworkCore/e22a277ba26d4877b0ee90d3f74b9b69`.
- Earlier SQLite-native-only diagnostic collection passed one test but exercised
  0/14 package lines and 0/4 branches. It correctly retained BelowThreshold status
  with ReportOnly; artifact `2ac597fa6a1d47e5b94a270d9dfad08d` under that package's
  coverage directory. This was collection evidence, not a coverage milestone.

## Owner coordination

Kuhn's five-folder selector is accepted and its exact file/reference selection is
verified: `Proxy;Authentication;Authorization;Caching;Logging`. It cannot compile
with only `-TestPackage vc.Ifx.Proxy`: Logging comes from the aggregator and
Authentication registration/provider implementations come from
`vc.Ifx.Proxy.AspNetCore`. The runner does not silently infer additional references.
Omit TestPackage to retain the centralized reference set while still selecting
only those source folders, and use CoveragePackage to select the measured module.
Coordinate the resulting dependency build with Orchestrator.

The attempted five-folder/Proxy-only build stopped before discovery with CS0234
Logging errors (`TestResults/tests/vc.Ifx.UnitTests/f2a386a79cd243dc9281cb63ec6548db`).
The four-folder attempt without Logging also stopped before discovery because
Authentication needed the ASP.NET Core provider reference
(`03d3b62e7f8749968493e45b15c29800`). These are reference-selection failures, not
test assertion results or absent framework APIs. No package behavior was changed.

Nash can name root legacy files literally, for example
`-TestSourceScope 'Aggregator;ConstantsTests.cs;FrameworkConstantsTests.cs' -TestPackage vc.Ifx`.
This exact source/reference/output selection passed infrastructure verification;
no aggregator test execution or coverage is claimed by this workstream.

The 48 script checks are not measured 100% script coverage. Complete global tests,
all-package coverage, final packs and hosted execution remain separate Orchestrator
gates. No full-suite run, final pack, publishing or repository settings action was
performed here.
