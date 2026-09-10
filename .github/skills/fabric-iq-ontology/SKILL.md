---
name: fabric-iq-ontology
title: Fabric IQ Ontology
description: Design Microsoft Fabric IQ ontologies with entity types, relationships, and governed business semantics when agents or users need a knowledge graph over enterprise data.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1360
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-copilot-studio
  - data-lineage
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - iq
  - ontology
  - graph
---

# Fabric IQ Ontology



Agent designs Fabric IQ ontologies that connect business entities, relationships, and logic into a governed knowledge graph. Agent aligns ontology structure with data binding and downstream agent use.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Fabric IQ ontology design | Use this skill |

| User asks for business entity modeling in Fabric | Use this skill |

| User asks for governed relationship graphs over data | Use this skill |

| User asks for agent grounding through ontology | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for transactional relational schema design | Use database design guidance |

| User asks for report-only metrics without entity graphs | Use `fabric-powerbi-integration` |

| User asks for simple table lineage only | Use `data-lineage` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Business domain | Yes | State the core entities, terms, and decision context. |

| Source systems | Yes | State the tables, models, or items that bind into the ontology. |

| Relationship rules | Yes | State how entities connect and what the edge meaning is. |

| Consumer type | Yes | State agent, analyst, or graph query consumer. |

| Governance expectations | Recommended | State owner, approval, and term stewardship rules. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Stable business nouns drive analysis | Entity types in ontology | Entity types capture governed business meaning beyond table names. |

| Cross-domain links drive discovery or agent reasoning | Relationship types | Relationship types expose how entities connect across systems. |

| Downstream agent grounding drives the solution | Ontology-backed data agent | Fabric IQ semantics improve agent context and traceable meaning. |

| Requirement is raw graph storage without semantic governance | Route to graph data modeling guidance | Ontology work starts from business semantics, not storage only. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent identifies domain entities and owned business terms. | Review nouns, definitions, and owners. | Each core entity has one clear business meaning. |

| 2 | Agent defines relationship types and graph intent. | Review edge names and direction. | Each relationship expresses one stable business fact. |

| 3 | Agent binds ontology types to Fabric data sources. | Review source mappings and key fields. | Entities and relationships resolve to authoritative data. |

| 4 | Agent enriches ontology with attributes and logic. | Review descriptive fields and rule notes. | Consumers receive enough context for discovery and grounding. |

| 5 | Agent validates preview and query behavior. | Inspect ontology preview or graph result. | Entity and relationship views match the planned business model. |

| 6 | Agent validates downstream consumption. | Test a data agent or graph query against the ontology. | Consumers retrieve grounded, connected business context. |



## Verification Checklist



- [ ] Agent defined entity types from business terms, not raw table names alone.

- [ ] Agent defined relationship types with stable meaning and direction.

- [ ] Agent bound ontology elements to authoritative Fabric sources.

- [ ] Agent validated ontology preview or query output.

- [ ] Agent tested one downstream consumer such as a data agent or graph query.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Ontology mirrors source schemas without business abstraction | Agent raises entities and relationships to business terms first. |

| Relationship names hide direction or meaning | Agent uses explicit edge semantics and validation examples. |

| Source bindings lack ownership or key stability | Agent records authoritative sources and join keys for each type. |

| Agent grounding starts before ontology coverage exists | Agent validates entity and relationship completeness before rollout. |

