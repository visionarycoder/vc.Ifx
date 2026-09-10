---
name: knowledge-graph-patterns
title: Knowledge Graph Patterns
description: Model knowledge graphs, entity relationships, graph stores, and traversal queries when a prompt needs Neo4j, Gremlin, RDF, or graph-centric design.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1810
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ontology-design-patterns
  - semantic-modeling-standards
  - terminology-normalization
  - xml-schema-inference
appliesTo: '**/*.{md,cypher,gremlin,sparql,ttl,owl,json,yml,yaml}'
tags:
  - knowledge-graph
  - neo4j
  - gremlin
  - rdf
  - graph
---
# Knowledge Graph Patterns

Agent designs knowledge-graph structures, graph-store choices, and traversal query patterns with explicit entity and relationship semantics.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs multi-hop entity relationships and graph traversals | Use this skill |
| Prompt needs property graph or RDF graph design | Use this skill |
| Prompt needs Neo4j Cypher, Gremlin, or SPARQL query scaffolds | Use this skill |
| Prompt needs graph-store selection versus relational or document alternatives | Use this skill |
| Prompt needs a knowledge graph seeded from ontology or glossary assets | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt needs taxonomy depth and semantic axioms more than traversal patterns | Use `ontology-design-patterns` |
| Prompt needs star-schema reporting and additive measures | Use `semantic-modeling-standards` |
| Prompt needs naming cleanup only | Use `terminology-normalization` |
| Prompt needs one-hop CRUD tables with no graph question | Use relational modeling patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entity types | Yes | Agent records node or resource categories. |
| Relationship questions | Yes | Agent records traversal questions before storage design. |
| Update pattern | Yes | Agent records merge, dedupe, and freshness behavior. |
| Query surface | No | Agent records Cypher, Gremlin, SPARQL, or API-wrapper targets. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent captures entities, edge semantics, and target traversal questions. | Review question matrix. | Each target question depends on named entities and edges. |
| 2 | Agent selects property graph, RDF graph, hybrid, or non-graph shape from the decision matrix. | Compare workload to matrix rows. | Store choice matches traversal depth and interoperability needs. |
| 3 | Agent defines node identity, edge cardinality, provenance, and merge rules. | Review graph schema. | Each entity and edge has one identity or dedupe rule. |
| 4 | Agent writes Cypher, Gremlin, or SPARQL patterns for the target paths. | Review query set. | Queries answer the target graph questions directly. |
| 5 | Agent maps glossary and ontology terms into labels, edge names, or RDF predicates. | Review vocabulary crosswalk. | Graph labels stay governed and consistent. |
| 6 | Agent defines verification paths for traversal correctness, duplicate suppression, and tabular projection. | Review validation pack. | Sample traversals and exports return intended results. |

## Decision Matrix

| Need | Preferred Model | Strength | Tradeoff | Example |
|---|---|---|---|---|
| Operational traversals over labeled nodes and edges | Property graph | Fast path navigation | Semantic interoperability stays lighter | Neo4j or Gremlin customer-to-order graph |
| Standards-based linked data and semantic reuse | RDF graph | Shared vocabulary and reasoning | Authoring complexity grows | SAAM Turtle or OWL graph |
| Both deep traversal and shared semantic contracts | Hybrid ontology plus property graph | Strong semantics plus performant navigation | Synchronization work grows | OWL vocabulary with Neo4j projection |
| Aggregated facts by fixed grain | Relational star schema | Strong analytic performance | Multi-hop traversal stays awkward | Fact and dimension warehouse |
| Nested records with weak relationship reuse | Document store | Flexible ingestion | Cross-document paths stay expensive | JSON profile store |

## Pattern Catalog

| Pattern | Use | Example |
|---|---|---|
| Canonical node | One business entity spans many source keys | `(:Customer {canonicalId:'C123'})` |
| Provenanced edge | Relationship source and confidence stay visible | `[:MATCHED_TO {source:'CRM', confidence:0.92}]` |
| Cypher traversal | Path analysis in Neo4j | `MATCH p=(c:Customer)-[:PLACED]->(:Order)-[:FULFILLED_BY]->(w:Warehouse) RETURN p` |
| Gremlin traversal | Property-graph traversal in TinkerPop stores | `g.V().has('Customer','id','123').out('PLACED').out('FULFILLED_BY')` |
| SPARQL pattern | RDF graph lookup | `SELECT ?warehouse WHERE { ?order saam:fulfilledBy ?warehouse . }` |
| Graph-to-tabular projection | Graph facts feed analytics outputs | Customer-to-segment bridge export |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Traversal relevance | Review target questions against edge set. | Every key edge supports at least one named traversal. |
| Identity quality | Review canonical keys and merge rules. | Duplicate entities resolve consistently. |
| Query correctness | Run or review sample Cypher, Gremlin, or SPARQL queries. | Queries return intended paths or neighborhoods. |
| Vocabulary alignment | Compare graph labels to glossary or ontology terms. | Labels and predicates reuse governed names. |
| Projection fit | Review graph-to-Fabric or warehouse export notes. | Downstream consumers receive stable tabular shapes. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Graph storage lands on a problem with one-hop joins only | Agent falls back to a simpler store. |
| Edge names reflect report labels instead of domain relationships | Agent rewrites edges around business semantics. |
| Duplicate nodes persist across source systems | Agent defines canonical identity and merge rules. |
| Traversal examples mix uncontrolled vocabularies | Agent normalizes labels before query design. |
| Graph design ignores downstream semantic-model consumers | Agent adds graph-to-tabular projection rules. |

## Reference Assets

| Resource | Role | Location |
|---|---|---|
| Guvi ontology article | Background for ontology versus knowledge-graph boundaries | https://www.guvi.in/blog/ontologies-in-ai/ |
| SAAM Turtle | Real-world RDF graph vocabulary | `docs/references/SAAM/Ontology/saam-core-turtle.txt` |
| SAAM OWL | Real-world semantic contract for graph seeding | `docs/references/SAAM/Ontology/saam-core-owl.xml` |
| SAAM Fabric semantic model | Example of graph-derived analytics projection targets | `docs/references/SAAM/Ontology/saam-fabric-semantic-model.json` |
