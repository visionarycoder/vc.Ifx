---
title: Prompt Catalog
doc_type: readme
status: active
last_updated: 2026-07-26
target_audience: both
complexity: low
estimated_tokens: 1103
prerequisites: []
related_skills: []
appliesTo: **/*.prompt.md
tags:
  - prompts
  - catalog
  - governance
---
# Prompt Catalog

This folder contains reusable Copilot prompts that provide global value across the repository.

## Mission

Maintain a clear catalog of reusable prompt assets that support consistent engineering workflows.

## Process

Use the criteria below to decide whether a prompt belongs in this folder and to maintain prompt lifecycle hygiene.

## Keep A Prompt Here When

- it is reusable across multiple features or interfaces
- it captures a repeatable engineering workflow
- it improves consistency of planning, review, or migration work

## Do Not Keep A Prompt Here When

- it only describes one completed task
- it is a checklist for work that is already done
- it is a one-off summary or status artifact

## Current Prompts

- `angular-vue-migration-plan.prompt.md`: comprehensive Angular to Vue 3 migration execution plan
- `build-perf.prompt.md`: build performance analysis and optimization workflow
- `code-testing-builder.prompt.md`: compile/build verification step for test generation pipelines
- `code-testing-fixer.prompt.md`: compilation/test error remediation workflow
- `code-testing-generator.prompt.md`: end-to-end test generation orchestration prompt
- `code-testing-implementer.prompt.md`: phased test implementation execution prompt
- `code-testing-linter.prompt.md`: formatting and lint-fix execution prompt
- `code-testing-planner.prompt.md`: test planning and phase breakdown prompt
- `code-testing-researcher.prompt.md`: repository and test-surface research prompt
- `code-testing-tester.prompt.md`: test execution and result handling prompt
- `documentation-remediation-sweep.prompt.md`: documentation governance remediation workflow
- `fems-iet-builder-migration.prompt.md`: FEMS IET builder migration planning and execution
- `msbuild-code-review.prompt.md`: MSBuild project-file quality review prompt
- `msbuild.prompt.md`: MSBuild troubleshooting and optimization prompt
- `optimize-skill-suite.prompt.md`: skill suite optimization and consolidation workflow
- `optimizing-dotnet-performance.prompt.md`: .NET performance review prompt
- `rapid-angular-vue-migration.prompt.md`: compressed AI-driven Angular to Vue migration in 1 week
- `template-engine.prompt.md`: .NET template engine discovery/instantiation/authoring prompt
- `test-quality-auditor.prompt.md`: multi-skill test suite audit prompt
- `testability-migration.prompt.md`: static dependency to abstraction migration prompt
- `unit-test-generation.prompt.md`: concise behavior-pinning unit test generation workflow
- `vbd-conformance-planning.prompt.md`: architecture, message-bus, and observability conformance planning
- `vbd-candidate-red-team.prompt.md`: adversarial review of one candidate without redesigning it
- `vbd-candidate-synthesis.prompt.md`: normalized candidate comparison and developer-selection pause
- `vbd-cutover-readiness.prompt.md`: use-case, data, operations, rollback, and traffic-readiness gate
- `vbd-developer-decision.prompt.md`: durable developer selection or explicit hybridization workflow
- `vbd-design-review.prompt.md`: reusable design review prompt
- `vbd-evidence-collection.prompt.md`: baseline, symbolic mapping, and use-case evidence workflow
- `vbd-independent-candidate.prompt.md`: isolated candidate design and estimation workflow
- `vbd-modernization-planning.prompt.md`: phased legacy analysis, independent candidate comparison, and contract-first planning prompt
- `vbd-migration-rehearsal.prompt.md`: routing, shadow comparison, replay, and rollback rehearsal
- `vbd-portfolio-review.prompt.md`: candidate scorecards, actuals, critical path, and dashboard review
- `vbd-quick-hints.prompt.md`: reusable quick-reference prompt
- `vbd-resume-checkpoint.prompt.md`: manifest-aware incremental resume workflow
- `vbd-selected-construction.prompt.md`: selected-candidate contract-first construction workflow

## Output

The expected output is a curated, up-to-date prompt inventory with clear inclusion and maintenance rules.

## Lifecycle Rules

- Promote reusable prompts from `docs` into this folder.
- Delete or archive prompts that only document completed work.
- Keep naming lower-case and dash-separated.
