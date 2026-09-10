---
name: saam-ontology
title: SAAM Ontology
description: Wrap the Washington State SAAM RDF and OWL ontology when a prompt needs semantic modeling, validation queries, or Fabric-aligned ontology integration.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1960
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ontology-design-patterns
  - knowledge-graph-patterns
  - semantic-modeling-standards
  - wa-state-saam
appliesTo: '**/*.{md,ttl,owl,rdf,jsonld,sparql,json,yml,yaml,cs}'
tags:
  - saam
  - ontology
  - rdf
  - sparql
  - semantic-web
  - washington-state
related_docs:
  - ../../../docs/references/SAAM/Ontology/saam-core-turtle.txt
  - ../../../docs/references/SAAM/Ontology/saam-fabric-semantic-model.json
  - ../wa-state-saam/SKILL.md
source_paths:
  - .github/skills/saam-ontology/ontology/saam-core.ttl
  - .github/skills/saam-ontology/references/ontology-usage-guide.md
---
# SAAM Ontology

Agent applies this skill to the Washington State accounting ontology packaged in `ontology/saam-core.ttl`.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs semantic modeling for Washington State SAAM, AFRS, or CGI Advantage data | Use this skill |
| Prompt needs SPARQL queries over the SAAM ontology classes, properties, or instance data | Use this skill |
| Prompt needs ontology-backed validation of funds, appropriations, object codes, transactions, or error records | Use this skill |
| Prompt needs Fabric semantic-model alignment from ontology classes and properties | Use this skill |
| Prompt needs RDF, OWL, Turtle, JSON-LD, or .NET graph integration for SAAM data | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt needs generic ontology design with no SAAM asset | Use `ontology-design-patterns` |
| Prompt needs graph traversal or graph-store selection more than SAAM vocabulary reuse | Use `knowledge-graph-patterns` |
| Prompt needs dimensional modeling only with no ontology contract | Use `semantic-modeling-standards` |
| Prompt needs policy interpretation, AFRS coding, or statewide accounting rules without RDF or OWL work | Use `wa-state-saam` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Business question | Yes | Record the reporting, validation, reconciliation, or lineage question. |
| Data scope | Yes | Record schema-only, instance, or merged graph scope. |
| SAAM coding scope | Yes | Record fund, appropriation, object code, revenue, agency, period, or batch coverage. |
| Integration target | Yes | Record SPARQL, .NET, Fabric, or pipeline target. |
| Validation outcome | No | Record ASK, SELECT, rule-set, or exception-export target. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent loads `ontology/saam-core.ttl` and identifies the active namespace set: `saam`, `afrs`, `cgi`, `dot`, and `err`. | Review namespace block. | Query or code uses the shipped namespaces. |
| 2 | Agent classifies the task as vocabulary lookup, data validation, semantic projection, or integration implementation. | Review the task matrix. | One task type and one target artifact are named. |
| 3 | Agent maps the business question to ontology classes, object properties, and datatype properties. | Review the mapping table. | Each required field resolves to named ontology terms. |
| 4 | Agent writes SPARQL patterns or .NET graph queries that answer the question directly. | Review query set or integration code. | Each query returns terms, links, or violations tied to the task. |
| 5 | Agent aligns ontology terms to Fabric tables, columns, hierarchies, and relationships when analytics output is in scope. | Review semantic-model mapping. | Fabric objects reuse ontology anchors. |
| 6 | Agent verifies the output against ontology structure and target rules. | Run SPARQL or inspect mapping assets. | Output stays semantically aligned and traceable. |

## Ontology Coverage Matrix

| Concept | Core Term | Role in the Ontology | Agent Focus |
|---|---|---|---|
| Fund | `saam:Fund` plus fund subclasses | Self-balancing accounting entity with named fund-type specialization. | Model fund identity and hierarchy. |
| Appropriation | `saam:Appropriation` | Legislative spending authority linked to fund and biennium. | Validate authority context and biennium joins. |
| ObjectCode | `saam:ObjectCode`, `saam:SubObject`, `saam:SubSubObject` | Classification hierarchy for expenditure and revenue coding. | Preserve code hierarchy and lineage. |
| RevenueSource | Revenue-source extension anchored to `saam:Revenue`, `saam:ProgramRevenue`, and `saam:GeneralRevenue` | Core TTL models revenue classes. Local implementations carry revenue-source code lists as extension triples or crosswalks. | Keep revenue-source codes governed without rewriting the core TTL. |
| FinancialTransaction | `saam:FinancialTransaction` and subclasses | Central event node linked to the main accounting dimensions. | Build validation and reporting joins from this hub. |
| AFRS and CGI operations | `afrs:*` and `cgi:*` terms | Operational layer for batches, errors, jobs, logs, and integration events. | Combine accounting semantics with exception lineage. |

## Fabric Integration Matrix

| Fabric Need | Ontology Anchor | Integration Pattern | Pass Target |
|---|---|---|---|
| Dimension table definition | `ontologyClass` values from the Fabric semantic-model JSON | Agent maps each Fabric table to a named ontology class. | Every curated dimension points to one ontology class. |
| Column semantics | `ontologyProperty` values in semantic-model columns | Agent maps key and attribute columns to ontology properties. | Each governed column references one ontology property or one documented derived term. |
| Relationship design | Object properties such as `saam:hasAppropriation` and `saam:belongsToFund` | Agent projects RDF links into tabular foreign-key relationships. | Relationship names preserve source semantics. |
| Hierarchy publication | `saam:ObjectCode` hierarchy and fund subtype hierarchy | Agent projects ontology hierarchy into Fabric hierarchies. | Fabric hierarchies reflect ontology structure. |
| Lineage and governance | Ontology base IRI plus workspace metadata | Agent publishes ontology IRIs beside Fabric metadata. | Analysts can trace a table or column back to one ontology term. |

## SPARQL Starter Set

| Scenario | Query Goal | Reference |
|---|---|---|
| Vocabulary inventory | List classes and properties in the shipped TTL. | `references/ontology-usage-guide.md#vocabulary-inventory-queries` |
| Appropriation lineage | Walk appropriation-to-fund and appropriation-to-biennium links. | `references/ontology-usage-guide.md#appropriation-and-fund-queries` |
| Transaction validation | Find transactions that miss fund, appropriation, object code, or period links. | `references/ontology-usage-guide.md#transaction-validation-queries` |
| Revenue-source extension | Query extension triples that classify revenue source codes. | `references/ontology-usage-guide.md#revenue-source-extension-pattern` |
| Fabric alignment | Inspect classes and properties used by the semantic model. | `references/ontology-usage-guide.md#fabric-semantic-model-alignment` |

## Reference Files

| File | Purpose |
|---|---|
| [references/ontology-usage-guide.md](references/ontology-usage-guide.md) | SPARQL recipes, validation patterns, Fabric alignment steps, and .NET integration examples. |
| [ontology/saam-core.ttl](ontology/saam-core.ttl) | Local copy of the shipped SAAM core Turtle ontology for direct packaging and reuse. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Frontmatter validity | Run `npm run frontmatter:validate`. | Frontmatter validation returns exit code `0`. |
| Ontology asset presence | Read `.github/skills/saam-ontology/ontology/saam-core.ttl`. | File exists and matches the source TTL content. |
| Namespace consistency | Review SPARQL prefixes and .NET namespace map. | Prefixes resolve to the shipped ontology IRIs. |
| Concept coverage | Compare task terms to `Fund`, `Appropriation`, `ObjectCode`, `RevenueSource`, and `FinancialTransaction` guidance. | Each requested concept maps to a documented modeling or extension path. |
| Fabric alignment | Review semantic-model mapping notes and sample queries. | Tables, columns, and relationships reuse ontology anchors. |
| Validation readiness | Review ASK or SELECT validation rules in the reference guide. | At least one executable pattern exists for the target validation scope. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Treating the shipped TTL as transaction data | Agent separates schema triples from instance triples and loads both only when instance validation is in scope. |
| Replacing ontology terms with local abbreviations in Fabric | Agent preserves ontology IRIs and labels, then adds local display names as mapped metadata. |
| Flattening object-code hierarchy into one text field | Agent keeps `ObjectCode`, `SubObject`, and `SubSubObject` as explicit hierarchy levels. |
| Modeling revenue-source codes as free text with no semantic anchor | Agent links revenue-source code lists to revenue classes through extension triples or governed crosswalk tables. |
| Mixing AFRS, CGI, and SAAM terms without namespace boundaries | Agent keeps prefixes explicit and preserves the source namespace on each term. |
| Writing validation rules with no observable output | Agent returns ASK results, exception rows, or mapped Fabric artifacts with named pass criteria. |
