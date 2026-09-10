---

name: code-testing-agent

title: Code Testing Generation Skill

description: Generate workable unit tests by using a research, planning, implementation, and verification pipeline that matches repository test conventions.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1760

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - run-tests

  - test-gap-analysis

  - assertion-quality

  - writing-mstest-tests

appliesTo: '**/*.{cs,js,ts,py,java,go,rb,rs,swift,kt,php,cpp,cc,cxx,h,hpp}'

tags:

  - testing

  - unit-tests

  - generation

  - polyglot

  - coverage

---



# Code Testing Generation Skill



Agent generates tests by matching project conventions, proving the tests compile, and proving the tests pass before completion.



## When to Use



| Condition | Agent Action |

|---|---|

| User asks for new unit tests or broader coverage | Use this skill |

| Existing tests only need execution | Use `run-tests` |

| Coverage report analysis is the main task | Use `coverage-analysis` or `crap-score` |

| MSTest-specific authoring or modernization is the main task | Use `writing-mstest-tests` |



## Strategy Matrix



| Scope | Agent Strategy | Pass |

|---|---|---|

| Single file or small focused change | Direct generation path | Tests are written and verified without unnecessary sub-agents. |

| Moderate multi-file feature area | One research → plan → implement cycle | Plan and tests cover the requested scope. |

| Large project-wide coverage task | Iterative cycles over prioritized phases | Each phase finishes with passing verification before the next phase starts. |



## Pipeline



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Classify scope | Agent identifies language, framework, current test pattern, and request size. | Agent records source files, target test project, and existing framework. | Scope, framework, and target test location are explicit. |

| 2. Research conventions | Agent reads source APIs, existing tests, and build or test commands. | Inspect selected source and test files in scope. | Conventions for naming, assertions, and test runner are clear. |

| 3. Plan cases | Agent lists happy, edge, and failure cases per target file or feature. | Review generated plan or working notes in scope. | Every target behavior has at least one planned test case. |

| 4. Implement tests | Agent writes test files that follow project structure and dependency patterns. | Inspect changed test files. | Tests match project conventions and target real behavior. |

| 5. Verify build and execution | Agent runs the smallest existing build and test commands that cover the new tests. | Run existing build and test commands in scope. | Zero build errors and zero failing tests. |

| 6. Run quality gate | Agent reviews remaining gaps and assertion quality by using related skills when the task scope benefits from them. | Run the chosen quality checks in scope. | New tests contain meaningful assertions and no obvious uncovered requested behavior remains. |



## Output Contract



Agent delivers:

- New or updated test files in the project test convention

- Passing build and test evidence for changed scope

- Short summary of covered behaviors and any explicit remaining gaps



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Build verification | Run existing build command for the touched project or solution. | Zero build errors. |

| Test verification | Run the smallest existing test command that covers the new tests. | Zero failing tests. |

| Coverage-shape verification | Inspect new tests or run existing focused quality tools in scope. | Happy, edge, and failure behavior each appear at least once in requested scope. |



## Guardrails



- Agent writes tests that exercise existing behavior instead of rewriting production logic to fit the test.

- Agent uses existing test frameworks and repository conventions.

- Agent completes the task only after build and test verification succeed.

