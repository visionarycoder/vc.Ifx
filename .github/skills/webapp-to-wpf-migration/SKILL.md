---
name: webapp-to-wpf-migration
title: WebApp to WPF Migration
description: Migrate web application features to WPF through screen-by-screen mapping of components, contracts, state, validation, and desktop-specific UX behavior.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1215
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - angular-to-vue3-migration
  - wpf-rest-client-integration
  - wpf-screen-generation
appliesTo: '**/*.{ts,html,scss,cs,xaml,json}'
tags:
  - migration
  - angular
  - wpf
  - desktop
---
# WebApp to WPF Migration

Agent migrates one web feature at a time into WPF by preserving contracts, re-mapping presentation patterns, and adapting UX for desktop workflows.

## When to Use

| Condition | Use |
|---|---|
| Agent moves an existing web feature into a WPF client. | Agent uses this skill. |
| Agent keeps the API contract stable while changing the client technology. | Agent uses this skill. |
| Agent needs one pilot screen that proves migration feasibility. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent keeps the feature on the web and changes only the framework. | Agent uses a web migration skill. |
| Agent creates only a new WPF shell. | Agent uses `wpf-project-setup`. |
| Agent faces unresolved domain or API redesign that invalidates the migration surface. | Agent stabilizes the contract first. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source feature scope | Yes | Scope identifies the route, component tree, or feature module. |
| Target WPF shell | Yes | Shell identifies the desktop host that receives the feature. |
| Shared API contracts | Yes | Contracts identify endpoints, DTOs, and auth flow. |
| Source state strategy | No | Strategy identifies store, service, or local component state. |
| Pilot success criteria | Yes | Criteria define what proves one migrated screen works. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Inventory the source feature | Agent records routes, components, services, state owners, and browser-only dependencies. | Migration inventory | Agent reviews the inventory. | The inventory identifies every feature seam that affects the pilot. |
| 2. Choose a pilot slice | Agent selects one workflow with stable contracts and manageable browser coupling. | Pilot choice | Agent reviews success criteria. | The pilot is representative without driving big-bang risk. |
| 3. Map UI structures | Agent maps pages, child components, and repeated visuals to windows, pages, user controls, or data templates. | UI mapping table | Agent reviews the table. | Each important web UI part has one WPF counterpart. |
| 4. Port contracts and services | Agent converts TypeScript models and HTTP services into C# contracts and typed client boundaries. | Desktop contract layer | Agent builds the desktop project. | The feature compiles against explicit C# contracts. |
| 5. Adapt state and validation | Agent maps web state into observable desktop state and maps form rules into WPF validation. | View-model state plan | Agent tests edits and selection flows. | Desktop state and validation remain explicit and observable. |
| 6. Prove desktop UX and contract parity | Agent validates one end-to-end pilot with real authentication, API traffic, and desktop-specific shortcuts or density. | Pilot report | Agent runs the pilot. | One migrated screen completes the core workflow in the WPF shell. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Desktop build | `dotnet build [project].csproj` | Zero compile errors |
| Pilot verification | Manual or integration pilot path | One representative workflow completes successfully |
| Regression verification | Existing unit tests or targeted `dotnet test [test-project].csproj` | Changed desktop logic passes |

## Verification Checklist

- [ ] Agent inventories routes, services, and browser-only dependencies.
- [ ] Agent preserves the API contract while changing the client.
- [ ] Agent models desktop state explicitly instead of copying web store shape blindly.
- [ ] Agent centralizes desktop authentication and token handling.
- [ ] Agent validates one usable pilot screen in the WPF shell.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent attempts a big-bang rewrite. | Agent migrates one pilot screen first. |
| Agent copies web store architecture without desktop justification. | Agent keeps only the desktop state semantics the feature needs. |
| Agent keeps browser storage or interceptor assumptions. | Agent adopts desktop-safe token and transport patterns. |
| Agent preserves responsive-layout compromises that hurt desktop density. | Agent redesigns the screen for desktop expectations. |
