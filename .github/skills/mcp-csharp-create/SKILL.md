---

name: mcp-csharp-create

title: C# MCP Server Creation

description: Scaffold and configure C# MCP servers for stdio or HTTP transport with explicit tool, prompt, and resource metadata.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1490

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - mcp-csharp-debug

  - mcp-csharp-publish

  - mcp-csharp-test

appliesTo: '**/*.{cs,csproj,json,md}'

tags:

  - mcp

  - csharp

  - scaffolding

---



# C# MCP Server Creation



Agent creates MCP servers by selecting one transport path, scaffolding one project shape, and wiring discoverable server metadata.



## When to Use



| Condition | Agent Action |

|---|---|

| New C# MCP server starts | Use this skill |

| Existing server needs local troubleshooting | Use `mcp-csharp-debug` |

| Server needs automated tests | Use `mcp-csharp-test` |

| Server needs packaging or deployment | Use `mcp-csharp-publish` |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Choose transport | Agent selects stdio for local subprocess use or HTTP for remote service use. | Agent records selected transport. | One transport path is selected for the new server. |

| 2. Scaffold project | Agent uses `dotnet new mcpserver -n <ProjectName>` or `dotnet new web -n <ProjectName>` plus required MCP package. | Run scaffold command. | Project files are created with the intended transport shape. |

| 3. Add MCP surface | Agent adds tool, prompt, or resource types with clear `[Description]` metadata. | Inspect changed source files. | Every public MCP callable member has description metadata. |

| 4. Configure hosting | Agent uses `.WithStdioServerTransport()` for stdio or HTTP MCP registration plus `MapMcp()` for web hosting. | Inspect `Program.cs`. | Transport registration matches the selected project shape. |

| 5. Verify startup | Agent builds and starts the server with the existing toolchain. | Run `dotnet build` and `dotnet run` in scope. | Zero build errors and server startup succeeds. |



## Transport Matrix



| Transport | Agent Uses When | Required Configuration | Pass |

|---|---|---|---|

| Stdio | Local CLI, IDE, or Copilot subprocess scenarios | `AddMcpServer().WithStdioServerTransport()` and stderr-only logging | Server waits for JSON-RPC on stdin with no stdout log noise. |

| HTTP | Remote service, container, or shared-client scenarios | HTTP MCP registration plus `app.MapMcp()` | MCP endpoint is reachable on the configured route. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Build verification | `dotnet build` for the touched project | Zero build errors. |

| Metadata verification | Inspect tool, prompt, and resource members. | Description metadata exists on each exposed MCP surface. |

| Startup verification | `dotnet run` for the touched project | Server starts with the intended transport path. |



## Guardrails



- Agent keeps stdio logs on stderr so stdout stays protocol-clean.

- Agent keeps HTTP hosting explicit with `MapMcp()`.

- Agent exposes only intentional tool, prompt, and resource types.

