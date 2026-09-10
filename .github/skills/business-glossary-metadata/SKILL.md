---
name: business-glossary-metadata
title: Business Glossary Metadata
description: Structure business glossaries, metadata catalogs, and data dictionaries when a prompt needs standardized terms, ownership, and asset mappings.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1710
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - data-lineage
  - ontology-design-patterns
  - semantic-layer-fabric
  - terminology-normalization
appliesTo: '**/*.{md,json,csv,yml,yaml}'
tags:
  - glossary
  - metadata
  - catalog
  - purview
  - governance
---
# Business Glossary Metadata

Agent structures glossary, catalog, and data-dictionary assets with explicit ownership, definition, lineage, and lifecycle fields.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs business term definitions, owners, or steward workflow | Use this skill |
| Prompt needs a metadata catalog or data dictionary structure | Use this skill |
| Prompt needs standard terms linked to reports, tables, APIs, or notebooks | Use this skill |
| Prompt needs Purview-ready term, asset, and classification mapping | Use this skill |
| Prompt needs glossary terms coordinated with ontology or semantic-model labels | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt needs formal classes, axioms, or RDF or OWL semantics | Use `ontology-design-patterns` |
| Prompt needs synonym cleanup and canonical naming only | Use `terminology-normalization` |
| Prompt needs DAX, calculation groups, or Fabric storage design | Use `semantic-layer-fabric` |
| Prompt needs fact and dimension schema design | Use `semantic-modeling-standards` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Domain areas | Yes | Agent records business domain, owner, and steward. |
| Source assets | Yes | Agent records tables, reports, APIs, notebooks, or files. |
| Governance scope | Yes | Agent records approval path, review rhythm, and exception handling. |
| Integration targets | No | Agent records Purview, Fabric, catalog exports, or ontology links. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent inventories candidate terms, existing definitions, and source assets. | Review intake matrix. | Each term has at least one source anchor. |
| 2 | Agent assigns canonical name, definition, owner, steward, and domain. | Review glossary schema. | Each governed term has one accountable owner and one steward path. |
| 3 | Agent links business terms to physical assets, metrics, and reports. | Review asset crosswalk. | Critical assets map to governed terms. |
| 4 | Agent records synonyms, deprecated labels, usage notes, and lineage links. | Review lifecycle table. | Alternate labels stay searchable and traceable. |
| 5 | Agent selects glossary, dictionary, catalog, or hybrid structure from the decision matrix. | Compare needs to matrix rows. | Chosen structure matches audience and operational scope. |
| 6 | Agent defines verification checks for ownership, coverage, and Fabric alignment. | Review validation checklist. | Coverage gaps remain measurable. |

## Decision Matrix

| Need | Preferred Asset | Primary Audience | Example |
|---|---|---|---|
| Business meaning and accountability | Business glossary | Domain owners and analysts | `Net Revenue` with owner and steward |
| Column-level technical metadata | Data dictionary | Engineers and data stewards | `fact_sales.net_amount decimal(18,2)` |
| Searchable asset inventory across platforms | Metadata catalog | Platform and governance teams | Semantic model, lakehouse, notebook, API |
| Formal semantic meaning and inference | Ontology link plus glossary | Semantic architects | Glossary term mapped to `saam:Fund` |
| Report-facing labels and KPI names | Semantic-model label set | Analysts and report authors | Measure caption and description set |

## Pattern Catalog

| Pattern | Use | Example |
|---|---|---|
| Glossary term record | Business definition with accountability | `Term, Definition, Owner, Steward, Domain` |
| Data dictionary row | Physical field metadata | `Asset, Column, Type, Nullability, Description` |
| Asset crosswalk | One term spans many assets | `Net Revenue -> column -> measure -> report card` |
| Lifecycle flag | Active, deprecated, or retired status | `status: deprecated` |
| Synonym register | Alternate labels stay discoverable | `Client -> Customer` |
| Ontology reference | Glossary term maps to formal class or property | `Fund -> saam:Fund` |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Ownership coverage | Review owner and steward fields for priority terms. | Critical terms have accountable roles. |
| Definition quality | Compare adjacent terms and near-duplicates. | Definitions separate business meaning clearly. |
| Asset mapping | Sample governed terms across systems. | Sampled terms link to physical assets consistently. |
| Lifecycle visibility | Review deprecated and synonym entries. | Historic labels remain searchable. |
| Fabric alignment | Compare glossary terms to semantic-model captions and Purview labels. | Fabric surfaces reuse governed terminology. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Glossary entries copy column names with no business meaning | Agent separates business definition from technical metadata. |
| Owner field names a team with no decision path | Agent records an accountable role and steward route. |
| Deprecated labels disappear from search | Agent preserves aliases and lifecycle status. |
| Catalog assets exist with no glossary linkage | Agent maps high-value assets back to business terms. |
| Ontology and glossary drift into separate vocabularies | Agent binds glossary terms to formal semantic anchors where needed. |

## Reference Assets

| Resource | Role | Location |
|---|---|---|
| Guvi ontology article | Background for shared meaning across systems | https://www.guvi.in/blog/ontologies-in-ai/ |
| SAAM JSON-LD | Real-world labels and definitions for glossary seeding | `docs/references/SAAM/Ontology/saam-core-jsonld.json` |
| SAAM Fabric semantic model | Real-world term-to-asset mapping examples | `docs/references/SAAM/Ontology/saam-fabric-semantic-model.json` |
