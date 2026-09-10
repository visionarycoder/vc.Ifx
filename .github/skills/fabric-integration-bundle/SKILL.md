---
name: fabric-integration-bundle
title: Fabric Integration Bundle
description: Routes Microsoft Fabric work to the correct specialist skill across ingestion, storage, semantic, analytics, real-time, governance, diagnostics, and Copilot surfaces.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 1190
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-powerbi-integration
  - fabric-data-factory-patterns
  - fabric-synapse-analytics
  - fabric-copilot-studio
  - fabric-lakehouses
  - fabric-lakehouse-ingestion
  - fabric-notebooks-spark
  - fabric-realtime-analytics
  - fabric-kql-diagnostics
  - fabric-data-activator
  - fabric-onelake-patterns
  - fabric-data-science
  - fabric-workspaces-governance
  - fabric-shortcuts-mirroring
  - fabric-medallion-architecture
  - fabric-pipeline-diagnostics
  - fabric-semantic-model-builder
  - semantic-layer-fabric
  - ontology-design-patterns
appliesTo: '**/*.{cs,csproj,json,md,ps1,py,sql,tmdl,tmsl,dax,kql,yml,yaml}'
tags:
  - fabric
  - onelake
  - powerbi
  - realtime
  - lakehouse
  - semantic
  - routing
---
# Fabric Integration Bundle

Agent uses this bundle for Microsoft Fabric requests that span data movement, OneLake layout, semantic modeling, analytics serving, real-time processing, governance, diagnostics, or Copilot experiences.

## Activation

| Need | Route |
|---|---|
| Copy, orchestration, ETL, or pipeline authoring | `fabric-data-factory-patterns` or `fabric-pipeline-diagnostics` |
| Lakehouse landing, open-table storage, or ingestion flow | `fabric-lakehouses` or `fabric-lakehouse-ingestion` |
| Warehouse SQL serving, semantic objects, or report models | `fabric-synapse-analytics`, `fabric-semantic-model-builder`, or `semantic-layer-fabric` |
| Streaming, KQL, alerting, or incident diagnosis | `fabric-realtime-analytics`, `fabric-kql-diagnostics`, or `fabric-data-activator` |
| OneLake layout, shortcuts, mirroring, workspaces, or promotion | `fabric-onelake-patterns`, `fabric-shortcuts-mirroring`, or `fabric-workspaces-governance` |
| Data science, notebooks, Power BI, Copilot, or IQ-style ontology alignment | `fabric-data-science`, `fabric-notebooks-spark`, `fabric-powerbi-integration`, `fabric-copilot-studio`, `semantic-layer-fabric`, or `ontology-design-patterns` |

## Coverage Matrix

| Skill | Coverage |
|---|---|
| `fabric-powerbi-integration` | Dashboards, metrics, report consumption |
| `fabric-data-factory-patterns` | Copy, ETL, orchestration, scheduled movement |
| `fabric-synapse-analytics` | Warehouse SQL serving and star-schema delivery |
| `fabric-copilot-studio` | Conversational experience over Fabric assets |
| `fabric-lakehouses` | Open tables, parquet, and lakehouse storage |
| `fabric-lakehouse-ingestion` | Landing, bronze intake, and ingestion reliability |
| `fabric-notebooks-spark` | Spark notebooks and engineering workflows |
| `fabric-realtime-analytics` | Event streams, KQL, and low-latency analytics |
| `fabric-kql-diagnostics` | KQL issue diagnosis and investigation |
| `fabric-data-activator` | Triggered actions from data signals |
| `fabric-onelake-patterns` | OneLake storage topology and item layout |
| `fabric-data-science` | Experiments, features, and model workflows |
| `fabric-workspaces-governance` | Workspace boundaries, promotion, and access |
| `fabric-shortcuts-mirroring` | Virtual access and replicated source access |
| `fabric-medallion-architecture` | Bronze, Silver, Gold design |
| `fabric-pipeline-diagnostics` | Pipeline failure triage and correction |
| `fabric-semantic-model-builder` | Model assembly and semantic-object authoring |
| `semantic-layer-fabric` | Curated semantic layer, measures, and RLS |
| `ontology-design-patterns` | IQ-style ontology and governed concept alignment |

## Decision Order

| Order | Agent Action |
|---|---|
| 1 | Agent sets workspace boundary and OneLake layout. |
| 2 | Agent selects shortcut, mirroring, ingestion, or ETL ownership. |
| 3 | Agent selects lakehouse, warehouse, or real-time storage shape. |
| 4 | Agent adds semantic model, Power BI, or Copilot surfaces from curated data only. |
| 5 | Agent adds notebooks, data science, diagnostics, or Data Activator where the workload requires them. |
| 6 | Agent adds ontology alignment when IQ-style semantic meaning needs a governed concept layer. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Map the workload | Agent maps movement, storage, semantic, analytics, and operations needs. | Agent lists routed skills. | Every need maps to one primary skill. |
| 2. Select the data shape | Agent records batch, open-table, warehouse, or stream as the primary shape. | Agent records one primary shape. | Shape fits latency and query style. |
| 3. Define the handoff chain | Agent records source, land, refine, serve, and consume stages. | Agent reviews stage order. | Flow has no missing stage. |
| 4. Add diagnostics and governance | Agent adds pipeline, KQL, and workspace diagnostics only where runtime risk exists. | Agent reviews operations scope. | Governance and diagnosis stay intentional. |
| 5. Verify the fit | Agent compares routes to latency, cost, access, and consumer inputs. | Agent reviews the final route set. | Route set matches the request. |

## Verification Matrix

| Track | Agent Verifies | Test | Pass |
|---|---|---|---|
| Storage | Lakehouse, warehouse, and OneLake roles stay distinct. | Inspect storage ownership. | Storage fits the workload. |
| Movement | Shortcut, mirroring, ingestion, and ETL paths stay distinct. | Inspect source-ingest path. | Movement matches source behavior. |
| Processing | Batch, Spark, science, and stream paths fit latency. | Inspect cadence and compute surface. | Processing fits the request. |
| Semantics | Semantic builder, semantic layer, Power BI, and ontology cues align. | Inspect model and concept ownership. | Metrics and concepts stay governed. |
| Operations | Workspace, pipeline, and KQL diagnostics are explicit. | Inspect failure and access paths. | Operators have one clear diagnosis path. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Using Power BI as the main transformation layer | Agent moves heavy transformation upstream. |
| Using ETL when shortcuts or mirroring fit the source better | Agent selects the lighter movement pattern. |
| Building Copilot or semantic models on raw ingestion outputs | Agent inserts a curated semantic layer first. |
| Treating KQL diagnostics as the same concern as streaming design | Agent routes design to real-time analytics and diagnosis to KQL diagnostics. |
| Treating ontology alignment as a report-label cleanup task only | Agent routes governed concept work through ontology and semantic-layer skills. |

