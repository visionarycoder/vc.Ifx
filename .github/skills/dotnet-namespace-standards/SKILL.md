---

name: dotnet-namespace-standards

title: .NET Namespace Standards

description: Standardize root namespace, folder alignment, and namespace ownership rules across .NET projects.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 980

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - dotnet-naming-standards

related_skills:

  - dotnet-naming-standards

  - dotnet-architectural-layers

  - directory-build-organization

appliesTo: '**/*.{cs,csproj}'

tags:

  - dotnet

  - namespace

  - naming

  - architecture

  - analyzers

---

# .NET Namespace Standards



Agent uses this skill to keep namespaces aligned with project identity, folder structure, and architectural ownership.



## When to Use



| User prompt | Use |

|---|---|

| User adds new projects, folders, or source files | Agent uses this skill |

| User moves files or renames projects | Agent uses this skill |

| User asks to audit namespace drift | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User edits generated or vendor code that keeps external names | Agent keeps the external namespace |

| User uses temporary migration shims with approved exceptions | Agent documents the exception instead of normalizing it |

| User mirrors external interop APIs | Agent keeps the external namespace shape |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Project or file path | Yes | Agent uses the path to derive namespace segments. |

| Root namespace | Yes | Agent uses the approved repository root. |

| Folder structure | Yes | Agent maps meaningful path segments only. |

| Approved exceptions | No | Agent records generated-code or test-project differences. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent identifies the expected root namespace. | Run `rg -n "<RootNamespace>|namespace Wa\.Wsdot\.Fin\.Idl" <project-or-path>`. | Root namespace matches the approved project pattern. |

| 2 | Agent maps meaningful folder segments to namespace segments. | Read file paths and namespace lines together. | Namespace segments match the owning folder path. |

| 3 | Agent fixes production and test namespaces separately. | Run `rg -n "^namespace " <scope>`. | Test files use test namespaces and production files use production namespaces. |

| 4 | Agent runs rename follow-up validation. | Run `dotnet build <project-or-solution>`. | Build passes and using directives resolve cleanly. |



## Rule Matrix



| Rule | Agent action |

|---|---|

| Root namespace | Agent keeps `Wa.Wsdot.Fin.Idl` intact and correctly cased. |

| Folder alignment | Agent maps meaningful folders to namespace segments. |

| Project identity | Agent keeps the owning project or bounded context visible. |

| Test projects | Agent uses a test-specific namespace suffix. |

| Temporary labels | Agent avoids `Temp`, `New`, and migration-state names in permanent namespaces. |

| Architecture ownership | Agent aligns namespace ownership to the correct layer. |



## Validation Checklist



- [ ] Agent kept the approved root namespace.

- [ ] Agent aligned folder and namespace segments.

- [ ] Agent used test namespaces for test projects.

- [ ] Agent aligned namespace ownership with architecture ownership.

- [ ] Agent ran a build after bulk namespace changes.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent abbreviates the root namespace for convenience | Agent restores the canonical root. |

| Agent leaves old segments after a file move | Agent updates namespace and using directives together. |

| Agent mirrors every incidental folder | Agent keeps only meaningful ownership segments. |

| Agent hides architecture drift with aliases | Agent fixes the ownership defect directly. |

