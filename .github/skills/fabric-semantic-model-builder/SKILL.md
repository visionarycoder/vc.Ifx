---
name: fabric-semantic-model-builder
title: Fabric Semantic Model Builder
description: Diagnose source-schema readiness for Microsoft Fabric semantic models and produce evidence-backed fact, dimension, relationship, and measure scaffolds.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1560
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-lakehouse-ingestion
  - fabric-kql-diagnostics
appliesTo: '**/*.{csv,json,bim,tmdl,dax,md}'
tags:
  - fabric
  - semantic-model
  - power-bi
  - tmdl
  - modeling
---
# Fabric Semantic Model Builder

Agent diagnoses schema readiness for a Microsoft Fabric or Power BI semantic model and produces a minimal model design.

## Use When

Agent uses this skill when:
- User provides table schemas and needs fact and dimension classification
- User needs relationship inference for a star schema
- User needs DAX measure scaffolds from finished source tables
- User needs TMDL or BIM-aligned model definitions after schema triage

## Do Not Use When

Agent does not use this skill when:
- User needs pipeline failure triage
- User needs Lakehouse ingestion troubleshooting
- User needs KQL query diagnostics
- User needs visual design advice without source-schema evidence

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Table list and columns | Yes | Agent uses names, types, and nullability first |
| Candidate keys | No | Agent verifies relationships more accurately when keys exist |
| Business grain | No | Agent uses grain to separate facts from dimensions |
| Requested output | No | Agent tailors output for report, DAX, TMDL, or BIM |

## Modeling Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Capture schema evidence | Agent records tables, column types, nullability, and naming patterns. | Agent verifies that every table has a column inventory. | Intake covers all tables with enough metadata for classification. |
| 2. Classify tables | Agent classifies each table as fact, dimension, date dimension, bridge, or unknown. | Agent verifies numeric measures, foreign-key density, and descriptive attributes. | Every table receives one classification and one confidence level. |
| 3. Verify relationships | Agent matches foreign keys to candidate primary keys and role-playing dimensions. | Agent verifies data type compatibility and naming alignment for each relationship. | Relationship map contains only evidence-backed links. |
| 4. Produce semantic scaffolds | Agent produces minimal DAX measures and model objects for the verified tables. | Agent verifies that each measure maps to a fact column and each relationship maps to a business grain. | Output contains only measures and objects supported by the source schema. |
| 5. Produce verification plan | Agent adds post-build tests for row counts, filter propagation, and role-playing behavior. | Agent verifies one measurable test per major model object. | Verification plan confirms that the model behaves as designed. |

## Classification Heuristics

| Signal | Likely classification |
|---|---|
| Table has many foreign keys and additive numeric columns | Fact |
| Table has a surrogate key, name, code, or category columns | Dimension |
| Table centers on calendar attributes and date keys | Date dimension |
| Table resolves many-to-many joins between business entities | Bridge |
| Table lacks stable keys or business grain | Unknown |

## Relationship Rules

| Rule | Agent action |
|---|---|
| Matching key names and compatible types | Agent infers a direct relationship |
| Multiple date foreign keys in one fact | Agent infers role-playing date dimensions |
| Two facts connect through shared business entities only | Agent recommends shared dimensions, not direct fact-to-fact joins |
| Many-to-many evidence appears | Agent flags a bridge requirement instead of forcing a direct relationship |

## Verification Targets

| Target | Test | Pass |
|---|---|---|
| Table classification | Agent verifies that each table has a single dominant grain | No table remains unclassified without an explicit `Unknown` note |
| Relationship quality | Agent verifies compatible types and matching business meaning | Each relationship is explainable from source keys and grain |
| Measure quality | Agent verifies that each DAX measure targets a fact column | No measure references a dimension attribute as an additive base |
| Measurable outcome | Agent verifies a model success target | Fact tables filter through conformed dimensions and core measures return expected aggregates on sample verification queries |

## Output Contract

| Output | Contents |
|---|---|
| Model design summary | Table classifications, grain notes, and relationship decisions |
| Relationship map | Fact-to-dimension links, cardinality, and role names |
| Measure scaffold | Minimal DAX for totals, counts, and date-aware measures |
| Deployment scaffold | TMDL or BIM-aligned structure when the schema is ready |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent infers relationships from names only | Agent also verifies type compatibility and grain |
| Agent creates direct fact-to-fact joins | Agent introduces shared dimensions or a bridge requirement |
| Agent generates measures before fact classification | Agent classifies facts first |
| Agent omits model verification | Agent adds filter and aggregate verification targets |
