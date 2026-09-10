---
name: blazor-patterns
title: Blazor Patterns
description: >
  Design Blazor applications when the task needs hosting-model selection, component interaction, 
  rendering control, and .NET 10 Blazor implementation patterns.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1779
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fluent2-design-comprehensive
  - dotnet-webapi
  - webapp-frontend-security
  - aspnetcore-identity-setup
related_docs:
  - references/fluent2-implementation-patterns.md
  - https://www.fluentui-blazor.net/
appliesTo: '**/*.{cs,csproj,razor,js,json,md}'
tags:
  - blazor
  - fluent
  - fluent2
  - components
  - webapp
  - rendering
  - dotnet
---
# Blazor Patterns

Agent designs Blazor applications with explicit hosting choices, component boundaries, render behavior, and validation strategy.

## When to Use

| Condition | Use |
|---|---|
| Task defines or revises a Blazor Server, WebAssembly, or Web App application | Use this skill |
| Task needs component lifecycle, rendering, or state-management guidance | Use this skill |
| Task adds authentication, forms, JS interop, or prerendering to Blazor UI | Use this skill |
| Task evaluates performance, real-time updates, or component testing in Blazor | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| UI stack is Angular, Vue, or React | Use the matching frontend skill |
| Task focuses on server API contracts only | Use `dotnet-webapi` |
| App needs extensive offline-first browser logic as the primary architecture driver | Use a dedicated SPA pattern review |
| Task concerns WPF desktop composition | Use the matching WPF skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Hosting constraints | Yes | Latency, scale, offline, SEO, deployment |
| Render-mode target | Yes | Static SSR, Interactive Server, Interactive WebAssembly, or Interactive Auto |
| State-sharing scope | Yes | Component, page, circuit, or browser session |
| Security model | Yes | Auth flow, claims, and protected API access |
| Test baseline | No | Existing bUnit, integration, or end-to-end coverage |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects the hosting model and render mode from the decision matrix before component work starts. | Review app constraints against the hosting matrix. | Selected model aligns with latency, SEO, offline, and scale goals. |
| 2 | Agent defines component boundaries, parameters, events, and lifecycle-sensitive logic. | Review component contract surface. | Data flow stays explicit and rendering side effects stay bounded. |
| 3 | Agent chooses state and communication patterns for the required scope. | Review state container and component interaction design. | Shared state has one clear owner and update path. |
| 4 | Agent adds forms, validation, auth, interop, and prerender-safe behavior. | Run `dotnet build [project].csproj`. | Build succeeds with zero compile errors. |
| 5 | Agent applies performance patterns, lazy loading, virtualization, and SignalR integration where the feature set needs them. | Run existing UI or integration tests. | Rendering remains responsive and real-time updates stay stable. |
| 6 | Agent validates component behavior with existing bUnit, integration, or end-to-end tests. | Run existing Blazor test commands. | Tests confirm rendering, validation, auth flow, and interop boundaries. |

## Hosting Model Decision Matrix

| Factor | Blazor Server | Blazor WebAssembly | Blazor Web App |
|---|---|---|---|
| First interaction | Fast after circuit connection | Browser download cost first | Flexible per page or component |
| SEO and prerender | Strong with server rendering | Weak without extra SSR path | Strong with unified SSR plus interactive render modes |
| Offline behavior | Minimal | Strong | Mixed by chosen render mode |
| Server resource load | Higher per connected user | Lower steady-state UI load | Mixed by interactive mode choice |
| Browser API depth | Interop-heavy path | Direct browser-side execution | Mixed by render mode |
| Best fit | Internal apps with strong server affinity | Rich client apps with browser-local execution | New .NET 8+ and .NET 10 apps that need one composition model |

## State and Communication Matrix

| Need | Preferred pattern | Avoid |
|---|---|---|
| Parent-to-child data flow | Parameters with immutable inputs where practical | Hidden mutation through shared mutable objects |
| Child-to-parent notifications | `EventCallback` and explicit event payloads | Deep component references for routine communication |
| Tree-wide shared state | Cascading values for ambient context and a scoped state container for mutable shared state | Cascading mutable state with no owner |
| Cross-page state | Scoped service state container with clear reset rules | Static state tied to the process |
| JavaScript interop | Thin typed wrapper service and disposal-aware object references | Inline ad-hoc JS calls spread across components |

## Rendering, Prerendering, and Performance Matrix

| Concern | Preferred pattern | Notes |
|---|---|---|
| Lifecycle work | Fetch async data in lifecycle methods with cancellation awareness | Keep render loops side-effect free |
| Prerendering | Guard browser-only interop until interactive render starts | Keep prerender path free of DOM assumptions |
| Large lists | `Virtualize` and incremental data loading | Pair with stable item keys |
| Lazy loading | Split assemblies or feature routes with deferred activation | Focus on rarely used feature areas |
| Real-time UI | SignalR hub updates routed through state containers | Coalesce bursts to avoid render storms |
| Forms | `EditForm`, data annotations, and custom validators for domain rules | Duplicate validation logic across client and server |
| .NET 10 pattern | Blazor Web App with explicit render modes and testable service boundaries | Keep interactive choice local to the feature |

## Verification Checklist

Agent verifies:
- [ ] Hosting model and render mode align with the app constraints
- [ ] Component parameters and callbacks describe the full interaction surface
- [ ] Shared state has one owner and deterministic reset behavior
- [ ] JS interop stays behind typed services and interactive guards
- [ ] Forms validate on both UI and server boundaries
- [ ] Auth state and API authorization stay consistent across render modes
- [ ] Virtualization or lazy loading covers large or expensive UI surfaces
- [ ] Existing bUnit or integration tests cover rendering, validation, and auth-sensitive paths

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Hosting model choice follows habit instead of constraints | Re-evaluate with the decision matrix |
| Component logic mutates shared state from many locations | Centralize state ownership in one container |
| JS interop runs during prerender | Move browser-dependent work to interactive-safe lifecycle paths |
| Server and client validation rules drift apart | Share validation contracts and re-run existing tests |
| Real-time updates trigger excessive rerendering | Batch state changes and use stable component boundaries |
