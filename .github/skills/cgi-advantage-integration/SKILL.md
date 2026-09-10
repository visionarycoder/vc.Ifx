---
name: cgi-advantage-integration
title: CGI Advantage Integration
description: Integrate CGI Advantage 4 APIs, files, and finance modules with stable contracts, resilient .NET clients, and AFRS-aware reconciliation.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1634
prerequisites:
  - dotnet-api-client-resilience
related_skills:
  - dotnet-webapi
  - azure-service-bus-patterns
  - fabric-lakehouse-ingestion
  - public-sector-accounting
  - wa-state-afrs
  - cgi-advantage-reporting
appliesTo: '**/*.{cs,csproj,json,xml,csv,sql,md,yml,yaml}'
tags:
  - cgi-advantage
  - trains-4
  - afrs
  - integration
  - gl
  - ap
  - ar
  - polly
related_docs:
  - docs/references/CGI_Advantage_4_Financial_GA_User_Guide.pdf
---
# CGI Advantage Integration

Agent integrates CGI Advantage 4, also called Trains 4, through ledger-safe APIs, file exchanges, and reconciliation workflows.

## When to Use

| Condition | Use |
|---|---|
| Work needs Advantage authentication, extraction, or posting | Use this skill |
| Work spans GL, AP, AR, vendors, or purchase orders | Use this skill |
| Work compares API and file-based exchange | Use this skill |
| Work needs resilient `.NET` client registration or replay control | Use this skill |
| Work maps Advantage output into AFRS reporting flows | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on reporting or dashboard consumption | Use `cgi-advantage-reporting` |
| Work focuses on legacy Trains cutover or rollback | Use `legacy-trains-migration` |
| Work focuses on generic HTTP hardening only | Use `dotnet-api-client-resilience` |
| Work focuses on brokered messaging only | Use `azure-service-bus-patterns` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Environment | Yes | Agent records tenant host, path root, and boundary. |
| Authentication path | Yes | Agent records token, session, certificate, or gateway flow. |
| Module scope | Yes | Agent records GL, AP, AR, vendor, purchase order, or mixed scope. |
| Transfer style | Yes | Agent records API, file, batch, or hybrid flow. |
| COA grain | Yes | Agent records fund and segment keys used in mappings. |
| Replay rule | Yes | Agent records checkpoint and duplicate-suppression behavior. |
| Reporting target | No | Agent records AFRS, warehouse, or internal consumer. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent inventories endpoints, batch jobs, drop zones, and module surfaces. | Review inventory. | Each entity maps to one source surface. |
| 2 | Agent selects API, file, or hybrid transport per entity and latency target. | Review transport matrix. | Each entity has one primary channel and one replay rule. |
| 3 | Agent designs authentication, authorization, timeout, and retry boundaries. | Review client registration. | Every channel has explicit auth and bounded retry behavior. |
| 4 | Agent maps COA segments, vendor keys, and period data into stable contracts. | Review mapping sheet. | Required fields map once and stay deterministic. |
| 5 | Agent validates extraction, posting, transformation, acknowledgement, and reconciliation behavior. | Run targeted tests. | Success, retry, and failure paths remain traceable. |

## Rule Matrix

| Concern | Rule | Reference |
|---|---|---|
| System terms | Agent records `CGI Advantage 4` once, then uses `Trains 4` in team-facing workflow notes. | `references/integration-reference.md` |
| Channel choice | Agent uses API for low-latency lookups and file or batch paths for large-volume movement. | `references/integration-reference.md` |
| COA mapping | Agent centralizes one governed crosswalk for source and target segments. | `references/integration-reference.md` |
| AFRS flow | Agent treats Trains 4 as source authority and AFRS as downstream reporting target. | `references/integration-reference.md` |
| Auth and replay | Agent retries idempotent reads only and stores replay keys for every mutable operation. | `references/integration-reference.md` |
| Output contracts | Agent emits stable internal contracts before downstream fan-out. | `references/integration-reference.md` |

## Pattern Matrix

| Pattern | Use | Output |
|---|---|---|
| Real-time API | Point lookups, status checks, and small posting requests | Typed client plus bounded latency budget |
| Batch API | Periodic deltas with pagination or watermarking | Resumable extraction workflow |
| File exchange | Large-volume handoff or tenant-standard drop zones | Deterministic batch file with control totals |
| Hybrid flow | Mixed master and transaction data shapes | Authority table plus per-channel replay rule |
| Bus relay | Internal fan-out after governed extraction | Immutable event envelope with correlation data |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Channel choice | Compare one entity per module to the transport matrix. | Each entity uses one approved primary channel. |
| COA synchronization | Compare sample source rows to mapped target rows. | Required segments reconcile with zero unmapped required fields. |
| Replay safety | Simulate duplicate delivery and transient failure scenarios. | Duplicate business effects remain at zero. |
| Error normalization | Force auth, validation, and posting failures. | Callers receive stable error categories and correlation data. |
| AFRS handoff | Compare source export, transformed batch, and acknowledgement totals. | Control totals tie out across all selected checkpoints. |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Transport selected | Each entity has one channel and one replay rule. |
| Auth bounded | Clients use explicit auth, timeout, and retry settings. |
| Crosswalk governed | COA and identifier mappings live in one place. |
| AFRS path traceable | Extract, transform, submit, acknowledge, and reconcile stages stay linked. |
| Reference detail preserved | Deep module, batch, and AFRS notes remain in the reference file. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Treating all extracts as real-time | Agent separates low-latency requests from bulk history movement. |
| Retrying unsafe writes blindly | Agent adds idempotency keys and duplicate suppression first. |
| Spreading COA logic across services and files | Agent centralizes the crosswalk. |
| Treating AFRS submission as one-step file copy | Agent separates extract, transform, validate, submit, acknowledge, and reconcile stages. |
