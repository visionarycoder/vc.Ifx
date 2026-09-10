---
name: azure-ai-services
title: Azure AI Services
description: Design Azure AI service integrations when work needs Azure OpenAI, Azure AI Search, Document Intelligence, Cognitive Services, or vision, speech, and language APIs.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1860
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-keyvault-dotnet
  - azure-app-configuration-dotnet
  - webapp-frontend-security
appliesTo: '**/*.{cs,csproj,json,bicep,yaml,md}'
tags:
  - azure
  - ai
  - openai
  - cognitive-services
  - search
  - document-intelligence
  - vision
  - speech
  - language
---
# Azure AI Services

Agent designs Azure AI integrations across Azure OpenAI, Azure AI Search, Document Intelligence, Vision, Speech, and Language with service-appropriate contracts and deployment boundaries.

## When to Use

| Condition | Use |
|---|---|
| Application needs hosted LLM, embeddings, or grounded chat flows | Use this skill |
| Work needs indexed retrieval, hybrid search, semantic ranking, or vector search | Use this skill |
| Workflow extracts text, tables, key-value pairs, or document fields | Use this skill |
| Product needs image, speech, or language intelligence from managed Azure services | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Requirement targets local Windows inference with no cloud dependency | Use `windows-ai-apis` |
| Work focuses on local Foundry iteration | Use `windows-ai-foundry-local` |
| Requirement is secret storage or configuration governance only | Use `azure-keyvault-dotnet` or `azure-app-configuration-dotnet` |
| Work needs generic HTTP client resilience with no AI-specific design | Use broader client guidance outside this skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Service set | Yes | Azure OpenAI, Search, Document Intelligence, Vision, Speech, Language, or mixed path. |
| Data sensitivity boundary | Yes | Agent uses it for auth, logging, and retention choices. |
| Retrieval or generation contract | Yes | Prompt, index schema, document model, image input, audio flow, or text-analysis flow. |
| Throughput and latency target | Yes | Agent uses it for batching, caching, and service partition choices. |
| Deployment boundary | No | Agent uses region, subscription, and environment mapping when present. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps each business task to the smallest fitting Azure AI service. | Review service matrix. | Every task lands on one primary service with no overlap drift. |
| 2 | Agent defines auth, secret, and network boundaries. | Inspect config and identity path. | Secrets stay outside source and access scope is explicit. |
| 3 | Agent defines request and response contracts for generation, retrieval, extraction, or classification. | Inspect schemas and payloads. | Contracts are typed, stable, and measurable. |
| 4 | Agent wires indexing, prompt, document, media, or language-processing flows. | Review code and deployment assets. | Runtime path matches the selected service set. |
| 5 | Agent verifies quality, safety, and operational signals. | Run targeted service tests. | Responses meet correctness targets and diagnostics expose failures. |

## Service Selection Matrix

| Service | Preferred Pattern | Avoid |
|---|---|---|
| Azure OpenAI | System prompt plus typed response contract and retrieved context | Free-form string parsing with no schema |
| Azure AI Search | Hybrid or vector retrieval over curated documents and metadata | Full-scan prompt stuffing with no retrieval stage |
| Document Intelligence | Model-per-document-family with field-level validation | One generic parser for unrelated document layouts |
| Vision | Task-specific image analysis pipeline | Sending full-resolution media when reduced input meets the goal |
| Speech | Separate recognition, synthesis, and diarization responsibilities | One endpoint contract that hides distinct speech tasks |
| Language | Task-specific sentiment, key-phrase, entity, or summarization contract | Treating every text task as a chat-completion request |

### Example: Azure OpenAI Chat Request

```json
{
  "messages": [
    { "role": "system", "content": "Return invoice totals as JSON." },
    { "role": "user", "content": "Invoice text goes here." }
  ],
  "response_format": {
    "type": "json_object"
  },
  "temperature": 0
}
```

### Example: Azure AI Search Index Fragment

```json
{
  "name": "knowledge-index",
  "fields": [
    { "name": "id", "type": "Edm.String", "key": true, "searchable": false },
    { "name": "content", "type": "Edm.String", "searchable": true },
    { "name": "category", "type": "Edm.String", "filterable": true },
    { "name": "contentVector", "type": "Collection(Edm.Single)", "searchable": true, "vectorSearchDimensions": 1536 }
  ]
}
```

### Example: Document Intelligence Result Guard

```csharp
if (!result.Documents.Any())
{
    return DocumentParseResult.Empty("No structured document result.");
}

var invoice = result.Documents[0];
var total = invoice.Fields["InvoiceTotal"].ValueCurrency.Amount;
var vendor = invoice.Fields["VendorName"].Content;
```

## MCP Hooks

| Task | MCP Tool | Assistance |
|---|---|---|
| Find AI clients, prompts, index schemas, and deployment assets | `github-mcp-server-search_code` | Locates Azure OpenAI callers, search schema files, and extraction flows. |
| Inspect checked-in config, Bicep, and environment files | `github-mcp-server-get_file_contents` | Reads service endpoints, feature flags, and deployment templates. |
| Review CI workflows for search, model, or app deployment | `github-mcp-server-actions_list` | Lists automation runs tied to AI service rollout. |
| Read failed workflow logs | `github-mcp-server-get_job_logs` | Surfaces secret, quota, packaging, or deployment errors. |
| Inspect workflow, run, or job details | `github-mcp-server-actions_get` | Retrieves deeper GitHub Actions metadata for deployment diagnosis. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Auth boundary | Start app with managed identity or configured secret source | Service client authenticates with no secret in source |
| Retrieval quality | Run one known-answer search or grounding query | Returned documents contain the expected supporting record |
| Generation contract | Invoke one structured Azure OpenAI request | Response matches the declared schema |
| Extraction quality | Submit one representative document | Field outputs match expected labels and values |
| Language task quality | Submit one representative text-analysis request | Returned entities, sentiment, or summary match the expected contract |
| Diagnostics | Review logs and metrics after one failure path | Failure exposes correlation data with no sensitive payload leak |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Prompt output drives business logic with no schema | Agent adds typed response contracts and validation. |
| Search index stores weak metadata | Agent adds filterable fields for tenant, source, and freshness boundaries. |
| Document extraction accepts low-confidence fields silently | Agent adds threshold checks and review branches. |
| Vision, speech, or language flows send oversized or noisy payloads blindly | Agent right-sizes media or text before service calls. |
| Deployment automation omits quota and secret checks | Agent adds workflow validation and secret-source inspection. |
