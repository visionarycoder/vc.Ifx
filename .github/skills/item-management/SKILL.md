---
name: item-management
description: Applies MSBuild item-group patterns for Include, Update, Remove, metadata, transforms, and batching.
license: MIT
title: MSBuild Item Management Patterns
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
  - property-patterns
  - including-generated-files
appliesTo: '**/*.{csproj,props,targets}'
tags:
  - msbuild
  - items
  - batching
  - ste
---
# MSBuild Item Management Patterns

This skill applies correct MSBuild item semantics and batching behavior.
This skill keeps item mutation precise and evaluation-friendly.

## Pattern Table

| Intent | Preferred pattern | Avoid |
|---|---|---|
| Add new items | `Include` | `Update` on items that do not already exist |
| Change metadata on SDK-globbed items | `Update` | Duplicate `Include` that causes repeated items |
| Remove default items | `Remove` | Replacing the entire SDK glob set without need |
| Transform per item | Metadata transforms such as `@(File->'%(Filename).g.cs')` | Manual string duplication |
| Batch work | Batch on the exact metadata key needed | Unintended cross-product batching |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Inventory | Agent inspects current item origins, metadata, and duplicate behavior. | Item map |
| 2. Select | Agent chooses `Include`, `Update`, `Remove`, or transform based on item origin. | Mutation plan |
| 3. Apply | Agent edits item groups and batching metadata with minimal churn. | Correct item flow |
| 4. Validate | Agent builds the affected project and checks for duplicate or missing items. | Verified item graph |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Semantic fit | Review each changed item operation. | Each operation matches the item origin and goal. |
| Duplicate control | Run the targeted build. | No new duplicate item warnings or missing item failures appear. |
| Batching | Review target execution count. | Targets run the expected number of times. |
| Cleanup | Review generated-file handling when applicable. | Generated items integrate with `Clean` through `FileWrites` or equivalent tracking. |
