---
name: fabric-medallion-architecture
title: Fabric Medallion Architecture
description: Implement Microsoft Fabric bronze, silver, and gold data zones when lakehouse data quality and reuse need clear progressive refinement.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1355
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-lakehouses
  - fabric-data-factory-patterns
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - medallion
  - bronze
  - silver
  - gold
---

# Fabric Medallion Architecture



Agent implements medallion architecture in Fabric lakehouses and OneLake. Agent keeps raw truth, validated enrichment, and curated serving data in separate zones with explicit quality boundaries.



## When to Use



| User prompt | Use |

|---|---|

| User asks for bronze, silver, and gold design in Fabric | Use this skill |

| User asks for lakehouse zone organization | Use this skill |

| User asks for staged data quality refinement | Use this skill |

| User asks for OneLake medallion implementation | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for one-off table creation only | Use `fabric-lakehouses` |

| User asks for report-only semantic modeling | Use `fabric-powerbi-integration` |

| User asks for operational replication without transformation | Use `fabric-shortcuts-mirroring` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Source inventory | Yes | State raw feeds, operational systems, and inbound cadence. |

| Quality gates | Yes | State validation, standardization, and conformance checks. |

| Consumer groups | Yes | State engineering, analytics, and reporting consumers. |

| Zone retention | Yes | State history, replay, and cleanup expectations. |

| Serving targets | Recommended | State semantic models, reports, or ML feature outputs. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Raw source preservation drives the layer | Bronze | Bronze keeps source truth with limited transformation. |

| Standardization and conformance drive the layer | Silver | Silver aligns cleansing, joining, and business-ready structure. |

| Curated metrics or subject-area products drive the layer | Gold | Gold aligns data to reporting and high-value consumption. |

| Users ask to skip directly from raw to report | Full medallion path | Quality and reuse improve when each layer has a distinct purpose. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent maps sources to bronze landing paths. | Review ingestion paths and retention plan. | Raw data lands without destructive transformation. |

| 2 | Agent defines silver cleansing and conformance rules. | Review validation, deduplication, and key alignment logic. | Silver outputs remove known quality issues. |

| 3 | Agent defines gold subject-area outputs. | Review business measures, dimensions, and serving tables. | Gold outputs match one consumer-facing analytic purpose. |

| 4 | Agent aligns pipelines, notebooks, or jobs to each zone. | Review orchestration boundaries. | Each transform stage writes to the correct zone. |

| 5 | Agent validates lineage and replay behavior. | Trace one record from bronze to gold. | Zone transitions remain observable and repeatable. |

| 6 | Agent validates consumer access. | Read gold outputs from the serving workload. | Reports, models, or data products use the intended zone. |



## Verification Checklist



- [ ] Agent preserved raw source truth in bronze.

- [ ] Agent defined explicit cleansing and conformance logic in silver.

- [ ] Agent limited gold outputs to curated analytic use cases.

- [ ] Agent verified lineage from source to consumer zone.

- [ ] Agent validated at least one downstream read from gold.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Bronze tables receive business transformations | Agent keeps bronze close to source truth and moves shaping to silver. |

| Silver and gold serve the same ambiguous purpose | Agent writes one purpose statement for each zone and refactors outputs accordingly. |

| Gold becomes a copy of every upstream table | Agent curates only the data products that support analytic consumers. |

| Replay and lineage disappear across transformations | Agent preserves audit columns and stage-to-stage traceability. |

