---
name: legacy-trains-migration
title: Legacy Trains Migration
description: Migrate legacy Trains workloads into CGI Advantage 4 when work needs mapping, reconciliation, parallel run, cutover sequencing, or rollback planning.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1538
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-cutover-migration
  - reconciliation-patterns
  - data-quality-reporting
  - dotnet-webapi
  - azure-service-bus-patterns
  - fabric-lakehouse-ingestion
appliesTo: '**/*.{cs,csproj,json,xml,csv,sql,md,xlsx,yml,yaml}'
tags:
  - trains
  - trains-4
  - migration
  - cutover
  - reconciliation
  - data-quality
---
# Legacy Trains Migration

Agent migrates legacy Trains data and operational workflows into CGI Advantage 4, known by the team as Trains 4, with explicit mapping, reconciliation, parallel run, and rollback control.

## When to Use

| Condition | Use |
|---|---|
| Work items need legacy Trains assessment, migration sequencing, or cutover planning | Use this skill |
| Work items need chart of accounts, vendor, or transaction-history conversion into Trains 4 | Use this skill |
| Work items need parallel run, reconciliation, or post-cutover validation design | Use this skill |
| Work items need .NET migration utilities, staging pipelines, or data-quality verification | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work items focus on steady-state CGI Advantage API integration after cutover | Use `cgi-advantage-integration` |
| Work items focus on native or analytical reporting after data already lands in Trains 4 | Use `cgi-advantage-reporting` |
| Work items focus on generic reconciliation logic outside migration scope | Use `reconciliation-patterns` |
| Work items focus on generalized cutover orchestration with no Trains domain mapping | Use `vbd-cutover-migration` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Legacy source inventory | Yes | Record tables, files, reports, interfaces, and business owners. |
| Target module scope | Yes | Record GL, AP, AR, vendor, purchase order, or mixed landing scope. |
| Historical retention rule | Yes | Record open-item, current-year, multi-year, or full-history strategy. |
| Cutover window | Yes | Record freeze point, parallel period, validation window, and go-live boundary. |
| Reconciliation basis | Yes | Record counts, totals, document, balance, and exception thresholds. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent inventories legacy Trains data, interfaces, reports, code tables, and owners. | Review source inventory and dependency maps. | Every migrated domain has one owner and one source authority entry. |
| 2 | Agent builds field-level mappings for COA, vendors, documents, balances, and history retention. | Review mapping tables. | Every required source field maps to one target field or one retirement rule. |
| 3 | Agent selects API, file, hybrid, or historical-archive migration paths per entity. | Compare entity plans to the migration matrix. | Each entity has one load path, one checkpoint rule, and one validation rule. |
| 4 | Agent executes parallel run, reconciliation, and user validation packs. | Review counts, totals, and exception outcomes. | Variances are resolved, accepted, or tied to an approved rule. |
| 5 | Agent validates cutover, rollback, and post-go-live outputs. | Execute the verification checklist. | Operational and analytical outputs reconcile to the approved target baseline. |

## Migration Pattern Matrix

| Pattern | Use | Verification Target |
|---|---|---|
| API-led master-data load | Vendors, reference codes, or controlled low-volume conversions | Reruns create zero duplicate masters. |
| File-led bulk conversion | Transaction history, large balances, or open-item backfill | Landed counts and totals match staged counts. |
| Staged hybrid conversion | APIs for masters and files for history or open items | No entity loads through two active authority paths. |
| Read-only historical warehouse landing | Archived detail retained for analytics and audit only | Historical loads never appear in operational posting queues. |

## Parallel Run Matrix

| Strategy | Use | Pass Target |
|---|---|---|
| Shadow extract | Target receives data while legacy stays authoritative | Differences stay inside approved tolerance. |
| Dual processing with compare-only close | Both systems process the same period with target held non-authoritative | Variance lists are understood and signed off. |
| Wave cutover by module | One module moves at a time | Closed modules show zero critical defects before the next wave. |
| Full cutover after rehearsal | One coordinated go-live across dependent modules | Rehearsal exit criteria are all green. |

## Reference Files

| File | Purpose |
|---|---|
| [references/domain-reference.md](references/domain-reference.md) | Terminology, target module reference, cutover governance, integration hooks, pitfalls, outputs, and source documents. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Field mapping completeness | Review required-source to target-field matrices. | Zero required source fields remain unmapped without an approved retirement rule. |
| COA conversion | Run sample and full-segment reconciliation. | Converted segments balance and every required code resolves through the crosswalk. |
| Vendor migration | Compare source and target active vendor populations. | Counts, key attributes, and duplicate rules reconcile. |
| Transaction history | Load representative historical slices and open items. | Counts, balances, and document states reconcile to the selected retention rule. |
| Parallel run | Compare legacy and Trains 4 outputs for one rehearsal or overlap window. | Variances remain within approved tolerance or carry signed exception disposition. |
| Cutover and rollback readiness | Review freeze steps, decision gates, and rollback tables. | Every irreversible step has a predecessor gate and evidence requirement. |
