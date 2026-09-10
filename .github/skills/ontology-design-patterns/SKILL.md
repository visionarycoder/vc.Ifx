---
name: ontology-design-patterns
title: Ontology Design Patterns
description: Design ontologies, RDF or OWL vocabularies, taxonomy hierarchies, and domain semantics when a prompt needs formal knowledge representation.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium-high
estimated_tokens: 1775
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - business-glossary-metadata
  - knowledge-graph-patterns
  - semantic-modeling-standards
  - terminology-normalization
appliesTo: '**/*.{md,ttl,owl,rdf,jsonld,sparql,yml,yaml}'
tags:
  - ontology
  - rdf
  - owl
  - taxonomy
  - knowledge-graph
---
# Ontology Design Patterns

Agent designs ontology assets with explicit classes, properties, individuals, axioms, and reasoning boundaries.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs a shared domain vocabulary with machine-readable semantics | Use this skill |
| Prompt needs RDF, RDFS, OWL, JSON-LD, Turtle, or SPARQL design | Use this skill |
| Prompt needs taxonomy hierarchy plus formal property rules | Use this skill |
| Prompt needs alignment across glossary terms, APIs, tables, and graph entities | Use this skill |
| Prompt needs a decision between taxonomy, ontology, and knowledge graph | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt needs business stewardship, owners, or catalog workflow only | Use `business-glossary-metadata` |
| Prompt needs graph traversals and store design more than semantic axioms | Use `knowledge-graph-patterns` |
| Prompt needs star-schema facts and dimensions for analytics | Use `semantic-modeling-standards` |
| Prompt needs naming cleanup with no formal semantics | Use `terminology-normalization` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Domain scope | Yes | Agent records bounded context, audience, and decision outcome. |
| Source concepts | Yes | Agent records schemas, glossaries, reports, APIs, or policy text. |
| Semantic depth | Yes | Agent records taxonomy-only, RDFS, or OWL-level reasoning depth. |
| Integration targets | No | Agent records graph store, Purview, Fabric semantic model, or API contract targets. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent inventories core concepts, source labels, and semantic conflicts. | Review concept intake. | Each core concept has one source anchor. |
| 2 | Agent defines classes, object properties, data properties, and identity rules. | Review ontology scaffold. | Each concept maps to one class, one property, or one controlled term set. |
| 3 | Agent adds individuals, axioms, and constraints only where the target questions need them. | Review reasoning layer. | Constraints map to explicit business meaning instead of storage convenience. |
| 4 | Agent selects the modeling depth from the decision matrix. | Compare problem shape to matrix rows. | Selected depth matches reuse, interoperability, and inference needs. |
| 5 | Agent maps alignments to glossary terms, graph nodes, and semantic-model labels. | Review alignment table. | Exact, broad, narrow, and related mappings stay explicit. |
| 6 | Agent defines competency questions and validation queries. | Review SPARQL or checklist output. | Target business questions resolve through named ontology terms. |

## Decision Matrix

| Need | Preferred Pattern | Reasoning Depth | Example |
|---|---|---|---|
| Hierarchical labels with broad and narrow relationships | SKOS taxonomy | Lightweight | `skos:prefLabel`, `skos:broader` |
| Shared domain vocabulary with classes and typed properties | RDF plus RDFS | Moderate | `saam:Fund rdfs:subClassOf saam:GovernmentalEntity` |
| Constraints, equivalence, transitivity, or disjointness | OWL ontology | High | `owl:equivalentClass`, `owl:TransitiveProperty` |
| Multi-hop traversal with lighter semantic governance | Property graph plus glossary crosswalk | Low | Neo4j entity graph with governed labels |
| Analytics labels and hierarchies with governed measures | Fabric semantic model mapped from ontology | Moderate | SAAM semantic model using ontologyClass and ontologyProperty |

## Pattern Catalog

| Pattern | Use | Example |
|---|---|---|
| Class hierarchy | Domain types inherit shared meaning | `saam:GovernmentalFund rdfs:subClassOf saam:Fund` |
| Object property | One resource links to another resource | `saam:appropriatedTo rdfs:domain saam:Fund` |
| Data property | One resource holds a literal fact | `saam:fundCode rdfs:range xsd:string` |
| Individual and assertion | One named instance carries typed facts | `saam:Fund001 rdf:type saam:GovernmentalFund` |
| Axiom boundary | Agent restricts invalid combinations | `owl:disjointWith` between conflicting classes |
| Alignment bridge | Cross-system terms resolve to one semantic concept | `owl:equivalentClass` or `skos:exactMatch` |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Concept coverage | Compare competency questions to ontology terms. | Each question resolves through explicit classes or properties. |
| Identity quality | Review URI, key, and naming rules. | No core class relies on ambiguous identity. |
| Hierarchy quality | Review subclass and broader or narrower chains. | Hierarchy expresses business meaning instead of folder shape. |
| Axiom discipline | Review constraints against real examples. | Axioms block invalid states without collapsing valid cases. |
| Integration fit | Compare ontology labels to graph and Fabric targets. | Downstream assets reuse governed semantic labels. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Table names become ontology classes with no semantic review | Agent separates storage artifacts from domain concepts. |
| Every synonym becomes exact equivalence | Agent distinguishes exact, broad, narrow, and related mappings. |
| OWL axioms land on a problem that only needs labels and hierarchy | Agent steps down to SKOS or RDFS depth. |
| Individuals appear before class and property rules stabilize | Agent defines schema first and example instances second. |
| Ontology replaces a simpler semantic layer with no reuse gain | Agent falls back to glossary or dimensional modeling patterns. |

## Reference Assets

| Resource | Role | Location |
|---|---|---|
| Guvi ontology article | Background for classes, properties, individuals, axioms, and reasoning | https://www.guvi.in/blog/ontologies-in-ai/ |
| SAAM JSON-LD | Real-world context and label patterns | `docs/references/SAAM/Ontology/saam-core-jsonld.json` |
| SAAM OWL | Real-world class and property design | `docs/references/SAAM/Ontology/saam-core-owl.xml` |
| SAAM Turtle | Real-world hierarchy and triple patterns | `docs/references/SAAM/Ontology/saam-core-turtle.txt` |
| SAAM Fabric semantic model | Ontology-to-analytics mapping example | `docs/references/SAAM/Ontology/saam-fabric-semantic-model.json` |
