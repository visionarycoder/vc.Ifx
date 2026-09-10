---
name: magic-strings
description: Replaces repeated literals with canonical Ifx surfaces for keys, tokens, enums, and protocol values.
title: Magic Strings
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 980
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - naming-standards
  - breaking-primitive-obsession
appliesTo: '**/*.{cs,json,md}'
tags:
  - literals
  - constants
  - ifx
  - ste
---
# Magic Strings

This skill removes repeated literals by moving them into canonical surfaces.
This skill favors discoverable names over scattered string duplication.

## Canonical Surface Table

| Literal kind | Preferred surface | Example |
|---|---|---|
| Configuration key | Named constant or options member | `ConfigurationKeys.PayrollApiUrl` |
| Feature flag | Central feature-flag surface | `FeatureFlags.EnableRetry` |
| Protocol token | Enum, constant, or value object | `MessageKinds.ScheduleRequested` |
| Repeated format string | Named constant or helper | `Formats.PayrollDate` |
| Operation code | Enum or command constant | `OperationCodes.RebuildLedger` |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Inventory | Agent finds repeated or domain-significant literals in the target scope. | Literal list |
| 2. Classify | Agent groups literals by key, token, format, or domain code. | Surface map |
| 3. Centralize | Agent creates or reuses the smallest canonical surface. | Named abstraction |
| 4. Replace | Agent rewrites call sites to use the canonical surface. | Consistent usage |
| 5. Validate | Agent builds and runs targeted tests when behavior is affected. | Verified replacement |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Canonical placement | Review new constants or enums. | Each moved literal lives in a surface that matches its domain meaning. |
| Duplication | Search the changed scope for old repeated literals. | Repeated literals no longer appear in changed scope except where intentionally retained. |
| Behavioral safety | Run targeted validation when parsing or protocol values changed. | Serialized values and configuration lookups still match the expected contract. |
