# CI Quality And Publishing

The authoritative GitHub entry point is
[`publish.yml`](../../.github/workflows/publish.yml). Its executable quality steps
are the local composite action
[`framework-quality/action.yml`](framework-quality/action.yml). GitHub discovers
workflows only under `.github/workflows`; the local action lets `.infra/yaml` own
the build/test/coverage/pack definition without a second pipeline implementation.

## Events And Permissions

| Event | Quality gate | Publishing |
| --- | --- | --- |
| Pull request, including forks | All changes | Never |
| Merge queue | Same complete quality job | Never |
| Push to main | Same complete quality job | GitHub Packages after success |
| Push of stable vX.Y.Z tag | Same complete quality job; versions must match tag | NuGet.org after success |
| Manual workflow dispatch | Same complete quality job | Never |

The workflow uses `pull_request`, never `pull_request_target`. PR code runs on an
ephemeral GitHub-hosted Ubuntu 24.04 runner with read-only repository permissions,
no supplied publish secrets, and no persisted checkout credentials. Publishing is
a separate job gated on both a trusted push event and successful quality result.
That job downloads only the package/symbol archives and SDK selection file from
the same workflow run. It does not check out or execute repository scripts.

The default token has `contents: read`. Only the publish job receives
`packages: write`; the NuGet.org key is passed only to its tag-publishing step.
No cloud identity, Azure credentials, deployment environment, or self-hosted
runner is required. Third-party action references are immutable commit SHAs with
the verified release tag beside each reference.

## Quality Sequence

1. Fail immediately unless both centralized test projects exist and declare
   `IsTestProject=true`.
2. Run `scripts/Test-FrameworkDependencies.ps1` for dependency boundaries and
   source/test/benchmark solution membership.
3. Restore and build `vc.Ifx.slnx` in Release using the shared mutex wrapper,
   `-WarningsAsErrors`, `-m:1`, and `-p:BuildInParallel=false`; capture fresh
   per-package SARIF and Compile fingerprints using `-ReportBuildDirectory`.
4. Run coverage-infrastructure and reporting-host regression/evaluation checks.
5. Run the shared wrapper with `-FullCoverage -Configuration Release -NoBuild
   -WarningsAsErrors`. This uses the just-built complete test assemblies, rejects
   empty discovery/results, merges only current-run coverage, and enforces exact
   100% line and branch counts for every expected package.
6. Generate versioned reports from the same build and coverage run. Missing
   coverage produces failed reports with Unknown measurements, not success.
7. Run the existing package validator with `-Pack`, then its real/mutated archive
   regression checks. Packing and validation happen after successful coverage.
8. For a release tag, reject any archive whose version differs from the stable
   tag. Upload validated package/symbol artifacts only after all quality steps pass.

Coverage is never filtered by test source folder, test name, or package in CI.
There is no report-only path or allowed-failure step. The generated-code policy
remains in `Directory.Build.targets` and [tests/README.md](../../tests/README.md).
Warnings are errors for compilation and restore; NuGet audits the complete
dependency graph. Package validation preserves the existing metadata, dependency,
README, symbol, and Source Link checks.

`TestResults/**` is retained for 14 days even on failure. The absence of artifacts
after a pre-build failure is reported as an upload warning, not a replacement for
the already-failed quality job. Package artifact globs are limited to the root
archive directory and cannot include the validator's deliberately broken fixtures.
Package/symbol artifacts also remain available for PR review but are never
published by PR runs.

## Versioning And Setup

`global.json` controls SDK selection. The prior workflow installed a floating NBGV
tool although this checkout has no version.json or NBGV package/tool manifest.
That unconfigured versioning step was removed. Packages use the repository's
evaluated PackageVersion; a release tag does not invent or rewrite package versions.
Set the intended package version in repository configuration before creating a
matching stable tag. Reusing a package version retains the existing
`--skip-duplicate` publishing behavior and will not overwrite a published package.

Repository administrators must configure the **Framework quality gate** check as
required in branch protection/rulesets. Merely committing a workflow does not
change those settings. Configure `NUGET_API_KEY` for NuGet.org releases and ensure
GitHub Packages permits this repository's token to publish. Hosted runner access,
fork-workflow approval policy, branch protections, and secret validity must be
verified by the Orchestrator. No repository settings or secrets were changed here.

The other workflows remain independent: `vbd-analysis.yml` checks VBD/documentation
automation; `verify-copilot-instructions.yml` checks instruction-file changes;
`update_changelog.yml` responds to published releases. None substitutes for the
framework quality gate or publishes framework packages.

## Local Verification And Handoff

Gate 5 adds compiler SARIF/Compile fingerprint capture, a non-packable report host,
host regression checks, and deterministic JSON/Markdown after the exact coverage
run. [Reporting documentation](../../docs/reporting/README.md) defines the current
schema, build/run binding, incomplete-evidence behavior and commands. Source package
inventory now includes `vc.Ifx.Roslyn.Reporting`; coverage remains exact for every
discovered package, with no source/test filters in the full CI run.

Syntax/structure checks validate the workflow and expanded composite action with
actionlint 1.7.12, parse both YAML documents with PyYAML 6.0.3, and check event,
permission, ordering, threshold, artifact, and publish-job contracts. PowerShell
blocks are parsed without executing publishing commands. These checks do not run
GitHub-hosted jobs or prove credentials, Linux native loading, artifact transfer,
or publishing behavior.

Local checkpoint on 2026-09-09: actionlint accepted the entry workflow and the
expanded composite; 43 YAML/structure/security assertions passed; all 10 embedded
PowerShell blocks and 3 modified shared scripts parsed; 15 existing coverage
infrastructure checks passed. Benchmark metadata evaluated as non-packable and the
Linux benchmark path pattern matched. No full solution build, full test-suite
rerun, package publication, or hosted workflow execution was performed in Gate 4.

Run the existing infrastructure checks locally with:

```powershell
pwsh -NoProfile -File tests/infrastructure/coverage/Test-CoverageInfrastructure.ps1
actionlint .github/workflows/publish.yml
```

The Orchestrator owns the coordinated warning-free full build, full suite, combined
100% coverage, final packaging snapshot, and first hosted execution. Known baseline
failures remain recorded in
[the unit baseline](../../docs/testing/initial-unit-baseline-2026-09-09.md).
This workflow deliberately fails while any of those quality conditions is unmet.

References: [GitHub composite actions](https://docs.github.com/en/actions/tutorials/create-actions/create-a-composite-action),
[workflow token permissions](https://docs.github.com/en/actions/tutorials/authenticate-with-github_token).
