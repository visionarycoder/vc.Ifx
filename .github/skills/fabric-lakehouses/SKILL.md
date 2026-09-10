---
name: fabric-lakehouses
title: Fabric Lakehouses
description: Design Microsoft Fabric lakehouses with Delta tables, OneLake layout, and lakehouse-versus-warehouse choices when analytics needs open data storage.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1320
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-onelake-patterns
  - fabric-medallion-architecture
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - lakehouse
  - delta
  - onelake
---

# Fabric Lakehouses



Agent designs Fabric lakehouses for open-format analytics. Agent aligns Delta tables, Files and Tables layout, and OneLake access with the workload shape.



## When to Use



| User prompt | Use |

|---|---|

| User asks for a Fabric lakehouse design | Use this skill |

| User asks for Delta Lake tables in Fabric | Use this skill |

| User asks whether a lakehouse or warehouse fits better | Use this skill |

| User asks for OneLake-native table organization | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for warehouse-only SQL serving | Use warehouse guidance |

| User asks for pipeline orchestration only | Use `fabric-data-factory-patterns` |

| User asks for medallion zone design across domains | Use `fabric-medallion-architecture` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Data shape | Yes | State structured tables, semi-structured files, or mixed assets. |

| Primary engine | Yes | State Spark, SQL, Power BI, or mixed access. |

| Data latency | Yes | State batch, near-real-time, or mixed refresh targets. |

| Governance scope | Yes | State workspace, domain, and access boundaries. |

| Open-format requirement | Recommended | State Delta or Iceberg interoperability needs. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Spark engineering and open files drive the workload | Lakehouse | Lakehouse aligns with Delta tables, notebooks, and Files plus Tables layout. |

| Strict relational serving with heavy SQL-only consumption drives the workload | Warehouse | Warehouse aligns with SQL-first serving and managed relational semantics. |

| Multiple engines reuse the same governed data | Lakehouse in OneLake | One copy of data aligns with Fabric's unified storage model. |

| Existing external data already lives in OneLake or ADLS Gen2 | Lakehouse plus shortcut | Shortcut avoids duplicate storage and preserves a unified namespace. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent classifies the workload as lakehouse, warehouse, or mixed. | Read storage and query requirements. | Chosen item type matches compute and serving needs. |

| 2 | Agent defines lakehouse folder and table boundaries. | Review planned Files and Tables paths. | Structured tables and raw files have separate locations. |

| 3 | Agent defines Delta table strategy. | Review partition, schema, and update patterns. | Table design supports ingestion and downstream reads. |

| 4 | Agent aligns OneLake access and workspace permissions. | Review roles and sharing scope. | Access boundaries match business ownership. |

| 5 | Agent adds ingestion and optimization steps. | Review load path, compaction, and maintenance plan. | Data lands in Delta format with stable query performance. |

| 6 | Agent validates engine access. | Query the lakehouse from Spark or SQL endpoint. | Tables and files resolve from the expected consumers. |



## Verification Checklist



- [ ] Agent selected lakehouse only when open-format storage or Spark access drives the workload.

- [ ] Agent separated raw files from curated tables.

- [ ] Agent defined Delta table layout and maintenance expectations.

- [ ] Agent aligned workspace permissions with data ownership.

- [ ] Agent validated at least one read path from the target engine.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Lakehouse holds warehouse-style serving tables only | Agent routes SQL-serving workloads to a warehouse or mixed design. |

| Raw files and curated tables share one unmanaged path | Agent separates landing, working, and curated locations. |

| Delta tables lack partition or maintenance design | Agent defines optimization and schema evolution expectations up front. |

| OneLake permissions exceed project scope | Agent narrows workspace and item access to the owning group. |

