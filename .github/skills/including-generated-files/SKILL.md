---
name: including-generated-files
description: Fixes MSBuild generation flows so build-generated files participate in compilation, output, and Clean.
license: MIT
title: Including Generated Files Into Your Build
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1120
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - item-management
  - target-authoring
appliesTo: '**/*.{csproj,props,targets}'
tags:
  - msbuild
  - generated-files
  - build
  - ste
---
# Including Generated Files Into Your Build

This skill fixes build-generated artifacts that never reach compilation, output, or `Clean`.
This skill aligns target timing, item updates, and cleanup registration.

## Pattern Table

| Problem | Preferred fix | Example |
|---|---|---|
| Source generated during build is missing from compile | Add the file to `Compile` in a target that runs before compile | `<Compile Include="$(IntermediateOutputPath)Generated\*.cs" />` |
| Generated asset is not cleaned | Register the file in `FileWrites` | `<FileWrites Include="@(GeneratedFiles)" />` |
| Globs miss generated files | Add items during target execution instead of relying on evaluation-time globs | Target-scoped item addition |
| Output writes to hardcoded `obj\` | Use `$(IntermediateOutputPath)` | Property-based path |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Locate | Agent finds the generation target and the missing artifact type. | Gap analysis |
| 2. Time | Agent chooses a target point before the consuming target. | Correct target order |
| 3. Include | Agent adds the generated file to the correct item group during execution. | Participating artifact |
| 4. Clean | Agent registers files for `Clean`. | Reversible build |
| 5. Validate | Agent builds and cleans the affected project. | Verified inclusion |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Compile participation | Run the targeted build. | Generated source participates in the intended target. |
| Clean registration | Run the targeted clean. | Generated artifacts are removed by `Clean`. |
| Path hygiene | Review generated paths. | The build uses `$(IntermediateOutputPath)` or another canonical property. |
| Timing | Review target order. | The generation target runs before the consuming target. |
