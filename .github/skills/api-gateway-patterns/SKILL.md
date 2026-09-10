---
name: api-gateway-patterns
title: API Gateway Patterns
description: Design and implement API gateway layers with YARP, Ocelot, Azure API Management, and resilient .NET traffic-control patterns.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1645
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - dotnet-webapi
related_skills:
  - api-infrastructure-bundle
  - middleware-authoring-aspnetcore
  - rate-limiting-aspnetcore
  - webapi-authz-hardening
  - dotnet-api-client-resilience
  - configuring-opentelemetry-dotnet
  - cors-configuration-aspnetcore
appliesTo: '**/*.{cs,csproj,sln,slnx,json,yml,yaml,config}'
tags:
  - api-gateway
  - yarp
  - ocelot
  - apim
  - dotnet
---
# API Gateway Patterns

Agent designs gateway layers with explicit routing, bounded transforms, and observable traffic controls.

## When to Use

| Condition | Use |
|---|---|
| Work adds a reverse proxy, edge gateway, or BFF for multiple downstream APIs | Use this skill |
| Work asks for YARP, Ocelot, or Azure API Management design | Use this skill |
| Work centralizes auth delegation, throttling, transformation, or aggregation at ingress | Use this skill |
| Work compares gateway and service-mesh responsibilities | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work changes one API with no cross-service ingress layer | Edit the API directly |
| Work needs east-west traffic policy inside the cluster only | Use service-mesh or platform networking guidance |
| Work adds static-file or CDN rules with no API routing logic | Use the matching edge-configuration path |
| Work hardens one outbound HTTP client inside an app service | Use `dotnet-api-client-resilience` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Downstream inventory | Yes | Record services, route groups, protocols, and ownership boundaries. |
| Traffic goals | Yes | Record routing, aggregation, throttling, auth delegation, or transformation goals. |
| Deployment topology | Yes | Record self-hosted ASP.NET Core, Azure-hosted edge, container platform, or hybrid topology. |
| Failure posture | Yes | Record timeout budget, breaker thresholds, retry ownership, and fallback intent. |
| Consumer set | No | Record web, mobile, partner, internal, or admin clients. |

## Pattern Matrix

| Scenario | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| In-process .NET reverse proxy with custom middleware | YARP | Keep policy code bounded and observable | Route and transform logic stay explicit in the gateway service |
| Config-centric gateway with built-in aggregation conventions | Ocelot | Keep extension scope narrow | Route and aggregation config match the intended contract |
| Managed external edge with subscriptions, products, and portal | Azure API Management | Keep platform and app responsibilities separate | Edge policies live in APIM with clear ownership |
| Managed external edge plus internal BFF composition | APIM plus YARP | Record which layer owns auth, throttling, and transforms | Cross-layer policy stays non-duplicative |
| Public ingress plus internal service policy | Gateway plus service mesh | Keep north-south and east-west ownership explicit | Gateway owns ingress and mesh owns service-to-service policy |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects YARP, Ocelot, APIM, or a layered combination. | Review deployment and policy needs. | One explicit gateway stack exists for the scope. |
| 2 | Agent defines route matching, destination clusters, health probes, and load balancing. | Review route and cluster configuration. | Ingress paths map to the intended downstream targets. |
| 3 | Agent applies transforms, aggregation, and BFF composition only where the caller contract needs them. | Inspect transform and aggregator code or config. | Gateway logic stays contract-focused and bounded. |
| 4 | Agent wires auth delegation, rate limiting, and resilience ownership intentionally. | Review auth, limiter, timeout, and breaker configuration. | Cross-cutting policies appear once in the chosen layer. |
| 5 | Agent adds observability for route selection, latency, breaker state, and rejection outcomes. | Inspect logs, traces, metrics, and health endpoints. | Traffic and failure states remain observable. |
| 6 | Agent validates routing, throttling, aggregation, and failure behavior. | Run targeted build and gateway tests. | Build passes and representative ingress flows pass. |

## Rules

| Topic | Rule |
|---|---|
| Boundary ownership | Keep business logic in downstream services. Keep ingress, composition, and cross-cutting policy in the gateway. |
| Route transforms | Use request and response transforms only when the contract requires them. |
| Retry safety | Avoid automatic retries for non-idempotent requests unless the design defines idempotency. |
| Aggregation | Aggregate a small set of latency-related calls with explicit failure semantics. |
| Policy duplication | Avoid conflicting auth or quota enforcement across APIM, YARP, and downstream APIs. |
| Observability | Propagate correlation and capture downstream timing and failure classification. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Route coverage | Call representative upstream routes. | Requests reach the intended downstream service or cluster. |
| Load balancing | Send repeated requests across healthy destinations. | Distribution follows the selected policy. |
| Transform correctness | Inspect outbound headers, path, query, and body shape. | Downstream receives the intended transformed contract. |
| Rate limiting | Exceed one client-facing quota path. | Excess requests return the intended rejection result. |
| Auth delegation | Call protected routes with valid, missing, and insufficient credentials. | Gateway returns the intended auth outcome and forwards only intended identity context. |
| Breaker and timeout behavior | Simulate slow or failed downstream responses. | Gateway short-circuits or rejects per configuration. |
| Observability | Inspect logs, traces, metrics, and health signals. | Route selection, latency, and failures stay observable. |

## Outputs

- Gateway stack selection for YARP, Ocelot, APIM, or a layered design
- Route, cluster, and load-balancing plan
- Transformation, aggregation, and BFF contract plan
- Auth delegation, throttling, and resilience ownership plan
- Verification evidence for routing, failure behavior, and observability
