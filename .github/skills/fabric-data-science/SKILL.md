---
name: fabric-data-science
title: Fabric Data Science
description: Build Microsoft Fabric data science workflows with experiments, MLflow tracking, AutoML, and deployment when teams train and serve models near governed data.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1390
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-notebooks-spark
  - fabric-copilot-studio
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - data-science
  - mlflow
  - automl
---

# Fabric Data Science



Agent builds Fabric data science workflows that stay close to governed enterprise data. Agent aligns notebooks, experiments, models, and serving paths with repeatable model operations.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Fabric machine learning workflows | Use this skill |

| User asks for MLflow tracking in Fabric | Use this skill |

| User asks for AutoML in Fabric | Use this skill |

| User asks for model deployment or scoring in Fabric | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for Spark ETL only | Use `fabric-notebooks-spark` |

| User asks for BI semantic models only | Use `fabric-powerbi-integration` |

| User asks for ontology-based knowledge graphs | Use `fabric-iq-ontology` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Business objective | Yes | State forecast, classification, recommendation, or anomaly goal. |

| Training data source | Yes | State lakehouse, warehouse, semantic model, or KQL source. |

| Evaluation metric | Yes | State the success metric and acceptance threshold. |

| Serving path | Yes | State batch scoring, real-time endpoint, or offline experiment only. |

| Governance scope | Recommended | State approval, lineage, and access expectations. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Tabular prediction with standard algorithms fits the goal | AutoML | AutoML speeds baseline model generation and comparison. |

| Custom feature engineering or specialized libraries fit the goal | Notebook-led training | Code-first training fits advanced feature and model control. |

| Reproducibility and audit drive the project | MLflow experiment tracking | Experiment lineage and metric capture support repeatable runs. |

| Low-latency inference drives the project | Model endpoint or real-time scoring path | A serving endpoint aligns model output with application demand. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent defines the ML objective, metric, and data scope. | Review problem statement and target variable. | Model work has one measurable success target. |

| 2 | Agent prepares governed training and validation data. | Review feature set, split logic, and leakage controls. | Data supports repeatable training and honest evaluation. |

| 3 | Agent selects AutoML or code-first training. | Review algorithm and library needs. | Training path matches complexity and control requirements. |

| 4 | Agent tracks runs with experiments and model metadata. | Review metrics, parameters, and artifacts. | Each run has traceable lineage and comparable metrics. |

| 5 | Agent registers or deploys the chosen model. | Review serving contract and environment binding. | Consumers reach one governed model version. |

| 6 | Agent validates scoring and monitoring. | Score sample inputs and review outcomes. | Predictions meet metric and deployment expectations. |



## Verification Checklist



- [ ] Agent defined a measurable objective and evaluation metric.

- [ ] Agent prepared training data without leakage across splits.

- [ ] Agent selected AutoML or notebook training based on control needs.

- [ ] Agent tracked experiments and model lineage.

- [ ] Agent validated scoring on the intended serving path.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Feature engineering logic lives only in one exploratory notebook | Agent promotes stable feature code into repeatable training steps. |

| Experiment runs lack metric or parameter capture | Agent enables MLflow tracking for every training run. |

| Deployment starts before acceptance criteria exist | Agent defines metric thresholds before model selection. |

| Real-time scoring uses features unavailable at inference time | Agent aligns serving features with production data availability. |

