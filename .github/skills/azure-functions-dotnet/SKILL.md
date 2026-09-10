---
name: azure-functions-dotnet
description: Build Azure Functions in .NET with explicit trigger selection, host setup, execution flow, and lifecycle validation.
title: Azure Functions for .NET
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1700
prerequisites:
  - dotnet-webapi
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-storage-dotnet
  - azure-app-configuration-dotnet
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - dotnet
  - azure-functions
  - serverless
  - cloud
  - runtime
---
# Azure Functions for .NET

Agent builds Azure Functions that keep trigger intent explicit, function bodies thin, and host behavior observable.

## When to Use

| Condition | Use |
|---|---|
| Agent implements serverless HTTP, timer, queue, or blob processing | Use this skill |
| Agent needs isolated-worker DI, configuration, and telemetry | Use this skill |
| Agent needs scheduled execution with timer triggers | Use this skill |
| Agent needs durable workflow orchestration | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Work exceeds host limits and needs durable checkpointed execution | Pair with `long-running-job-patterns` or choose another host |
| Work depends on Quartz-style holiday or business-day Schedule rules | Use Quartz skills |
| Work needs always-on process control with App Service semantics | Use WebJobs or worker-service patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Trigger type | Yes | HTTP, Timer, Queue, Blob, Service Bus, or Durable |
| Function responsibility | Yes | One clear business outcome |
| Dependencies | Yes | Services, clients, repositories, and options |
| Configuration sources | Yes | App settings, Key Vault, or App Configuration |
| Lifecycle expectations | No | Timeout, cancellation, poison handling, or replay behavior |

## Required Workflow

Agent performs these steps:
1. Agent selects the trigger type from the trigger matrix before authoring code.
2. Agent configures the isolated worker host with DI, configuration, and telemetry.
3. Agent keeps each function entry point thin and delegates domain work to services.
4. Agent propagates cancellation and classifies replay or poison-message behavior.
5. Agent validates local execution, trigger behavior, and observable outcomes.

Test: Agent runs `dotnet build [project].csproj` and existing local function tests or representative trigger execution for the changed function.
Pass: Agent observes zero compile errors. Agent confirms one representative trigger execution and one lifecycle edge case such as cancellation, retry, or poison handling.

## Trigger Pattern Matrix

| Need | Preferred trigger | Notes |
|---|---|---|
| HTTP request/response | HTTP trigger | Validate input before side effects |
| Fixed recurring execution | Timer trigger | Use explicit NCRONTAB and timezone documentation |
| Message-driven processing | Queue or Service Bus trigger | Design for replay and poison handling |
| File-driven processing | Blob trigger | Keep file parsing and storage access delegated |
| Stateful workflow | Durable orchestration plus activities | Keep activity work idempotent |

## Timer and Lifecycle Matrix

| Concern | Preferred pattern | Avoid |
|---|---|---|
| Timer recurrence | Explicit NCRONTAB with documented intent | Undocumented timer strings |
| Timer validation | Preview next occurrences in existing tests or helper code | Deploy-first validation |
| Startup | `HostBuilder` plus `ConfigureFunctionsWebApplication()` | Hidden static initialization |
| Execution | Thin function method plus injected service | Large business logic block in the trigger method |
| Cancellation | `CancellationToken` passed to inner async calls | Ignored host stop or timeout signals |
| Failure | Structured logs plus host-visible exception behavior | Silent poison-message loops |

## Pattern Matrix

| Concern | Preferred pattern | Notes |
|---|---|---|
| DI | Register services in `Program.cs` | Keep function classes focused on orchestration |
| Configuration | Use settings, Key Vault, or App Configuration | Keep secrets out of source control |
| HTTP responses | Use explicit status and safe payloads | Pair error contracts with Problem Details guidance |
| Long work | Route large workflows to Durable Functions or durable checkpoints | Respect plan and timeout constraints |
| Observability | Include correlation IDs and business identifiers | Logs support replay and operator review |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Local host verification | Existing local function run or integration test | Host starts and binds the changed trigger |
| Trigger verification | Existing test or representative HTTP, timer, queue, or blob execution | Function completes the intended business step |
| Lifecycle verification | Existing test or controlled retry/cancellation drill | Edge-case behavior matches documented intent |

## Verification Checklist

Agent verifies:
- [ ] Trigger type matches the workload
- [ ] Function method delegates business work to services
- [ ] Host registration includes required extensions and telemetry
- [ ] Configuration sources are explicit and secret-safe
- [ ] Cancellation and replay behavior are intentional
- [ ] Local or test-host execution passes before deployment

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Function body mixes orchestration and domain logic | Move domain logic into services |
| Timer string lacks documentation or validation | Add a plain-language recurrence note and preview occurrences |
| Queue handler retries non-idempotent work blindly | Add replay guards or durable checkpoints |
| Function setup relies on hidden static state | Move setup into the isolated worker host |
| Telemetry omits correlation identifiers | Log correlation and business keys |
