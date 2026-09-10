---
title: REST Route Standardization Reference
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 860
---
# REST Route Standardization Reference

Agent standardizes routes by aligning resource names, HTTP verbs, and version placement across changed endpoints.

## When to Use

| Condition | Agent Action |
|---|---|
| Controller or minimal API route templates change | Use this reference |
| Endpoint naming drifts across the same resource area | Use this reference |
| Compatibility alias route stays temporarily active | Pair with `compatibility-migration.md` |

## Route Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| ROUTE-001 | Collections use nouns | Rename verb-heavy collection segments |
| ROUTE-002 | HTTP verb carries the action | Remove duplicated action verbs from standard CRUD paths |
| ROUTE-003 | Item routes identify one resource | Use stable key segment or composite identity pattern |
| ROUTE-004 | Version placement stays consistent | Keep version segment or versioning attribute pattern aligned in scope |
| ROUTE-005 | Nested resources reflect real ownership | Keep subresource depth intentional and limited |
| ROUTE-006 | Command endpoints stay explicit | Use non-CRUD action segment only for real commands |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Map resources | Agent lists changed resources, collections, item routes, and command routes. | Agent records endpoint-to-resource mapping. | Every changed endpoint appears once. |
| 2. Normalize templates | Agent aligns route segments and verbs with one resource pattern per area. | Inspect changed route attributes or mapped endpoints. | Changed routes use one intentional pattern per resource area. |
| 3. Preserve callers when needed | Agent isolates compatibility aliases to migration scope only. | Inspect changed routing code for duplicate long-term aliases. | Compatibility aliases stay limited to recorded migration scope. |
| 4. Verify results | Agent runs existing routing, controller, or API tests in scope. | Run existing build and API test commands in scope. | Zero build errors and zero failing tests. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Route template verification | Inspect changed route declarations or endpoint snapshots. | Collection, item, and command routes follow one resource pattern. |
| Behavior verification | Run existing API tests for changed routes. | Endpoints respond on the intended route and verb combination. |
| Compatibility verification | Run existing legacy route tests when aliases remain. | Legacy aliases continue only where the migration scope records them. |

## Outputs

- Resource-to-route mapping for changed scope
- Standardized route templates and verbs
- Compatibility notes for any preserved aliases
