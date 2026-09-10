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
That job downloads the package/symbol archives, SDK selection file, and exact
package manifest from the same workflow run. It verifies the manifest and archive
hashes before installing the SDK. It does not check out or execute repository scripts.

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
3. Run coverage, reporting-process, package-manifest/wrapper, and build-provenance
   infrastructure regression checks before capture; some checks build test fixtures.
4. Restore and build the complete Release solution through the shared mutex wrapper
   with warnings as errors, one MSBuild node, and a fresh ReportBuildDirectory.
   Capture compiler SARIF plus evaluated inputs and DLL/PDB build identity.
5. Set IFX_REPORT_BUILD to that capture and run unfiltered full coverage with
   `-FullCoverage -Configuration Release -NoBuild -WarningsAsErrors`.
   Reject empty discovery/results and enforce exact 100% lines and branches for
   every package using only this run's coverage.
6. Generate the report from the same captured build and coverage run. When failed
   coverage leaves a usable context, retain a failed report without masking failure.
7. Run `scripts/packaging/Invoke-ValidatedPackageArtifacts.ps1`. It verifies the
   tested build before/after packaging, packs without rebuild/restore, validates
   archives without the validator's `-Pack` switch, matches DLL/PDB payload hashes,
   and stages only the exact manifest inventory.
8. Run real/mutated archive regression checks against the raw package directory.
9. For a tag, require stable vX.Y.Z and verify every staged package version matches.
10. Recheck the exact validated upload manifest after the self-tests. The workflow
    uploads that staging directory only after successful quality completion.

Coverage is never filtered by test source folder, test name, or package in CI.
There is no report-only path or allowed-failure step. The generated-code policy
remains in `Directory.Build.targets` and [tests/README.md](../../tests/README.md).
Warnings are errors for compilation and restore; NuGet audits the complete
dependency graph. Package validation preserves the existing metadata, dependency,
README, symbol, and Source Link checks.

`TestResults/**` is retained for 14 days even on failure. The absence of artifacts
after a pre-build failure is reported as an upload warning, not a replacement for
the already-failed quality job. Package uploads use the exact validated staging directory and manifest inventory,
not a broad archive glob; the validator's deliberately broken fixtures remain outside
that directory.
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

The [2026-09-10 local checkpoint](../../docs/planning/local-verification-20260910.md)
records the completed warning-free full build, 3,632 passing tests, all 28 strict
coverage results, passed report, and validated package/symbol pairs. The
[initial unit baseline](../../docs/testing/initial-unit-baseline-2026-09-09.md) is
historical, not the current failure list. First hosted execution, actual artifact
transfer/publication, administrator settings, and hands-on IDE acceptance remain
open. Local acceptance does not prove these external conditions.

References: [GitHub composite actions](https://docs.github.com/en/actions/tutorials/create-actions/create-a-composite-action),
[workflow token permissions](https://docs.github.com/en/actions/tutorials/authenticate-with-github_token).
