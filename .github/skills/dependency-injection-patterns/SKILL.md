---

name: dependency-injection-patterns

description: Apply practical dependency injection patterns in .NET with clear composition roots, correct lifetimes, scoped work handling, keyed services, and test-friendly registration.

title: Dependency Injection Patterns

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1360

appliesTo: '**/*.{cs,csproj,json}'

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - configuration-options-pattern

  - wpf-project-setup

  - webjobs-authoring

tags:

  - dependency-injection

  - dotnet

  - services

  - architecture

---

# Dependency Injection Patterns



Agent uses this skill to shape a clean composition root and predictable service graph in .NET hosts.



## When to Use



| User prompt | Use |

|---|---|

| User asks for service registration strategy | Agent uses this skill |

| User reports lifetime confusion or scope defects | Agent uses this skill |

| User needs test-friendly dependency replacement | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User avoids dependency injection entirely | Agent uses the existing host pattern |

| User asks about configuration binding only | Agent uses `configuration-options-pattern` |

| User asks about caching, auth, or HTTP resilience only | Agent uses the matching domain skill |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Host type | Yes | Agent records API, worker, desktop, or library scope. |

| Service categories | Yes | Agent groups repositories, clients, processors, UI services, and utilities. |

| Lifetime constraints | No | Agent records state, scoped dependencies, and construction cost. |

| Testing strategy | No | Agent records how tests replace services. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent defines one composition root or registration extension path. | Run `rg -n "Add.*Module|Add.*Services|CreateBuilder|HostApplicationBuilder" <scope>`. | Registration code is centralized. |

| 2 | Agent assigns singleton, scoped, and transient lifetimes intentionally. | Read the registration list. | Each lifetime matches state, dependency shape, and cost. |

| 3 | Agent injects typed options instead of raw configuration keys when configuration has shape. | Run `rg -n "Configure<|IOptions<|IOptionsSnapshot<" <scope>`. | Options types replace raw string-key configuration access. |

| 4 | Agent uses scope factories for scoped work inside singleton hosts. | Run `rg -n "IServiceScopeFactory|CreateScope|CreateAsyncScope" <scope>`. | Scoped services are not captured by singletons. |

| 5 | Agent limits keyed services to stable strategy selection. | Run `rg -n "AddKeyed|FromKeyedServices" <scope>`. | Keyed services exist only where strategy selection is explicit. |

| 6 | Agent runs container validation and targeted tests. | Run `dotnet build <project-or-solution>` and targeted tests. | Container builds, scopes validate, and tests pass. |



## Pattern Matrix



| Concern | Agent action |

|---|---|

| Composition root | Agent keeps registration in startup or registration extensions. |

| Lifetimes | Agent uses singleton for stateless shared services, scoped for request or unit-of-work services, and transient for lightweight stateless services. |

| Runtime creation | Agent uses factories or scope factories instead of service locator access. |

| Test replacement | Agent removes and replaces registrations in tests without editing production code. |

| Validation | Agent enables scope and build validation in startup or tests. |



## Validation Checklist



- [ ] Agent centralized service registration.

- [ ] Agent chose lifetimes intentionally.

- [ ] Agent used typed options for shaped configuration.

- [ ] Agent prevented singleton capture of scoped services.

- [ ] Agent limited keyed services to explicit strategy selection.

- [ ] Agent ran build, validation, and targeted tests.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent injects `IServiceProvider` everywhere | Agent uses constructor injection and factories only at integration edges. |

| Agent lets singletons capture scoped services | Agent changes the design or uses a scope factory. |

| Agent registers everything as transient | Agent chooses lifetimes from behavior and cost. |

| Agent skips container validation | Agent enables validation during startup or tests. |

