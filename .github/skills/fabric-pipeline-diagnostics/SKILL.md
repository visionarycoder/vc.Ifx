---
name: fabric-pipeline-diagnostics
title: Fabric Pipeline Diagnostics
description: Diagnose Microsoft Fabric Data Factory and pipeline activity failures, timeout cascades, retry gaps, and dependency-chain defects.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1470
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-lakehouse-ingestion
  - fabric-kql-diagnostics
appliesTo: '**/*.{json,txt,md}'
tags:
  - fabric
  - pipeline
  - data-factory
  - diagnostics
  - orchestration
---
# Fabric Pipeline Diagnostics

Agent diagnoses Microsoft Fabric pipeline failures from activity run output, error text, or dependency evidence.

## Use When

Agent uses this skill when:
- User reports a failed Fabric pipeline or activity
- User needs root-cause isolation from run JSON
- User needs timeout, retry, or dependency-chain triage
- User needs a minimal remediation plan before a rerun

## Do Not Use When

Agent does not use this skill when:
- User needs Lakehouse file ingestion design
- User needs KQL query tuning
- User needs semantic model design
- User needs broad Data Factory training without failure evidence

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Run JSON or error text | Yes | Agent uses exact run evidence first |
| Pipeline name or run ID | No | Agent uses it for traceability |
| Activity configuration | No | Agent verifies retry, timeout, and dependency settings when present |
| Linked service details | No | Agent verifies connectivity findings when the failure points there |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Capture run evidence | Agent extracts activity names, types, statuses, start times, and error details. | Agent verifies that each failed activity has status and error evidence. | Intake contains one complete failure record per failed activity. |
| 2. Isolate the first real failure | Agent orders failures by start time and dependency position. | Agent verifies which activity failed before downstream skips or `DependencyFailed` results. | Agent names one primary failing activity. |
| 3. Classify the defect | Agent classifies the primary failure as connectivity, schema, authorization, timeout, dependency, or transient platform fault. | Agent verifies the class from exact error text or configuration evidence. | Primary failure class matches the evidence and excludes cascade noise. |
| 4. Verify retry and dependency design | Agent reviews retry counts, retry intervals, timeout values, and dependency graph edges. | Agent verifies whether transient failures lacked retries or whether downstream steps amplified the fault. | Agent identifies the smallest configuration change that reduces repeat failure risk. |
| 5. Produce rerun plan | Agent outputs the minimal remediation and rerun order. | Agent verifies that each rerun step maps to one diagnosed defect. | Rerun plan starts with the root fix and ends with a measurable verification step. |

## Common Findings

| Finding | Signal | Minimal correction |
|---|---|---|
| Dependency cascade | Multiple downstream activities show `DependencyFailed` | Fix and rerun the first failed upstream activity first |
| Timeout gap | Activity fails near configured timeout with no upstream error | Increase timeout only after verifying query or copy scope |
| Retry gap | Transient connector or storage failure occurs with low or zero retries | Add targeted retry count and interval for the failing activity |
| Schema mapping defect | Copy activity reports missing or incompatible columns | Align source and sink mapping before rerun |
| Connectivity defect | Linked service or path failure appears in the first activity error | Verify linked service configuration and target path before rerun |

## Verification Targets

| Target | Test | Pass |
|---|---|---|
| Root-cause isolation | Agent verifies that later failures depend on the named root failure | Only one primary activity remains after cascade removal |
| Retry quality | Agent verifies that transient activities have explicit retry settings | Retry policy matches transient risk for the activity type |
| Rerun readiness | Agent verifies that the plan contains one fix per defect | Plan contains no unrelated remediation steps |
| Measurable outcome | Agent verifies the rerun success signal | Next run completes the prior failing activity and reduces failed-activity count to zero |

## Output Contract

| Output | Contents |
|---|---|
| Triage summary | Root activity, failure class, and evidence |
| Dependency summary | Upstream cause and downstream impact |
| Remediation plan | Minimal configuration or data fix plus rerun order |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent treats every failed activity as a root cause | Agent isolates the earliest independent failure first |
| Agent raises timeout values without evidence | Agent verifies workload scope before timeout changes |
| Agent recommends retries for deterministic schema defects | Agent limits retries to transient faults |
| Agent omits rerun verification | Agent ends with a measurable rerun target |
