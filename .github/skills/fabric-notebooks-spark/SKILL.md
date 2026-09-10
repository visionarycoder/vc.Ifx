---
name: fabric-notebooks-spark
title: Fabric Notebooks and Spark
description: Build Microsoft Fabric notebooks and Spark jobs for PySpark engineering, interactive analysis, and scheduled execution when data work centers on notebook code.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1335
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-lakehouses
  - fabric-data-science
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - notebooks
  - spark
  - pyspark
---

# Fabric Notebooks and Spark



Agent authors and operates Fabric notebooks for Spark-based data work. Agent aligns language choice, job orchestration, and lakehouse integration with the execution goal.



## When to Use



| User prompt | Use |

|---|---|

| User asks for PySpark or Spark SQL in Fabric | Use this skill |

| User asks for a Fabric notebook job | Use this skill |

| User asks for scheduled notebook execution | Use this skill |

| User asks for notebook-based data engineering | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for low-code ingestion only | Use `fabric-data-factory-patterns` |

| User asks for semantic model design only | Use `fabric-powerbi-integration` |

| User asks for warehouse-only T-SQL serving | Use warehouse guidance |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Notebook objective | Yes | State exploration, ETL, feature engineering, or ML training. |

| Runtime language | Yes | State PySpark, Spark SQL, Scala, or SparkR. |

| Data attachment | Yes | State lakehouse, warehouse, KQL, or file source. |

| Execution mode | Yes | State interactive, pipeline-triggered, or scheduled job. |

| Parameter set | Recommended | State runtime parameters and environment differences. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Interactive exploration or profiling drives the work | Notebook session | Cell execution and visualization fit iterative analysis. |

| Reusable Spark transformation drives the work | Notebook plus job definition | Parameterized execution fits repeatable production runs. |

| Complex orchestration spans many tasks | Notebook inside pipeline | Pipeline scheduling and dependency control fit the run graph. |

| ML experimentation drives the work | Notebook plus experiment tracking | Notebook execution fits code-first feature and model loops. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent defines the notebook objective and target language. | Review job inputs and expected outputs. | Notebook scope matches one clear execution goal. |

| 2 | Agent attaches the notebook to the correct data item and environment. | Review default lakehouse, libraries, and session config. | The notebook resolves the intended data sources. |

| 3 | Agent structures the notebook into setup, transform, validate, and publish cells. | Read the notebook flow. | Each stage has one observable purpose. |

| 4 | Agent adds parameters and reusable functions. | Review input cells or job parameters. | The same notebook runs across environments with limited edits. |

| 5 | Agent defines scheduling or pipeline invocation. | Review job, schedule, or pipeline binding. | Execution starts from the intended trigger. |

| 6 | Agent validates runtime outputs and logs. | Run a notebook job or targeted cells. | Output tables, files, and logs match the success criteria. |



## Verification Checklist



- [ ] Agent aligned the notebook language with the processing goal.

- [ ] Agent attached the notebook to the correct Fabric data item.

- [ ] Agent separated setup, processing, and validation logic.

- [ ] Agent parameterized values that vary by environment or run.

- [ ] Agent validated interactive or scheduled execution end to end.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Notebook mixes exploration and production logic without boundaries | Agent separates reusable job cells from ad hoc exploration cells. |

| Hard-coded workspace or lakehouse names appear in code | Agent lifts values into parameters or configuration cells. |

| Large outputs stay only in notebook display cells | Agent writes durable tables or files for downstream use. |

| Schedule exists without output validation | Agent adds post-run data checks and failure visibility. |

