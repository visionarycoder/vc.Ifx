---
name: semantic-data-bundle
title: Semantic Data Bundle
description: Route semantic data requests across ontology design, semantic layers, glossaries, knowledge graphs, terminology normalization, NLP-to-query, and semantic modeling.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 930
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ontology-design-patterns
  - semantic-layer-fabric
  - business-glossary-metadata
  - terminology-normalization
  - knowledge-graph-patterns
  - nlp-to-query-translation
  - semantic-modeling-standards
appliesTo: '**/*.{md,json,yml,yaml,tmdl,tmsl,dax,kql,sql}'
tags:
  - semantic-data
  - ontology
  - fabric
  - knowledge-graph
  - glossary
  - nlp
---
# Semantic Data Bundle

Agent uses this bundle for semantic data work spanning terminology, ontology structure, semantic models, graph patterns, and natural-language query translation.

## Activation

| Prompt Signal | Use Bundle | Primary Route |
|---|---|---|
| "ontology", "taxonomy", "class model", "controlled vocabulary" | Yes | `ontology-design-patterns` |
| "semantic model", "Fabric model", "Power BI model", "metrics layer" | Yes | `semantic-layer-fabric` |
| "business glossary", "term catalog", "definition governance" | Yes | `business-glossary-metadata` |
| "term harmonization", "synonym cleanup", "canonical naming" | Yes | `terminology-normalization` |
| "entity relationships across domains", "graph traversal", "knowledge graph" | Yes | `knowledge-graph-patterns` |
| "natural language to SQL", "NLQ", "query translation" | Yes | `nlp-to-query-translation` |
| "modeling standards", "naming standards", "semantic consistency" | Yes | `semantic-modeling-standards` |

## When to Use

| Request Pattern | Use |
|---|---|
| Multi-step semantic architecture or governance request | Use this bundle |
| Decision needed across glossary, ontology, semantic model, and graph layers | Use this bundle |
| Fabric + Purview + graph integration request | Use this bundle |

## When Not to Use

| Request Pattern | Route |
|---|---|
| Single glossary curation task only | `business-glossary-metadata` |
| Single Fabric semantic model task only | `semantic-layer-fabric` |
| Single NLQ prompt translation task only | `nlp-to-query-translation` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Data domain | Yes | Finance, operations, HR, supply chain, or other bounded scope |
| Consumer pattern | Yes | Analytics, governance, search, application, or conversational query |
| Platform context | No | Fabric, Purview, graph database, SQL engine, BI client |
| Output target | No | Glossary, ontology, semantic model, graph schema, NLQ interface |

## Coverage Matrix

| Concern | Specialist Skill | Primary Output |
|---|---|---|
| Class, property, and ontology reuse patterns | `ontology-design-patterns` | Ontology structure |
| Fabric-ready semantic layer design | `semantic-layer-fabric` | Measures, dimensions, model contracts |
| Business term stewardship and metadata | `business-glossary-metadata` | Governed glossary |
| Canonical term alignment and synonym reduction | `terminology-normalization` | Normalized vocabulary |
| Multi-hop relationship modeling and graph traversal design | `knowledge-graph-patterns` | Graph schema and patterns |
| Natural-language intent mapping to executable query plans | `nlp-to-query-translation` | NLQ translation flow |
| Cross-model naming, granularity, and semantic consistency | `semantic-modeling-standards` | Modeling standards |

## Decision Order

| Primary Need | Choose First | Reason | Escalate Next |
|---|---|---|---|
| Controlled definitions and shared business meaning | Business glossary | Definitions stabilize language before modeling. | Ontology |
| Formal concept hierarchy and reusable meaning across systems | Ontology | Ontology encodes classes, properties, and constraints. | Semantic model |
| Curated measures, dimensions, and analytic consumption | Semantic model | Semantic model optimizes reporting and metric use. | NLQ translation |
| Relationship-centric exploration across many entity types | Knowledge graph | Graph design optimizes connected traversal and discovery. | Ontology |
| Natural-language access to governed analytics | Semantic model | Stable metrics and dimensions ground query translation. | NLP-to-query |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies the request by semantic layer. | Agent maps the request to glossary, ontology, model, graph, or NLQ. | Every requested concern has one primary layer. |
| 2 | Agent selects the first specialist skill from the decision order. | Agent records the selected skill. | One starting skill exists. |
| 3 | Agent adds dependent skills for adjacent layers only. | Agent checks handoff count. | Routing includes no unrelated skills. |
| 4 | Agent aligns terminology across all chosen layers. | Agent compares canonical terms across outputs. | Core entities and measures use one vocabulary. |
| 5 | Agent verifies platform fit for Fabric, Purview, and graph storage. | Agent checks integration mapping. | Each target platform has a defined role. |

## Linguistic Cross-Over Patterns

| Stage | Input Artifact | Transformation | Output Artifact | Primary Skill |
|---|---|---|---|---|
| 1 | Raw tables, files, events | Extract candidate nouns, verbs, measures, codes | Source term inventory | `terminology-normalization` |
| 2 | Source term inventory | Resolve synonyms, abbreviations, collisions | Canonical terminology set | `business-glossary-metadata` |
| 3 | Canonical terminology set | Formalize concepts, relations, constraints | Ontology classes and properties | `ontology-design-patterns` |
| 4 | Ontology classes and properties | Shape facts, dimensions, metrics, hierarchies | Semantic model | `semantic-layer-fabric` |
| 5 | Semantic model | Map intents, entities, filters, metrics to query grammar | NLQ contract | `nlp-to-query-translation` |
| 6 | NLQ contract | Execute governed query against model or graph | Natural-language query experience | `semantic-modeling-standards` |

## Microsoft Fabric Integration Guidance

| Platform | Role | Integration Pattern |
|---|---|---|
| Fabric semantic model | Analytics serving layer | Publish curated dimensions, facts, measures, and metric definitions from governed terminology. |
| Microsoft Purview | Metadata and lineage authority | Register glossary terms, data products, lineage links, and semantic ownership. |
| Graph database | Relationship exploration layer | Persist high-cardinality entity relationships, cross-domain links, and traversal-heavy use cases. |
| Bundle orchestration | Cross-platform alignment | Keep business terms stable in Purview, map them to Fabric model objects, and link graph entities to the same canonical identifiers. |

## Verification Matrix

| Concern | Test | Pass |
|---|---|---|
| Terminology | Compare canonical terms against source synonyms. | One approved term exists per concept. |
| Ontology | Inspect class and property reuse. | Duplicate concept structures do not exist. |
| Semantic model | Inspect metric grain and dimension conformance. | Measures resolve against one declared grain. |
| Knowledge graph | Inspect node and edge intent. | Every edge answers a real traversal question. |
| NLQ | Trace prompt to governed metric and filter mapping. | Query translation resolves to explicit semantic objects. |
| Integration | Inspect Fabric, Purview, and graph identifier mapping. | Canonical identifiers stay consistent across platforms. |

## Verification Checklist

- [ ] Agent routed each concern to one primary specialist skill.
- [ ] Agent applied the decision order before adding adjacent layers.
- [ ] Agent aligned glossary, ontology, model, graph, and NLQ terms.
- [ ] Agent defined Fabric, Purview, and graph roles.
- [ ] Agent verified canonical identifier consistency.

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Start with a graph before term cleanup | Normalize terminology first. |
| Build a semantic model without ontology or glossary alignment | Reconcile canonical concepts before measure design. |
| Use ontology as a reporting layer | Route analytics consumption to the semantic model. |
| Use Fabric model for deep relationship traversal | Route traversal-heavy use cases to the graph layer. |
| Translate NLQ directly from raw schema names | Translate from governed business terms and semantic objects. |
