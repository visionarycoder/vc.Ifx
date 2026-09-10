---

name: clr-activation-debugging

title: CLR Activation Debugging

description: Diagnose .NET Framework CLR activation defects by reading CLRLoad logs and tracing shim decisions for runtime selection, Feature on Demand, and COM activation.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1252
prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - access to CLRLoad log files

  - PowerShell or text tools

related_skills:

  - agent-instruction-governance


appliesTo: '**/*.md'

tags:

  - dotnet

  - clr

  - diagnostics

  - fod

---

# CLR Activation Debugging



Agent uses this skill to identify why the .NET Framework shim selected, failed to select, or suppressed a CLR.



## When to Use



| User prompt | Use |

|---|---|

| User reports `0x80131700` or `Unable to find a version of the runtime to use` | Agent uses this skill |

| User reports unexpected or suppressed Feature on Demand dialogs | Agent uses this skill |

| User reports wrong CLR selection for native EXE, COM, or legacy host APIs | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User targets modern .NET or CoreCLR | Agent uses a modern .NET diagnostics workflow |

| User reports assembly binding defects after CLR load | Agent uses Fusion diagnostics |

| User reports a runtime crash after successful CLR load | Agent uses crash diagnostics |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| `*.CLRLoad*.log` files | Yes | Agent waits for process exit before reading the files. |

| Symptom and HRESULT | Yes | Agent records the visible defect and error code. |

| Registry and launch context | Recommended | Agent uses this input for SEM and registration checks. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent enables CLRLoad logging with the exact `COMPLUS_CLRLoadLogDir` or registry path and verifies the log directory exists first. | Read the environment variable or registry value and the directory path. | Logging points to an existing directory. |
| 2 | Agent reproduces the defect and collects log files after process exit. | Read `*.CLRLoad*.log` in the target directory. | At least one log file exists for the failing process. |
| 3 | Agent scans literal decision markers. | Run `Select-String -Path *.log -Pattern "Decided on runtime:","ERROR:","Launching feature-on-demand","Had option to launch feature-on-demand","V2.0 Capping"`. | Output contains the decision-path lines or confirms their absence. |
| 4 | Agent traces entry point, config, policy, SEM state, and CLSID registration. | Read matching log lines and registry keys under `HKLM\SOFTWARE\Microsoft\.NETFramework\` and `HKCR\CLSID\{guid}\InprocServer32`. | Diagnosis names the exact lines and registry facts that drove the result. |
| 5 | Agent applies the narrowest fix and reruns the scenario. | Reproduce with the same launch path. | `Decided on runtime:` changes as intended or the error disappears. |
| 6 | Agent removes temporary logging settings after capture. | Read the environment variable or registry path again. | Temporary logging state is removed or documented for cleanup. |



## Decision Matrix



| Symptom | Likely cause | Agent action |

|---|---|---|

| `0x80131700` with no usable runtime | Legacy bind plus missing .NET 3.5 or missing config | Agent installs the required runtime or adds config that points to v4. |

| Unexpected Feature on Demand dialog | Legacy COM activation plus dialog-allowed SEM state | Agent fixes registration or runtime policy and records SEM state. |

| Suppressed Feature on Demand message | Parent process sets `SEM_FAILCRITICALERRORS` | Agent records the inherited SEM state and adjusts the parent path when the prompt includes that scope. |

| Wrong CLR selected | Config order, policy, or host API request favors another runtime | Agent corrects config or host policy. |

| Mixed v2 and v4 loads | Separate activation paths exist in one process | Agent aligns host policy or startup config. |



## Validation Checklist



- [ ] Agent verified the log directory before enabling logging.

- [ ] Agent collected `*.CLRLoad*.log` files after process exit.

- [ ] Agent cited literal decision lines from the logs.

- [ ] Agent cited matching registry or SEM facts.

- [ ] Agent reran the scenario after the fix.

- [ ] Agent removed temporary logging settings or documented cleanup.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent reads the log before process exit | Agent waits for process exit because the file stays open. |

| Agent changes registry paths or literal message text | Agent preserves the exact paths and message fragments. |

| Agent skips SEM state for Feature on Demand cases | Agent records the parent-process error mode. |

| Agent fixes assembly binding instead of CLR activation | Agent routes assembly binding defects to Fusion diagnostics. |

