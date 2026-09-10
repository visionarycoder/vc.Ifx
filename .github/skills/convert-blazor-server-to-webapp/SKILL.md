---
name: convert-blazor-server-to-webapp
title: Convert Blazor Server App to Blazor Web App
description: Migrate legacy Blazor Server hosting to the .NET Blazor Web App model while preserving interactive server behavior, auth flow, and startup correctness.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1220
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-webapi
  - webapi-error-contract-hardening
  - webapp-cutover-strategy
appliesTo: '**/*.{cs,csproj,razor,cshtml,json}'
tags:
  - dotnet
  - blazor
  - migration
  - webapp
  - ui
---
# Convert Blazor Server App to Blazor Web App

Agent migrates a legacy Blazor Server host to the Blazor Web App model by replacing the host-page pattern, updating startup services, and preserving interactive server rendering.

## When to Use

| Condition | Use |
|---|---|
| Agent upgrades a .NET 6 or .NET 7 Blazor Server app to .NET 8 or later. | Agent uses this skill. |
| Agent replaces `_Host.cshtml` hosting with the root-component document model. | Agent uses this skill. |
| Agent needs Interactive Server render mode in the newer hosting model. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent already sees `AddRazorComponents` and `MapRazorComponents`. | Agent skips migration and performs targeted fixes only. |
| Agent targets WebAssembly or hosted WASM. | Agent uses a WASM-specific migration path. |
| Agent intentionally stays on the legacy hosting model. | Agent preserves the existing host. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Blazor project | Yes | Project includes the `.csproj`, startup code, and root components. |
| Target framework | Yes | Target framework is `net8.0` or later. |
| Current host page | Yes | Current host page is usually `Pages/_Host.cshtml`. |
| Auth and prerender context | No | Context identifies cascaded auth state and prerender expectations. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Confirm the legacy pattern | Agent verifies that the app uses `AddServerSideBlazor`, `MapBlazorHub`, or `_Host.cshtml`. | Migration baseline | Agent reviews startup and host files. | The app qualifies as a legacy Blazor Server host. |
| 2. Update the hosting model | Agent replaces server-side Blazor service and endpoint registration with Razor Components registration and Interactive Server render mode. | Updated startup | Agent builds the project. | Startup compiles with the new hosting APIs. |
| 3. Split root and routing content | Agent moves document-shell concerns into `App.razor` and router concerns into `Routes.razor`. | Root-component structure | Agent reviews the new component tree. | The document shell and routing shell have clear ownership. |
| 4. Remove legacy host-page dependencies | Agent migrates script references, auth-state wiring, and antiforgery behavior away from `_Host.cshtml`. | Legacy-host cleanup | Agent runs the app. | The app starts without depending on `_Host.cshtml`. |
| 5. Re-verify auth and prerender | Agent validates cascaded auth state, prerender behavior, and route startup under the new host. | Behavior verification | Agent exercises representative routes. | Auth and prerender behavior matches the intended end state. |
| 6. Finish with startup and route smoke tests | Agent runs targeted build and startup checks. | Smoke report | Agent starts the app and navigates key routes. | Key routes load successfully through the new host model. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Build verification | `dotnet build [project].csproj` | Zero compile errors |
| Startup verification | Existing app startup path | App starts without host-page errors |
| Route verification | Manual smoke pass | Representative routes load and render |

## Verification Checklist

- [ ] Agent confirms the project used the legacy host pattern before migrating it.
- [ ] Agent replaces `AddServerSideBlazor` and `MapBlazorHub` with Razor Components equivalents.
- [ ] Agent splits document-shell and router concerns between `App.razor` and `Routes.razor`.
- [ ] Agent removes `_Host.cshtml` as the active host dependency.
- [ ] Agent revalidates auth, prerendering, and startup behavior.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent migrates an app that already uses the Web App model. | Agent stops early and performs only targeted corrections. |
| Agent forgets antiforgery middleware after the hosting change. | Agent adds the required antiforgery configuration in startup. |
| Agent copies old router wrappers blindly. | Agent rebinds auth and routing to the Web App-compatible pattern. |
| Agent changes prerender behavior accidentally. | Agent records the intended prerender behavior and tests it explicitly. |
