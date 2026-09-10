---
name: quartz-job-authoring
description: Author Quartz.NET jobs with explicit DI, execution context, cancellation, and failure handling.
title: Quartz.NET Job Authoring
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1500
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - quartz-calendar-schedules
  - quartz-scheduling-patterns
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - quartz
  - job
  - authoring
---
# Quartz.NET Job Authoring

Agent authors Quartz jobs that stay thin, cancellable, and observable.

## When to Use

| Condition | Use |
|---|---|
| Agent creates a new `IJob` implementation | Use this skill |
| Agent adds DI-backed scheduled processing | Use this skill |
| Agent passes runtime data through Quartz context | Use this skill |
| Agent defines retry or failure behavior for Quartz work | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Work is event-driven and triggerless | Use queue or event handlers |
| Work lives better in Azure Functions or WebJobs | Use the host-specific skill |
| Work is a continuous service with no trigger schedule | Use hosted service patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Job responsibility | Yes | One clear business outcome |
| Dependencies | Yes | Services, repositories, clients, and options |
| Job data contract | No | Keys and expected types in `JobDataMap` |
| Failure policy | Yes | Retry now, fail, or log-and-stop |
| Runtime duration | No | Use with cancellation and checkpoint design |

## Required Workflow

Agent performs these steps:
1. Agent implements a stateless `IJob` that delegates domain work to injected services.
2. Agent reads runtime parameters from `context.MergedJobDataMap` with explicit keys.
3. Agent passes `context.CancellationToken` through every async dependency.
4. Agent maps transient and terminal failures to explicit Quartz behavior.
5. Agent verifies manual triggering, cancellation, and failure logging.

Test: Agent runs `dotnet build [project].csproj` and existing job tests or targeted execution tests.
Pass: Agent observes zero compile errors. Agent confirms one successful execution path and one failure path in the changed scope.

## Lifecycle Matrix

| Stage | Agent verifies | Preferred pattern | Avoid |
|---|---|---|---|
| Startup | DI resolves the job and dependencies | Quartz job registration plus hosted service | `new` dependency creation inside the job |
| Execution | Job delegates business work | Thin `Execute` method that calls a service | Large domain logic block inside `Execute` |
| Data access | Job data keys stay explicit | `MergedJobDataMap` with known key names | Hidden magic strings across the codebase |
| Cancellation | Long work stops promptly | `context.CancellationToken` passed to each async hop | Ignored token in loops or I/O |
| Failure | Failure path is classified | `JobExecutionException` or intentional non-throw path | Silent exception swallowing |
| Completion | Logs and result state stay observable | Structured logs and `context.Result` when useful | No completion signal |

## Data and Retry Matrix

| Concern | Preferred pattern | Notes |
|---|---|---|
| Runtime parameters | `UsingJobData(...)` plus typed reads | Keep keys stable and documented |
| Transient failure | `JobExecutionException(ex, refireImmediately: true)` when replay is safe | Limit to idempotent or guarded work |
| Terminal failure | `JobExecutionException(ex, refireImmediately: false)` | Pair with alerting or operator review |
| Expected no-op | Log and return without throw | Use for empty workload conditions |
| Long work | Pair with checkpoint strategy | Route to `long-running-job-patterns` when duration grows |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Unit verification | Existing MSTest project for the job | Happy-path test passes |
| Failure verification | Existing MSTest project with mocked dependency failure | Failure path logs and throws expected Quartz exception |
| Trigger verification | Existing app path or targeted scheduler test | Job executes from its trigger or manual `TriggerJob` call |

## Verification Checklist

Agent verifies:
- [ ] Job implements `IJob`
- [ ] Constructor injection supplies dependencies
- [ ] Job registration and trigger registration both exist
- [ ] Job data keys are explicit and documented
- [ ] Cancellation token reaches long-running operations
- [ ] Failure handling matches retry intent
- [ ] Logs identify start, completion, and failure

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Job creates dependencies directly | Move creation to DI registration |
| Job stores mutable state on the class | Keep the job stateless and use job data or persistence |
| Job ignores cancellation | Pass `context.CancellationToken` through each async call |
| Job swallows all exceptions | Classify the exception and surface explicit Quartz behavior |
| Job mixes trigger configuration into business logic | Keep trigger setup in registration code |
