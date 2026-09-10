---

name: vbd-drift-analyzer-design

title: VBD Drift Analyzer Design

description: Design Roslyn analyzers, safe code fixes, and generators that prevent VBD architecture drift.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1260

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - vbd-system-design

related_skills:

  - vbd-architecture-conformance

  - vbd-boundary-contract-mapping

appliesTo: '**/*.{cs,csproj,sln,slnx,json,md}'

tags:

  - vbd

  - analyzers

  - code-fixes

  - generators

---



# VBD Drift Analyzer Design



Agent designs enforcement that catches architectural drift early and fixes only deterministic cases automatically.



## When to Use



| Condition | Agent Action |

|---|---|

| VBD rules need compiler-time enforcement | Use this skill |

| Safe rename or code-shape correction is deterministic | Add code fix |

| Rule needs generated boilerplate or derived artifacts | Add generator |

| Rule depends on subjective architecture judgment only | Do not automate the fix |



## Enforcement Matrix



| Rule Type | Agent Uses | Test | Pass |

|---|---|---|---|

| Naming, reference, or API-shape drift | Analyzer | Write diagnostic tests. | Diagnostic appears on violating code and stays absent on compliant code. |

| Deterministic syntax correction | Code fix | Write fix tests. | Fix rewrites only the intended syntax span. |

| Derived registration or metadata output | Generator | Write generator snapshot tests. | Generated output is deterministic and idempotent. |

| Ambiguous architecture advice | Documentation or review guidance | Review rule scope. | No unsafe auto-fix path is shipped. |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Build rule catalog | Agent converts each VBD drift concern into one precise diagnostic rule. | Agent records rule ID, title, and trigger. | Every planned rule has one trigger and one intent statement. |

| 2. Choose enforcement | Agent selects analyzer, code fix, generator, or documentation path per rule. | Review enforcement matrix. | Each rule maps to one enforcement path. |

| 3. Design tests first | Agent writes analyzer, fixer, or generator tests before broad implementation. | Run existing Roslyn test project in scope. | Tests fail before implementation and pass after implementation. |

| 4. Control adoption | Agent sets severity, message, and rollout guidance that fit existing repo policy. | Inspect diagnostic metadata and defaults. | Severity and message align with the intended adoption path. |

| 5. Verify performance and compatibility | Agent keeps `src/ifx/` work compatible with `netstandard2.0` and C# 8. | Run existing build and analyzer tests in scope. | Zero build errors, zero failing tests, zero newer-language syntax in `src/ifx/`. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Analyzer verification | Run existing analyzer test project in scope. | Target diagnostics appear only on violating cases. |

| Code-fix verification | Run existing code-fix tests in scope. | Fix output matches expected text exactly. |

| Build verification | Run existing build command for touched `src/ifx/` projects. | Zero build errors. |



## Guardrails



- Agent keeps Roslyn project changes inside C# 8 language limits.

- Agent avoids broad fixes that rewrite unrelated code.

- Agent records false-positive boundaries in tests before rollout.

