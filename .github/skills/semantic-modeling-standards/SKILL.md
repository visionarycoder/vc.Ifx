---
name: semantic-modeling-standards
title: Semantic Modeling Standards
description: Design dimensional models, fact and dimension patterns, and governed analytics structures when a prompt needs star-schema, snowflake, or SCD-aware semantic modeling.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1840
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - business-glossary-metadata
  - fabric-semantic-model-builder
  - semantic-layer-fabric
  - terminology-normalization
appliesTo: '**/*.{md,sql,json,yml,yaml,tmdl,bim}'
tags:
  - dimensional-modeling
  - star-schema
  - snowflake-schema
  - scd
  - analytics
---
# Semantic Modeling Standards

Agent designs dimensional analytics models with explicit grain, conformed dimensions, fact types, and history rules.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs star-schema or snowflake-schema design | Use this skill |
| Prompt needs fact and dimension standards for analytics | Use this skill |
| Prompt needs additive, semi-additive, or non-additive measure handling | Use this skill |
| Prompt needs slowly changing dimension patterns | Use this skill |
| Prompt needs conformed dimensions across subject areas | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt needs DAX, RLS, or Fabric storage-mode design | Use `semantic-layer-fabric` |
| Prompt needs ontology, RDF, or linked-data semantics | Use `ontology-design-patterns` |
| Prompt needs business stewardship and glossary governance only | Use `business-glossary-metadata` |
| Prompt needs operational third-normal-form transaction design | Use relational application modeling patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Business process | Yes | Agent records the event or snapshot that defines fact grain. |
| Candidate measures | Yes | Agent records additive behavior and KPI rules. |
| Candidate dimensions | Yes | Agent records descriptive attributes, hierarchies, and reuse scope. |
| History policy | No | Agent records SCD handling, late-arriving facts, and restatement rules. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent defines the business process and one atomic grain per fact. | Review grain statement. | Fact grain reads as one measurable event or snapshot. |
| 2 | Agent classifies measures and fact types. | Review fact and measure matrix. | Each measure and fact table has one explicit semantic role. |
| 3 | Agent designs conformed dimensions, surrogate keys, hierarchies, and role-playing uses. | Review dimension scaffold. | Shared dimensions expose reusable business attributes. |
| 4 | Agent selects star, selective snowflake, bridge, or snapshot patterns from the decision matrix. | Compare requirements to matrix rows. | Relationship design matches query simplicity and reuse goals. |
| 5 | Agent assigns SCD handling and late-arriving-data rules. | Review history plan. | Each changing dimension has one declared history rule. |
| 6 | Agent defines reconciliation and duplicate-grain tests before semantic-layer build. | Review validation set. | Model quality remains measurable before deployment. |

## Decision Matrix

| Modeling Need | Preferred Pattern | Strength | Tradeoff | Example |
|---|---|---|---|---|
| High-clarity BI with simple joins | Star schema | Easy reporting and filter behavior | Attribute repetition grows | `FactSales` with conformed dimensions |
| Reused attribute groups across many dimensions | Selective snowflake | Centralized reuse | Query complexity grows | Geography split from customer |
| Many-to-many dimensional membership | Bridge table | Accurate weighting and membership | Allocation logic needs precision | Customer-to-segment bridge |
| Event-based measurements | Transaction fact | Strong detail and drill-down | Storage volume grows | Order-line sales |
| Period-end state tracking | Periodic snapshot fact | Trend analysis over time | Snapshot load volume grows | Daily balances |
| Milestone lifecycle tracking | Accumulating snapshot fact | Cycle-time analysis | Update logic grows | Claim or case lifecycle |

## Pattern Catalog

| Pattern | Use | Example |
|---|---|---|
| Transaction fact | Atomic business event | `FactSales(OrderLineKey, DateKey, CustomerKey, SalesAmount)` |
| Periodic snapshot | Repeated state at fixed intervals | `FactBalanceDaily(AccountKey, DateKey, EndingBalance)` |
| Accumulating snapshot | Milestone-tracked lifecycle | `FactClaimLifecycle(OpenDateKey, CloseDateKey, ResolutionDays)` |
| SCD Type 1 | Non-historic correction | Current display name overwrite |
| SCD Type 2 | Full attribute history | Effective dates and current flag |
| SCD Type 3 | Limited prior-state tracking | Current region and prior region |
| Conformed dimension | Shared slicing across facts | Common date, agency, or fund dimension |
| Bridge table | Many-to-many or weighted membership | Policy-to-insured allocation bridge |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Grain clarity | Review fact grain against source keys. | Grain resolves to one event or snapshot with no ambiguity. |
| Duplicate prevention | Review unique-key strategy at fact grain. | No unexplained duplicate-grain path remains. |
| Measure semantics | Review additive behavior and KPI formulas. | Each metric has one explicit aggregation rule. |
| Dimension conformance | Compare shared dimensions across facts. | Shared slices use the same keys and definitions. |
| History handling | Review SCD plan and effective-date fields. | Each changing dimension has one declared history rule. |
| Fabric handoff | Review mapping into semantic-model objects. | Semantic-layer inputs match the dimensional design. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Fact grain mixes header and line semantics | Agent splits facts or rewrites the grain. |
| Ratio metrics land as stored additive facts | Agent stores base components and derives ratios later. |
| Snowflake branches spread with no reuse value | Agent collapses back to a star shape. |
| SCD handling changes per table with no rule set | Agent assigns one history rule per dimension. |
| Bridge tables omit allocation or membership semantics | Agent records weighting and membership behavior explicitly. |

## Reference Assets

| Resource | Role | Location |
|---|---|---|
| Guvi ontology article | Background for semantic meaning beyond raw tables | https://www.guvi.in/blog/ontologies-in-ai/ |
| SAAM Fabric semantic model | Real-world dimensional objects, hierarchies, and keys | `docs/references/SAAM/Ontology/saam-fabric-semantic-model.json` |
| SAAM JSON-LD | Business-term anchors for dimensions and facts | `docs/references/SAAM/Ontology/saam-core-jsonld.json` |
