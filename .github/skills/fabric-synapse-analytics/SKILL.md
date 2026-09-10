---
name: fabric-synapse-analytics
title: Fabric and Synapse Analytics
description: Plan Microsoft Fabric and Synapse Analytics interoperability across workspaces, SQL pools, Spark pools, and notebooks when workloads span both platforms.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1365
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-onelake-patterns
  - fabric-notebooks-spark
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - synapse
  - sql-pools
  - spark-pools
---

# Fabric and Synapse Analytics



Agent plans interoperable analytics designs across Microsoft Fabric and Azure Synapse Analytics. Agent aligns SQL pools, Spark pools, notebooks, and OneLake or ADLS access with workload boundaries.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Fabric and Synapse coexistence | Use this skill |

| User asks for Synapse Spark or SQL pool alignment with Fabric | Use this skill |

| User asks for OneLake access from Synapse | Use this skill |

| User asks for migration or shared notebook patterns across platforms | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for Fabric-only lakehouse design | Use `fabric-lakehouses` |

| User asks for Fabric-only notebook authoring | Use `fabric-notebooks-spark` |

| User asks for Power BI semantic modeling only | Use `fabric-powerbi-integration` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Platform boundary | Yes | State which workloads stay in Synapse and which move to Fabric. |

| Compute pattern | Yes | State SQL pool, Spark pool, notebook, or mixed execution. |

| Storage path | Yes | State OneLake, ADLS Gen2, warehouse, or lakehouse location. |

| Migration posture | Yes | State coexistence, phased cutover, or Fabric-first greenfield. |

| Consumer expectations | Recommended | State BI, data science, or engineering outcomes. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Existing Synapse workloads stay active during adoption | Coexistence pattern | A phased path reduces disruption and preserves current delivery. |

| Shared open data drives both platforms | OneLake or ADLS-aligned storage | Open storage keeps data reachable from both engines. |

| Fabric notebook and lakehouse features become the primary engineering path | Fabric-first pattern | Fabric consolidates storage and workload experiences. |

| Dedicated Synapse SQL or Spark pool capabilities remain central | Hybrid pattern | Platform-specific compute stays where its strengths matter most. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent maps workloads to Fabric, Synapse, or hybrid placement. | Review current jobs, pools, and user groups. | Each workload has one owning platform path. |

| 2 | Agent aligns storage and data sharing strategy. | Review OneLake, ADLS, and export boundaries. | Data moves through open, supportable access paths. |

| 3 | Agent aligns notebook and compute execution. | Review Spark pool, Fabric notebook, and SQL execution patterns. | Each compute step runs on the platform that fits the requirement. |

| 4 | Agent defines migration or coexistence controls. | Review cutover plan and dependency map. | Platform transitions preserve lineage and access. |

| 5 | Agent validates cross-platform reads or writes. | Execute a representative data access path. | Fabric and Synapse resolve the intended shared data. |

| 6 | Agent validates downstream consumers. | Review BI or ML workloads on the chosen outputs. | Consumers read from the governed target platform. |



## Verification Checklist



- [ ] Agent defined explicit boundaries between Fabric and Synapse workloads.

- [ ] Agent aligned storage to shared open access patterns.

- [ ] Agent validated at least one notebook or query flow on each required platform.

- [ ] Agent documented migration or coexistence assumptions.

- [ ] Agent checked consumer access on the final target outputs.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Fabric and Synapse both own the same workload without boundaries | Agent writes one platform owner for each major workload. |

| Data copies multiply across both platforms without purpose | Agent prefers shared open storage before export duplication. |

| Notebook migration starts without library and runtime review | Agent inventories runtime dependencies before cutover. |

| Consumer tools switch before semantic or data parity exists | Agent validates downstream parity before platform retirement. |

