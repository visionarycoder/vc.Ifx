---
name: naming-standards
description: Applies consistent C# naming conventions by matching local repository patterns and analyzer expectations.
license: MIT
title: Modern C# Naming Standards
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1000
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - method-signature-conventions
  - dotnet-naming-standards
appliesTo: '**/*.{cs,csproj,editorconfig}'
tags:
  - dotnet
  - csharp
  - naming
  - ste
---
# Modern C# Naming Standards

This skill applies repository-aligned C# naming rules.
This skill follows dominant local patterns before it falls back to language defaults.

## Naming Table

| Artifact | Preferred pattern |
|---|---|
| Types, interfaces, enums | `PascalCase` with repository-specific prefixes only when the local scope already uses them |
| Methods and properties | `PascalCase` |
| Parameters and locals | `camelCase` |
| Private fields | Match local field prefix style, then apply it consistently |
| Async methods | Verb plus `Async` suffix |
| Collections | Plural nouns when the value contains multiple elements |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Detect | Agent inspects sibling files, public APIs, and existing analyzers. | Local naming pattern |
| 2. Plan | Agent limits renames to the requested scope and identifies affected references. | Rename plan |
| 3. Rename | Agent applies consistent names and updates references in scope. | Aligned symbols |
| 4. Validate | Agent builds and runs the smallest relevant tests or analyzers. | Verified rename |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Local pattern | Review changed names against nearby code. | New names match the dominant local convention. |
| Async naming | Review changed async members. | Every async member uses the `Async` suffix when appropriate. |
| Scope control | Review diff breadth. | The rename stays inside the intended task scope. |
| Validation | Run the smallest targeted build or analyzer command. | Renamed symbols compile cleanly and references resolve. |
