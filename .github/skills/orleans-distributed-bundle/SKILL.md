---
name: orleans-distributed-bundle
title: Orleans Distributed Bundle
description: Routes Orleans distributed-system work across virtual actors, collaboration patterns, Aspire components, and Aspire orchestration.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 970
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - orleans-virtual-actors
  - orleans-patterns
  - aspire-components
  - aspire-orchestration
  - dotnet-aspire-vbd-development
appliesTo: '**/*.{cs,csproj,json,md,yml,yaml}'
tags:
  - orleans
  - virtual-actors
  - aspire
  - distributed-systems
  - routing
---
# Orleans Distributed Bundle

Agent uses this bundle for Orleans requests that span grain modeling, collaboration patterns, and Aspire-hosted local orchestration.

## Activation

| Request Signal | Use Bundle | Route Detail |
|---|---|---|
| Need grain identities, state, clustering, or persistence | Yes | Agent routes to `orleans-virtual-actors` |
| Need streams, observers, reminders, or coordinator patterns | Yes | Agent routes to `orleans-patterns` |
| Need Aspire resource integrations inside Orleans services | Yes | Agent routes to `aspire-components` |
| Need AppHost topology, service discovery, or dashboard wiring | Yes | Agent routes to `aspire-orchestration` |
| Need one narrow Orleans concern with no routing choice | No | Agent uses the direct specialist skill |

## Coverage Matrix

| Need | Primary Skill | Primary Outcome |
|---|---|---|
| Grain contracts and persistence | `orleans-virtual-actors` | Stable virtual-actor model |
| Streams, observers, reminders, and coordination | `orleans-patterns` | Explicit collaboration pattern |
| Dependency components | `aspire-components` | Typed resource clients and health wiring |
| AppHost topology | `aspire-orchestration` | Local distributed run path |
| Architecture alignment | `dotnet-aspire-vbd-development` | Boundary-aware distributed design |

## Decision Order

| Decision | Agent Action |
|---|---|
| Grain identity and state come first | Agent chooses the virtual-actor model before collaboration details. |
| Collaboration pattern comes second | Agent selects direct calls, streams, observers, timers, or reminders next. |
| Resource integration comes third | Agent adds Aspire components after grain contracts are stable. |
| Local orchestration comes fourth | Agent wires AppHost, discovery, and dashboard last. |
| Cross-boundary architecture review stays active | Agent keeps service and grain boundaries explicit throughout routing. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify the Orleans scope | Agent maps grain, collaboration, component, and orchestration needs. | Agent lists routed skills. | Every concern maps to one primary skill. |
| 2. Select the core actor model | Agent records grain identity, state, and placement assumptions. | Review the actor model. | One primary grain model exists. |
| 3. Select the collaboration path | Agent records call, stream, observer, timer, or reminder ownership. | Review interaction flow. | Collaboration path is explicit. |
| 4. Add Aspire surfaces | Agent routes dependencies and AppHost concerns only where the runtime needs them. | Review hosting scope. | Aspire additions stay intentional. |
| 5. Verify the fit | Agent compares the routed set to state, latency, and operations needs. | Review the final route set. | Route set matches the distributed workload. |

## Verification Matrix

| Area | Agent Verifies | Test | Pass |
|---|---|---|---|
| Grain model | Identity and persistence match business ownership. | Inspect grain contracts and state. | Grain model fits the entity boundary. |
| Collaboration | Streams, observers, and reminders match delivery semantics. | Inspect message and recurrence flow. | Interaction pattern fits the workload. |
| Aspire integration | Resource names, references, and health wiring stay aligned. | Inspect AppHost and service startup. | Resource wiring is consistent. |
| Orchestration | Local run path exposes logs, traces, and dependencies. | Inspect dashboard and resource graph. | Operators have one clear local diagnosis path. |
| Architecture | Service and grain seams stay explicit. | Inspect boundary ownership. | Distributed boundaries stay intentional. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Designing streams before grain identity is stable | Agent fixes the grain model first. |
| Mixing reminder and timer semantics | Agent chooses durable versus active-only recurrence explicitly. |
| Adding Aspire resources with no Orleans ownership boundary | Agent binds each resource to one service or grain-facing path. |
| Treating local AppHost topology as production deployment design | Agent limits Aspire routing to local orchestration intent. |
| Letting grains absorb unrelated infrastructure concerns | Agent keeps hosting concerns in Aspire layers and business concerns in grains. |

