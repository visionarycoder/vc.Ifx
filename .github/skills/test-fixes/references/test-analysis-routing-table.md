---
title: Test Analysis Routing Table
doc_type: reference
status: active
last_updated: 2026-07-27
target_audience: ai
complexity: low
estimated_tokens: 426
prerequisites:
  - test-fixes
related_skills:
  - test-anti-patterns
  - test-gap-analysis
  - test-smell-detection
  - assertion-quality
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - test
  - routing
  - analysis
---
# Test Analysis Routing Table

Use this single reference to choose the correct test-analysis skill.

| Request shape | Route to | Use when | Expected output |
|---|---|---|---|
| Pragmatic quality audit | `.github/skills/test-anti-patterns/SKILL.md` | The user wants severity-ranked quality findings on an existing suite. | Anti-pattern findings, evidence, prioritized fixes. |
| Mutation-style gap analysis | `.github/skills/test-gap-analysis/SKILL.md` | The user asks whether current tests would catch subtle bugs or escaped defects. | Missed behaviors, risky paths, highest-value additions. |
| Academic smell catalog review | `.github/skills/test-smell-detection/SKILL.md` | The user explicitly asks for testsmells.org or named literature smells. | Canonical smell findings with citations and remediation. |
| Assertion depth/diversity audit | `.github/skills/assertion-quality/SKILL.md` | The user wants to assess assertion quality, depth, or variety. | Assertion profile, shallow spots, strengthening guidance. |

## Trigger Shortcuts

- `quality review`, `weak tests`, `shallow tests` -> anti-pattern audit
- `would tests catch this`, `mutation`, `missed edge cases` -> gap analysis
- `testsmells.org`, `academic smell catalog` -> smell detection
- `assertion quality`, `assertion diversity` -> assertion-quality
