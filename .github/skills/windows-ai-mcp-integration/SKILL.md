---
name: windows-ai-mcp-integration
title: Windows AI MCP Integration
description: Integrate Windows AI MCP file connectors, model connectors, Semantic Kernel plugins, Windows ML inference, and registry-based connector discovery for local AI workflows.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1503
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mcp-integration-patterns
  - mcp-csharp-create
  - windows-ai-apis
  - windows-ai-foundry-local
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - windows-ai
  - mcp
  - semantic-kernel
  - file-connector
  - model-connector
  - windows-ml
---
# Windows AI MCP Integration

Agent integrates Windows AI connectors through Windows ODR discovery, user-approved file access, model connectors, Semantic Kernel plugins, and local inference paths.

## When to Use

| Condition | Use |
|---|---|
| Work uses the Windows File Explorer MCP connector for file or document operations | Use this skill |
| Work connects a .NET host to Windows-registered MCP servers through ODR | Use this skill |
| Work combines MCP connector data with Semantic Kernel orchestration | Use this skill |
| Work adds local inference through a model connector or Windows ML path | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work only creates a general MCP server or client | Use `mcp-integration-patterns` |
| Work only scaffolds a C# MCP server | Use `mcp-csharp-create` |
| Work focuses on Foundry Local prompt loops or local model catalog usage | Use `windows-ai-foundry-local` |
| Work does not target Windows ODR or Windows-local inference | Use a platform-neutral MCP or AI integration skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Connector target | Yes | Record file connector, model connector, or both. |
| Data path | Yes | Record approved folders, model files, and output destinations. |
| Inference runtime | Yes | Record Windows ML, model connector, ONNX Runtime, or mixed path. |
| Orchestration layer | No | Record Semantic Kernel, raw MCP client, or mixed pipeline. |
| Model alias or model file | No | Record connector-exposed model identity or ONNX asset path. |

## Pattern Matrix

| Scenario | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| User-approved file read, write, search, zip, or folder operations | Windows File Explorer MCP connector | Narrow approved roots and destructive operations explicitly | Tool inventory covers the required file operation |
| MCP-mediated local inference with runtime-defined tools | Windows AI model connector | Bind to discovered tool schemas instead of hard-coded names | Required inference tool appears at runtime |
| App-owned ONNX inference pipeline | Windows ML or ONNX Runtime directly, MCP only for discovery or file access | Keep model execution explicit and testable | Runtime path matches hardware and deployment goals |
| Agent orchestration over connector tools | Semantic Kernel plugins over MCP operations | Keep plugin methods thin and one-to-one | Kernel functions delegate to one MCP or local inference call |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies ODR discovery for the target connector. | Run `odr.exe list` or inspect host inventory output. | Target connector appears with usable launch metadata. |
| 2 | Agent lists connector tools and separates read, write, search, and inference surfaces. | List tools from the connected server. | Intended tool names appear with usable schemas. |
| 3 | Agent wires the .NET MCP client and one representative connector call. | Review connection setup and one call site. | Host connects and one representative call succeeds. |
| 4 | Agent wraps connector calls in Semantic Kernel plugins when orchestration exists. | Inspect plugin methods and attributes. | Each plugin method maps to one MCP operation. |
| 5 | Agent selects model-connector execution or direct Windows ML execution for local inference. | Review runtime setup and model loading code. | Runtime path matches the deployment goal. |
| 6 | Agent validates privacy, containment, and local-data boundaries. | Inspect folder scope, model placement, and telemetry paths. | File and model data stay inside the intended device boundary. |

## Rules

| Topic | Rule |
|---|---|
| Discovery | Treat runtime tool listing as authoritative for model connectors. |
| File access | Record write intent and approved roots before invoking destructive file tools. |
| Plugin design | Separate connector access, orchestration, and inference into thin layers. |
| Large inputs | Chunk large document payloads before inference. |
| Runtime validation | Run one direct runtime validation path before release when the app owns inference. |
| Containment | Keep file data, model files, and telemetry inside the intended Windows-local boundary. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| ODR discovery | Run `odr.exe list`. | Target connector appears once with launch metadata. |
| Tool inventory | List connector tools. | Required file or inference tools appear. |
| Representative call | Call `read_text_file` or another target tool. | Tool returns the expected result envelope. |
| Thin plugins | Review plugin methods. | Each plugin method delegates to one MCP or one local inference call. |
| Inference runtime | Run one representative inference. | Session starts and returns typed results or tensors. |
| Acceleration path | Inspect runtime configuration. | Windows ML or DirectML-aware setup matches the hardware plan. |

## Outputs

- Connector discovery and tool inventory summary
- .NET MCP client setup plan
- Semantic Kernel plugin plan when orchestration exists
- Local inference runtime selection record
- Verification steps for connector calls and local inference

## Reference Files

- [Connector examples reference](references/connector-examples.md)
