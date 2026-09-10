---

name: convert-to-cpm

license: MIT

title: Convert to Central Package Management

description: Convert .NET projects to NuGet Central Package Management with baseline capture, conflict review, centralized versions, and post-conversion verification.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1200

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - binlog-generation

related_skills:

  - build-perf-baseline

appliesTo: '**/*.{csproj,fsproj,vbproj,sln,slnx,props,targets}'

tags:

  - dotnet

  - nuget

  - cpm

  - packages

  - msbuild

---

# Convert to Central Package Management



Agent uses this skill to migrate PackageReference-based .NET projects to `Directory.Packages.props`.



## When to Use



| User prompt | Use |

|---|---|

| User wants centralized package versions across multiple projects | Agent uses this skill |

| User wants to convert a solution or repository to CPM | Agent uses this skill |

| User wants a before-and-after package baseline | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User has `packages.config` projects | Agent runs PackageReference migration first |

| User already uses CPM cleanly in scope | Agent updates the existing CPM file only |

| User wants one package version bump with no CPM scope | Agent uses a package-update workflow |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Conversion scope | Yes | Agent records project, solution, or directory scope. |

| Conflict decision | Yes | Agent records how version conflicts are resolved. |

| Shared props or targets | No | Agent uses this input to trace hidden versions. |

| Validation scope | No | Agent records build-only or build-plus-test verification. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent verifies that every in-scope project uses PackageReference. | Run `rg -n "packages.config|PackageReference" <scope>`. | Scope contains PackageReference projects only. |

| 2 | Agent captures baseline build and package data. | Run `dotnet build <scope>` and `dotnet list <scope> package --format json`. | Baseline build passes and baseline package data exists. |

| 3 | Agent audits conflicts, conditional references, and property-driven versions. | Read project files and imported props. | Conflict list and property sources are documented before edits. |

| 4 | Agent writes or updates `Directory.Packages.props` and removes inline versions. | Run `rg -n "PackageVersion|VersionOverride|Version=" <scope>`. | Central versions exist and inline versions remain only for intentional overrides. |

| 5 | Agent rebuilds and compares resolved package data to baseline. | Run `dotnet build <scope>` and `dotnet list <scope> package --format json`. | Post-conversion build passes and package resolution matches the intended decisions. |

| 6 | Agent writes a persistent conversion report under `docs/instructions/`. | Read the report file. | Report records scope, conflicts, decisions, risks, and follow-up work. |



## Decision Matrix



| Situation | Agent action |

|---|---|

| `packages.config` detected | Agent stops and routes to PackageReference migration. |

| Multiple versions for one package | Agent records the conflict and uses the approved decision. |

| Property-based package versions | Agent traces the property source before centralization. |

| Conditional package references | Agent preserves conditions or uses `VersionOverride` deliberately. |

| Existing CPM file governs the scope | Agent extends the existing file instead of adding a competing file. |



## Validation Checklist



- [ ] Agent verified PackageReference scope first.

- [ ] Agent captured baseline build and package data before edits.

- [ ] Agent documented conflicts and property-driven versions.

- [ ] Agent centralized package versions in `Directory.Packages.props`.

- [ ] Agent rebuilt and compared package resolution after the conversion.

- [ ] Agent wrote a conversion report under `docs/instructions/`.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent converts without a baseline | Agent captures build and package data first. |

| Agent resolves conflicts silently | Agent records each conflict and decision. |

| Agent deletes comparison artifacts | Agent keeps baseline and after-state data. |

| Agent removes property-based versions without tracing them | Agent audits imports and remaining references first. |

