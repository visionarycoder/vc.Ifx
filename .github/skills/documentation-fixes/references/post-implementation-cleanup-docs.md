---
title: Post-Implementation Cleanup Docs
doc_type: guide
status: active
last_updated: 2026-07-20
target_audience: ai
complexity: low
estimated_tokens: 246
prerequisites:
  - documentation-fixes
related_skills:
  - documentation-fixes
appliesTo: '**/*.md'
tags:
  - documentation
---
# Post-Implementation Cleanup Docs

Use this playbook after implementation is complete and validated.

## Goals

- Preserve current-state docs.
- Archive or delete low-value historical implementation artifacts.

## Actions

1. Verify completion gate (build/tests/validation done).
2. Classify files as keep/archive/delete.
3. Archive by default under `docs/archive/implementation-cleanup/YYYY-MM-DD/<feature>/`.
4. Delete only no-value files or when explicitly requested.
5. Write `cleanup-manifest.md` listing archived/deleted files.

## Validation

- Current-state docs remain discoverable and accurate.
- Historical clutter removed from active working set.
- Manifest exists for traceability.
