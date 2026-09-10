---
title: Compatibility Migration Reference
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 820
---
# Compatibility Migration Reference

Agent migrates legacy routes, payloads, and behaviors without breaking active clients inside the recorded compatibility window.

## When to Use

| Condition | Agent Action |
|---|---|
| Route, DTO, or status-code modernization keeps legacy clients alive | Use this reference |
| New endpoint starts with no legacy clients | Do not use this reference |
| Error payload migration is in scope | Pair with `error-contract-hardening.md` |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify compatibility scope | Agent identifies legacy routes, payload fields, and callers that stay supported. | Agent lists preserved contracts and retirement target when available. | Every preserved contract appears once. |
| 2. Add migration path | Agent introduces adapter, versioned route, alias field, or compatibility shim in the narrowest scope. | Inspect changed endpoint or DTO code. | Legacy support stays isolated to recorded scope. |
| 3. Record retirement path | Agent marks deprecation or migration notes in code or nearby docs when the repo pattern already uses them. | Inspect changed documentation or annotations in scope. | Every preserved contract has one visible retirement path or compatibility note. |
| 4. Verify old and new behavior | Agent runs existing compatibility and endpoint tests in scope. | Run existing build and API test commands in scope. | Zero build errors and zero failing compatibility tests. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Legacy route verification | Run existing tests for preserved endpoints or fields. | Legacy callers still succeed inside preserved scope. |
| New contract verification | Run existing tests for the modernized path. | New path returns intended route, payload, and status behavior. |
| Isolation verification | Inspect changed files for broad compatibility spread. | Compatibility code appears only in touched migration scope. |

## Outputs

- Preserved compatibility map for changed endpoints
- Narrow migration shim or versioned path
- Retirement note or deprecation evidence in changed scope
