---

name: microbenchmarking

license: MIT

title: Benchmark Authoring Guidelines

description: Design, run, and review BenchmarkDotNet microbenchmarks with explicit comparison scope, controlled case count, and measured correctness checks.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1070

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - eval-performance

  - dotnet-trace-collect

appliesTo: '**/*.{cs,csproj,xml,json,md}'

tags:

  - dotnet

  - benchmarkdotnet

  - microbenchmarking

  - performance

---

# Benchmark Authoring Guidelines



Agent uses this skill when BenchmarkDotNet is part of the task.



## When to Use



| User prompt | Use |

|---|---|

| User asks to create or review BenchmarkDotNet benchmarks | Agent uses this skill |

| User asks to compare implementations, runtimes, packages, or configuration | Agent uses this skill |

| User asks to verify a performance claim with measurement | Agent uses this skill |



## When Not to Use



| User prompt | Route |

|---|---|

| User asks for production profiling or tracing | Agent uses a profiling skill |

| User asks for load or end-to-end testing | Agent uses a load-test workflow |

| User asks for telemetry or observability work | Agent uses an observability skill |



## Required Inputs



| Input | Required | Notes |

|---|---|---|

| Benchmark goal | Yes | Agent records coverage, investigation, validation, or feedback scope. |

| Comparison axis | Yes | Agent records A/B code, runtime, package, configuration, or scale comparison. |

| Benchmark scope | Yes | Agent records APIs and representative inputs. |

| Cost budget | No | Agent uses this input to choose job length. |



## Workflow



| Step | Agent action | Test | Pass |

|---|---|---|---|

| 1 | Agent reads the matching BenchmarkDotNet references before editing or running benchmarks. | Read the referenced guidance files. | Benchmark rules and job choices are loaded first. |

| 2 | Agent defines the comparison axis and case count. | Read the benchmark plan. | Plan states baseline, parameters, and expected case count. |

| 3 | Agent writes benchmark code with `GlobalSetup`, representative inputs, and no manual timing loops. | Read the benchmark class. | Setup and benchmark methods separate setup work from measured work. |

| 4 | Agent runs a dry or short job first. | Run `dotnet run -c Release --project <benchmark-project> -- --job Dry --filter *` or `--job Short`. | Benchmark builds and produces a summary. |

| 5 | Agent runs the final job only after the short run passes. | Run the selected final job. | Final run completes and summary tables are available. |

| 6 | Agent reports comparative results instead of absolute claims. | Read the final summary. | Output names the baseline, winner, and tradeoff. |



## Comparison Matrix



| Axis | Agent action |

|---|---|

| A vs B implementation | Agent writes side-by-side benchmarks with a baseline. |

| Runtime comparison | Agent adds multiple jobs or runtime arguments. |

| Package or build comparison | Agent benchmarks explicit before and after artifacts. |

| Scale comparison | Agent uses parameterized representative inputs. |

| Configuration comparison | Agent isolates GC, JIT, or runtime settings by job. |



## Validation Checklist



- [ ] Agent read the matching BenchmarkDotNet references first.

- [ ] Agent defined a baseline and comparison axis.

- [ ] Agent controlled parameter count and job cost.

- [ ] Agent ran a dry or short job before the final run.

- [ ] Agent kept setup work out of measured methods.

- [ ] Agent reported comparative results with the benchmark summary.



## Common Pitfalls



| Pitfall | Agent fix |

|---|---|

| Agent writes benchmarks from memory | Agent reads the current guidance files first. |

| Agent measures too many parameter combinations | Agent estimates case count before running. |

| Agent times manual loops | Agent lets BenchmarkDotNet control invocation count. |

| Agent treats one number as universal truth | Agent compares against the defined baseline. |
