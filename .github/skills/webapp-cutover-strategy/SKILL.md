---
name: webapp-cutover-strategy
title: Web Application Cutover Strategy
description: Execute a safe web application cutover through sticky cohorts, telemetry gates, rollback triggers, communication plans, and post-release retirement steps.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1204
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - frontend-module-consolidation
  - vue3-e2e-testing-playwright
  - configuring-opentelemetry-dotnet
appliesTo: '**/*.{ts,vue,json,yaml,yml,cs,bicep}'
tags:
  - webapp
  - cutover
  - strategy
---
# Web Application Cutover Strategy

Agent moves users from one web application generation to another through reversible routing, measured rollout gates, and explicit rollback control.

## When to Use

| Condition | Use |
|---|---|
| Agent releases a consolidated or migrated web application to real users. | Agent uses this skill. |
| Agent needs staged traffic movement with cohort control. | Agent uses this skill. |
| Agent needs an operational rollback path that works without a rebuild. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent lacks parity, security, accessibility, or recovery evidence. | Agent completes readiness work first. |
| Agent performs an internal refactor with no user-visible routing impact. | Agent uses normal deployment practices. |
| Agent depends on an untested data-restore process for rollback. | Agent redesigns rollback first. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Release owner | Yes | Owner has authority to advance or stop rollout. |
| Cohort mechanism | Yes | Mechanism identifies feature flags, routing, or allowlists. |
| Baseline metrics | Yes | Metrics define the old app baseline. |
| Rollback thresholds | Yes | Thresholds define measurable reversal triggers. |
| Communication plan | Yes | Plan identifies audience, timing, and support channels. |
| Data compatibility evidence | Yes | Evidence proves both app generations coexist safely. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Define the cutover unit | Agent chooses the smallest sticky release slice such as a route, feature, role cohort, or account cohort. | Cutover scope | Agent reviews routing behavior. | Users remain on one coherent experience during a session. |
| 2. Externalize release control | Agent implements feature-flag or routing control outside the application deployment. | Release-control path | Agent toggles the release control. | Traffic shifts without a rebuild. |
| 3. Standardize telemetry | Agent emits comparable dimensions, outcome metrics, and safe correlation data across both app generations. | Telemetry schema | Agent inspects dashboards or queries. | Old and new cohorts compare on the same metric definitions. |
| 4. Define readiness and rollback gates | Agent records pre-cutover gates and sustained rollback triggers before rollout starts. | Gate matrix | Agent reviews the matrix. | Release criteria and reversal criteria are explicit. |
| 5. Roll out in stages | Agent advances through internal, pilot, partial, and full cohorts with observation windows between stages. | Stage log | Agent compares stage results to thresholds. | No stage advances without evidence. |
| 6. Retire the old application safely | Agent removes old routes, flags, and secrets only after the stability window closes. | Retirement plan | Agent reviews post-cutover traffic and artifacts. | Old app traffic reaches zero before retirement work finishes. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Readiness verification | Pre-cutover review against the gate matrix | Every required gate is complete |
| Rollback verification | Production-like rehearsal | Routing reverses without a rebuild |
| Post-cutover verification | Smoke tests and telemetry review | Critical journeys and dashboards remain healthy |

## Verification Checklist

- [ ] Agent uses sticky cohorts.
- [ ] Agent keeps release control external to the deployment artifact.
- [ ] Agent compares old and new behavior through common telemetry dimensions.
- [ ] Agent defines rollback triggers before rollout starts.
- [ ] Agent keeps retirement work after the stability window closes.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent routes requests randomly for authenticated users. | Agent uses identity-based or otherwise sticky cohorts. |
| Agent ties rollback to a rebuild. | Agent uses an external routing or flag kill switch. |
| Agent advances stages while telemetry is missing. | Agent blocks advancement until telemetry returns. |
| Agent retires the old app before proving zero traffic. | Agent waits for stable zero-traffic evidence. |
