---
name: azure-keyvault-dotnet
title: Azure Key Vault for .NET
description: Use Azure Key Vault in .NET with managed identity, direct secret access, certificate loading, bounded caching, and safe rotation-aware patterns.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 920
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-app-configuration-dotnet
  - azure-ad-authentication-dotnet
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - dotnet
  - azure
  - keyvault
  - secrets
  - security
---
# Azure Key Vault for .NET

Agent uses this skill to add or review secure secret and certificate retrieval in .NET services.

## When to Use

| User prompt | Use |
|---|---|
| User asks to move secrets out of code or config files | Use this skill |
| User asks for managed identity secret retrieval | Use this skill |
| User asks for certificate loading from Key Vault | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for non-sensitive feature or app settings | Use `azure-app-configuration-dotnet` |
| User asks for large file storage | Use storage skills |
| User asks for client-side secret storage | Use a different design |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Vault URI | Yes | Use `https://<vault>.vault.azure.net/`. |
| Auth path | Yes | Use managed identity or `DefaultAzureCredential`. |
| Secret or certificate names | Yes | List every required item. |
| Cache window | Recommended | Set bounded refresh timing for repeated reads. |
| Failure policy | Recommended | Define not-found, auth, and transient handling. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent chooses config-provider access, direct client access, or both. | Review access plan. | Access path matches the app use case. |
| 2 | Agent wires approved credentials. | Inspect registration code. | `DefaultAzureCredential` or managed identity is present. |
| 3 | Agent loads secrets or certificates by name. | Review client calls. | Code avoids embedded secret values. |
| 4 | Agent adds bounded caching for repeated reads. | Inspect cache path. | Repeated reads avoid unnecessary vault calls. |
| 5 | Agent handles not-found, permission, and transient failures separately. | Review exception handling. | Logs contain safe metadata only. |
| 6 | Agent validates rotation behavior. | Review version usage. | Latest-version reads or explicit-version reads are intentional. |

## Pattern Matrix

| Need | Preferred pattern |
|---|---|
| App-wide configuration | Add Key Vault as a configuration provider |
| Targeted runtime retrieval | Inject `SecretClient` or certificate client directly |
| Production auth | Use managed identity |
| Development auth | Use `DefaultAzureCredential` with local developer identity |
| Repeated reads | Add memory cache with explicit expiration |
| Certificate usage | Load certificate material through vault-aware APIs |

## Security Matrix

| Concern | Agent action |
|---|---|
| Secret storage | Agent keeps values out of source files. |
| Logging | Agent logs names, versions, and failure categories only. |
| Fallbacks | Agent fails clearly or uses an approved backup path. |
| Rotation | Agent documents cache duration and version policy. |

## Validation Checklist

- [ ] Agent used managed identity or `DefaultAzureCredential`.
- [ ] Agent kept secret values out of source, logs, and exception detail.
- [ ] Agent separated not-found, auth, and transient failures.
- [ ] Agent added caching where repeated reads exist.
- [ ] Agent handled rotation with either latest-version or explicit-version reads.
