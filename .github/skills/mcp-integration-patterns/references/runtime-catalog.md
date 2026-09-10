---
title: MCP Runtime Catalog
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
---
# MCP Runtime Catalog

## Available MCP Tools Reference

| MCP Surface | Tool Prefix or Names | Use When | Notes |
|---|---|---|---|
| GitHub MCP discovery | `github-mcp-server-search_code`, `search_issues`, `search_pull_requests`, `search_repositories`, `search_users` | Agent needs exact code search or repository discovery across GitHub. | `search_code` fits symbol lookup. `search_issues` fits natural-language issue discovery. |
| GitHub MCP repository reads | `github-mcp-server-get_file_contents`, `get_commit`, `list_commits`, `list_branches` | Agent needs repository content or history context. | Read paths and commit metadata without cloning a remote repository. |
| GitHub MCP work items | `github-mcp-server-issue_read`, `list_issues`, `pull_request_read`, `list_pull_requests` | Agent needs issue, PR, review, or comment context. | `pull_request_read` exposes diff, files, reviews, comments, and checks. |
| GitHub MCP actions | `github-mcp-server-actions_list`, `actions_get`, `get_job_logs` | Agent inspects workflows, jobs, artifacts, or failed logs. | Use focused fields to limit payload size. |
| GitHub MCP spaces | `github-mcp-server-list_copilot_spaces`, `get_copilot_space` | Agent consumes curated Copilot space context. | Space content arrives as documents with separators. |
| Aspire MCP environment | `aspire-doctor`, `list_apphosts`, `select_apphost`, `refresh_tools` | Agent needs local Aspire environment selection or refresh. | `doctor` works without a running AppHost. |
| Aspire MCP resources | `aspire-list_resources`, `execute_resource_command` | Agent inspects or operates on resources in a selected AppHost. | Use `start`, `stop`, or resource-specific commands through `execute_resource_command`. |
| Aspire MCP diagnostics | `aspire-list_console_logs`, `list_structured_logs`, `list_traces`, `list_trace_structured_logs` | Agent investigates runtime failures, health, or distributed traces. | Trace-scoped logs fit cross-resource diagnosis. |
| Aspire MCP docs | `aspire-list_docs`, `search_docs`, `get_doc`, `list_integrations` | Agent needs Aspire product guidance or integration inventory. | `get_doc` fits full page retrieval by slug. |
| Windows AI MCP | File Explorer connector tools such as `read_text_file`, `search_files`, `create_text_file`, plus registry discovery through `odr.exe list` | Agent needs user-approved file access or Windows-registered server discovery. | Tool inventory remains runtime-discoverable through MCP listing. |

## .NET Client Pattern

| Decision | Agent Action |
|---|---|
| Local subprocess server | Agent uses `StdioClientTransport`. |
| HTTP server | Agent uses an HTTP transport client and targets one MCP endpoint. |
| LLM tool calling | Agent passes `McpClientTool` results into the AI client or kernel abstraction. |

```csharp
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "GitHub MCP",
    Command = "github-mcp-server",
    Arguments = ["stdio"]
});

await using var client = await McpClient.CreateAsync(transport);
var tools = await client.ListToolsAsync();
var result = await client.CallToolAsync(
    "search_code",
    new Dictionary<string, object?> { ["query"] = "\"WithStdioServerTransport\" language:C#" },
    cancellationToken: CancellationToken.None);
```

## .NET Server Pattern

| Surface | Agent Pattern | Metadata Rule |
|---|---|---|
| Tool | Agent marks a class with `[McpServerToolType]` and methods with `[McpServerTool]`. | Agent adds `[Description]` to every public MCP method. |
| Resource | Agent marks a class with `[McpServerResourceType]` and members with `[McpServerResource]`. | Agent uses stable URI identity and read-focused payloads. |
| Prompt | Agent marks a class with `[McpServerPromptType]` and members with `[McpServerPrompt]`. | Agent uses named arguments and deterministic template output. |

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync();

[McpServerToolType]
public static class RepositoryTools
{
    [McpServerTool, Description("Returns the current branch name.")]
    public static string GetCurrentBranch() => "main";
}
```

## Registry Integration Matrix

| Scenario | Agent Action | Verification |
|---|---|---|
| Windows packaged local server | Agent registers through package identity metadata and validates discovery through ODR. | `odr.exe list` returns the server manifest. |
| Windows unpackaged bundle | Agent uses MCP bundle or installer registration and records reduced-containment implications. | Server appears only under the intended registration path. |
| Remote server registration | Agent uses manual ODR registration with explicit endpoint metadata. | Discovery output shows the remote endpoint and policy state. |
| Host app connection | Agent resolves command and arguments from the manifest and creates the matching transport. | Client connects and lists tools successfully. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Server logs on stdout during stdio transport | Agent routes logs to stderr only. |
| Server exposes tool names with vague verbs | Agent renames surfaces with explicit nouns and outcomes. |
| Skill guidance references an MCP family without tool names | Agent adds exact prefixes or tool names and one intent line. |
| HTTP servers expose MCP without host filtering | Agent constrains host names and route scope in ASP.NET Core. |
| Windows registration exists without discovery verification | Agent runs the ODR inventory step before completion. |

## Outputs

| Output | Description |
|---|---|
| Transport selection | Stdio or streamable HTTP guidance |
| .NET client and server integration pattern | Host and transport examples for MCP runtime wiring |
| Tool, resource, and prompt schema map | Surface and contract guidance |
| Registry verification path | Windows ODR or manifest discovery checks |
| Skill-level hook guidance | Exact MCP tool references by intent |
