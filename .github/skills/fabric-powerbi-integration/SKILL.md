---
name: fabric-powerbi-integration
title: Fabric Power BI Integration
description: Integrate Microsoft Fabric data with Power BI semantic models, DAX, report generation, embedded delivery, and row-level security when governed analytics reach end users.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1395
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-lakehouses
  - fabric-workspaces-governance
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - powerbi
  - semantic-model
  - dax
---

# Fabric Power BI Integration



Agent connects Fabric data items to Power BI semantic and reporting experiences. Agent aligns semantic models, DAX measures, report generation, embedded delivery paths, and row-level security with governed analytics consumption.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Power BI reports from Fabric data | Use this skill |

| User asks for semantic models in a Fabric workspace | Use this skill |

| User asks for DAX or row-level security over Fabric data | Use this skill |

| User asks for application-facing report delivery from Fabric-backed models | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for raw ingestion or ETL design | Use `fabric-data-factory-patterns` |

| User asks for lakehouse storage design only | Use `fabric-lakehouses` |

| User asks for real-time event routing only | Use `fabric-realtime-analytics` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Source item | Yes | State lakehouse, warehouse, KQL queryset, or mirrored source. |

| Consumer surface | Yes | State report, dashboard, app, or embedded application. |

| Security scope | Yes | State row-level security, workspace roles, and audience groups. |

| Model grain | Yes | State required facts, dimensions, and measure semantics. |

| Refresh or freshness target | Recommended | State import, direct query, or live expectation. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Reusable business metrics drive many reports | Shared semantic model | Centralized measures improve consistency and governance. |

| KQL queryset or database output drives the report | Power BI report from KQL semantic model | Fabric supports report creation directly from KQL query results. |

| Application users consume reports through a host app | Embedded delivery path backed by a governed semantic model | Embedded scenarios work best when one semantic layer governs access and metrics. |

| Audience access differs by tenant or business unit | Row-level security plus workspace governance | RLS and workspace controls keep one model while narrowing data visibility. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent selects the Fabric source item and consumer surface. | Review data source and audience. | The chosen model path matches the report or app experience. |

| 2 | Agent designs the semantic model grain and measures. | Review facts, dimensions, and DAX plan. | Model structure supports the required visuals and filters. |

| 3 | Agent applies security controls. | Review RLS rules and workspace roles. | Users see only the intended data slice. |

| 4 | Agent builds the report or embedded delivery contract. | Review visual, navigation, or host-app requirements. | Consumers reach the intended report surface. |

| 5 | Agent validates freshness and performance. | Test refresh or live-query behavior. | Data appears on the intended schedule or live path. |

| 6 | Agent validates business measures. | Compare sample outputs to source data. | Measures and filters match expected business results. |



## Verification Checklist



- [ ] Agent selected a semantic model strategy that matches report reuse needs.

- [ ] Agent defined DAX measures or calculations from business rules.

- [ ] Agent applied row-level security or equivalent access controls.

- [ ] Agent validated report or embedded delivery with the intended audience path.

- [ ] Agent checked freshness and measure accuracy against source data.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Every report defines separate business measures | Agent centralizes shared logic in one semantic model. |

| RLS appears after broad report sharing starts | Agent designs audience segmentation before publication. |

| Embedded delivery bypasses semantic governance | Agent keeps the application path on governed workspace artifacts. |

| Report visuals hide source-grain problems | Agent validates model grain before visual tuning starts. |

