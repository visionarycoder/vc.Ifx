---
name: fabric-realtime-analytics
title: Fabric Real-Time Analytics
description: Implement Microsoft Fabric real-time analytics with eventstreams, KQL databases, and streaming ingestion when low-latency data drives dashboards or actions.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1380
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-data-activator
  - fabric-powerbi-integration
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - realtime
  - kql
  - eventstreams
---

# Fabric Real-Time Analytics



Agent designs Fabric real-time analytics flows for data in motion. Agent combines eventstreams, Eventhouse or KQL databases, KQL query paths, and downstream visuals with low-latency objectives.



## When to Use



| User prompt | Use |

|---|---|

| User asks for streaming ingestion in Fabric | Use this skill |

| User asks for KQL databases or Eventhouse design | Use this skill |

| User asks for event-driven dashboards | Use this skill |

| User asks for Eventstream routing in Fabric | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for batch ETL only | Use `fabric-data-factory-patterns` |

| User asks for alerting or trigger rules only | Use `fabric-data-activator` |

| User asks for long-cycle warehouse modeling only | Use lakehouse or warehouse guidance |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Event source | Yes | State IoT, application logs, database CDC, or message broker source. |

| Latency target | Yes | State seconds, minutes, or dashboard refresh boundary. |

| Query pattern | Yes | State time-series, anomaly scan, aggregation, or join pattern. |

| Destination | Yes | State KQL database, Activator, Power BI, or mixed sinks. |

| Retention scope | Recommended | State hot data window and archival path. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Low-latency event routing drives ingestion | Eventstream | Eventstream fits continuous input, routing, and simple transformations. |

| KQL exploration and time-series queries drive storage | KQL database or Eventhouse | KQL storage and query patterns fit high-volume telemetry and log data. |

| Real-time detection drives the outcome | Real-Time Analytics plus `fabric-data-activator` | KQL plus Activator aligns analytics with downstream actions. |

| Raw data lands once and serves many consumers | OneLake-connected real-time pattern | Unified storage and shared access reduce duplicate pipelines. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent maps the streaming source, schema, and latency target. | Review event contract and freshness target. | Source and service choice align with the required latency. |

| 2 | Agent defines Eventstream ingestion and routing. | Review inputs, transformations, and outputs. | Events arrive at the intended destinations. |

| 3 | Agent defines KQL database or Eventhouse tables and retention. | Review table mappings and retention settings. | Query surfaces match expected volume and history. |

| 4 | Agent writes KQL queries for operational insight. | Execute sample queries. | Queries return the expected aggregates or event slices. |

| 5 | Agent connects downstream visuals or triggers. | Review report, dashboard, or action bindings. | Consumers receive fresh data from the live query path. |

| 6 | Agent validates throughput, failure visibility, and freshness. | Monitor ingest lag and query response. | Ingestion, query, and output paths meet the stated target. |



## Verification Checklist



- [ ] Agent matched Eventstream and KQL components to the latency target.

- [ ] Agent defined table mappings and retention expectations.

- [ ] Agent validated at least one live KQL query against ingested data.

- [ ] Agent wired a downstream dashboard, report, or action path.

- [ ] Agent checked ingest lag or freshness against the requirement.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Batch ingestion tools handle event-driven workloads | Agent routes low-latency scenarios to Eventstream and KQL components. |

| Schema drift reaches queries without handling | Agent adds explicit field management and validation checkpoints. |

| Retention settings ignore cost and query windows | Agent matches hot data retention to operational query needs. |

| Real-time dashboards read from stale export paths | Agent keeps the visualization on the live query surface. |

