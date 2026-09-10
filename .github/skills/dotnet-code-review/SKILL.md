---
name: dotnet-code-review
title: .NET Code Review
description: Perform repository-aligned .NET code reviews prioritizing layer integrity, specialist rule application, actionable findings.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 1350
prerequisites:
  - dotnet-architectural-layers
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-code-quality-standards
  - dotnet-architectural-layers
  - analyzing-dotnet-performance
  - webapi-hardening-controller
  - test-anti-patterns
appliesTo: "**/*.{cs,csproj,sln,slnx,props,targets,json,md}"
tags:
  - code-review
  - architecture
  - dotnet
  - quality
  - security
  - testing
---

# .NET Code Review

Agent performs review work enforcing repository architecture, specialist quality rules, clear finding formatting. Agent surfaces concrete risks with fix context.

## When to Use

Use when:
- Reviewing PR diffs, files, modules, or projects in .NET solution
- Auditing layer boundaries across `AzureAPI/src`, `src/component`, `src/util`, `src/ifx`
- Combining architecture, security, performance, testing, API feedback into one report
- Producing findings for built-in `code-review` agent or manual review workflow

## When Not to Use

Do not use when:
- Implementing code instead of reviewing it
- Security-only exploit hunting (use dedicated security reviewer first)
- Tiny local edits where narrow specialist skill is sufficient

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Review scope | Yes | Diff, file, folder, project, or module |
| Review mode | Yes | Diff review, standalone audit, or targeted concern review |
| Target frameworks | Yes | `net10.0`, `netstandard2.0`, or mixed |
| Expected output | No | PR comments, summary findings, or architecture audit |

## Review Workflow

Agent performs these steps:

1. Agent loads governing baselines: `dotnet-code-quality-standards`, layer rules, relevant VBD skills when scope touches seams
2. Agent classifies review surface: diff vs full module, app code vs analyzer/tooling code, high-risk boundary vs local implementation
3. Agent verifies dependency direction before local code style (architecture defects invalidate otherwise clean code)
4. Agent applies specialist lenses in order: layer integrity, quality rules, security boundaries, performance and async, tests and API design
5. Agent emits only actionable findings with evidence, risk, fix path

## Review Areas

| Area | Agent Verifies | Escalate When |
|---|---|---|
| Layering | Allowed project references, DTO ownership, contract-vs-service boundaries, VBD invariants | Lower layer leaks upward OR caller skips contract boundary |
| Quality baseline | Naming, nullability, cancellation, async suffixes, suppressions, framework-specific rules | Issue conflicts with `dotnet-code-quality-standards` |
| Security | Validation, auth/authz, injection risk, secret handling, unsafe deserialization | Problem is exploitable OR crosses trust boundary |
| Performance | Sync-over-async, avoidable allocations, poor query shape, hidden hot-path costs | Issue affects measured OR obvious hot path |
| Tests and API fit | Coverage depth, assertion quality, error contracts, boundary semantics | Change weakens confidence OR breaks consumer expectations |

## Finding Format

| Field | Requirement |
|---|---|
| Severity | Use `high`, `medium`, `low` based on impact, confidence |
| Rule | Name violated skill, rule ID, or architecture boundary |
| Location | File and symbol (not general area) |
| Evidence | Quote dependency edge, API misuse, or concrete behavior causing risk |
| Recommendation | Give the smallest clear next step, not a vague preference. |

## Minimum Checks

- Confirm target framework assumptions before applying framework-specific advice.
- Inspect `.csproj` references and namespace usage for forbidden dependency direction.
- Use specialist skill guidance over reviewer preference when a repository rule already exists.
- Keep VBD boundary rules active when reviewing contract ownership, proxy, bus, or component seams.
- Distinguish real security or performance defects from optional hardening suggestions.

## MCP Hooks

| Task | MCP Tool | Assistance |
|---|---|---|
| Search changed APIs, contracts, and cross-project references in GitHub-hosted source | `github-mcp-server-search_code` | Locates exact review targets before diff analysis. |
| Read one remote file or historical version during review context gathering | `github-mcp-server-get_file_contents` | Pulls review evidence without local checkout changes. |
| Review pull request scope and discussion context | `github-mcp-server-list_pull_requests` | Surfaces review scope and merge context. |
| Inspect workflow runs tied to the reviewed change | `github-mcp-server-actions_list` | Exposes failing automation relevant to review findings. |
| Read failed job logs | `github-mcp-server-get_job_logs` | Pulls concrete CI evidence for build, test, or packaging findings. |

## Validation Checklist

- [ ] The review scope and framework classification are explicit.
- [ ] Architecture and dependency direction were checked before stylistic concerns.
- [ ] Findings reference concrete evidence and a governing rule or boundary.
- [ ] Security, performance, and testing concerns were considered where relevant.
- [ ] Output uses concise, actionable finding records instead of broad commentary.
- [ ] No specialist area was contradicted by generic advice.

## Common Pitfalls

| Pitfall | Safer move |
|---|---|
| Leading with style nits while missing a layer violation | Check dependency direction first. |
| Reporting generic security advice without a concrete flaw | Tie the finding to an actual trust-boundary risk. |
| Ignoring framework differences between app and analyzer projects | Classify the target framework before applying rules. |
| Writing findings with no fix path | Include the allowed boundary or recommended next action. |
