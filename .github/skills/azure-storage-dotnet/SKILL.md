---
name: azure-storage-dotnet
description: Implement Azure Blob, Queue, and Table Storage in .NET with managed identity, SAS control, retries, and efficient transfer patterns.
title: Azure Storage for .NET
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1490
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-functions-dotnet
  - azure-keyvault-dotnet
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - dotnet
  - azure
  - storage
  - blobs
  - queues
---
# Azure Storage for .NET

Agent implements Blob, Queue, and Table Storage access in .NET. Agent chooses the smallest storage service that matches the workload.

## When to Use

| User prompt | Use |
|---|---|
| User asks for file or binary storage | Use Blob Storage patterns |
| User asks for simple async work queues | Use Queue Storage patterns |
| User asks for simple key-partitioned entity storage | Use Table Storage patterns |
| User asks for time-limited external access | Use SAS patterns |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for relational joins or transactions across entities | Use SQL data stores |
| User asks for enterprise messaging features | Use Service Bus or Event Hubs |
| User asks for document search | Use a search service |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Storage account or endpoint | Yes | Use service endpoint or secure connection string. |
| Auth path | Yes | Use managed identity in production. |
| Resource names | Yes | Define container, queue, or table names. |
| Data shape | Yes | State blob stream, queue payload, or entity schema. |
| Retry and timeout policy | Recommended | Define failure behavior. |

## Service Matrix

| Need | Service | Preferred pattern |
|---|---|---|
| File upload/download | Blob | `BlobServiceClient` plus container client |
| Work queue | Queue | `QueueServiceClient` plus visibility timeout handling |
| Simple keyed entities | Table | `TableServiceClient` plus `PartitionKey`/`RowKey` design |
| Delegated access | Blob or container SAS | Short-lived SAS with least privileges |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent chooses Blob, Queue, or Table Storage from the workload. | Review service mapping. | Chosen service matches data and access patterns. |
| 2 | Agent wires secure client authentication. | Inspect startup code. | Managed identity or secure config supplies credentials. |
| 3 | Agent creates service abstractions with cancellation support. | Review method signatures. | Async APIs accept `CancellationToken`. |
| 4 | Agent adds create-if-missing logic where provisioning belongs. | Inspect initialization calls. | Resource creation is intentional and idempotent. |
| 5 | Agent adds retry, timeout, and failure handling. | Review client options and exceptions. | Transient failures retry and permanent failures surface clearly. |
| 6 | Agent validates service-specific behavior. | Run targeted tests. | Blob transfer, queue visibility, or table lookups work as designed. |
| 7 | Agent logs safe operational context. | Inspect logs. | Logs include resource names and outcomes, not secrets. |

## Pattern Matrix

| Concern | Preferred pattern |
|---|---|
| Production auth | Managed identity plus endpoint URI |
| Development auth | User secrets or local secure config |
| Large uploads | Stream or chunk upload APIs |
| Queue processing | Receive, process, delete, then handle poison-message strategy |
| Table design | Stable `PartitionKey` and `RowKey` strategy |
| SAS | Short lifetime, least privilege, clock-skew allowance |

## Validation Matrix

| Test | Agent verifies |
|---|---|
| Blob path | Upload, read, and delete flows succeed. |
| Queue path | Message send, receive, invisibility, and delete flows succeed. |
| Table path | Insert and keyed lookup succeed. |
| Auth path | 403 failures disappear after correct role assignment. |
| Resilience | Retry path handles transient errors without duplicate business effects. |
| Logging | Logs exclude connection strings, keys, and SAS secrets. |

## Validation Checklist

- [ ] Agent selected the correct storage service for the workload.
- [ ] Agent used managed identity or secure configuration for credentials.
- [ ] Agent exposed async APIs with cancellation support.
- [ ] Agent defined retry and timeout behavior.
- [ ] Agent handled SAS permissions and expiration narrowly.
- [ ] Agent validated the main success and failure paths for the selected service.
