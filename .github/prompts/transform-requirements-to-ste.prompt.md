---
mode: agent
title: Transform Requirements to STE Spec
description: Transform vague user requirements into deterministic STE-aligned specifications with explicit actors and measurable verification.
doc_type: prompt
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 894
invokes_skills:
  - documentation-governance
prerequisites:
  - skill-suite-optimization
related_skills:
  - documentation-governance
appliesTo: '**/*'
tags:
  - prompts
  - ste
  - requirements
---
# Transform Requirements to STE Spec

Agent converts ambiguous requirements into explicit requirements, tests, and pass criteria.

## Inputs

| Input | Required | Notes |
|---|---|---|
| Source requirement | Yes | Agent records the exact user wording first. |
| Scope | Yes | Agent records file, endpoint, component, or workflow scope. |
| Delivery goal | Yes | Agent records build, migration, review, or contract outcome. |
| Numeric targets | No | Agent records latency, coverage, count, size, or status thresholds. |
| Constraint set | No | Agent records policy, security, or runtime limits. |

## Ambiguity Matrix

| Signal | Detection Pattern | Rewrite Target |
|---|---|---|
| Modal wording | Token from the prohibited modal list | Explicit action sentence |
| Vague scope | `appropriate`, `reasonable`, `relevant` | Named files, layers, or endpoints |
| Missing actor | Passive voice or noun-only fragment | `Agent`, `User`, or named system subject |
| Missing test | No command, scenario, or observable result | `Test` and `Pass` rows |
| Non-boolean condition | `if needed`, `when appropriate` | Numeric or state-based trigger |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent copies the original requirement and tags each ambiguity signal. | Review tagged source text. | Every ambiguous phrase has one tag. |
| 2 | Agent rewrites each sentence with one explicit subject and one observable action. | Review rewritten requirement list. | Every sentence has one subject and one action. |
| 3 | Agent adds one test and one pass condition for each requirement. | Review verification rows. | Every requirement has measurable verification. |
| 4 | Agent asks one narrow clarification only if a numeric threshold or scope boundary remains unknown. | Review clarification list. | Zero or one clarification is emitted for the unresolved gap. |
| 5 | Agent returns the final STE-shaped specification. | Review output contract. | Output follows the template exactly. |

## Rewrite Matrix

| Source Shape | Target Shape |
|---|---|
| `Validate input` | `Agent verifies input format. Test: Send invalid payload. Pass: Status = 400.` |
| `Handle errors appropriately` | `Agent returns RFC 9457 Problem Details. Test: Trigger validation error. Pass: Content-Type = application/problem+json.` |
| `Use caching if needed` | `Agent uses caching when P95 latency exceeds the agreed threshold.` |
| `Secure authentication` | `Agent verifies token signature, expiration, and required claims.` |

## Output Contract

```markdown
## Requirement: <title>

**Scope:** <path or component>

1. Agent <action>.
   Test: <command or scenario>
   Pass: <observable result>

2. Agent <action>.
   Test: <command or scenario>
   Pass: <observable result>
```

## Verification

| Check | Pass |
|---|---|
| Explicit subjects | Every sentence names the actor. |
| Modal removal | Zero modal verbs remain. |
| Measurable tests | Every requirement has `Test` and `Pass`. |
| Boolean conditions | Every trigger uses a numeric or state-based boundary. |
