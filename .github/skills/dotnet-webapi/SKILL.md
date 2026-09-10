---

name: dotnet-webapi

title: .NET Web API Project

description: Scaffold or refactor ASP.NET Core Web API projects with OpenAPI, dependency injection, typed contracts, and testable endpoint structure.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1179
prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - webapi-authz-hardening

  - webapi-input-validation

  - webapi-error-contract-hardening

  - webapi-rest-route-standardization

  - webapi-rest-route-standardization

  - webapi-error-contract-hardening

  - rfc-7807-compliance

appliesTo: '**/*.{cs,csproj}'

tags:

  - aspnetcore

  - webapi

  - minimal-api

  - openapi

  - dotnet

---

# .NET Web API Project



Agent uses this skill to scaffold, configure, or refactor ASP.NET Core Web API projects.



## When to Use



| User prompt | Use |

|---|---|

| User asks for a new Web API project | Agent uses this skill |

| User converts a console or library project into a Web API | Agent uses this skill |

| User asks to add OpenAPI, DTOs, routing, or endpoint structure | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for authorization or authentication design | Agent uses `webapi-authz-hardening` or auth skills |

| User asks for input-validation rules | Agent uses `webapi-input-validation` |

| User asks for Problem Details or exception-pipeline design only | Agent uses `webapi-error-contract-hardening` or `aspnet-exception-handler` |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Project name | Yes | Agent uses the repository naming pattern. |

| Target framework | No | Agent uses the current approved .NET target when the prompt does not specify one. |

| Style | No | Agent chooses minimal API or controllers from endpoint count and binding complexity. |

| Persistence provider | No | Agent wires persistence only when the prompt includes it. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent scaffolds or updates the project file with nullable and implicit usings enabled. | Run `rg -n "<TargetFramework>|<Nullable>|<ImplicitUsings>" <project>.csproj`. | Project file contains the intended target framework and enabled compiler defaults. |

| 2 | Agent chooses minimal APIs or controllers and keeps one primary style. | Read `Program.cs` and endpoint files. | Project uses one primary endpoint style. |

| 3 | Agent wires OpenAPI, Problem Details, HTTPS redirection, and dependency injection in startup. | Read `Program.cs`. | Startup registers OpenAPI, Problem Details, and service dependencies. |

| 4 | Agent writes request and response DTOs as explicit contracts. | Run `rg -n "record .*Request|record .*Response" <project>`. | Endpoint contracts use dedicated DTO types. |

| 5 | Agent routes business logic through services instead of endpoint bodies. | Read endpoint handlers and service classes. | Endpoint handlers stay thin and delegate to services. |

| 6 | Agent adds typed results and endpoint metadata. | Run `rg -n "TypedResults|Produces<|ProducesProblem" <project>`. | Endpoints expose typed responses and OpenAPI metadata. |

| 7 | Agent runs build and targeted tests. | Run `dotnet build <project-or-solution>` and `dotnet test <test-project>`. | Build passes and API tests pass. |



## Style Matrix



| Decision | Agent action |

|---|---|

| ≤30 straightforward endpoints | Agent uses minimal APIs. |

| Many endpoints or heavy MVC filters | Agent uses controllers. |

| DTO design | Agent uses dedicated request and response records. |

| Time values | Agent uses `DateTimeOffset` in wire contracts. |

| Endpoint results | Agent uses `TypedResults` and metadata. |

| Cross-cutting exception flow | Agent routes unhandled exceptions through the exception pipeline. |



## Validation Checklist



- [ ] Agent enabled nullable and implicit usings.

- [ ] Agent kept one primary endpoint style.

- [ ] Agent registered OpenAPI and Problem Details.

- [ ] Agent kept business logic out of endpoint bodies.

- [ ] Agent used dedicated DTO contracts.

- [ ] Agent used typed results and response metadata.

- [ ] Agent ran build and API test verification.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent puts business logic in handlers | Agent extracts services. |

| Agent reuses EF entities as wire contracts | Agent writes dedicated DTOs. |

| Agent omits `CancellationToken` on I/O paths | Agent threads cancellation through handlers and services. |

| Agent leaves OpenAPI metadata incomplete | Agent adds `Produces<T>()` and `ProducesProblem()`. |

