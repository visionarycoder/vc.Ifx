---
name: test-fixes-bundle
description: Routes test-analysis requests through the canonical test-fixes bundle and its specialized skills.
title: Test Fixes Bundle Alias
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: low
estimated_tokens: 680
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-fixes
  - test-gap-analysis
  - test-anti-patterns
  - assertion-quality
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - test
  - bundle
  - routing
  - ste
---
# Test Fixes Bundle Alias

This skill routes test-analysis requests through the canonical `test-fixes` bundle.
This skill keeps one routing surface while preserving specialized downstream skills.

## Routing Table

| Request signal | Route |
|---|---|
| General test-quality analysis | `.github/skills/test-fixes/SKILL.md` |
| Pragmatic anti-pattern review | `.github/skills/test-anti-patterns/SKILL.md` via `test-fixes` |
| Missing edge-case or escaped-defect analysis | `.github/skills/test-gap-analysis/SKILL.md` via `test-fixes` |
| Assertion depth or diversity audit | `.github/skills/assertion-quality/SKILL.md` via `test-fixes` |
| Academic smell catalog review | `.github/skills/test-smell-detection/SKILL.md` via `test-fixes` |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Normalize | Agent maps the request to the canonical `test-fixes` entry point. | Routing decision |
| 2. Delegate | Agent follows the routing table in `test-fixes`. | Specialized analysis |
| 3. Report | Agent returns findings and follow-up actions from the specialized skill. | Consolidated result |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Canonical entry | Review the selected bundle. | The request routes through `test-fixes` instead of duplicating bundle logic here. |
| Specialized route | Compare user intent to downstream skill choice. | The downstream skill matches the requested test-analysis surface. |
| Result shape | Review final reporting. | The final output preserves actionable findings and follow-up actions. |
