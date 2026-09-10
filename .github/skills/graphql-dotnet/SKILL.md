---
name: graphql-dotnet
title: GraphQL .NET Patterns
description: Design and implement GraphQL APIs in .NET with Hot Chocolate, optimized schema patterns, security controls, and client integration guidance.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1705
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - dotnet-webapi
related_skills:
  - signalr-patterns
  - webapi-authz-hardening
  - jwt-authentication-dotnet
  - dotnet-api-client-generation
  - dotnet-api-client-resilience
  - configuring-opentelemetry-dotnet
  - rfc-9457-compliance
appliesTo: '**/*.{cs,csproj,sln,slnx,json,yml,yaml}'
tags:
  - graphql
  - hot-chocolate
  - api
  - dotnet
  - signalr
---
# GraphQL .NET Patterns

Use this skill to design, implement, or refactor GraphQL APIs in .NET with explicit schema contracts, bounded resolver behavior, and production-ready client integration.

## When to Use

| User prompt | Use |
|---|---|
| User adds a GraphQL endpoint, schema, resolver, or subscription flow in .NET. | Use this skill. |
| User asks for Hot Chocolate setup, schema design, DataLoader usage, or N+1 mitigation. | Use this skill. |
| User compares GraphQL and REST for one feature or one bounded domain. | Use this skill. |
| User adds GraphQL clients with StrawberryShake or `GraphQL.Client`. | Use this skill. |

## When Not to Use

| User prompt | Route |
|---|---|
| User exposes only fixed CRUD endpoints with simple filtering and no client-driven shape. | Use REST guidance first. |
| User needs document-store query language design outside HTTP API scope. | Use the data-access skill that matches the store. |
| User asks for SignalR hub design with no GraphQL contract. | Use `signalr-patterns`. |
| User needs OpenAPI-only documentation or controller route cleanup. | Use Web API skills. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Domain boundary | Yes | Record the aggregate roots, ownership boundaries, and caller personas. |
| Operation mix | Yes | Record query, mutation, subscription, or mixed usage. |
| Auth model | Yes | Record anonymous, user, role, policy, or tenant-scoped access. |
| Data access shape | Yes | Record EF Core, Dapper, HTTP clients, or message-driven backends. |
| Client type | No | Record web, mobile, BFF, or server-to-server clients. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Choose GraphQL, REST, or hybrid shape from caller query needs. | One API style matches the feature boundary. |
| 2 | Configure Hot Chocolate registration, schema modules, and transport endpoints. | Startup reflects the intended query, mutation, and subscription mix. |
| 3 | Define explicit object, input, enum, and payload contracts. | Schema avoids leaking persistence models. |
| 4 | Add DataLoader, projection, filtering, paging, or batching where resolver fan-out exists. | High-fan-out paths use a deliberate optimization strategy. |
| 5 | Apply authorization, validation, and sanitized error handling, then verify representative operations. | Build passes and changed flows behave correctly. |


## API Shape Decision Matrix

| Scenario | Use | Tradeoff |
|---|---|---|
| Caller needs one round trip with field selection across related resources | GraphQL queries | Query-cost governance enters the scope. |
| Contract stays fixed and cache-friendly | REST | Caller loses shape flexibility. |
| Public REST plus internal UI composition | Hybrid design | Team maintains two surface styles. |
| Command-style mutation with explicit response | GraphQL mutation with input and payload types | Validation and payload design need care. |
| Real-time UI needs pushed domain events | Subscriptions | Connection lifecycle and scale-out matter. |


## Schema and Resolver Matrix

| Topic | Pattern | Avoid |
|---|---|---|
| Server framework | Hot Chocolate | Mixed server stacks in one feature |
| Contracts | Explicit object, input, enum, and payload types | Direct EF entity exposure |
| Query roots | Bounded fields with clear nullability | Catch-all query fields |
| Mutations | Typed payloads with domain data and validation details | Bare `bool` success flags |
| Data access | Thin resolvers that delegate to services or repositories | Nested resolver I/O chains |
| N+1 control | DataLoader or pre-batched service calls | Per-item child fetches in loops |
| Validation | Validate before side effects and return deterministic errors | Raw exception text |


## Pagination and Federation Matrix

| Need | Pattern |
|---|---|
| Infinite scroll or live list growth | Cursor paging with deterministic ordering |
| Admin grids with bounded page numbers | Offset paging |
| One gateway composes downstream GraphQL services | Schema stitching |
| Many teams own separate subgraphs | Federation |
| Subscription delivery through ASP.NET Core | Subscriptions with aligned transport, auth, and scale topology |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Server and schema registration | Search for `AddGraphQLServer`, `MapGraphQL`, and changed type registrations. | One intentional GraphQL pipeline exists with explicit contracts. |
| Query optimization and auth | Inspect resolvers, DataLoader use, paging, and authorization rules. | Hot paths are batched or paged and protected fields declare access intent. |
| Error, client, and realtime flow | Run the narrowest build plus representative query, mutation, subscription, and client tests. | External responses stay sanitized and changed operations succeed. |
| Observability | Inspect traces, metrics, or request logs. | Slow operations and N+1 indicators remain visible. |


## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Resolver code performs one database or HTTP call per child item. | Add DataLoader or batched service access. |
| Schema exposes EF entities directly. | Agent writes dedicated GraphQL contracts and keeps persistence models internal. |
| Mutation returns `bool` only. | Return a typed payload with domain data and validation details. |
| Authorization lives at the HTTP endpoint only. | Add field, type, or resolver-level authorization where sensitive data appears. |
| Cursor paging uses unstable sort order. | Add deterministic ordering and opaque cursor encoding. |
| GraphQL replaces every REST endpoint without caller-driven value. | Use the API shape matrix and keeps REST where resource semantics fit better. |
| Subscription transport ignores connection scale topology. | Align subscriptions with SignalR or managed realtime infrastructure in scope. |

## Outputs

- Hot Chocolate server and schema plan
- Query, mutation, subscription, paging, and DataLoader guidance
- Authorization and sanitized error-handling plan
- Client integration and observability checks

