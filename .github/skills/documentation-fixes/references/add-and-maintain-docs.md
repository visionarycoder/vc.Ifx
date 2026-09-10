---
title: Add and Maintain Docs
doc_type: guide
status: active
last_updated: 2026-07-20
target_audience: ai
complexity: low
estimated_tokens: 228
prerequisites:
  - documentation-fixes
related_skills:
  - documentation-fixes
appliesTo: '**/*.md'
tags:
  - documentation
---
# Add and Maintain Docs

Use this playbook for feature-linked doc updates.

## Goals

- Keep docs synchronized with behavior changes.
- Ensure public APIs/config/auth changes are reflected immediately.

## Actions

1. Identify changed behavior, contracts, and config keys.
2. Update canonical docs in `docs/**` and relevant README files.
3. Update XML/JSDoc comments for touched public symbols.
4. Remove stale references to renamed/removed symbols.

## Validation

- All changed contracts and flags are documented.
- No stale symbol/path references remain in touched docs.
- Docs and implementation describe the same current behavior.
