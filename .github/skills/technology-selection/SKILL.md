---
name: technology-selection
description: Maps common .NET AI and ML scenarios to the smallest viable stack with guardrails and validation.
title: .NET AI & Machine Learning (Technology Selection)
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1260
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-webapi
  - webapi-authz-hardening
appliesTo: '**/*.{cs,md,ps1}'
tags:
  - ai
  - ml
  - llm
  - rag
  - ste
---
# .NET AI & Machine Learning (Technology Selection)

This skill maps common .NET AI and ML scenarios to the smallest viable stack.
This skill keeps layering, DI registration, security, and validation explicit.

## Decision Matrix

| Scenario | Preferred stack | Agent rule |
|---|---|---|
| Tabular prediction, ranking, anomaly detection | `Microsoft.ML` | Agent uses ML.NET instead of an LLM. |
| Prompt to response chat or extraction | `Microsoft.Extensions.AI` plus one provider SDK | Agent keeps one abstraction path per workflow. |
| Tool calling or multi-step agent loops | `Microsoft.Extensions.AI` plus `Microsoft.Agents.AI` | Agent sets iteration and token limits. |
| Local or air-gapped inference | `OllamaSharp` or ONNX runtime | Agent selects local inference only when offline or privacy constraints require it. |
| RAG and semantic search | Embeddings plus `VectorData.Abstractions` and a vector store | Agent adds chunking, caching, and source attribution. |
| Copilot extension | `GitHub.Copilot.SDK` | Agent scopes the choice to Copilot-hosted experiences. |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Classify | Agent identifies the problem shape, data type, latency target, and hosting constraints. | Scenario classification |
| 2. Select | Agent chooses the smallest stack from the decision matrix. | Technology decision |
| 3. Register | Agent routes clients through DI and moves secrets into secure configuration surfaces. | Safe composition |
| 4. Guard | Agent adds token limits, retries, structured outputs, or benchmark baselines as the stack requires. | Operational guardrails |
| 5. Validate | Agent builds, tests, and verifies scenario-specific acceptance criteria. | Release-ready evidence |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Stack fit | Review the chosen stack against the scenario classification. | The chosen stack is the smallest viable option for the problem. |
| Layer discipline | Review package set and composition root. | The workflow does not mix competing abstraction layers without reason. |
| Security | Review secret handling and external-client registration. | Secrets stay out of source, and all clients flow through DI. |
| Scenario validation | Run the smallest scenario-appropriate validation. | The chosen stack passes build, tests, and scenario-specific checks. |
