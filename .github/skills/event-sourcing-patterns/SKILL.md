---
name: event-sourcing-patterns
title: Event Sourcing Patterns
description: Design .NET event-sourced systems when the task needs durable event history, replay, projections, and explicit consistency boundaries.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1700
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-engine-standards
  - dotnet-dto-standards
  - dotnet-unit-testing
  - long-running-job-patterns
appliesTo: '**/*.{cs,csproj,json,sql,md}'
tags:
  - event-sourcing
  - cqrs
  - projections
  - sagas
  - dotnet
---
# Event Sourcing Patterns

Agent designs event-sourced .NET systems with explicit event contracts, aggregate rules, replay safety, and read-model separation.

## When to Use

| Condition | Use |
|---|---|
| Domain behavior depends on a durable history of business facts | Use this skill |
| Auditability, replay, or temporal queries drive the design | Use this skill |
| Read models differ sharply from write models | Use this skill |
| Business rules center on aggregate consistency and ordered event streams | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Data shape stays simple and history adds little value | Use CRUD patterns |
| Team capacity does not cover projections, replay, and schema evolution | Use simpler persistence |
| Cross-aggregate immediate consistency dominates the workflow | Use transactional relational patterns |
| Reporting needs focus on snapshots of current state only | Use state-based models |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Aggregate boundaries | Yes | Command consistency scope |
| Domain event catalog | Yes | Business facts, not persistence deltas |
| Event store choice | Yes | EventStoreDB, Marten, or SQL-based store |
| Read-model latency target | Yes | Immediate, near-real-time, or batch |
| Retention and migration policy | No | Replay horizon, archival, and upcasting scope |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent defines aggregates, commands, and invariant boundaries before storage design. | Review command and aggregate map. | Each command targets one aggregate boundary. |
| 2 | Agent selects the event store from the decision matrix and defines stream identity, ordering, and optimistic concurrency. | Review append and load contract. | Stream identity and expected-version handling stay explicit. |
| 3 | Agent designs immutable event schemas with metadata, version markers, and upcasting rules. | Review event contract set. | Events describe business facts and support forward schema evolution. |
| 4 | Agent implements aggregate rehydration, command handling, and append-only writes. | Run `dotnet build [project].csproj`. | Build succeeds with zero compile errors. |
| 5 | Agent adds projections, snapshots, replay controls, and saga coordination where the domain needs them. | Run existing projection or integration tests. | Read models converge and replay paths stay deterministic. |
| 6 | Agent validates migration, eventual consistency, and temporal query behavior. | Run existing tests for replay, migration, and consistency lag. | Tests confirm replay safety, upcasting correctness, and bounded lag handling. |

## Event Store Decision Matrix

| Option | Best fit | Strengths | Tradeoffs |
|---|---|---|---|
| EventStoreDB | Dedicated event streams and subscription-heavy systems | Native stream semantics, subscriptions, expected-version append | Extra infrastructure surface |
| Marten | PostgreSQL-backed .NET systems with document and event needs together | Strong .NET integration, projections, snapshots, one platform | PostgreSQL dependency shapes scaling model |
| SQL-based store | Existing relational estates with tight operational standards | Familiar operations, direct SQL access, incremental adoption | More custom infrastructure for subscriptions, replay tooling, and concurrency controls |

## Aggregate and Projection Matrix

| Concern | Preferred pattern | Avoid |
|---|---|---|
| Aggregate design | One stream per aggregate instance with command-side invariants inside the aggregate | Cross-aggregate invariants enforced by in-process orchestration alone |
| Event naming | Past-tense domain facts with stable semantic meaning | CRUD-style names like `CustomerUpdated` for unrelated field changes |
| Snapshotting | Snapshot on measured rehydration cost or event-count threshold | Snapshotting every stream from day one |
| Projections | Idempotent handlers with checkpointed positions | Read-model updates with hidden side effects |
| Temporal queries | Rebuild state from stream position or timestamped replay | Mixing current-state tables into historical reconstruction |
| Eventual consistency | UI and API contracts that expose stale-read tolerance and recovery path | Read-your-own-write assumptions across independent projections |

## CQRS, Saga, and Migration Matrix

| Need | Pattern | Notes |
|---|---|---|
| Separate write and read concerns | CQRS with event-sourced write model and projection-backed read model | Keep command validation on the write side |
| Distributed transaction coordination | Saga or process manager driven by domain events | Persist saga state and idempotency keys |
| Event schema evolution | Versioned events plus upcasters at load or projection boundaries | Keep historical events immutable |
| Replay after projector changes | Full or scoped replay with isolated checkpoints | Keep replay side effects out of external systems |
| .NET 10 implementation | Async append/load APIs, `System.Text.Json` source generation, hosted projector services | Keep serialization stable and explicit |

## Verification Checklist

Agent verifies:
- [ ] Event names describe business facts
- [ ] Aggregate boundaries match invariant boundaries
- [ ] Append operations enforce expected-version concurrency
- [ ] Projection handlers stay idempotent and checkpointed
- [ ] Snapshot policy follows measured rehydration cost
- [ ] Replay and temporal queries produce deterministic state
- [ ] Upcasting or migration logic covers historical versions
- [ ] Tests cover aggregate behavior, projection convergence, and saga idempotency

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| CRUD events mirror table updates instead of domain facts | Rewrite events around business meaning |
| Aggregate loads become slow with no evidence | Add snapshots from measured thresholds |
| Projection handlers call external systems during replay | Isolate side effects behind replay-aware boundaries |
| Event versions drift with ad-hoc serializer changes | Add explicit versioning and stable serialization settings |
| Team expects strict immediate consistency across all models | Expose bounded lag and redesign the workflow where needed |
