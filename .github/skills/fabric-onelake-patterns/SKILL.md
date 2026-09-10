---
name: fabric-onelake-patterns
title: Fabric OneLake Patterns
description: Plan Microsoft OneLake architectures, shortcuts, and ADLS Gen2 interoperability when one logical data lake spans Fabric workloads.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1310
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fabric-lakehouses
  - fabric-shortcuts-mirroring
appliesTo: '**/*.{md,json,yml,yaml,py,sql,kql,dax,ipynb}'
tags:
  - fabric
  - onelake
  - shortcuts
  - adls
---

# Fabric OneLake Patterns



Agent plans OneLake as the unified storage layer for Fabric. Agent uses workspaces, domains, shortcuts, and open storage access to keep one logical lake with distributed ownership.



## When to Use



| User prompt | Use |

|---|---|

| User asks for OneLake architecture in Fabric | Use this skill |

| User asks for unified data lake patterns | Use this skill |

| User asks for ADLS Gen2 access to Fabric data | Use this skill |

| User asks for OneLake shortcuts across domains | Use this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for detailed lakehouse table design | Use `fabric-lakehouses` |

| User asks for CDC replication from operational databases | Use `fabric-shortcuts-mirroring` |

| User asks for workspace security and release governance | Use `fabric-workspaces-governance` |



## Required Inputs



| Input | Required | Description |

|---|---|---|

| Organization layout | Yes | State domains, teams, and workspace ownership boundaries. |

| Data locations | Yes | State Fabric-native, ADLS Gen2, AWS, or mixed sources. |

| Access paths | Yes | State Fabric UX, API, file explorer, or external engine access. |

| Duplication tolerance | Yes | State whether edge copies are allowed. |

| Governance controls | Recommended | State catalog, lineage, and security expectations. |



## Decision Matrix



| Condition | Preferred choice | Reason |

|---|---|---|

| One logical lake spans many teams | OneLake with workspace ownership | OneLake centralizes storage while preserving distributed administration. |

| Existing external data stays in place | Shortcut pattern | Shortcut preserves a unified namespace without copy operations. |

| External tool needs file-system-style access | ADLS Gen2 or OneLake API path | OneLake exposes open access patterns for external consumers. |

| Governance and discovery drive adoption | OneLake catalog and domain model | Catalog and domain structure improve discovery and stewardship. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent maps domains, workspaces, and data owners. | Review tenant and workspace boundaries. | Each dataset has one clear owning workspace. |

| 2 | Agent identifies Fabric-native and external data locations. | Review source inventory. | Each source has a planned Fabric path or shortcut path. |

| 3 | Agent selects direct storage, shortcut, or external API access. | Review duplication and access requirements. | The storage pattern matches cost, latency, and governance goals. |

| 4 | Agent aligns security and discoverability. | Review roles, catalog visibility, and lineage needs. | Owners and consumers have the intended access posture. |

| 5 | Agent validates data access from the target engines. | Open files or tables from the chosen path. | External and Fabric consumers reach the data through the planned interface. |

| 6 | Agent reviews cross-domain operational impact. | Check naming, ownership, and dependency paths. | The design avoids duplicate unmanaged copies. |



## Verification Checklist



- [ ] Agent mapped OneLake design to domains and workspaces.

- [ ] Agent chose direct storage or shortcuts based on duplication tolerance.

- [ ] Agent aligned access with Fabric and external engine needs.

- [ ] Agent considered catalog visibility and security boundaries.

- [ ] Agent validated at least one real access path to the data.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Teams create separate unmanaged lakes for each project | Agent centralizes storage in OneLake with workspace ownership. |

| Shortcuts target paths that lack lifecycle control | Agent documents source ownership and break conditions for every shortcut. |

| External tools rely on copied extracts only | Agent uses open access paths before duplicate export flows. |

| Governance appears after storage sprawl starts | Agent defines catalog, lineage, and security posture during design. |

