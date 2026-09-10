---
name: find-untested-sources
license: MIT
disable-model-invocation: true
title: Find Untested Sources
description: Pair source files to tests with static heuristics and emit a prioritized worklist of likely untested files.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 920
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - coverage-analysis
  - test-gap-analysis
  - code-testing-agent
appliesTo: '**/*.{cs,py,ts,tsx,js,jsx,go,java,rs,rb}'
tags:
  - tests
  - coverage-gap
  - static-analysis
  - prioritization
---
# Find Untested Sources

Agent builds a cheap source-to-test pairing map before slower coverage work begins.

## When to Use

| User prompt | Use |
|---|---|
| User asks where to add tests next | Use this skill |
| User asks for files with no obvious test pair | Use this skill |
| User asks to validate whether generated tests map to intended sources | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for executed line or branch coverage | Use `coverage-analysis` |
| User asks whether current tests are strong | Use `test-gap-analysis` or `assertion-quality` |
| User asks for CRAP scores | Use `crap-score` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Repo root | Yes | Agent needs the analysis boundary. |
| Language profile | No | Agent chooses Roslyn or tree-sitter when omitted. |
| Output limit | No | Agent returns Top N candidates when brevity matters. |

## Engine Table

| Engine | Agent uses it when | Strength | Agent caution |
|---|---|---|---|
| Roslyn script | Repo is effectively .NET-only | Namespace-aware pairing and better symbol matching | Agent avoids tree-sitter when Roslyn precision is available. |
| Tree-sitter script | Repo is polyglot or one cross-language pass is needed | Broad language coverage | Agent states that heuristic precision is lower. |

## Output Table

| Field | Meaning |
|---|---|
| `repo` or `repo_root` | Analysis root |
| `summary` or `counts` | Totals for source, test, paired, and untested files |
| `untested` or `untested_sources` | Prioritized candidate files |
| `source_to_tests` or `tested_sources` | Known pairings |
| `suggested_test_path` | Likely next test location |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent chooses Roslyn or tree-sitter. | Review repo language mix. | Engine fits the repository. |
| 2 | Agent excludes build, vendor, and generated folders. | Review discovery rules. | Worklist avoids noise. |
| 3 | Agent builds the source-to-test pairing map. | Review produced output. | Source and test counts are present. |
| 4 | Agent ranks untested files by declaration surface or likely impact. | Review ordering. | High-value files rise to the top. |
| 5 | Agent states that results are static heuristics, not runtime coverage truth. | Review report wording. | Limitations stay explicit. |

## Verification Checklist

- [ ] Agent picked the correct engine for repo language mix.
- [ ] Agent excluded generated and build artifacts.
- [ ] Agent returned prioritized untested candidates with suggested test paths.
- [ ] Agent described the result as static pairing only.
- [ ] Agent kept the output concise enough to drive next actions.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent presents the result as coverage proof | Agent states heuristic limits explicitly. |
| Agent chooses tree-sitter for a pure C# repo without reason | Agent prefers Roslyn. |
| Agent reads the whole repo manually first | Agent lets the analyzer build the initial worklist. |
| Agent ignores weak pairings that deserve follow-up | Agent surfaces them as candidates for deeper review. |
