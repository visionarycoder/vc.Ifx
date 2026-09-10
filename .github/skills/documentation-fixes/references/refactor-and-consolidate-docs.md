---
title: Refactor and Consolidate Docs
doc_type: guide
status: active
last_updated: 2026-07-20
target_audience: ai
complexity: low
estimated_tokens: 199
prerequisites:
  - documentation-fixes
related_skills:
  - documentation-fixes
appliesTo: '**/*.md'
tags:
  - documentation
---
# Refactor and Consolidate Docs

Use this playbook to reduce duplication and drift.

## Goals

- One canonical source per topic.
- Remove contradictory or copy-pasted guidance.

## Actions

1. Inventory related docs for a topic.
2. Pick canonical file per topic.
3. Merge or replace duplicates with short summaries + links.
4. Keep ADR history intact; do not rewrite prior decisions.

## Validation

- Canonical locations are clear.
- Duplicate sections removed or redirected.
- Navigation/index links updated.
