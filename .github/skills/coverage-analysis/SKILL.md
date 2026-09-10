---
name: coverage-analysis
title: Coverage Analysis
description: Analyze .NET coverage and CRAP hotspots from Cobertura data and generate a short priority report.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 894
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - crap-score
  - run-tests
  - code-testing-agent
appliesTo: '**/*.{cs,csproj,sln,slnx,json,xml}'
tags:
  - coverage
  - cobertura
  - crap
  - analysis
  - dotnet
---
# Coverage Analysis

Agent analyzes .NET coverage data and ranks risky low-coverage code.

## When to Use

| User prompt | Use |
|---|---|
| User asks why coverage stalled | Use this skill |
| User asks where tests add the most value | Use this skill |
| User asks for coverage hotspots from Cobertura data | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for one method CRAP score | Use `crap-score` |
| User asks to write tests | Use `code-testing-agent` |
| User asks for raw HTML only | Generate the short report first |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Source scope | Yes | Project or solution path |
| Cobertura files | No | Reuse existing files first |
| Coverage thresholds | No | Use when the user gives pass bands |
| Output path | No | Default to `TestResults/coverage-analysis/` |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads the scope and looks for Cobertura XML. | Run `rg -n "coverage\.cobertura\.xml|cobertura" <scope>`. | Existing coverage files are listed or none are found. |
| 2 | Agent reuses existing coverage files when files exist. | Read the selected file paths. | Agent skips new test runs. |
| 3 | Agent generates coverage only when no usable Cobertura file exists. | Run the repo coverage command or `dotnet test` coverage command. | A Cobertura XML file exists under `TestResults`. |
| 4 | Agent preserves raw inputs under `TestResults/coverage-analysis/raw/`. | Read `TestResults/coverage-analysis/raw/`. | Raw coverage files exist. |
| 5 | Agent computes line coverage, branch coverage, and hotspot ranking. | Read the generated summary. | Summary includes overall coverage and hotspot rows. |
| 6 | Agent writes `TestResults/coverage-analysis/coverage-analysis.md`. | Read the markdown file. | File exists and contains summary, hotspots, and next steps. |
| 7 | Agent generates HTML or CSV only when the user asks. | Read the user prompt. | Extra artifacts exist only for explicit requests. |

## Report Content

| Section | Requirement |
|---|---|
| Coverage summary | Include line coverage and branch coverage |
| Hotspots | List risky low-coverage methods or components |
| Recommendations | List 1-3 next test targets |
| Limits | State blockers or missing data |

## Verification Checklist

- [ ] Agent reused existing Cobertura data first.
- [ ] Agent ran coverage only when reuse failed.
- [ ] Agent preserved raw coverage inputs.
- [ ] Agent wrote a short markdown summary.
- [ ] Agent ranked hotspots, not percentages only.
- [ ] Agent kept optional rich artifacts behind an explicit user request.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent runs expensive tests first | Agent reads for Cobertura files first. |
| Agent reports percentages only | Agent adds hotspot ranking and next steps. |
| Agent overwrites user files | Agent writes under `TestResults/coverage-analysis/`. |
| Agent generates HTML by default | Agent writes the markdown summary first. |
