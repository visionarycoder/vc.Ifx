---
name: fabric-data-activator
title: Fabric Data Activator
description: Configure Microsoft Fabric Activator rules, objects, and triggers when monitored data streams need near-real-time alerts or automated actions.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1285
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-realtime-analytics
  - fabric-copilot-studio
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - activator
  - alerts
  - triggers
---

# Fabric Data Activator



Agent configures Fabric Activator for rule-based monitoring and action. Agent models objects, event conditions, and trigger paths that convert streaming changes into notifications or flows.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Fabric alerts on streaming data | Use this skill |

| User asks for Activator rules or objects | Use this skill |

| User asks for threshold or state-change triggers | Use this skill |

| User asks for reflex-style monitoring in Fabric | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for streaming storage and query design | Use `fabric-realtime-analytics` |

| User asks for batch notifications from pipeline jobs | Use `fabric-data-factory-patterns` |

| User asks for enterprise incident tooling outside Fabric flows | Use the owning platform guidance |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Event source | Yes | State Eventstream, dashboard, or report source. |

| Object key | Yes | State the identifier that groups events into monitored objects. |

| Trigger condition | Yes | State threshold, range exit, increase, decrease, or state change. |

| Action path | Yes | State email, Teams, Power Automate, or downstream endpoint. |

| Noise tolerance | Recommended | State debounce, cooldown, or duplicate suppression needs. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Stateless threshold detection fits the need | Direct Activator rule | Simple rule evaluation keeps the design small. |

| Per-device or per-asset state tracking fits the need | Object-based rule grouping | Object grouping aligns events to one monitored entity. |

| Human workflow follows each trigger | Activator plus Power Automate | Action orchestration fits notification and approval flows. |

| Root need is query and storage, not action | Route to `fabric-realtime-analytics` | Activator depends on an upstream data surface. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent identifies the event source and monitored object. | Review event payload and grouping key. | Each event maps to one business object. |

| 2 | Agent defines the rule condition and state behavior. | Review thresholds, state changes, and cooldown rules. | The rule logic matches the alert intent. |

| 3 | Agent configures the action path. | Review recipients, flow endpoints, or connectors. | Trigger output reaches the intended destination. |

| 4 | Agent limits noise with suppression or grouping controls. | Review duplicate scenarios. | Repeated signals do not flood the action path. |

| 5 | Agent validates live trigger behavior. | Replay or observe sample events. | Expected events fire and nonmatching events stay silent. |

| 6 | Agent verifies operational visibility. | Review alert history or run output. | Operators trace rule results and action outcomes. |



## Verification Checklist



- [ ] Agent mapped each event to a stable monitored object.

- [ ] Agent defined rule logic that matches the business condition.

- [ ] Agent configured an action path with observable outcomes.

- [ ] Agent reduced duplicate or noisy trigger behavior.

- [ ] Agent validated both match and nonmatch event cases.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Rules evaluate raw events without a stable object key | Agent groups events by a business identifier first. |

| Trigger actions fire on every repeated event | Agent adds cooldown or state-transition logic. |

| Teams or email alerts lack ownership context | Agent includes object identity and rule intent in the action payload. |

| Activator replaces upstream ingestion design | Agent keeps Eventstream and KQL design separate from trigger logic. |

