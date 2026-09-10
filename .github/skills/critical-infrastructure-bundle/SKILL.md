---
name: critical-infrastructure-bundle
title: Critical Infrastructure Bundle
description: Route critical infrastructure design and implementation requests to the correct platform skill and keep cross-technology decisions consistent.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: low
estimated_tokens: 1090
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - graphql-dotnet
  - api-gateway-patterns
  - event-sourcing-patterns
  - blazor-patterns
  - docker-dotnet
  - kubernetes-dotnet
appliesTo: '**/*.{cs,csproj,sln,slnx,json,yml,yaml,Dockerfile,md}'
tags:
  - infrastructure
  - architecture
  - routing
  - platform
---
# Critical Infrastructure Bundle

Agent uses this bundle for requests that choose, combine, or implement core platform patterns across API shape, state model, UI stack, packaging, and orchestration.

## When to Use

| User prompt | Use |
|---|---|
| User asks for GraphQL, gateway, event sourcing, Blazor, Docker, or Kubernetes guidance | Use this bundle |
| User asks which platform pattern fits a .NET system | Use this bundle |
| User asks for one coordinated pass across several infrastructure technologies | Use this bundle |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for one narrow implementation detail inside an existing chosen stack | Route to the named specialist skill |
| User asks for generic Web API hardening or endpoint cleanup | Route to `webapi-hardening-controller` or matching Web API skill |
| User asks for non-.NET infrastructure outside this bundle scope | Route to the matching platform skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| System goal | Yes | API, UI, deployment, or event-flow outcome |
| Current stack | Yes | Existing .NET app type, hosting model, and data shape |
| Target scale | No | Team size, traffic shape, and runtime footprint |
| Constraints | No | Latency, offline support, ops maturity, or contract rules |

## Activation

| Signal | Use Bundle | Route Detail |
|---|---|---|
| API contract choice is open | Yes | Classify `graphql-dotnet` or `api-gateway-patterns` |
| State model choice is open | Yes | Classify `event-sourcing-patterns` or CRUD baseline |
| .NET UI stack choice is open | Yes | Classify `blazor-patterns` |
| Packaging or hosting model is open | Yes | Classify `docker-dotnet` or `kubernetes-dotnet` |
| Request spans two or more listed technologies | Yes | Coordinate order and integration edges |
| Technology set is fixed and only bug fixing is needed | No | Use the active specialist skill directly |

## Coverage Matrix

| Specialist Skill | Primary Scope | Entry Trigger | Key Output |
|---|---|---|---|
| `graphql-dotnet` | GraphQL APIs and schema design | Flexible query shape, aggregation, client-driven selection | Schema, resolvers, contract rules |
| `api-gateway-patterns` | YARP, Ocelot, API Management | Edge routing, aggregation, auth, policy, throttling | Gateway topology and route policy |
| `event-sourcing-patterns` | Event sourcing and CQRS/ES | Audit trail, temporal replay, append-only domain flow | Event model and projection flow |
| `blazor-patterns` | Blazor Server, WASM, Web App | .NET-first UI delivery and component reuse | Hosting mode and component pattern |
| `docker-dotnet` | Container packaging for .NET apps | Portable runtime, build parity, image hardening | Dockerfile and container runtime pattern |
| `kubernetes-dotnet` | Kubernetes orchestration | Multi-service scheduling, scaling, rollout control | Workload, config, and service topology |

## Decision Order

| Order | Decision | Agent Checks First | Primary Route |
|---|---|---|---|
| 1 | Client contract model | Query flexibility, endpoint count, client ownership | `graphql-dotnet` or REST baseline |
| 2 | Domain state model | Audit depth, replay needs, write complexity | `event-sourcing-patterns` or CRUD baseline |
| 3 | UI stack | .NET reuse, interactivity, SPA investment | `blazor-patterns` or existing Angular/Vue path |
| 4 | Packaging | Environment parity, dependency isolation, release flow | `docker-dotnet` or native deployment |
| 5 | Orchestration | Service count, autoscaling, ops maturity | `kubernetes-dotnet` or simpler hosting |
| 6 | Edge topology | Cross-service ingress, policy centralization | `api-gateway-patterns` |

## Technology Selection Matrix

| Decision | Select Left When | Select Right When | Primary Concern |
|---|---|---|---|
| GraphQL vs REST | Clients need selective fields, nested reads, one schema surface | Contracts stay resource-oriented, cache-friendly, and simple | Contract shape and client autonomy |
| Event Sourcing vs CRUD | Domain needs audit replay, temporal history, rich write intent | State changes stay simple and current-state storage fits | Write model complexity |
| Blazor vs Angular/Vue | Team wants .NET-first UI, shared C# models, server or hybrid rendering | Team needs established JS SPA ecosystem or existing Angular/Vue investment | Team fit and UI runtime model |
| Docker vs native deployment | Runtime parity, dependency isolation, image-based release flow matter | Host environment is stable and container overhead adds no value | Packaging consistency |
| Kubernetes vs simpler hosting | Many services need scaling, rollout control, self-healing, service discovery | Few services fit app service, VM, or container app hosting | Operational complexity |

## Integration Patterns

| Combination | Use Pattern | Key Checks |
|---|---|---|
| GraphQL + Kubernetes | Run GraphQL gateway or API pods behind ingress with horizontal scale | Sticky-state avoidance, cache strategy, probe coverage |
| GraphQL + API Gateway | Put auth, rate limits, and edge routing in gateway; keep schema logic in GraphQL layer | Avoid duplicate aggregation rules |
| Event Sourcing + Docker | Package event store client, projection workers, and consumers with identical runtime images | Durable volume and startup ordering |
| Event Sourcing + Kubernetes | Split writers, projectors, and consumers into separate workloads | Idempotent consumers and projection recovery |
| Blazor + Docker | Package Server or Web App hosts with fixed runtime dependencies | Static asset compression and config injection |
| Blazor + Kubernetes | Scale interactive server workloads and colocated APIs independently | Session model, circuit resiliency, ingress timeouts |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies requested technologies and open decisions. | Agent records one decision map. | Every requested area maps to one row in this bundle. |
| 2 | Agent selects the first unresolved decision from the decision order table. | Agent records active decision order position. | Work starts with the earliest unresolved decision. |
| 3 | Agent routes each decision to one specialist skill or baseline alternative. | Agent records one route per decision. | Every in-scope decision has one route. |
| 4 | Agent applies integration patterns for combined technologies. | Agent records every technology pair in scope. | Every active pair has one integration rule. |
| 5 | Agent verifies code, config, and deployment artifacts with existing commands. | Run existing build, test, or deployment validation in scope. | Zero new errors in changed scope. |

## Verification Matrix

| Area | Agent Verifies | Test | Pass |
|---|---|---|---|
| Routing accuracy | The chosen specialist skill matches the real decision point. | Compare request language to coverage matrix. | One clear skill route exists per decision. |
| Contract fit | API choice matches consumer access pattern. | Inspect client query needs and endpoint contract. | Selected contract shape fits the stated client need. |
| State fit | Persistence model matches audit and replay needs. | Inspect write flow and reporting needs. | Selected state model fits domain history needs. |
| UI fit | UI stack matches team and runtime constraints. | Inspect hosting, interactivity, and skill profile. | Selected UI path fits delivery constraints. |
| Packaging fit | Packaging model matches release and dependency needs. | Inspect runtime parity and deployment flow. | Selected packaging path supports repeatable deployment. |
| Orchestration fit | Hosting model matches service count and ops maturity. | Inspect scale and control-plane needs. | Selected hosting path fits operational complexity. |

## Verification Checklist

- [ ] Agent created `.github/skills/critical-infrastructure-bundle/SKILL.md`.
- [ ] Agent added full frontmatter with `complexity: low`.
- [ ] Agent kept `estimated_tokens` under 1200.
- [ ] Agent included activation, coverage, decision, selection, integration, workflow, verification, and pitfalls tables.
- [ ] Agent kept wording free of modal verbs.

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| GraphQL is selected for every API | Agent checks real client-driven query pressure first |
| Event sourcing is selected for simple CRUD workflows | Agent uses CRUD baseline when replay and temporal history add no value |
| Blazor is selected without checking current front-end investment | Agent compares existing Angular or Vue investment before routing |
| Docker and Kubernetes are selected together by default | Agent selects Kubernetes only after packaging value and orchestration value are both present |
| Gateway logic duplicates GraphQL or app logic | Agent keeps policy at the edge and domain logic inside the service |
