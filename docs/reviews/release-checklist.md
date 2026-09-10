# Release Checklist

Use this checklist with the [branching playbook](branching-strategy.md) and
[quality workflow guide](../../.infra/yaml/README.md). Publication is an explicit
release operation; documentation updates do not authorize tagging or publishing.

## Before Tagging

- [ ] Review and merge the changes intended for this release.
- [ ] Set the intended package version in repository configuration; verify every
  package's evaluated PackageVersion matches the planned stable vX.Y.Z tag.
- [ ] Obtain a passing quality run for the intended commit, including full coverage,
  reports, tested-binary package validation, and exact archive manifest checks.
- [ ] Verify the configured Ubuntu hosted job and any separately required platform
  or IDE checks. The workflow does not provide an all-platform test matrix.
- [ ] Confirm required-check settings, publishing permissions, and NUGET_API_KEY.
- [ ] Update affected package documentation, architecture decisions, and release
  notes. Update best practices or the dated radar only when a review warrants it.
- [ ] Obtain release approval and record the exact commit and version.

## Tag And Publish

- [ ] Create and push an annotated stable vX.Y.Z tag on the approved commit.
- [ ] Verify the tag's quality job passes, including exact package-version matching.
- [ ] Verify the publishing job validates the same-run archive manifest and publishes
  the intended packages and symbols to NuGet.org.
- [ ] Check package availability and versions. Duplicate versions are skipped, not
  overwritten; a successful job alone does not prove changed contents replaced an
  existing version.
- [ ] Retain build, coverage, report, and package-manifest evidence.

A main push publishes to GitHub Packages after quality passes. A stable tag
publishes to NuGet.org only. Neither action derives package versions from the branch
or tag.

## After Publication

- [ ] Publish the GitHub Release with reviewed release notes.
- [ ] Review and merge the changelog PR opened by the release-published workflow.
- [ ] Record actual hosted/IDE acceptance and any remaining limitations in the plan;
  do not mark external checks complete from local evidence alone.
