---
name: semantic-layer-fabric
title: Semantic Layer Fabric
description: Design or review Microsoft Fabric semantic layers when a prompt needs Power BI semantic models, DAX measures, calculation groups, or storage-mode decisions.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1790
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - business-glossary-metadata
  - fabric-semantic-model-builder
  - semantic-modeling-standards
  - terminology-normalization
appliesTo: '**/*.{md,dax,tmdl,bim,json,yml,yaml}'
tags:
  - fabric
  - power-bi
  - semantic-layer
  - dax
  - tabular
---
# Semantic Layer Fabric

Agent designs Microsoft Fabric semantic layers with explicit grain, storage, measure, hierarchy, and security rules.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs Power BI or Fabric semantic model design or review | Use this skill |
| Prompt needs DAX measures, calculation groups, or KPI definition patterns | Use this skill |
| Prompt needs Import, DirectQuery, Direct Lake, or composite-model selection | Use this skill |
| Prompt needs row-level security, object visibility, or semantic-link alignment | Use this skill |
| Prompt needs ontology or glossary terms mapped into semantic-model objects | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt needs source facts, dimensions, and SCD design first | Use `semantic-modeling-standards` |
| Prompt needs metadata ownership and business-term stewardship only | Use `business-glossary-metadata` |
| Prompt needs terminology cleanup across systems before model design | Use `terminology-normalization` |
| Prompt needs ingestion, notebook, or pipeline repair instead of tabular modeling | Use the matching Fabric ingestion or pipeline skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source grain | Yes | Agent records fact grain, dimension keys, and refresh path. |
| Query latency target | Yes | Agent uses it for storage-mode selection. |
| Freshness target | Yes | Agent records intra-day, daily, or snapshot expectations. |
| Security scope | Yes | Agent records geography, tenant, role, or org filters. |
| Consumption pattern | No | Agent records dashboards, ad hoc BI, notebook access, or mixed usage. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent records source grain, refresh path, governed labels, and KPI questions. | Review intake table. | Facts, dimensions, refresh, and personas appear in one intake set. |
| 2 | Agent selects the storage mode from the decision matrix. | Compare workload to matrix rows. | Mode choice matches latency, volume, and freshness targets. |
| 3 | Agent defines relationships, base measures, calculation groups, and hierarchy captions. | Review semantic scaffold. | Each object maps to one business rule and one grain. |
| 4 | Agent defines RLS, OLS, and ambiguity controls. | Review role and relationship design. | Security and filter propagation paths stay explicit. |
| 5 | Agent aligns glossary and ontology labels to captions, descriptions, and measure names. | Review vocabulary crosswalk. | Semantic objects reuse governed business terms. |
| 6 | Agent defines validation queries for totals, drill paths, refresh, and role behavior. | Review DAX validation set. | Measures and filters behave as designed. |

## Decision Matrix

| Workload Shape | Preferred Mode | Strength | Tradeoff | Example |
|---|---|---|---|---|
| Stable data volume with scheduled refresh | Import | Fast query performance | Refresh copies data | Daily finance model |
| Large live source with strict freshness | DirectQuery | Near-source freshness | Source latency reaches users | Live warehouse reporting |
| Fabric-native large model in OneLake | Direct Lake | Low-latency Fabric access | Lakehouse design quality drives results | Lakehouse-backed KPI model |
| Hot live slice plus historic imported aggregates | Composite | Balanced freshness and speed | Relationship complexity grows | Current month live plus prior history import |
| Reusable time or currency transformations | Calculation group | Centralized logic | Precedence review grows | YTD, MTD, currency conversion |

## Pattern Catalog

| Pattern | Use | Example |
|---|---|---|
| Base measure | One reusable numeric rule | `Sales Amount := SUM ( FactSales[SalesAmount] )` |
| Derived KPI | One business formula over governed base measures | `Margin % := DIVIDE ( [Margin], [Revenue] )` |
| Calculation group | Shared time or currency transformations | `SELECTEDMEASURE ()` wrapper |
| Aggregation table | Fast path for common report grain | Monthly totals by program and fund |
| Role-playing dimension | One table serves multiple date roles | Posted Date and Due Date |
| Ontology caption mapping | Semantic object carries governed term or definition | `ontologyClass` and `ontologyProperty` alignment from SAAM example |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Storage fit | Compare selected mode to latency and freshness targets. | Mode choice matches workload constraints. |
| Measure discipline | Review DAX against base facts and filter context. | Measures avoid hidden grain shifts and duplicated business logic. |
| Relationship clarity | Review filter directions and inactive links. | Each drill path remains explainable. |
| Security behavior | Test representative role filters. | Restricted users see only allowed rows and valid totals. |
| Vocabulary alignment | Compare captions and descriptions to glossary or ontology terms. | User-facing labels stay governed and consistent. |
| Fabric handoff | Review model against TMDL or BIM target. | Deployment assets reflect the approved design. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| DirectQuery lands on every table by default | Agent isolates the freshness-critical slice. |
| DAX logic bypasses governed base measures | Agent defines base measures first and derived KPIs second. |
| RLS filters fact tables before dimensions | Agent anchors security on conformed dimensions. |
| Calculation groups duplicate one-off report formulas | Agent centralizes repeatable transformations only. |
| Semantic captions drift from glossary or ontology labels | Agent reuses governed terms from the vocabulary source. |

## Reference Assets

| Resource | Role | Location |
|---|---|---|
| Guvi ontology article | Background for semantic meaning versus raw data storage | https://www.guvi.in/blog/ontologies-in-ai/ |
| SAAM Fabric semantic model | Real-world table, hierarchy, and ontology mapping example | `docs/references/SAAM/Ontology/saam-fabric-semantic-model.json` |
| SAAM JSON-LD | Vocabulary source for semantic captions and definitions | `docs/references/SAAM/Ontology/saam-core-jsonld.json` |
