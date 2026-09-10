---
name: audit-trail-compliance
title: Audit Trail Compliance
description: Apply audit trail, control evidence, segregation, and event-sourced .NET patterns when financial-system compliance workflows are in scope.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1927
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - journal-entry-patterns
  - domain-events-dotnet
  - cqrs-patterns-dotnet
  - repository-unitofwork-efcore
appliesTo: '**/*.{cs,csproj,sql,md}'
tags:
  - accounting
  - audit
  - compliance
  - controls
  - dotnet
---
# Audit Trail Compliance

Agent applies audit trail, control, and event-history patterns for financial systems.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Financial workflow needs who, what, when, and why trace fields | Use this skill |
| Work item covers SOX-aligned approval, access, or evidence controls | Use this skill |
| Segregation-of-duties or exception reporting is in scope | Use this skill |
| .NET services need event-sourced history or immutable change tracking | Use this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Journal lifecycle without control-design focus | `journal-entry-patterns` |
| Reconciliation matching rules only | `reconciliation-patterns` |
| Period-close orchestration only | `closing-cycle-patterns` |
| Double-entry fundamentals only | `double-entry-accounting` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Control objectives | Yes | Traceability, authorization, completeness, tamper evidence |
| Actor model | Yes | Preparer, approver, poster, admin, reviewer, auditor |
| Evidence policy | Yes | Retained fields, retention period, evidence source |
| Exception policy | Yes | Threshold, routing, remediation |
| Compliance scope | No | SOX, entity, or report scope |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps each financial action to actor, action, target, timestamp, reason, and source evidence fields. | Review audit record schema. | Every controlled action has all trace fields. |
| 2 | Agent separates duties across create, approve, post, and administer actions. | Run role and transition tests. | No forbidden actor combination completes one incompatible sequence. |
| 3 | Agent records immutable history for inserts, updates, approvals, postings, and access changes. | Review history write path. | History preserves prior value, new value, actor, and time. |
| 4 | Agent routes exceptions, overrides, and failed controls into review queues. | Read exception queue output. | Every exception has owner, severity, and resolution state. |
| 5 | Agent enforces access controls and approval hierarchies before sensitive action completion. | Run authorization and hierarchy tests. | Sensitive actions fail without the required role path. |
| 6 | Agent runs targeted build or compliance tests. | Run targeted control and history tests. | Traceability, authorization, and exception assertions pass. |

## Audit Evidence Matrix

| Control Area | Required Evidence | Test | Pass |
|---|---|---|---|
| Who | User, service, or delegated actor id | Review action log records. | Every controlled event records the acting principal. |
| What | Entity type, entity id, field set, action code | Review change history rows. | Every event identifies the changed business object and action. |
| When | UTC timestamp and ordered sequence key. | Compare event order across one aggregate. | Event order remains deterministic and queryable. |
| Why | Reason code, memo, ticket id, or approval reference. | Review override and approval actions. | Sensitive actions include business justification. |
| Where | Source channel, API route, import file, or batch id. | Review source metadata fields. | Every event traces to an originating channel. |
| Outcome | Success, rejection, exception, or rollback result. | Review event result field. | Every controlled action records final outcome. |

## Control Pattern Table

| Control Pattern | Pattern | Test | Pass |
|---|---|---|---|
| SOX traceability | Immutable evidence for create, approve, post, config, and access events | Review retained history by action type. | Evidence exists for every in-scope action type. |
| Segregation of duties | Block incompatible create-approve-post or admin-approve combinations | Run incompatible-role test matrix. | Forbidden combinations fail every time. |
| Change tracking | Record before value, after value, actor, time, and reason | Review change event payloads. | Payload contains prior and new values for tracked fields. |
| User access controls | Enforce least privilege and record grants, removals, and emergency access | Run authorization and access-history tests. | Access changes require authorization and leave evidence. |
| Approval hierarchy | Derive approver chain from amount, entity, and threshold | Run threshold and delegation tests. | Approver chain matches policy for each scenario. |
| Exception reporting | Group overrides, failed controls, stale approvals, and unusual access patterns into review reports | Review exception report output. | Report contains owner, severity, aging, and resolution status. |

## .NET Implementation Guidance

| Concern | Agent Action |
|---|---|
| Event model | Immutable audit events with actor, action, target, reason, correlation id, outcome |
| Event sourcing | Append-only streams for high-control aggregates plus read models |
| Change tracking | Controlled-field deltas via EF Core interceptors, domain methods, or audit services |
| Authorization | Centralized role and approval checks before persistence |
| Reporting | Exception reports, access reviews, and action histories through read-side DTOs |
| Retention | Archive without deleting required compliance evidence from the source of record |

## Integration Hooks

| Hook | Agent Action |
|---|---|
| EF Core | Map append-only audit events, access changes, and tracked-field history |
| Domain events | Raise business domain events and separate audit events for control evidence |
| CQRS patterns | Use commands for grant, approve, post, override, and resolve flows; queries for histories and dashboards |
| Correlation | Propagate correlation and causation ids across commands, events, and audit records |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate`. | Command returns zero validation errors. |
| STE wording | Run the repository STE violation grep on the skill file. | Scan returns zero matches. |
| Trace completeness | Review sample controlled events. | Every sample contains who, what, when, why, where, and outcome fields. |
| Segregation control | Run incompatible-role tests for create, approve, post, and admin actions. | Forbidden combinations fail every time. |
| Exception reporting | Review unresolved exceptions. | Every open exception has severity, owner, age, and resolution state. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Audit trail stores current value only | Agent records prior and new value plus action context. |
| Approval evidence lacks business reason | Agent requires reason code or linked support reference for sensitive actions. |
| Event stream and business transaction share one mutable row | Agent stores append-only events separate from mutable projections. |
| Emergency access leaves no follow-up review trail | Agent writes grant, use, and revoke events plus exception-report entries. |
| Exception queue omits aging and owner fields | Agent adds owner, severity, created time, and last-action time. |
