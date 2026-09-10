---

name: configuring-opentelemetry-dotnet

license: MIT

title: Configuring OpenTelemetry in .NET

description: Configure OpenTelemetry tracing, metrics, and logging for .NET services with stable resource identity, explicit custom sources, and exporter verification.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1120

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - dotnet-telemetry-standards

  - application-insights-dotnet

appliesTo: '**/*.{cs,csproj,json}'

tags:

  - dotnet

  - opentelemetry

  - telemetry

  - observability

  - tracing

---

# Configuring OpenTelemetry in .NET



Agent uses this skill to add OpenTelemetry tracing, metrics, and logging to .NET hosts.



## When to Use



| User prompt | Use |

|---|---|

| User asks for distributed tracing, metrics, or OTLP logging | Agent uses this skill |

| User asks to standardize exporter configuration | Agent uses this skill |

| User asks to add custom `ActivitySource` spans or `Meter` metrics | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for logging only with no trace or metric scope | Agent uses a logging workflow |

| User asks for direct Application Insights SDK guidance | Agent uses `application-insights-dotnet` |

| User asks for vendor-specific APM that bypasses OpenTelemetry | Agent uses the vendor workflow |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Host type | Yes | Agent records API, worker, function, or library scope. |

| Export target | Yes | Agent records collector, Aspire dashboard, or backend. |

| Custom source names | No | Agent registers each source name explicitly. |

| Diagnostics goal | No | Agent records new setup, hardening, or troubleshooting scope. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent adds only required packages. | Inspect the project file. | Package list matches the enabled signals and host scope. |

| 2 | Agent sets service name and version once. | Read startup registration code. | Resource metadata appears in one central registration path. |

| 3 | Agent wires tracing, metrics, and optional logging through `AddOpenTelemetry()`. | Run `rg -n "AddOpenTelemetry|WithTracing|WithMetrics|WithLogging" <scope>`. | Startup contains one unified OpenTelemetry pipeline. |

| 4 | Agent registers every custom `ActivitySource` and `Meter` name. | Compare source and meter names in code and startup. | Registration names match code names exactly. |

| 5 | Agent configures exporter endpoints and safe filters. | Read config and startup code. | Exporter settings come from configuration and health-noise filters are explicit. |

| 6 | Agent runs build and exporter verification. | Run `dotnet build <project-or-solution>` and inspect the backend or collector output. | Build passes and traces or metrics arrive with correlation data. |



## Pattern Matrix



| Concern | Agent action |

|---|---|

| Core packages | Agent adds hosting, instrumentation, and exporter packages only for the selected signals. |

| Resource identity | Agent uses one `ConfigureResource` path. |

| Tracing | Agent adds ASP.NET Core and HttpClient instrumentation plus required extras only. |

| Metrics | Agent registers custom meters and low-cardinality tags. |

| Logging | Agent adds correlated OpenTelemetry logging only when the prompt includes logging scope. |

| Sensitive data | Agent omits request bodies, secrets, and unbounded tags. |



## Validation Checklist



- [ ] Agent used configuration-driven exporter settings.

- [ ] Agent registered tracing, metrics, and optional logging in one pipeline.

- [ ] Agent matched custom source and meter names exactly.

- [ ] Agent preserved correlation identifiers across exported signals.

- [ ] Agent kept metric dimensions bounded and safe.

- [ ] Agent ran build and backend verification.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent registers a custom source name in code but not startup | Agent centralizes source names and matches them exactly. |

| Agent uses console exporters as a production default | Agent uses the approved backend exporter path. |

| Agent tags telemetry with sensitive or high-cardinality data | Agent removes unsafe or unbounded tags. |

| Agent installs every instrumentation package | Agent installs only packages required by the host scope. |

