---

name: dotnet-aspire-vbd-development

title: .NET Aspire for VBD Development

description: Use Aspire to run and observe VBD development topology while keeping AppHost resources separate from logical component boundaries.

doc_type: skill

status: active

last_updated: 2026-08-30

target_audience: ai

complexity: medium

estimated_tokens: 1230

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - vbd-system-design

related_skills:

  - vbd-phased-modernization

  - ifx-component-communication

  - configuring-opentelemetry-dotnet

appliesTo: '**/*.{cs,csproj,json,md,yml,yaml}'

tags:

  - vbd

  - aspire

  - development

  - testing

  - observability

---

# .NET Aspire for VBD Development



Agent uses this skill to add Aspire as a development and integration-test host for VBD solutions.



## When to Use



| User prompt | Use |

|---|---|

| User asks to run multiple executables and dependencies together | Agent uses this skill |

| User asks for local service discovery, logs, traces, or metrics across a topology | Agent uses this skill |

| User asks for repeatable distributed integration tests | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User has one executable and no useful external dependency | Agent uses a simpler host setup |

| User uses AppHost to justify new service boundaries | Agent uses VBD design skills first |

| User maps AppHost resources directly to production deployment units | Agent keeps deployment design separate |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Executable projects | Yes | Agent lists every host, worker, and client. |

| Resource inventory | Yes | Agent lists stores, queues, caches, and external services. |

| Logical dependency map | Yes | Agent aligns AppHost references to approved VBD relationships. |

| Test topology | Yes | Agent states which dependencies are real, simulated, or emulated. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent adds an AppHost and Service Defaults project. | Run `rg -n "AppHost|ServiceDefaults" .`. | Solution contains the required Aspire host projects. |

| 2 | Agent models executables and resources with stable names. | Read the AppHost resource declarations. | Resource names contain stable logical names and omit environment-specific ports. |

| 3 | Agent wires service discovery and shared observability defaults. | Read Service Defaults and AppHost references. | Projects consume references instead of hardcoded endpoints. |

| 4 | Agent marks simulators and emulators explicitly. | Read project and resource names. | Each non-real dependency has an explicit simulator or emulator label. |

| 5 | Agent runs the topology and reads dashboard signals. | Run `dotnet run --project <AppHostProject>`. | AppHost starts, resources reach healthy state, and dashboard data appears. |

| 6 | Agent adds distributed integration tests for startup, calls, retries, timeouts, and cancellation. | Run `dotnet test <test-project>`. | Distributed tests pass without fixed sleep delays. |

| 7 | Agent documents deployment neutrality. | Read the changed documentation or comments. | AppHost resources are not described as permanent VBD boundaries. |



## Pattern Matrix



| Concern | Agent action |

|---|---|

| Service discovery | Agent uses AppHost references instead of localhost constants. |

| Shared telemetry | Agent keeps logs, traces, and metrics in Service Defaults. |

| Business behavior | Agent keeps business rules out of Service Defaults and Ifx interceptors. |

| Optional dependencies | Agent models simulators and emulators as replaceable resources. |

| Development mode | Agent registers simulators in AppHost for fast local iteration. |

| Simulator toggle | Agent uses configuration to switch between simulator and real resources. |

| Integration tests | Agent starts AppHost, waits for readiness, runs the scenario, and stops cleanly. |



## Simulator Resources in AppHost



Agent registers simulators as AppHost resources so local development starts quickly without external dependencies.



```csharp
// AppHost.cs
var builder = DistributedApplication.CreateBuilder(args);

var useSimulator = builder.Configuration.GetValue<bool>("UseSimulator", defaultValue: true);

if (useSimulator)
{
    // Register simulator services with in-memory behavior and fast startup
    builder.AddProject<Projects.ImportManagerSimulator>("import-manager");
    builder.AddProject<Projects.StorageAccessSimulator>("storage-access");
}
else
{
    // Register real services that depend on external infrastructure
    builder.AddProject<Projects.ImportManager>("import-manager")
        .WithReference(sqlServer);

    builder.AddProject<Projects.StorageAccess>("storage-access")
        .WithReference(azureStorage);
}

builder.AddProject<Projects.WpfClient>("wpf-client")
    .WithReference(builder.GetResource("import-manager"))
    .WithReference(builder.GetResource("storage-access"));
```

Test: AppHost launches successfully in simulator mode and real mode.
Pass: Client discovers services in both modes and the dashboard shows the expected resource set.



## Validation Checklist



- [ ] Agent kept AppHost topology separate from logical VBD boundaries.

- [ ] Agent used stable resource names.

- [ ] Agent replaced hardcoded endpoints with service discovery.

- [ ] Agent exposed correlated logs, traces, and metrics in the dashboard.

- [ ] Agent marked simulators and emulators explicitly.

- [ ] Agent ran distributed tests that wait for readiness.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent turns AppHost topology into architecture | Agent keeps logical and physical maps separate. |

| Agent adds business logic to Service Defaults | Agent limits Service Defaults to cross-cutting host behavior. |

| Agent uses fixed delays in tests | Agent waits for health or readiness signals. |

| Agent leaves localhost constants in project code | Agent uses Aspire service discovery references. |
