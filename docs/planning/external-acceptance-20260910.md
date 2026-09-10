---
title: External Acceptance Follow-up
doc_type: report
status: active
last_updated: 2026-09-10
---

# External Acceptance Follow-up

Owner: Orchestrator; Updated: 2026-09-10.

Scope: Gate 4 only. Preserve concurrent VBD scripts/workflow/solution edits and
existing local commits. Do not merge, publish, change repository security settings,
install an IDE, or change credentials as part of this verification.

## Verified Hosted Failure

The existing [main quality run](https://github.com/visionarycoder/vc.Ifx/actions/runs/34498384866)
tested revision `7a77f69af365059d68c6a4da25ff24a42ab7c1a7` on Ubuntu. It passed
dependency inventory, 48 coverage infrastructure checks, three reporting process
checks, 31 package-manifest checks, and 12 package-wrapper checks. It then failed
in `Test-BuildProvenance.ps1` while removing a temporary hidden analyzer
configuration without `-Force`. The full solution build, coverage, report, and
validated-package upload were skipped. Publication was skipped.

Downloaded the actual failure artifact `10160800049` (899 ZIP entries) through
the authenticated connector and verified the bytes against GitHub's digest:

`6e0104e226c38fe0a80ed4a37638ff72c8a5402aa70d0d79822d1cac622fa47a`

Local archive: `TestResults/external-acceptance/34498384866/quality-evidence.zip`.
This proves transfer of failure evidence only. Its synthetic package fixtures are
not the 28 validated framework packages and do not satisfy release acceptance.

## Cleanup Correction

The regression now removes only its exact temporary configuration with `-Force`,
sets the Hidden attribute on Windows to exercise the same cleanup requirement,
and explicitly verifies removal. No production source, provenance assertions,
coverage threshold, or workflow publishing conditions changed.

`pwsh -NoProfile -File tests/infrastructure/provenance/Test-BuildProvenance.ps1`
passed all 30 checks locally; the isolated Release fixture built with zero warnings
and errors. Evidence:
`TestResults/provenance-regressions/cc90d9ceaae74af5a68067a84c2e1027/results.json`.

Only this correction was submitted in [draft PR #9](https://github.com/visionarycoder/vc.Ifx/pull/9),
branch `codex/gate4-hidden-config-cleanup`, commit
`d0530080dccbc486ac3b1b395ab969b325470bfe`, based on the observed main revision.
The existing local branch and unrelated changes were not pushed or committed.
The PR-triggered [quality run](https://github.com/visionarycoder/vc.Ifx/actions/runs/34499807987)
passed all 30 provenance checks on Linux and built the complete solution with zero
warnings/errors. It then stopped before coverage with `Changed build output
inventory: vc.Ifx.IntegrationTests`. The captured manifest includes Coverlet's
hidden `.msCoverageSourceRootsMapping_vc.Ifx.IntegrationTests`; the live inventory
enumeration omitted hidden files. This is a second cross-platform verifier defect,
not evidence of failing framework behavior tests.

Downloaded artifact `10161418178` to
`TestResults/external-acceptance/34499807987/quality-evidence.zip` and verified its
SHA256 `cf99e03da100fc1169ff83b9eb29aa345d79fd6031fa01bdaaea96fb3bb6de0f`.
Inspected the actual captured integration output identity to confirm the hidden
mapping entry. Coverage, reports, validated packages, and publication were skipped.

The second correction includes hidden outputs in live inventory enumeration rather
than excluding them from provenance. Tests cover unchanged, changed, missing, and
extra hidden outputs, preserving original attributes through mutation/restoration.
All 34 local provenance checks passed in
`TestResults/provenance-regressions/5152b167adca4b699e1df46355cc5e82`.
`pwsh -NoProfile -File tests/infrastructure/packaging/Test-ValidatedPackageArtifacts.ps1`
passed 12 checks using the real provenance module and stubbed packaging tools;
evidence `TestResults/package-wrapper-tests/f21f9963d8034b9c85cf65d75b1b8507`.

At commit `05b42c4a8d662bbe03af6466b7b1b1c70e7f8607`, PR #9 included only the
provenance module and regression script. Its
[second hosted run](https://github.com/visionarycoder/vc.Ifx/actions/runs/34500464709)
passed the corrected provenance checks and complete warning-free solution build.
It executed 3,617 unit tests: 3,482 passed, 135 failed, none skipped. Integration,
final coverage acceptance, and validated packages were not reached.

## Portable Test Fixtures

The 135 failures exposed five bounded test-fixture issues:

- Generator attribute source lookup used CallerFilePath, rewritten to `/_/` by
  deterministic CI compilation. Locate the checkout from AppContext.BaseDirectory
  instead; keep compiling the actual attribute sources.
- Local storage helper tests assumed Windows drives/backslashes. Construct native
  paths and filesystem roots while retaining filename/directory expectations.
- FTP FileInfo tests assumed Windows full-path and UNC normalization. Use native
  FileInfo shapes for accepted paths and an explicit drive path for rejection.
- Unix permits unlinking an open file. Assert that platform behavior while still
  checking held content; Windows retains the delete-sharing failure assertion.
- TenantContext's publicly mutable Dictionary/List collections are caller-owned,
  not concurrent collections. Apply caller synchronization in the legacy concurrent
  write test, consistent with the existing documented contract.

No runtime API or behavior changed, tests were not skipped, and strict coverage
thresholds were not relaxed. The targeted Release command with warnings as errors
passed all 371 tests, none skipped:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Configuration Release -WarningsAsErrors -Filter 'FullyQualifiedName~Generators.Implementation|FullyQualifiedName~Storage.StorageServiceTests|FullyQualifiedName~Storage.Local.LocalStorageProviderTests|FullyQualifiedName~Storage.Ftp.FtpStorageProviderTests|FullyQualifiedName~Authentication.TenantContextTests'
```

Evidence: `TestResults/tests/vc.Ifx.UnitTests/aaac73eca34f453db58063e5b2bf3ab0/tests.trx`.
The old hidden-output enumeration was also evaluated in an isolated in-memory
module against the unchanged hidden fixture: it failed with the expected inventory
mismatch, while the corrected module accepted the same fixture.

PR #9 contains the two provenance files and these five test files, at commit
`623257381f3c95d3e35a48b7cab0fae6e35e97dc`. Hosted acceptance passed for its
GitHub test-merge revision, recorded below. The PR remains unmerged and draft.

## Successful Hosted Acceptance

[Quality run 34501314692](https://github.com/visionarycoder/vc.Ifx/actions/runs/34501314692)
passed on Ubuntu 24.04. Actual tested merge revision:
`7586ccfaa2bbe96a9e7fcd76aed182afb461f381`.

- Complete Release solution build: zero warnings and errors, 36 projects.
- Unfiltered tests: 3,617 unit and 15 integration passed; zero failures or skips.
- All 28 libraries: exact 9,408/9,408 lines and 5,332/5,332 branches covered.
- Paired report: passed, zero issues.
- Tested-binary packaging: all 28 version 1.0.0 package/symbol pairs validated;
  19 archive-validator checks and final manifest recheck passed.
- Quality and package artifact uploads succeeded. Publication was skipped because
  this was a pull-request run, not a main/tag push.

Exact hosted evidence directories under TestResults:

- Build: `reporting/66036554747c4bc9ab504dd118bec729`.
- Coverage: `coverage/full/19554890e94d4858ba513624e168af56`.
- Packages: `package-artifacts/65a0baae028d47bb91af3f0d177a204b/validated`.

Downloaded both artifacts through the authenticated connector and verified their
ZIP bytes against GitHub's independently retrieved artifact digests:

| Artifact | ID | SHA256 |
| --- | --- | --- |
| Validated packages | 10162175245 | `5c2c8a36d917fb10c4012ce8ab6a0c29c45c349fdcda7a4be806fa110f09fc8b` |
| Quality evidence | 10162174614 | `cfb51a071168f008690e88b3f313a81152f582b6d09611ff50e9e8d0741f48d7` |

Local copies are under `TestResults/external-acceptance/34501314692/`; package
archives are extracted to its fresh `validated/` directory. Verified all 58 exact
entries with the repository's real manifest verifier:

```powershell
Import-Module ./scripts/packaging/PackageArtifactManifest.psm1 -Force
Test-PackageArtifactManifest -Directory TestResults/external-acceptance/34501314692/validated -Revision 7586ccfaa2bbe96a9e7fcd76aed182afb461f381 -ManifestSha256 7a273758922643381abf550cf2dbae43be5e4e78a9500c12e3a854c9cb999a76
```

The expected manifest SHA came from the hosted job's final verification step, not
from trusting the downloaded manifest alone. Its build/coverage SHA256 bindings
also match the exact entries inside the independently verified quality artifact:

- Build identity: `4989128e86e1d36a9a5ba8ad378c916a0662b5488ef14ee033ad60bbe38d5868`.
- Coverage summary: `29209904d04bcbcc26a632682368718dde25dfa3c47a07b7137ca76bab49912c`.

Read both transferred TRX files and the report/coverage JSON to confirm the counts,
zero skips, exact thresholds, passing report, and tested revision. The artifact
transfer check is complete; the publish job's own download-action execution and
feed pushes remain unexecuted. Nothing was published or merged by this task.

## Settings And Environment

- Public branch metadata reports main is unprotected, with no required status
  contexts. The only returned ruleset, `RestrictAccess` (9959001), is disabled and
  has no required-status-check rule. The quality check is not currently enforced.
- The connector reports the user has repository admin permission, but its branch
  protection endpoint returns 403 `Resource not accessible by integration`.
  User permissions do not grant the integration unrestricted administration.
- The installed Git credential helper did not supply a credential noninteractively.
  No local GitHub CLI/token was available. The connector supports PR runs but not
  workflow dispatch or secrets metadata; feed permission and key validity remain
  unverified. No secret value was retrieved or printed.
- An attempt to retry the previous PR run 34498292844 returned 403 `This workflow
  run cannot be retried`; draft PR #9 provides a fresh non-publishing run instead.
- Browser automation failed during initialization. `vswhere` reports only SSMS,
  not development Visual Studio. Actual documentation refresh, analyzer discovery,
  code-fix light bulbs, and Fix All interaction remain unverified.
- WSL lists Ubuntu but reports invalid `wsl2.swap=8Gb` configuration; the local
  Linux tool probe did not find a usable toolchain. No machine settings were changed.

## Remaining Acceptance

- [x] Successful full Linux quality run for the correction.
- [x] Download the actual validated 28-package artifact and verify exact manifest,
  revision, archive inventory, and hashes after transfer.
- [ ] Review and merge PR #9 through the authorized release process; this successful
  test-merge run does not put the correction on main or validate unrelated local edits.
- [ ] Authorized administrator configuration of the required quality check and
  verification of feed permissions/credentials.
- [ ] Explicitly authorized publication and confirmation of intended feed versions.
- [ ] Development Visual Studio documentation and compiler-tooling acceptance.

The [local checkpoint](local-verification-20260910.md) remains dated evidence for
its captured working tree, not current hosted acceptance or proof of later edits.
Gate 4 is Blocked only on the remaining approvals, access, and IDE environment;
no hosted job started by this task remains running.
