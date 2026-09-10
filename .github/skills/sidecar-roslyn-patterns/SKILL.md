---
name: sidecar-roslyn-patterns
title: Sidecar Roslyn Patterns
description: Routes compile-time sidecar work to the correct Roslyn, proxy, resilience, and observability skills when a .NET service needs generated cross-service plumbing.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 1090
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - roslyn-source-generator-authoring
related_skills:
  - roslyn-source-generator-authoring
  - roslyn-codegen-patterns
  - dotnet-api-client-generation
  - dotnet-api-client-resilience
  - configuring-opentelemetry-dotnet
  - dotnet-telemetry-standards
appliesTo: '**/*.{cs,csproj,props,targets,json,md,yml,yaml}'
tags:
  - sidecar
  - roslyn
  - source-generators
  - observability
  - resilience
  - routing
---
# Sidecar Roslyn Patterns

Agent uses this router when a .NET service needs compile-time sidecar behavior through Roslyn generation instead of a standalone runtime sidecar.

## Activation

| Request Signal | Use Skill | Route Detail |
|---|---|---|
| Need generated HTTP or gRPC proxies from contracts | Yes | Agent routes to `dotnet-api-client-generation` and `roslyn-codegen-patterns` |
| Need generated retry, timeout, or circuit-breaker wrappers | Yes | Agent routes to `dotnet-api-client-resilience` |
| Need generated traces, metrics, or log envelopes | Yes | Agent routes to `configuring-opentelemetry-dotnet` and `dotnet-telemetry-standards` |
| Need incremental generator design, diagnostics, or deterministic output | Yes | Agent routes to `roslyn-source-generator-authoring` |
| Need polyglot runtime mesh, transparent traffic interception, or live policy changes | No | Agent routes to platform mesh guidance outside this skill |

## Coverage Matrix

| Need | Primary Skill | Primary Outcome |
|---|---|---|
| Generator trigger and syntax pipeline | `roslyn-source-generator-authoring` | Deterministic incremental generator design |
| Shared emitted contract helpers | `roslyn-codegen-patterns` | Stable generated file and API shape |
| Typed service proxy emission | `dotnet-api-client-generation` | Contract-driven HTTP or gRPC clients |
| Resilience policy injection | `dotnet-api-client-resilience` | Generated retry, timeout, and breaker behavior |
| Trace, metric, and log injection | `configuring-opentelemetry-dotnet` | Generated telemetry hooks |
| Telemetry naming and evidence conventions | `dotnet-telemetry-standards` | Stable observability contract |

## Decision Order

| Decision | Agent Action |
|---|---|
| Contract surface comes first | Agent fixes interface, attribute, and namespace ownership before emission design. |
| Transport choice comes second | Agent assigns HTTP, gRPC, or mixed transport before resilience or telemetry hooks. |
| Generator boundary comes third | Agent decides which features emit at compile time and which remain runtime configuration. |
| Observability and resilience come next | Agent applies shared conventions after the transport and method model are stable. |
| Runtime mesh tradeoff comes last | Agent keeps this pattern for C#-first compile-time ownership only. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Define the contract | Agent records the interface, attribute, or partial-type trigger. | Review the public trigger surface. | One explicit trigger model exists. |
| 2. Select emitted artifacts | Agent records proxies, registration helpers, diagnostics, and telemetry wrappers. | Review the artifact map. | Each artifact has one owner skill. |
| 3. Apply the generator path | Agent routes generator implementation to the Roslyn skills. | Review incremental pipeline and diagnostics. | Output stays deterministic. |
| 4. Apply service concerns | Agent routes resilience and telemetry to their specialist skills. | Review emitted wrappers and naming rules. | Cross-service plumbing stays consistent. |
| 5. Verify the tradeoff | Agent compares compile-time sidecar behavior to runtime mesh needs. | Review language mix and policy-change frequency. | Compile-time ownership fits the platform. |

## Verification Matrix

| Area | Agent Verifies | Test | Pass |
|---|---|---|---|
| Determinism | Generator output stays stable across identical inputs. | Run generation twice. | Output stays identical. |
| Proxy correctness | Generated clients match contract signatures and transport rules. | Compile and inspect generated clients. | Signatures and serialization align. |
| Resilience | Generated policy wrappers map to declared profiles. | Review emitted policy registration. | Retry, timeout, and breaker hooks stay explicit. |
| Observability | Generated traces, metrics, and logs follow one convention. | Inspect emitted telemetry names and tags. | Call paths share one telemetry contract. |
| Scope fit | Compile-time sidecar stays inside C#-first service concerns. | Review remaining runtime concerns. | Traffic-mesh concerns stay out of scope. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Generator design starts before contract ownership is stable | Agent fixes the trigger surface first. |
| Emitted code mixes resilience naming from one source and telemetry naming from another | Agent routes both through one shared convention set. |
| Compile-time design tries to replace every runtime mesh concern | Agent limits scope to application-facing plumbing. |
| Generator output hides serializer or endpoint selection rules in string parsing | Agent emits typed settings and explicit method metadata. |
| Cross-language estate adopts a C#-only sidecar by accident | Agent redirects that case to platform mesh guidance. |

