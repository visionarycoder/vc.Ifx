---
name: xml-schema-inference
title: XML Schema Inference
description: Infer XML structure, namespace defects, and flat projection fields from XML files and report counted findings with row and path scope.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1195
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - data-contract-frontmatter
appliesTo: '**/*.{xml,xsd,json,md}'
tags:
  - xml
  - schema
  - inference
  - ingestion
---
# XML Schema Inference

Agent uses this skill to infer XML structure, namespace defects, and flat projection fields from XML files.

## When to Use

| User prompt | Use |
|---|---|
| User asks to infer XSD from XML | Agent uses this skill |
| User asks to flatten XML into table fields | Agent uses this skill |
| User asks to find namespace or mixed-content defects in XML | Agent uses this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User provides XSD only and asks standards questions | Agent answers directly |
| User asks to find finance duplicates or balance defects | Agent routes to finance skills |
| User asks to write source XML changes | Agent reports findings only |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| XML file | Yes | Agent reads `.xml`. |
| XSD file | No | Agent runs XSD comparison when the file exists. |
| Sample limit | No | Agent uses `1000` scalar values per path when the user omits a value. |
| Preview row limit | No | Agent uses `500` flattened rows when the user omits a value. |

## Inference Pattern Table

| Pattern | Trigger | Output | Test | Pass |
|---|---|---|---|---|
| Namespace map | Root or nested namespace declarations exist | Namespace inventory | Run namespace scan. | Every prefix maps to one URI or one conflict row. |
| Namespace defect | Undeclared prefix or conflicting URI exists | Defect row | Run namespace comparison. | Every defect row includes prefix and path. |
| Repeating element | Same sibling name repeats under one parent | Table root candidate | Run sibling frequency scan. | Every candidate path has count `> 1`. |
| Attribute field | Element has attributes | Column candidate | Run attribute scan. | Every attribute field includes source path. |
| Mixed content | Element has text and child elements | Exclusion or warning row | Run node content scan. | Every flagged path includes text-plus-child evidence. |
| Optional field | Path missing in at least one peer row | Nullable field | Run presence ratio scan. | Every nullable field has ratio `< 1.0`. |
| Scalar type | Sample values fit one primitive type | XSD primitive | Run scalar coercion scan. | Every typed field lists primitive type and sample count. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads XML and records namespace declarations. | Run XML parse and namespace scan. | Agent reports root path, namespace count, and parse status. |
| 2 | Agent walks the tree and counts element paths and sibling repeats. | Run path frequency scan. | Every repeated path has count and parent path. |
| 3 | Agent records attributes, text nodes, and mixed-content paths. | Run node content scan. | Every flagged path includes node type and path scope. |
| 4 | Agent samples scalar values and infers primitive type per leaf path. | Run scalar coercion scan. | Every inferred field lists primitive type, nullable flag, and sample count. |
| 5 | Agent selects table root candidates from repeating elements. | Run candidate filter on repeated paths. | Every table root candidate has child field count `>= 1`. |
| 6 | Agent generates XSD field rows and flat projection rows. | Read generated schema rows. | Every output row maps to one source path. |
| 7 | Agent compares inferred rows to XSD when a source XSD exists. | Run path and type comparison. | Every mismatch row includes inferred value and XSD value. |

## Workflow Output Table

| Output | Agent action |
|---|---|
| Namespace inventory | Agent reports prefix, URI, and path scope. |
| Inferred fields | Agent reports path, primitive type, nullable flag, and cardinality. |
| Table roots | Agent reports repeating parent path and row count. |
| XSD comparison | Agent reports path mismatches and type mismatches when XSD exists. |
| Flat preview | Agent reports projected column names and preview row count. |

## Decision Table

| Condition | Agent action |
|---|---|
| XML parse fails | Agent stops and reports line and column scope. |
| No repeating path exists | Agent generates one-record schema output. |
| File size exceeds `100 MB` | Agent reads first `10 MB` and reports sampling scope. |
| Mixed content exists on a candidate table root | Agent excludes the path from flat preview rows. |

## Verification

| Scope | Test | Pass |
|---|---|---|
| Modal verbs | Run modal-verb scan command. | Scan returns zero banned-term matches. |
| Front matter | Run `npm run frontmatter:validate`. | Command returns zero validation errors. |
| Pattern coverage | Read the inference pattern table and workflow table. | Every workflow step maps to one pattern or one output row. |
| Output clarity | Read the workflow output table. | Every output row includes path scope. |
