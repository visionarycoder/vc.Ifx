---

name: dotnet-trace-collect

title: .NET Trace Collect

description: Capture cross-platform .NET trace artifacts for performance diagnostics by choosing the correct tool, command, and artifact handoff data.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1164
prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - dump-collect



appliesTo: '**/*'

tags:

  - dotnet

  - tracing

  - diagnostics

  - performance

---

# .NET Trace Collect



Agent uses this skill to collect .NET trace artifacts only. Agent leaves trace analysis to a different workflow.



## When to Use



| User prompt | Use |

|---|---|

| User reports high CPU, excessive GC, slow requests, hangs, networking defects, or assembly-load defects | Agent uses this skill |

| User asks for exact trace collection commands | Agent uses this skill |

| User asks for artifact handoff data with a trace file | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for source-code optimization review | Agent uses `analyzing-dotnet-performance` |

| User asks for dump collection | Agent uses `dump-collect` |

| User asks for trace analysis | Agent uses the matching analysis workflow |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Symptom | Yes | Agent maps the symptom to a trace profile. |

| Runtime and version | Yes | Agent separates modern .NET from .NET Framework. |

| Host environment | Yes | Agent distinguishes Windows, Linux, container, and Kubernetes scope. |

| Access level | Yes | Agent records admin or root access before choosing the tool. |

| Repro duration | No | Agent uses this input for circular-buffer collection. |



## Tool Matrix



| Environment | Agent action |

|---|---|

| Windows + modern .NET + admin | Agent uses PerfView first. |

| Windows + .NET Framework | Agent uses PerfView. |

| Linux + root + .NET 10+ | Agent uses `dotnet-trace collect-linux`. |

| Linux without root or older runtime | Agent uses `dotnet-trace collect`. |

| Container or Kubernetes with console access | Agent uses `dotnet-trace` in the target scope. |

| Container or Kubernetes without console access | Agent uses `dotnet-monitor` sidecar collection. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent records runtime, OS, PID, UTC timestamps, and symptom. | Read the collection note or command block. | Metadata contains runtime, OS, PID, timestamps, and symptom. |

| 2 | Agent chooses the collection tool from the environment matrix. | Compare environment facts to the matrix. | Tool choice matches runtime, OS, and access level. |

| 3 | Agent verifies the PID before attach. | Run `dotnet-trace ps`, `Get-Process`, `ps`, or `kubectl exec ... ps`. | Target PID matches the intended process. |

| 4 | Agent runs the exact collection command. | Run the chosen command, such as `dotnet-trace collect -p <PID> -o .\artifacts\app.nettrace` or `PerfView /ThreadTime collect /AcceptEULA /NoGui /DataFile:.\artifacts\app.etl.zip`. | Command exits with a trace artifact path. |

| 5 | Agent verifies artifact existence and handoff data. | Read the artifact path and supporting notes. | Artifact exists and handoff data lists command, path, PID, and timestamps. |

| 6 | Agent stops after collection and routes analysis separately. | Read the final handoff message. | Output contains collection results only. |



## Scenario Matrix



| Symptom | Agent action |

|---|---|

| Excessive GC | Agent uses a GC-focused profile such as `gc-verbose`. |

| Slow requests | Agent uses thread-time collection. |

| Networking defects | Agent adds `System.Net.*` providers. |

| Assembly-load defects | Agent adds assembly-loader events. |

| Long repro | Agent uses a circular-buffer or stop-trigger collection plan. |



## Validation Checklist



- [ ] Agent chose the tool from runtime, OS, and access facts.

- [ ] Agent verified the PID before attach.

- [ ] Agent recorded the exact command and artifact path.

- [ ] Agent recorded runtime, OS, PID, and UTC timestamps.

- [ ] Agent stopped at artifact handoff and did not analyze the trace.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent uses `dotnet-trace` for .NET Framework | Agent uses PerfView. |

| Agent collects without PID verification | Agent confirms the PID first. |

| Agent over-collects providers | Agent adds only providers that match the symptom. |

| Agent triggers on recovery instead of the symptom | Agent uses a stop trigger that matches the symptom. |

