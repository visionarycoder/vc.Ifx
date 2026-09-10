---

name: aspnetcore-identity-setup

title: ASP.NET Core Identity Setup

description: Configure ASP.NET Core Identity with secure defaults, EF-backed stores, account flows, and authorization rules for apps that own user accounts.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1030

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - dotnet-webapi

related_skills:

  - azure-ad-authentication-dotnet

  - webapi-authz-hardening

appliesTo: '**/*.{cs,csproj,json}'

tags:

  - dotnet

  - aspnetcore

  - identity

  - authentication

  - authorization

---

# ASP.NET Core Identity Setup



Agent uses this skill to add application-managed users, passwords, roles, claims, and account flows with ASP.NET Core Identity.



## When to Use



| User prompt | Use |

|---|---|

| User builds registration and login for app-owned accounts | Agent uses this skill |

| User adds role or claim-based authorization | Agent uses this skill |

| User adds confirmation, reset, lockout, or MFA flows | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User builds an API-only system with JWT-only auth and no Identity store | Agent uses a JWT workflow |

| User relies entirely on an external identity provider | Agent uses the external-provider workflow |

| User delegates account lifecycle to another service | Agent uses that service contract instead of Identity |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| User store provider | Yes | Agent records SQL Server, PostgreSQL, SQLite, or equivalent EF scope. |

| Account policies | Yes | Agent records password, lockout, unique-email, and confirmation rules. |

| Auth surface | No | Agent records UI pages, API endpoints, admin flows, external providers, and MFA scope. |

| Authorization model | No | Agent records role and claim rules before implementation. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent defines the user type and EF-backed store. | Read the DbContext and user type. | Store code uses Identity EF patterns and the intended user type. |

| 2 | Agent configures Identity options explicitly. | Run `rg -n "AddIdentity|AddIdentityCore|Password|Lockout|RequireConfirmed" <scope>`. | Startup code sets password, lockout, and confirmation rules explicitly. |

| 3 | Agent wires authentication and authorization middleware in order. | Read `Program.cs`. | Authentication runs before authorization and endpoint mapping. |

| 4 | Agent adds registration, login, confirmation, reset, and sign-out flows that match the prompt scope. | Run targeted auth tests or inspect the route set. | Required account flows exist and respond with safe outcomes. |

| 5 | Agent applies roles and claims intentionally. | Read policies, roles, and claim checks. | Authorization code matches the defined model. |

| 6 | Agent runs build and auth-path verification. | Run `dotnet build <project-or-solution>` and targeted auth tests. | Build passes and auth flows pass. |



## Security Matrix



| Concern | Agent action |

|---|---|

| Passwords | Agent uses Identity hashing only. |

| Confirmation | Agent requires confirmed email when account trust depends on it. |

| Lockout | Agent enables bounded failed-login lockout. |

| Error detail | Agent avoids user-enumeration detail in public responses. |

| Authorization | Agent uses roles and claims for distinct purposes. |



## Validation Checklist



- [ ] Agent wired the user type, DbContext, and stores correctly.

- [ ] Agent set password, lockout, and confirmation rules explicitly.

- [ ] Agent ordered authentication and authorization middleware correctly.

- [ ] Agent implemented only the requested account flows.

- [ ] Agent matched roles and claims to the defined authorization model.

- [ ] Agent ran build and targeted auth verification.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent treats Identity as generic JWT setup | Agent uses Identity only when the app owns user lifecycle and policy. |

| Agent logs raw token or account-state data | Agent exposes safe outcomes only. |

| Agent skips confirmation or lockout decisions | Agent sets both policies explicitly. |

| Agent mixes roles and claims without a model | Agent defines the authorization model first. |

