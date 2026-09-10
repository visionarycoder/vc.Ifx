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

PR #9 now includes only the provenance module and regression script, at commit
`05b42c4a8d662bbe03af6466b7b1b1c70e7f8607`. Its new non-publishing hosted result is
pending verification. No full framework test pass or package acceptance is claimed
for either correction until the hosted sequence succeeds.

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

- [ ] Successful full Linux quality run for the correction.
- [ ] Download the actual validated 28-package artifact and verify exact manifest,
  revision, archive inventory, and hashes after transfer.
- [ ] Authorized administrator configuration of the required quality check and
  verification of feed permissions/credentials.
- [ ] Explicitly authorized publication and confirmation of intended feed versions.
- [ ] Development Visual Studio documentation and compiler-tooling acceptance.

The [local checkpoint](local-verification-20260910.md) remains dated evidence for
its captured working tree, not current hosted acceptance or proof of later edits.
