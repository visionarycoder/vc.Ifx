---
name: fabric-data-factory-patterns
title: Fabric Data Factory Patterns
description: Build Microsoft Fabric Data Factory pipelines, copy jobs, and orchestration patterns when batch ingestion or ETL spans multiple systems.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1410
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-notebooks-spark
  - fabric-shortcuts-mirroring
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - data-factory
  - pipelines
  - orchestration
---

# Fabric Data Factory Patterns



Agent designs Fabric Data Factory solutions for movement, transformation, and orchestration. Agent chooses copy jobs, pipelines, dataflows, or event-driven starts from the workload shape.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Fabric pipelines or activities | Use this skill |

| User asks for Fabric ETL or orchestration | Use this skill |

| User asks for copy jobs or ingestion schedules | Use this skill |

| User asks for migration from Azure Data Factory patterns | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for zero-copy OneLake virtualization | Use `fabric-shortcuts-mirroring` |

| User asks for notebook-centric Spark engineering | Use `fabric-notebooks-spark` |

| User asks for low-latency event routing | Use `fabric-realtime-analytics` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Source and destination list | Yes | State all systems, formats, and Fabric targets. |

| Transformation scope | Yes | State copy only, low-code shaping, or custom compute. |

| Orchestration depth | Yes | State dependencies, branching, retries, and event triggers. |

| Refresh cadence | Yes | State one-time, scheduled, incremental, or CDC. |

| Connection model | Recommended | State Fabric connections and any linked-service migration mapping. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Simple raw ingestion with minimal orchestration fits the workload | Copy job | Copy job handles bulk, incremental, and CDC copy with limited setup. |

| Full dependency management and parameterized activities fit the workload | Pipeline | Pipeline orchestration fits branching, scheduling, and control flow. |

| Low-code shaping before publish fits the workload | Dataflow Gen2 | Dataflow design fits business-friendly transformation steps. |

| Streaming ingest drives the workload | Eventstream | Data Factory batch patterns do not fit low-latency event flow. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent inventories source systems, targets, and refresh cadence. | Review movement requirements. | Each source-target pair has one planned movement pattern. |

| 2 | Agent chooses copy job, pipeline, dataflow, or mixed orchestration. | Review transformation and dependency needs. | Selected components match complexity and latency targets. |

| 3 | Agent configures connections, parameters, and control flow. | Review connection bindings and pipeline graph. | Runtime references stay environment-aware and reusable. |

| 4 | Agent defines scheduling, event starts, retries, and failure handling. | Review trigger and retry paths. | Runs start and recover through the intended control flow. |

| 5 | Agent validates movement outputs. | Inspect copied data and activity results. | Data lands in the correct Fabric item with expected shape. |

| 6 | Agent validates monitoring and rerun posture. | Review run history and alert paths. | Operators trace failures and rerun safely. |



## Verification Checklist



- [ ] Agent selected copy job, pipeline, dataflow, or eventstream from the actual orchestration need.

- [ ] Agent parameterized environment-specific values.

- [ ] Agent defined schedule or event triggers plus retry behavior.

- [ ] Agent validated output shape and location in Fabric.

- [ ] Agent checked operational monitoring or rerun visibility.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Pipelines appear for simple one-step copies | Agent starts with copy job for smaller ingestion patterns. |

| Environment values stay hard-coded in activities | Agent moves runtime differences into parameters or configuration. |

| Copy patterns ignore downstream medallion placement | Agent maps every output to a bronze, silver, or gold purpose. |

| Monitoring starts only after production failures | Agent defines run-history and alert review in the initial workflow. |

