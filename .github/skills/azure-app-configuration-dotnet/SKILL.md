---
name: azure-app-configuration-dotnet
description: Integrate Azure App Configuration into .NET with centralized settings, labels, feature flags, dynamic refresh, and Key Vault references.
title: Azure App Configuration for .NET
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1460
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-keyvault-dotnet
  - configuration-options-pattern
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - dotnet
  - azure
  - configuration
  - options
  - cloud
---
# Azure App Configuration for .NET

Agent integrates Azure App Configuration for centralized settings, labels, refresh, and feature flags.

## When to Use

| User prompt | Use |
|---|---|
| User asks to centralize settings across apps or environments | Use this skill |
| User asks for dynamic refresh without redeploy | Use this skill |
| User asks for Azure-backed feature flags | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to store secrets directly | Use `azure-keyvault-dotnet` |
| User asks for simple local-only settings | Use local configuration |
| User asks for large binary payload storage | Use storage skills |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| App Configuration endpoint | Yes | Use endpoint or connection string. |
| Auth path | Yes | Use managed identity in production. |
| Key filters and labels | Yes | Define prefixes and environment labels. |
| Refresh plan | Recommended | Use sentinel keys and cache intervals. |
| Feature flag plan | No | List flags and their labels. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects connection string or managed identity access. | Inspect bootstrap config. | Production paths avoid embedded secrets. |
| 2 | Agent loads only required keys and labels. | Review `Select(...)` filters. | Base and environment keys resolve predictably. |
| 3 | Agent wires refresh with a sentinel key. | Inspect refresh setup. | Changed sentinel triggers reload after cache expiry. |
| 4 | Agent adds `AddAzureAppConfiguration()` and middleware where the host requires refresh. | Inspect services and middleware order. | Runtime refresh path is active. |
| 5 | Agent binds typed options for app code. | Review options registration. | Consumer code reads strongly typed settings. |
| 6 | Agent uses feature flags only where toggle semantics are real. | Inspect feature management usage. | Flags load and evaluate by label. |
| 7 | Agent resolves Key Vault references when sensitive values exist. | Review Key Vault config. | Secret-backed keys hydrate through App Configuration. |

## Pattern Matrix

| Need | Preferred pattern |
|---|---|
| Central settings | `AddAzureAppConfiguration(...)` |
| Environment overrides | `LabelFilter.Null` plus environment label |
| Dynamic refresh | Sentinel key plus bounded cache expiration |
| Typed config | `IOptionsSnapshot<T>` |
| Feature flags | `UseFeatureFlags(...)` plus `AddFeatureManagement()` |
| Secret-backed settings | `ConfigureKeyVault(...)` with approved credentials |

## Validation Matrix

| Test | Agent verifies |
|---|---|
| Startup load | Required keys load at app start. |
| Label override | Environment label overrides the unlabeled base value. |
| Sentinel refresh | Updating the sentinel changes resolved values after cache expiration. |
| Typed options | `IOptionsSnapshot<T>` reflects refreshed values in scoped consumers. |
| Feature flags | Enabled and disabled paths switch correctly. |
| Key Vault reference | Secret references resolve without hard-coded secret values. |

## Validation Checklist

- [ ] Agent kept secrets in Key Vault or secure config sources.
- [ ] Agent filtered keys by prefix and label instead of loading everything.
- [ ] Agent configured sentinel-based refresh with explicit cache timing.
- [ ] Agent used `IOptionsSnapshot<T>` for refresh-aware consumers.
- [ ] Agent enabled feature management only when the app uses flags.
- [ ] Agent validated refresh and label behavior with a real change.
