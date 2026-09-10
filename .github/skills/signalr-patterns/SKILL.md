---
name: signalr-patterns
title: ASP.NET Core SignalR Patterns
description: Implement ASP.NET Core SignalR hubs with strong contracts, connection lifecycle handling, group patterns, and scale-out guidance for Redis and Azure-hosted fan-out.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1476
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - dotnet-webapi
related_skills:
  - webapi-authz-hardening
  - configuring-opentelemetry-dotnet
  - quartz-scheduling-patterns
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - signalr
  - realtime
  - websockets
  - redis
---
# ASP.NET Core SignalR Patterns

Agent designs SignalR hubs with typed contracts, predictable connection behavior, and production-ready fan-out patterns.

## When to Use

| Condition | Use |
|---|---|
| Work adds real-time notifications, dashboards, chat, progress streams, or presence | Use this skill |
| Work needs hub design, group targeting, user targeting, or out-of-hub publishing | Use this skill |
| Work scales realtime delivery across multiple nodes | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work needs one-way server push with no hub RPC contract | Use SSE or streaming HTTP patterns |
| Work documents REST endpoints only | Use OpenAPI documentation skills |
| Work needs durable queue processing or delayed delivery guarantees | Use messaging patterns first and add SignalR for live fan-out |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Interaction model | Yes | Record broadcast, caller, user, group, or request-response flow. |
| Identity model | Yes | Record anonymous, cookie, bearer, or custom user identifier rules. |
| Scale target | Yes | Record single node, Redis backplane, Azure SignalR Service, or event-ingress topology. |
| Delivery contract | No | Record ordering, idempotency, reconnect replay, and payload version rules. |

## Pattern Matrix

| Scenario | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Single-node app or local development | Direct SignalR hosting | Keep hub logic thin and state external | Typed delivery works with no backplane |
| Multi-node app on self-managed infrastructure | Redis backplane | Record sticky-session requirements and stable group keys | Cross-node delivery reaches the intended audience |
| Multi-node Azure deployment with high connection counts | Azure SignalR Service | Treat Azure SignalR Service as the backplane role | Managed fan-out matches deployment topology |
| Cross-service events arrive through messaging | Hosted service plus `IHubContext` publisher | Treat Service Bus as ingress, not the backplane | Event relay reaches the intended clients |
| Per-user targeting across reconnects | `Context.UserIdentifier` or custom `IUserIdProvider` | Avoid long-lived identity based on connection IDs | Reconnect preserves user targeting |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent defines a typed client contract and a thin hub surface. | Search for `Hub<` and client interfaces in scope. | Outbound calls use typed contracts. |
| 2 | Agent implements explicit connect, join, leave, and disconnect flow. | Read `OnConnectedAsync`, `OnDisconnectedAsync`, and membership methods. | Lifecycle code updates groups and transient state intentionally. |
| 3 | Agent routes broadcast, user, connection, and group delivery through the correct target. | Review hub methods and `IHubContext` publishers. | Delivery target matches the intended audience. |
| 4 | Agent keeps durable presence and business state outside hub instances. | Review hub fields and injected services. | Hub instances stay stateless beyond `Context.Items`. |
| 5 | Agent wires Redis, Azure SignalR Service, or event-ingress infrastructure explicitly. | Search startup and background publisher code. | Selected topology appears in startup and publisher paths. |
| 6 | Agent validates build, representative hub flows, and background publishing. | Run targeted build and realtime tests. | Build passes and representative flows pass. |

## Rules

| Topic | Rule |
|---|---|
| Hub lifetime | Treat hubs as transient. Store durable state in services or data stores. |
| Client invocation | Prefer strongly typed hubs over raw method-name strings. |
| Groups | Use stable, namespaced group keys with explicit join and leave methods. |
| Identity | Target users through user identifiers instead of cached connection IDs. |
| Publishing | Use `IHubContext` from controllers, workers, and message handlers. |
| Reliability | Add idempotency or replay logic in application services when reconnect gaps matter. |
| Topology terms | Treat Redis and Azure SignalR Service as backplane choices. Treat Service Bus as upstream ingress. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Typed contracts | Search for typed hubs and client interfaces. | Outbound client calls use typed interfaces. |
| Thin hubs | Review hub methods. | Hubs orchestrate and delegate instead of owning durable business state. |
| Group flows | Run narrow realtime or integration tests. | Join, leave, and targeted delivery flows succeed. |
| Lifecycle cleanup | Review disconnect handling. | Disconnect paths remove transient presence safely. |
| Scale path | Review startup and infrastructure wiring. | Topology matches deployment shape. |
| Background publishing | Review `IHubContext` publishers. | Out-of-hub notifications reach the intended clients. |

## Outputs

- Typed hub contract and DTO plan
- Connection lifecycle and group membership plan
- Scale-out selection with startup wiring
- Background publishing pattern for controllers, workers, or handlers
- Verification steps for build and representative hub flows

## Reference Files

- [SignalR examples reference](references/examples.md)
