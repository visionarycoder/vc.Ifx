---

name: nuget-trusted-publishing

license: MIT

title: NuGet Trusted Publishing Setup

description: Set up or migrate nuget.org publishing to GitHub Actions OIDC trusted publishing with exact policy values, workflow permissions, and publish verification.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1065
prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:



appliesTo: '**/*.{csproj,props,yml,yaml,md}'

tags:

  - nuget

  - oidc

  - github-actions

  - publishing

---

# NuGet Trusted Publishing Setup



Agent uses this skill to replace nuget.org API-key publishing with GitHub Actions OIDC trusted publishing.



## When to Use



| User prompt | Use |

|---|---|

| User asks to set up keyless NuGet publishing on GitHub Actions | Agent uses this skill |

| User asks to migrate away from `NUGET_API_KEY` | Agent uses this skill |

| User asks to fix a `NuGet/login@v1` publish workflow | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User publishes to private feeds or Azure Artifacts | Agent uses the feed-specific workflow |

| User uses a non-GitHub Actions CI system | Agent uses the CI-specific workflow |

| User edits package metadata with no publishing change | Agent uses a package metadata workflow |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Packable project scope | Yes | Agent records package identity and packability facts. |

| Workflow filename | Yes | Agent uses the exact filename for nuget.org policy setup. |

| Repository owner and name | Yes | Agent gives the exact policy values from these inputs. |

| Existing publish workflow | No | Agent uses this input for in-place migration. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent audits packable projects and existing publish workflows. | Run `rg -n "IsPackable|dotnet pack|dotnet nuget push|NuGet/login@v1|NUGET_API_KEY" <scope>`. | Audit lists packable projects and existing publish paths. |

| 2 | Agent states the exact nuget.org trusted publishing policy values. | Read the migration note. | Note lists owner, repository, workflow filename, and optional environment values exactly. |

| 3 | Agent writes or updates the GitHub Actions workflow with `id-token: write` and `NuGet/login@v1`. | Read the workflow YAML. | Workflow contains explicit permissions and OIDC login. |

| 4 | Agent uses the temporary API key from the login step for push commands. | Read the push step. | Push step uses `${{ steps.login.outputs.NUGET_API_KEY }}`. |

| 5 | Agent keeps the old secret until the first OIDC publish path passes. | Read the migration note and workflow change. | Old secret stays available until first publish verification finishes. |

| 6 | Agent runs local pack verification. | Run `dotnet pack -c Release -o .\artifacts\packages`. | Pack step succeeds for each intended package. |



## Workflow Matrix



| Concern | Agent action |

|---|---|

| Job permissions | Agent adds `id-token: write` and explicit read permissions. |

| Policy match | Agent uses the exact publishing workflow filename. |

| Push auth | Agent uses the login-step output key only. |

| Migration safety | Agent removes long-lived secrets after first verified pass only. |

| Scope | Agent uses this path for nuget.org only. |



## Validation Checklist



- [ ] Agent audited packable projects and workflows first.

- [ ] Agent gave exact policy values for nuget.org setup.

- [ ] Agent added `id-token: write` and `NuGet/login@v1`.

- [ ] Agent used the login-step output for package push.

- [ ] Agent kept old secrets until the first verified OIDC pass.

- [ ] Agent ran local pack verification.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent uses a workflow filename that differs from the nuget.org policy | Agent uses the exact filename. |

| Agent removes `NUGET_API_KEY` immediately | Agent keeps it until the first trusted publish path passes. |

| Agent omits `id-token: write` | Agent adds explicit job permissions first. |

| Agent applies nuget.org guidance to private feeds | Agent routes private feeds to a different workflow. |

