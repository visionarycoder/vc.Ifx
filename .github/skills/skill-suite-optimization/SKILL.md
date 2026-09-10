---
name: skill-suite-optimization
title: Skill Suite Optimization
description: Optimize skills, prompts, instructions, and policy indexes for lower token cost, clearer routing, and STE-compliant wording.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1604
prerequisites:
  - documentation-governance
  - documentation-frontmatter
related_skills:
  - create-skill
  - documentation-governance
  - solution-markdown-sync
appliesTo: '.github/**/*.{md,prompt.md,instructions.md}'
tags:
  - optimization
  - guidance
  - ste
  - token-budget
---
# Skill Suite Optimization

Agent optimizes governed guidance files for deterministic parsing, lower token cost, and measurable verification.

## When to Use

| Condition | Use |
|---|---|
| Scope spans many skills, prompts, or instructions | Use this skill |
| A quarterly or release cleanup is requested | Use this skill |
| Token budget violations appear across a guidance family | Use this skill |
| STE wording drift appears across many files | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| One skill needs a small wording update | Edit that skill directly |
| Only front matter is wrong | Use `documentation-frontmatter` |
| Only links or placement are wrong | Use `documentation-governance` |
| A new skill is needed | Use `create-skill` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Scope | Yes | Agent records directories, file types, and excluded paths. |
| Priority rule | Yes | Agent records token, STE, routing, or metadata priority. |
| Fix limit | Yes | Agent records the maximum direct edits for the pass. |
| Report path | Yes | Agent records the output report file before edits start. |
| Validation commands | Yes | Agent records the exact checks for front matter, token counts, and wording. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent builds the governed inventory and counts tokens with `Math.Ceiling(content.Length / 4)`. | Review inventory output. | Inventory lists every file in scope once. |
| 2 | Agent ranks issues by impact: budget overage, broken routing, STE drift, then metadata drift. | Review ranked queue. | Queue order matches the rule matrix. |
| 3 | Agent rewrites high-impact files with tables, compact wording, and front-loaded activation criteria. | Review diffs. | Each rewritten file removes redundancy and keeps the original intent. |
| 4 | Agent moves bulky examples or reference tables into `references/*.md` files beside the governing artifact. | Review file placement. | Main guidance file shrinks and the reference file preserves detail. |
| 5 | Agent reruns token, modal, and link checks, then records before and after metrics. | Review validation output. | Changed files meet their target or carry a named residual risk. |

## Rule Matrix

| Artifact | Target | Primary Optimization Rule | Escalation Rule |
|---|---:|---|---|
| Specialist skill | <2000 tokens | Keep only routing, inputs, workflow, rules, patterns, and verification. | Move examples or catalogs to `references/`. |
| Orchestrator bundle | <2500 tokens | Keep routing tables and child-skill triggers only. | Move deep decision trees to `references/`. |
| Router bundle | <1000 tokens | Keep entry conditions and route table only. | Remove repeated specialist summaries. |
| Prompt | <1500 tokens | Keep inputs, workflow, and output contract only. | Move long examples to `references/` or shorten to one example. |
| Instruction | <800 tokens | Keep rule table and one procedure table only. | Remove narrative repetition. |
| Policy index | Lowest safe count | Keep hard rules, entry points, and verification commands only. | Replace exhaustive inventories with summary tables and filesystem lookup rules. |

## Pattern Matrix

| Concern | Pattern | Location |
|---|---|---|
| Alias retention | Agent records important aliases in one routing row instead of a dedicated section. | Main file |
| Large catalog data | Agent stores inventory, matrix, or glossary detail in reference files. | `references/*.md` |
| Bundle families | Agent extends the existing CA, CWE, MSTest, testing, documentation, and Web API bundles. | Main file + child bundle |
| Section order | Agent uses the same headings across optimized skills. | Main file |
| Audit logic | Agent uses the baseline scripts already stored for this skill. | `references/audit-script.md` |
| Historical comparison | Agent compares new counts to the saved benchmark snapshot. | `references/optimization-history.md` |

## Verification Matrix

| Check | Command or Test | Pass |
|---|---|---|
| Front matter | `npm run frontmatter:validate` | Exit code `0` |
| Modal verbs | Run the repository modal-verb scan on changed files. | Zero prohibited modal matches exist. |
| Token estimates | Recalculate `Math.Ceiling(content.Length / 4)` | Front matter value matches the recalculated value |
| Broken related skills | Compare `related_skills` values to `.github\skills\*\SKILL.md` folder names | Zero broken entries in changed files |
| Link validity | Resolve local relative links from each changed file | Zero broken local links |
| Section shape | Compare optimized skills to the standard heading order | Each optimized skill follows the shared order |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Audit completed | Inventory, ranked queue, and baseline metrics exist. |
| Top issues fixed | Highest-impact items in scope have direct edits. |
| Reference files added | New reference files sit beside the optimized artifact. |
| Counts updated | Token counts and report metrics reflect the final file contents. |
| Residual risk recorded | Remaining out-of-budget or stale files appear in the report. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Rewriting for brevity and dropping verification detail | Agent keeps one measurable test row for each important rule family. |
| Leaving stale `estimated_tokens` values after edits | Agent recalculates the value after every content change. |
| Splitting detail into the wrong directory | Agent uses sibling `references/` folders. |
| Repeating the same route table in many files | Agent centralizes the route table in one bundle or reference file. |
| Editing low-impact files before index files and oversized skills | Agent fixes the ranked queue from top to bottom. |
