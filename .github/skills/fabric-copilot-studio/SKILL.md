---
name: fabric-copilot-studio
title: Fabric Copilot Studio Integration
description: Connect Microsoft Fabric data agents and Microsoft Copilot Studio agents when chatbot experiences need grounded access to governed enterprise data.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1345
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-data-science
  - fabric-iq-ontology
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - copilot-studio
  - agents
  - chatbot
---

# Fabric Copilot Studio Integration



Agent connects Fabric data agents with Microsoft Copilot Studio agent experiences. Agent grounds chatbot responses in governed Fabric data and checks tenant, publication, and compliance boundaries.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Fabric data agents in Copilot Studio | Use this skill |

| User asks for chatbot grounding on Fabric data | Use this skill |

| User asks for Copilot Studio and Fabric integration | Use this skill |

| User asks for Power Virtual Agents successor patterns over Fabric data | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for report creation only | Use `fabric-powerbi-integration` |

| User asks for ontology design only | Use `fabric-iq-ontology` |

| User asks for notebook or model training only | Use `fabric-data-science` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Agent goal | Yes | State question answering, task routing, or guided workflow outcome. |

| Fabric data source | Yes | State lakehouse, warehouse, KQL database, mirrored database, semantic model, or ontology. |

| Tenant alignment | Yes | State Fabric and Copilot Studio tenant relationship. |

| Capacity and licensing | Yes | State Fabric capacity and Copilot licensing posture. |

| Compliance boundary | Recommended | State cross-geo or external processing constraints. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Conversational experience needs governed enterprise grounding | Fabric data agent plus Copilot Studio | Connected agents keep the chatbot on governed Fabric sources. |

| Data source already has business semantics in ontology | Fabric IQ-backed data agent | Ontology improves grounded entity understanding. |

| Responses cross compliance or geo boundaries | Explicit compliance review before integration | Fabric guidance highlights external processing and storage considerations. |

| Root need is report embedding, not conversation | Power BI integration path | Chat agents add unnecessary complexity for report-only delivery. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent confirms the chatbot objective and target Fabric sources. | Review question patterns and data scope. | One grounded agent path matches the use case. |

| 2 | Agent validates Fabric capacity, tenant alignment, and data-agent readiness. | Review prerequisites and published state. | The data agent is available and reachable from the same tenant. |

| 3 | Agent configures the Fabric data agent description and access scope. | Review source bindings and permissions. | The data agent exposes only the intended governed data. |

| 4 | Agent connects the Fabric data agent to Copilot Studio. | Review connected-agent configuration. | Copilot Studio resolves the Fabric agent successfully. |

| 5 | Agent validates prompt grounding and answer behavior. | Run representative questions. | Responses reflect source data and stay within scope. |

| 6 | Agent validates compliance and operational boundaries. | Review geo, retention, and audit expectations. | The integration aligns with governance requirements. |



## Verification Checklist



- [ ] Agent confirmed Fabric data-agent readiness before connection.

- [ ] Agent validated tenant, capacity, and licensing prerequisites.

- [ ] Agent restricted access to the intended Fabric sources.

- [ ] Agent tested grounded answers with representative questions.

- [ ] Agent reviewed cross-geo or compliance implications.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Copilot Studio connects before the data agent is published and described clearly | Agent publishes and documents the data agent first. |

| Chatbot access scope exceeds the business scenario | Agent narrows source bindings and permissions to the target audience. |

| Compliance review starts after cross-geo traffic exists | Agent checks external processing boundaries before rollout. |

| Prompt tests use only happy-path questions | Agent adds ambiguous and out-of-scope questions to grounding validation. |

