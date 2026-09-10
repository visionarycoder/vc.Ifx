---
name: ai-integration-bundle
title: AI Integration Bundle
description: Routes AI integration work to the correct Windows, Foundry Local, MCP, or Azure AI skill and selects the correct local, hybrid, or cloud path.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 980
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - windows-ai-apis
  - windows-ai-mcp-integration
  - azure-ai-services
appliesTo: '**/*.{cs,csproj,json,md,ps1,py,ts,tsx,yml,yaml}'
tags:
  - ai
  - windows
  - azure
  - foundry-local
  - mcp
  - integration
---
# AI Integration Bundle

Agent uses this bundle for AI requests that span Windows on-device inference, Foundry Local, MCP-connected tools, or Azure AI cloud services.

## When to Use

| Prompt Pattern | Use This Bundle |
|---|---|
| Runtime selection across device, local model catalog, and cloud | Yes |
| AI workflow needs MCP-aware tool orchestration | Yes |
| Local-first plus cloud fallback design | Yes |
| One AI surface is already fixed and no routing choice remains | No |

## When Not to Use

| Prompt Pattern | Agent Route |
|---|---|
| Direct Windows inference API work only | `windows-ai-apis` |
| Foundry Local, Windows connector, or ODR discovery flow only | `windows-ai-mcp-integration` |
| Azure-hosted model or managed AI service only | `azure-ai-services` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Inference location | Yes | On-device, cloud, or hybrid |
| Connectivity expectation | No | Offline, connected, or intermittent |
| Data sensitivity | No | Local-only, regulated, or cloud-approved |
| Tooling pattern | No | App API, MCP server, managed endpoint, or hybrid |

## Activation

| Request Signal | Use Bundle | Route Detail |
|---|---|---|
| Need WinML, DirectML, ONNX Runtime, or local model sessions | Yes | Agent routes to `windows-ai-apis` |
| Need Foundry Local model catalog, prompt loop, or local LLM setup | Yes | Agent routes to `windows-ai-mcp-integration` |
| Need MCP tool exposure, registry discovery, or connector-mediated file access | Yes | Agent routes to `windows-ai-mcp-integration` |
| Need Azure OpenAI, Search, Speech, Vision, or managed safety controls | Yes | Agent routes to `azure-ai-services` |
| Need offline-first local inference with cloud fallback | Yes | Agent combines `windows-ai-apis` or `windows-ai-mcp-integration` with `azure-ai-services` |

## Coverage Matrix

| Need | Primary Skill | Primary Outcome |
|---|---|---|
| Local inference runtime selection | `windows-ai-apis` | Explicit WinML, DirectML, or ONNX execution path |
| Foundry Local setup and local model catalog use | `windows-ai-mcp-integration` | Alias-based local model workflow with on-device execution |
| MCP connector, registry, and tool-contract wiring | `windows-ai-mcp-integration` | Stable tool surface across host and local AI runtime |
| Managed cloud AI services | `azure-ai-services` | Azure-hosted model, search, speech, vision, or document pipeline |
| Hybrid AI architecture | This bundle | Local and cloud tasks split by latency, privacy, and scale |

## Decision Order

| Decision | Agent Action |
|---|---|
| Offline execution, local privacy, or device latency dominates | Agent selects `windows-ai-apis` or `windows-ai-mcp-integration` first. |
| Foundry Local model lifecycle or connector discovery dominates | Agent selects `windows-ai-mcp-integration` first. |
| Enterprise scale, hosted models, or managed safety dominates | Agent selects `azure-ai-services` first. |
| Tool interoperability matters | Agent adds MCP after the runtime choice is explicit. |
| Hybrid execution remains in scope | Agent assigns each task to one local or cloud owner before implementation. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify runtime | Agent maps the request to local, cloud, MCP, or hybrid tracks. | Agent lists one or more tracks. | Every requested capability maps to one track. |
| 2. Select the primary runtime | Agent chooses Windows, Foundry Local, or Azure as the first implementation surface. | Agent records one primary runtime. | Runtime choice fits latency, privacy, and connectivity inputs. |
| 3. Add MCP only when needed | Agent maps tool hosts, connector boundaries, and schema ownership. | Agent records provider and consumer roles. | MCP appears only where tool interoperability adds value. |
| 4. Route to specialist skills | Agent reads only the routed skills and writes the smallest aligned plan or change. | Agent reviews scope. | Output stays inside the routed surfaces. |
| 5. Verify operational fit | Agent checks privacy, latency, scale, and fallback behavior. | Agent compares architecture to stated inputs. | Selected pattern fits the request constraints. |

## Verification Matrix

| Track | Agent Verifies | Test | Pass |
|---|---|---|---|
| Windows runtime | Local inference path fits device and privacy goals. | Inspect runtime location and provider choice. | On-device execution stays intentional. |
| Foundry Local | Local model alias and prompt loop fit the task. | Inspect alias, catalog, and local prompt path. | Model execution stays local and reproducible. |
| Azure AI | Cloud path fits managed scale and service operations. | Inspect endpoint, auth, and service selection. | Azure route uses managed services intentionally. |
| MCP | Tool contract is stable and runtime-aware. | Inspect tool schema and host boundary. | Tool inputs and outputs stay explicit. |
| Hybrid | Local and cloud tasks have explicit boundaries. | Inspect task routing and data flow. | No task crosses boundaries without a reason. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Sending local-only tasks to cloud by default | Agent keeps those tasks on device. |
| Treating Foundry Local as a cloud substitute with no connector or runtime plan | Agent records alias, hardware path, and invocation flow explicitly. |
| Adding MCP where no tool contract exists | Agent removes MCP and keeps direct API integration. |
| Using local inference for workloads that need hosted scale or managed models | Agent routes those workloads to Azure AI. |
| Mixing local and cloud data paths with no ownership boundary | Agent assigns each workflow step to one runtime. |

