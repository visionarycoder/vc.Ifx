---
name: windows-ai-foundry-local
title: Windows AI Foundry Local
description: Build local Windows AI workflows when work needs Foundry Local model testing, prompt engineering, local inferencing, or hardware-aware offline validation.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1760
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - windows-ai-apis
  - windows-ai-mcp-integration
  - mcp-integration-patterns
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - windows
  - ai
  - foundry-local
  - prompt-engineering
  - local-inference
  - on-device
---
# Windows AI Foundry Local

Agent designs and validates Windows AI Foundry Local workflows for local model catalog use, prompt iteration, offline inferencing, and hardware-aware developer loops.

## When to Use

| Condition | Use |
|---|---|
| Work needs local LLM chat or completion on Windows with no cloud round trip | Use this skill |
| Team needs prompt engineering and fast model iteration against local files or sample payloads | Use this skill |
| Work needs model download, cache, load, and unload flow through Foundry Local | Use this skill |
| Requirement includes offline validation across developer GPU, NPU, or CPU hardware | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work depends on Azure-hosted inference, search, or document pipelines | Use `azure-ai-services` |
| Work focuses on raw Windows ML or ONNX Runtime integration | Use `windows-ai-apis` |
| Work focuses on MCP connector wiring and Semantic Kernel tool wrappers | Use `windows-ai-mcp-integration` |
| Target platform excludes Windows | Use platform-neutral local-model guidance outside this skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Model alias or family | Yes | Agent records the local alias from the Foundry catalog. |
| Prompt contract | Yes | Agent records system prompt, user prompt, and expected output shape. |
| Hardware expectation | Yes | Agent records GPU, NPU, or CPU preference and fallback order. |
| Test corpus | Yes | Agent records the representative files, prompts, or fixtures. |
| Offline boundary | No | Agent records whether files and outputs stay fully local. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies Foundry Local CLI and SDK availability. | Run `foundry --version` and restore the target project. | CLI responds and project restore succeeds. |
| 2 | Agent selects a small validation model or the target model alias from the local catalog. | Run `foundry model list` or inspect catalog code. | Selected alias exists and matches the task scope. |
| 3 | Agent defines a prompt contract with bounded temperature, token, and output-shape settings. | Read prompt configuration and sample invocation. | Prompt inputs and expected outputs are explicit. |
| 4 | Agent wires download, cache, load, completion, and disposal flow. | Read runtime setup code. | Local lifecycle steps exist end to end. |
| 5 | Agent validates prompt quality, offline behavior, and fallback execution on representative hardware. | Run one representative local completion. | Completion returns usable output with no cloud dependency. |

## Decision Matrix

| Scenario | Preferred Pattern | Avoid |
|---|---|---|
| First-pass prompt iteration | Small local model alias for short loops | Starting with the largest available model |
| Sensitive document summarization | Local file read plus local completion | Uploading raw documents for early validation |
| Stable structured output | Temperature near zero plus explicit JSON instruction | Free-form output parsing with no contract |
| Mixed hardware fleet | Record accelerator preference and fallback order | Assuming one GPU or NPU profile across every machine |
| Disposable developer loop | Load model once, run a prompt batch, then dispose | Repeated load and unload between single prompts |

## Foundry Local Pattern

```csharp
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;

await FoundryLocalManager.CreateAsync(new Configuration { AppName = "journal-loader" }, NullLogger.Instance);
var manager = FoundryLocalManager.Instance;
var catalog = await manager.GetCatalogAsync();
var model = await catalog.GetModelAsync("qwen2.5-0.5b") ?? throw new InvalidOperationException("Model alias not found.");
if (!await model.IsCachedAsync())
{
    await model.DownloadAsync(_ => { });
}

await model.LoadAsync();
var chatClient = await model.GetChatClientAsync();
var response = await chatClient.CompleteChatAsync([
    new ChatMessage { Role = "system", Content = "Summarize the input in three bullets." },
    new ChatMessage { Role = "user", Content = "Document text goes here." }
]);
manager.Dispose();
```

## MCP Hooks

| Task | MCP Tool or Connector | Assistance |
|---|---|---|
| Find local prompt assets, config, and test fixtures | `github-mcp-server-search_code` | Locates prompt files, SDK setup, and sample payloads. |
| Inspect checked-in local-model config | `github-mcp-server-get_file_contents` | Reads model aliases, runtime config, and developer settings. |
| Feed approved local files into prompt loops | Windows AI MCP file connector | Supplies document text or image inputs from approved folders. |
| Route model invocation through a discovered local connector surface | Windows AI MCP model connector | Bridges Foundry Local-adjacent tool flows when the environment exposes model tools. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| CLI availability | Run `foundry --version` | CLI responds successfully |
| Model catalog | Run `foundry model list` | Target alias appears in the catalog |
| Cache and load | Run one initialization path | Model downloads or loads with no alias fault |
| Prompt contract | Run one representative completion | Output matches the expected shape or style |
| Offline boundary | Disconnect the app from cloud dependencies where applicable and rerun | Workflow still completes locally |
| Hardware fallback | Disable the preferred accelerator and rerun | Alternate local path still returns usable output |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Prompt loop starts with a large model before fit is known | Agent starts with a small alias and scales after evaluation. |
| Prompt settings drift between test runs | Agent records temperature, token, and schema settings beside the prompt. |
| Model lifecycle code skips disposal | Agent disposes the manager and loaded model resources at the end of the run. |
| Local validation quietly calls a cloud service | Agent verifies the runtime path stays local before sign-off. |
| Test corpus is too narrow | Agent adds representative files and failure-oriented prompts. |
