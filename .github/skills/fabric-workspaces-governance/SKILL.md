---
name: fabric-workspaces-governance
title: Fabric Workspaces and Governance
description: Configure Microsoft Fabric workspaces, capacities, security, and Git-backed governance when teams need controlled collaboration and release flow.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1375
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-onelake-patterns
  - fabric-powerbi-integration
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - governance
  - workspace
  - git
---

# Fabric Workspaces and Governance



Agent configures Fabric workspace structure, capacity placement, and governance controls. Agent aligns collaboration, security, lifecycle management, and Git integration with operating boundaries.



## When to Use



| User prompt | Use |

|---|---|

| User asks for Fabric workspace design | Use this skill |

| User asks for Fabric capacity management | Use this skill |

| User asks for Git integration or deployment pipelines | Use this skill |

| User asks for Fabric governance or security posture | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for item-level lakehouse modeling only | Use `fabric-lakehouses` |

| User asks for notebook implementation only | Use `fabric-notebooks-spark` |

| User asks for ontology modeling only | Use `fabric-iq-ontology` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Team topology | Yes | State owners, contributors, and consumers. |

| Environment model | Yes | State dev, test, prod, or equivalent stage layout. |

| Capacity plan | Yes | State region, SKU, and workload isolation needs. |

| Security model | Yes | State workspace roles, item security, and data controls. |

| Release process | Recommended | State Git, deployment pipeline, and approval expectations. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| Environment isolation drives lifecycle control | Separate stage workspaces | Stage separation reduces release risk and accidental overwrite. |

| Shared domain ownership drives data discovery | Domain plus workspace model | Domain structure improves governance and delegated ownership. |

| Frequent content promotion drives release flow | Git integration plus deployment pipeline | CI and promotion controls support reviewable change flow. |

| Sensitive data drives security posture | Workspace roles plus item security and labels | Layered controls reduce broad access exposure. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent maps business domains, teams, and environment stages. | Review ownership and lifecycle boundaries. | Each workspace has one role and one stage purpose. |

| 2 | Agent aligns workspaces to capacities and region constraints. | Review SKU, region, and workload mix. | Capacity placement matches performance and residency expectations. |

| 3 | Agent configures workspace roles and item access. | Review contributor and viewer assignments. | Least-privilege access matches team responsibilities. |

| 4 | Agent enables governance surfaces such as catalog, lineage, and security review. | Review discovery and audit requirements. | Admins and owners inspect posture without ad hoc scripts. |

| 5 | Agent wires Git integration and promotion flow. | Review repository binding and deployment stages. | Changes move through the intended release path. |

| 6 | Agent validates security and lifecycle behavior. | Test access, sync, and promotion. | Roles, Git sync, and stage promotion behave as designed. |



## Verification Checklist



- [ ] Agent defined workspace boundaries by domain or environment.

- [ ] Agent aligned capacities with region and workload expectations.

- [ ] Agent applied least-privilege workspace and item access.

- [ ] Agent enabled governance or audit surfaces for owners and admins.

- [ ] Agent validated Git sync or deployment promotion behavior.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| One workspace spans dev, test, and prod content | Agent separates stage workspaces before release automation starts. |

| Capacity placement ignores data residency or workload contention | Agent maps workloads to region and SKU requirements early. |

| Git integration starts without ownership or branch rules | Agent defines repository, branch, and promotion responsibilities first. |

| Broad workspace admin rights replace item-level access design | Agent narrows privileged roles and uses item controls where needed. |

