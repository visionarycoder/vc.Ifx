---

name: dump-collect

description: Configure and collect crash dumps for modern .NET applications by choosing the correct runtime path, collection command, and artifact handoff data.

license: MIT

title: .NET Crash Dump Collection

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1250

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills: []

appliesTo: '**/*'

tags:

  - dump

  - collect

  - diagnostics

---

# .NET Crash Dump Collection



Agent uses this skill to enable or capture crash dumps for modern .NET applications. Agent stops after collection.



## When to Use



| User prompt | Use |

|---|---|

| User asks to enable automatic crash dumps for CoreCLR or Native AOT | Agent uses this skill |

| User asks to capture a dump from a running .NET process | Agent uses this skill |

| User asks to collect dumps in Docker or Kubernetes | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks to analyze a dump file | Agent uses a dump-analysis workflow |

| User asks for profiling or tracing only | Agent uses tracing or profiling skills |

| User targets .NET Framework | Agent uses a .NET Framework dump workflow |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Goal | Yes | Agent distinguishes automatic collection from one-time capture. |

| Platform | Yes | Agent records Windows, Linux, macOS, container, or Kubernetes scope. |

| Runtime | Yes | Agent distinguishes CoreCLR from Native AOT. |

| Process identifier | No | Agent uses this input for live collection. |

| Output directory | Yes | Agent verifies write access before collection. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent identifies CoreCLR, Native AOT, or non-supported runtime. | Run platform-appropriate runtime checks or read the binary metadata. | Runtime is classified before any dump command runs. |

| 2 | Agent verifies the output directory. | Read the target path permissions. | Output directory exists and allows writes. |

| 3 | Agent chooses the collection path. | Compare the scenario to the selected command. | Command matches automatic collection or one-time capture scope. |

| 4 | Agent runs the collection command or writes runtime settings. | Run commands such as `dotnet-dump collect -p <PID> -o .\artifacts\app.dmp` or set `DOTNET_DbgEnableMiniDump=1`. | Command succeeds or settings read back correctly. |

| 5 | Agent verifies the artifact path. | Read the output directory after collection or crash reproduction. | Dump file exists at the reported path. |

| 6 | Agent reports the artifact path and stops. | Read the final handoff note. | Output contains artifact location only and omits analysis. |



## Scenario Matrix



| Scenario | Agent action |

|---|---|

| Running CoreCLR process | Agent uses `dotnet-dump collect` or `createdump`. |

| Running Native AOT process | Agent uses platform-native dump collection for the process. |

| Automatic Linux or container dumps | Agent uses `DOTNET_DbgEnableMiniDump`, dump type, and dump name settings. |

| Kubernetes scope | Agent verifies volume access before enabling dump output. |

| Existing dump file | Agent reports that analysis is out of scope. |



## Validation Checklist



- [ ] Agent identified a supported runtime before collection.

- [ ] Agent verified the output directory.

- [ ] Agent used a command or setting that matches the scenario.

- [ ] Agent verified the dump file path.

- [ ] Agent stopped after collection and did not analyze the dump.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent collects for .NET Framework with modern .NET guidance | Agent routes to a .NET Framework workflow. |

| Agent skips output-directory verification | Agent verifies write access first. |

| Agent installs analysis tools during collection work | Agent installs collection tools only. |

| Agent continues into root-cause investigation | Agent stops after reporting artifact paths. |

