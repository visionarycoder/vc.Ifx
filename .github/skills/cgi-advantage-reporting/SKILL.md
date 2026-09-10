---
name: cgi-advantage-reporting
title: CGI Advantage Reporting
description: Build CGI Advantage 4 reporting, scheduling, export, and analytics flows when work needs operational reports, warehouse extracts, or Power BI and Fabric consumption.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1599
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-powerbi-integration
  - fabric-lakehouse-ingestion
  - dotnet-webapi
  - data-quality-reporting
appliesTo: '**/*.{cs,csproj,sql,json,rdl,rpt,md,csv,xlsx,pbix,yml,yaml}'
tags:
  - cgi-advantage
  - trains-4
  - reporting
  - crystal-reports
  - power-bi
  - fabric
---
# CGI Advantage Reporting

Agent designs CGI Advantage 4, known by the team as Trains 4, reporting flows across native reports, scheduled exports, warehouse extracts, and analytics handoffs.

## When to Use

| Condition | Use |
|---|---|
| Work needs custom financial, operational, or control reports from Trains 4 data | Use this skill |
| Work needs scheduling, distribution, or export automation | Use this skill |
| Work needs Excel, CSV, PDF, Crystal, Fabric, or Power BI outputs | Use this skill |
| Work compares real-time inquiry to staged history | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on posting APIs or source-system extraction | Use `cgi-advantage-integration` |
| Work focuses on cutover or reconciliation during migration | Use `legacy-trains-migration` |
| Work focuses on semantic model design after curated data lands in Fabric | Use `fabric-powerbi-integration` |
| Work focuses on generic SQL profiling | Use `data-quality-reporting` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Audience | Yes | Record finance operations, leadership, audit, vendor support, or analytics consumers. |
| Grain | Yes | Record transaction, document, vendor, fund, period, or summarized grain. |
| Delivery cadence | Yes | Record on-demand, intra-day, nightly, close-cycle, or archival cadence. |
| Output format | Yes | Record inquiry, file export, Crystal render, API payload, or semantic dataset. |
| Filter set | Yes | Record fiscal period, fund, department, vendor, document, and status filters. |
| Source surface | Yes | Record native report, inquiry, report job, staging schema, or warehouse source. |
| Volume target | No | Use for row caps, batching, partitioning, and file packaging. |

## Pattern Matrix

| Need | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Analyst inquiry or low-latency balance review | Inquiry or API-backed interactive report | Enforce narrow filters, paging, and row caps | Response stays inside the interactive budget |
| Large recurring distribution | Scheduled export package | Stamp batch ID, period, row count, and control totals | Export package reconciles to source counts |
| Pixel-perfect operational document | Native or Crystal report render | Keep layout logic separate from source-query logic | Rendered output matches approved totals and layout |
| Cross-period analytics or dashboard history | Warehouse or lakehouse extract | Land immutable batches with watermark lineage | Load count and watermark match source selection |
| Current-period inquiry plus trend history | Hybrid interactive plus batch | Record one authority rule for current values and one for history | Users see no conflicting totals |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps audience, authority source, cadence, and output contract. | Review the report specification. | Each report has one audience and one authority source. |
| 2 | Agent selects interactive, scheduled, rendered, warehouse, or hybrid delivery. | Compare the request to the pattern matrix. | Each report has one primary delivery pattern. |
| 3 | Agent defines parameters, joins, export schema, file naming, and control totals. | Review the parameter and output contract. | Filters and reconciliation fields stay explicit. |
| 4 | Agent implements query shape, scheduling path, render path, and distribution metadata. | Run a representative render or extract. | Output shape and metadata match the contract. |
| 5 | Agent wires downstream Fabric, Power BI, or .NET consumers to the governed report or extract. | Review lineage and refresh hooks. | Each consumer traces to one governed source. |
| 6 | Agent validates totals, parameter behavior, large-report performance, and schedule observability. | Execute the verification checklist. | Totals reconcile and runtime stays inside the selected budget. |

## Rules

| Topic | Rule |
|---|---|
| Terminology | Use `Trains 4` in team-facing notes and keep `CGI Advantage 4` in formal references. |
| Reuse | Reuse native reports and inquiries before building raw transactional joins. |
| Dataset authority | Stabilize the dataset before Crystal or PDF layout work. |
| Export packaging | Include period stamp, batch ID, row count, and control totals on every package. |
| Analytics lineage | Route Power BI and Fabric loads through governed staged extracts or curated layers. |
| Variant consistency | Drive Excel, CSV, PDF, and API variants from one parameter contract. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Source alignment | Trace one output column set to source fields. | Each output column maps to one documented field or derived rule. |
| Parameter behavior | Run narrow and broad filters. | Filters return expected subsets with no ambiguous expansion. |
| Export integrity | Generate the required output formats. | Row counts, totals, and file names reconcile across variants. |
| Performance | Run one representative high-volume report. | Execution time stays inside the agreed budget. |
| Schedule observability | Execute one scheduled run. | Schedule, completion, and retry status remain observable. |
| Analytics handoff | Land one extract in staging. | Refresh uses the expected watermark and batch ID. |

## Outputs

- Report catalog and audience map
- Delivery-pattern decision record
- Parameter, filter, and reconciliation contract
- Render, extract, and distribution plan
- Analytics handoff design
- Verification plan for totals, scheduling, and performance

## Reference Files

- [Reporting surfaces reference](references/reporting-surfaces.md)
