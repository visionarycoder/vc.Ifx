---
name: mcp-integration-patterns
title: MCP Integration Patterns
description: Design or implement Model Context Protocol clients, servers, registry flows, and skill-level tool references across .NET, GitHub MCP, Aspire MCP, and Windows AI MCP.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1353
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - mcp-csharp-create
  - mcp-csharp-debug
  - mcp-csharp-publish
  - mcp-csharp-test
  - windows-ai-mcp-integration
appliesTo: '**/*.{cs,csproj,json,md,yml,yaml}'
tags:
  - mcp
  - json-rpc
  - stdio
  - sse
  - windows-ai
  - github
  - aspire
---
# MCP Integration Patterns

Agent designs MCP clients, servers, and skill references with protocol-clean transports, explicit schemas, and registry-aware discovery.

## When to Use

| Condition | Agent Action |
|---|---|
| Work adds or refactors an MCP client in .NET | Use this skill |
| Work creates or publishes an MCP server in .NET | Use this skill |
| Work maps GitHub MCP, Aspire MCP, or Windows AI MCP into agent workflows | Use this skill |
| Work updates skill guidance that references MCP tools by name or prefix | Use this skill |

## When Not to Use

| Condition | Agent Action |
|---|---|
| Work only scaffolds one C# MCP server project | Use `mcp-csharp-create` |
| Work only debugs one existing MCP server | Use `mcp-csharp-debug` |
| Work only packages or deploys one MCP server | Use `mcp-csharp-publish` |
| Work focuses on Windows AI file and local model flows | Use `windows-ai-mcp-integration` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Integration role | Yes | Host, client, server, or registry scope. |
| Transport | Yes | Stdio or streamable HTTP with request-scoped SSE replies. |
| Surface type | Yes | Tools, resources, prompts, or a mixed surface. |
| Runtime target | No | GitHub MCP, Aspire MCP, Windows AI MCP, or a custom server. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies the integration as host, client, server, or registry work. | Read changed files and task scope. | One primary integration role is recorded. |
| 2 | Agent selects the transport and maps the exposed surface. | Inspect process model, hosting model, and public contracts. | Transport and schemas match the deployment shape. |
| 3 | Agent wires .NET client or server code with protocol-clean behavior. | Read startup code, client setup, and metadata attributes. | Transport code and discovery metadata align. |
| 4 | Agent records exact MCP tool names or prefixes in skill guidance. | Read changed skill guidance. | Skill references identify one clear MCP retrieval or action path. |
| 5 | Agent verifies discovery and one representative call. | Run existing build or targeted verification in scope. | Build passes and one representative MCP operation succeeds. |

## Transport and Surface Matrix

| Concern | Pattern | Pass |
|---|---|---|
| Base protocol | UTF-8 JSON-RPC 2.0 messages | Requests, responses, and notifications stay protocol-clean |
| Stdio transport | Newline-delimited JSON-RPC on stdin and stdout, logs on stderr | Stdout carries MCP traffic only |
| Streamable HTTP | One MCP endpoint with JSON or request-scoped SSE replies | Hosted endpoints return compatible envelopes |
| Tool surface | Side-effecting actions with explicit input schemas | Tools expose clear names and argument contracts |
| Resource surface | Durable content with stable URIs | Resource identities stay stable and read-focused |
| Prompt surface | Reusable prompt templates with named arguments | Templates remain deterministic |

## Skill Hook Matrix

| Skill Authoring Case | Agent Reference Pattern | Pass |
|---|---|---|
| Skill depends on external GitHub state | Cite `github-mcp-server-*` tools by exact name and intent. | Tool references map to one retrieval or action path. |
| Skill depends on local distributed-app state | Cite `aspire-*` tools by resource or diagnostics goal. | Tool references identify apphost, resource, or logs paths. |
| Skill depends on Windows connector discovery | Cite Windows ODR or connector listing before tool calls. | Discovery occurs before connector-specific calls. |
| Skill only needs repository-local files | Omit MCP references and use repository tools instead. | Skill scope stays local and direct. |

## Reference Files

| File | Purpose |
|---|---|
| [references/runtime-catalog.md](references/runtime-catalog.md) | Tool catalogs, .NET client and server examples, registry patterns, pitfalls, and outputs. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Transport alignment | Read client or server startup code. | Stdio or HTTP selection matches the deployment shape. |
| Protocol cleanliness | Inspect stdout and logging configuration for stdio servers. | Stdout carries MCP traffic only. |
| Surface discovery | Run the narrowest discovery flow in scope. | Expected tools, resources, or prompts appear once. |
| Schema clarity | Inspect public MCP members. | Names, descriptions, and arguments are explicit. |
| Registry discovery | Run `odr.exe list` or the host inventory path when Windows ODR applies. | Intended servers are discoverable. |
| Representative call | Invoke one tool, resource, or prompt. | Calls succeed and return the expected envelope. |
