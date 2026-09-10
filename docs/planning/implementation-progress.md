# Implementation Progress

Snapshot: 2026-09-10. The [parallel plan](framework-upgrade-parallel-plan.md)
remains authoritative for ownership, dependencies, status, and exact evidence.
Local implementation acceptance is complete: all 28 libraries, 31 of 32 plan
workstreams. Gate 4's external acceptance remains open; this is not a publication
or IDE certification. [Exact evidence and remaining checks](local-verification-20260910.md).

## Current Acceptance Run

- [x] Resumed full Release checkpoint: 3,617 unit and 15 integration tests,
  zero skips, all 28 libraries at exact 100% line/branch coverage.
  Build `global-20260910-02`; coverage `b0c68673ee8e4c84b4efa7d74ef0eff7`.
- [x] Reproduced and fixed missing embedded-resource/protobuf build-input tracking.
  All 29 real build-identity regressions and seven isolated package-wrapper
  regressions pass. Evidence is recorded in `../testing/build-provenance.md`.
- [x] Fresh final candidate `global-20260910-03` builds with zero warnings/errors;
  its identity includes the actual query schema and gRPC protobuf definition.
- [x] Complete paired full coverage, durable reporting and all 28 archive pairs.
- [x] Correct package-verifier filename collisions for localized host resources;
  31 artifact and 12 wrapper checks pass, including modified private payload rejection.
- [x] Correct compiler-package reference resolution to honor no-build packaging;
  reproduced NETSDK1085, added the required reference-build flag and reran all
  12 wrapper checks successfully. Actual all-package verification passed.

The independent acceptance audit and final gates establish local acceptance of all
library implementations. Hosted CI, artifact transfer/feed
permissions, documentation Solution Explorer behavior and compiler IDE interaction
remain explicitly separate unverified checks. No publication is claimed.

## Combined Verification

- [x] Fresh complete Release solution build: zero warnings and errors, including
  libraries, both test projects, four benchmark projects and reporting host.
- [x] Unfiltered Release tests: 3,617 unit plus 15 integration, all passing,
  with zero skips.
- [x] Strict combined coverage: all 28 libraries at 100% lines and branches.
- [x] Generate and validate the paired durable report: passed, zero issues.
- [x] Refresh all 54 benchmark execution cases on the integrated runtime source.
- [x] Refresh and validate all 28 package/symbol pairs.
- [x] Accept local package-host, CI, documentation-project and plan reviews;
  actual hosted/IDE checks remain separate external acceptance.

Build evidence: `TestResults/reporting/global-20260910-03`.
Coverage and test evidence:
`TestResults/coverage/full/d27e86fffbba406da1c533510c9948fb`.
Final package evidence:
`TestResults/package-artifacts/59468a485ff34d26884375e111912ac6/validated`.
All 56 version 1.0.0 archives pass, with 19 archive-validator checks and the exact
58-file staged inventory (archives, SDK pin and manifest) verified.
The full coverage run used the fresh complete build without rebuilding, no
filters or source selections, and no historical coverage merge. The earlier
Tables zero-coverage discrepancy did not recur; its cause is not claimed proven.

## Verified Package Milestones

Each row passed targeted tests and measured 100% line and branch coverage.
Zero branch points means there were no branches to measure, not skipped coverage.

| Package (vc.Ifx prefix) | Passing tests | Covered lines | Covered branches |
| --- | ---: | ---: | ---: |
| Abstractions | 28 | 95 | 18 |
| Primitives | 376 | 403 | 196 |
| Filtering | 86 | 357 | 354 |
| Filtering.EntityFrameworkCore | 4 | 14 | 4 |
| Querying | 110 | 323 | 156 |
| Storage.Abstractions | 67 | 68 | 14 |
| Storage.Local | 48 | 211 | 40 |
| Storage.Ftp | 39 | 278 | 124 |
| Storage.Azure.Blobs | 32 | 262 | 114 |
| Secrets.Abstractions | 26 | 19 | 2 |
| Secrets.Local | 54 | 39 | 10 |
| Secrets.Azure.KeyVault | 75 | 172 | 78 |
| Roslyn | 42 | 22 | 0 |
| Generators.Abstractions | 31 | 43 | 0 |
| Generators | 134 | 1,437 | 516 |
| WebApi (Release) | 109 | 280 | 116 |
| Pipeline | 33 | 314 | 92 |
| Pipeline.Grpc | 12 | 53 | 18 |
| Data.Azure.Tables | 25 | 281 | 98 |
| Messaging.Azure.Queues | 17 | 248 | 72 |
| Observability | 19 | 70 | 16 |
| Analyzers | 506 | 2,024 | 1,090 |
| Roslyn.Reporting | 9 | 229 | 244 |
| Proxy.Http | 24 | 153 | 110 |
| Proxy.AspNetCore | 25 | 561 | 190 |
| CodeFixes | 96 | 194 | 176 |
| Proxy | 463 | 2,294 | 692 |
| Aggregator (vc.Ifx, Release) | 1,095 | 1,137 | 803 |

## Handoff State

| Owner | Current work |
| --- | --- |
| Orchestrator | Local acceptance complete; external hosted/IDE checklist retained |
| All implementation/review agents | Handed off and closed; no active edits or commands |

Proxy core passed 463 scoped tests, with 2,294/2,294 lines and 692/692 branches
covered and no build warnings. Evidence: `9dd7e51162384aa586c68073ef9dd05d`.
The legacy authentication assertion correction separately passed eight Release
tests. Core implementation and combined verification are now accepted as Complete
under the plan's local project standard.

28 of 28 libraries have verified local 100% line/branch milestones, now also
confirmed together in the fresh Release run above. Subsequent source changes
require re-verification. Reporting added the 28th library; final package
validation must include it.

## Shared Milestones

- Historical first package milestone: 27 pairs and 18 validator checks. Superseded
  by the final 28 pairs and 19 validator checks above, not reused for acceptance.
- Test infrastructure now passed 48 checks, including exact coverage thresholds,
  missing-test rejection, multi-folder/root-file selection, reference-sensitive
  isolated outputs, and serialized execution. Reporting host passed eight checks.
- Documentation is now a real SDK-backed solution project with recursive relative
  files. Fourteen checks pass for automatic item discovery and output-free CLI
  restore/build/clean/pack/rebuild. Actual Visual Studio load/refresh remains
  unverified because a development Visual Studio installation is unavailable.
- CI workflow passed local structural, actionlint, and PowerShell checks; hosted
  execution and repository required-check configuration remain unverified.
- Filtering and Querying include real SQLite translation checks.
- All library mission READMEs exist; implementation owners refine their contracts.
- Four benchmark projects now contain 54 cases; all passed Dry execution and one
  Short job passed. These validate execution, not a reliable performance baseline.
  Fresh integrated Dry evidence: `TestResults/benchmarks/dcba41ef8dca40d3b7cd9f484c8d472c`;
  Core 10, Filtering/Querying 20, Proxy/Pipeline/HTTP 16, Storage 8.
- Current metadata validation passed for 28 libraries, six non-packable test and
  benchmark projects, and the separately checked non-packable reporting host.
- Dependency and plan reconciliation now includes the non-packable documentation
  project: 36 solution projects, 28 libraries, and 25 passing boundary probes.
  Accepted scope decisions and checklist corrections are recorded in
  `docs/planning/implementation-acceptance-audit.md`.

## Remaining Release Gates

- Close review findings and reconcile implementation checklists with evidence.
- Preserve the verified separate-install compiler package contract during final
  packaging: Analyzers emits diagnostics; CodeFixes supplies actions without
  bundling another analyzer copy.
- Correct expected-failure self-test exit handling, bind NoBuild verification to
  actual build identities, and restrict publication to validated archive identities.
- Preserve the passing complete suite, zero-warning build and 100% combined
  coverage when resolving any remaining review findings.
- Refresh and validate all final packages and package-host behavior.
- Review CI, reporting, and benchmark artifacts against the same source state.

No workstream is marked Complete solely because its local milestone passed.

Compiler-host packaging remediation passed 48 host checks, 19 archive-validator
checks, six compiler probes and both fresh-cache NuGet consumers. Combined
Analyzers/CodeFixes installation now emits IFX1000 exactly once; CodeFixes-only
installation exposes fixes without IFX analyzers. Green evidence:
`TestResults/compiler-host-packages/b096e65d29104d539734b3e9247c8502`.
These five scoped archives are not the final 28-package release snapshot.

## Integration Checkpoint

Latest complete combined Release run: 3,606/3,606 unit tests and 15/15 integration
tests passed, zero skips, with warnings treated as errors. Evidence:
`TestResults/coverage/full/04266acfb7c44db3b074f7eb74b28e38`.
The strict global coverage gate correctly failed: 27 packages measured 100%, but
Azure Tables reported zero despite its 25 tests passing. A fresh scoped Release
rerun passed 25/25 and measured 218/218 lines, 98/98 branches
(`974e5010c0fe444dbf747e91df511db1`). Investigation and clean-snapshot combined
verification remain pending; targeted evidence is not substituted into the
failed combined result.

Prior complete Release unit run: 3,604/3,605 passed, one failed, zero skipped.
Evidence:
`TestResults/tests/vc.Ifx.UnitTests/552d822c04bc4a28aa6b4fef9b124ffa/tests.trx`.
The remaining failure was an outdated authentication validation message assertion;
the corrected type/parameter contract passed a separate eight-test Release run
(`41dc673d374949c0bfe24b837eb3a566`) and the subsequent full run above.

Independent WebApi review found inaccurate hosting guarantees: built-in ASP.NET
exception middleware can short-circuit canceled requests as 499 and clears
pre-exception response headers. Eleven real hosting-path regressions and corrected
documentation now pass, including full-source compilation and the latest combined
run. Independent review accepted the corrections; the earlier direct-handler
coverage alone did not prove these hosting guarantees. See the resolution entries
in `docs/architecture/verified-package-review.md`.

### Earlier Checkpoints

Aggregator now passes 1,095 selected tests in both Debug and Release, without
skips or warnings. Strict whole-module coverage: Release 1,137/1,137 lines and
803/803 branches (`68d0069ce1df4c438fd3c10454642b48`); Debug 1,556/1,556 lines and
813/813 branches (`c4b337fa6436451c991bfde30870efac`). Different counts reflect
compiler configuration, not source exclusions. This supersedes the partial
aggregator measurements below, but is not an unfiltered full-suite result.

The first completed fresh full-unit run executed 3,413 tests: 3,412 passed,
one failed, zero skipped. It no longer stalls in retries. Artifact:
`TestResults/tests/vc.Ifx.UnitTests/47a993373052498286844a5ff586f458/tests.trx`.
The remaining timestamp assertion incorrectly used the test assembly instead of
the framework assembly; corrected and awaiting the next complete run.

A later diagnostic run passed 3,420/3,420 with only the actively implemented
CodeFixes tests filtered out. Aggregator coverage remains partial: 969/1,535 lines,
292/831 branches. This is not a full-suite or 100% coverage result. The unfiltered
interim run picked up 23 failing new code-fix tests, routed to their owner.

Complete Release build checkpoint: all four benchmark projects and reporting host
built; the solution failed on two in-flight compile errors, routed to CodeFixes
and Proxy.AspNetCore owners. Command: `Invoke-FrameworkTests.ps1 -BuildOnly -Project
vc.Ifx.slnx -Configuration Release -WarningsAsErrors`. No successful whole-solution
build is claimed by this checkpoint.

The centralized integration project now passes 15/15 real tests with no filters,
skips or build warnings: Proxy plus HTTP retry/cancellation/ownership, and real
SQLite Filtering plus Querying translation/composition. Evidence:
`TestResults/tests/vc.Ifx.IntegrationTests/6c57f007ce8b45949b8dadafe7146a9f/tests.trx`.
These replaced the placeholder test; they are not a coverage measurement.

Contract audit passed 35-project/28-library inventory and 16 validator probes.
It identified disconnected legacy authorization registration; Proxy's canonical
compatibility bridge has passed its first four real DI/pipeline tests, with final
core verification pending. Public namespaces and current Azure dependency
retention remain documented compatibility decisions.

Independent review found TimeOnly precision loss despite the original 100% result.
The fix has red/green regression evidence and fresh strict coverage: Filtering
86 tests, 357/357 lines and 354/354 branches; Querying 110 tests and EF 4 tests
also retain 100% after the dependency change. Coverage alone is not treated as
proof of semantic correctness.

Aggregator extension regressions: 416/416 pass with warnings-as-errors, including
the previously skipped batching test. Five new contract tests also pass, including
independent named/default local storage roots and branch-specific dictionary factories.
