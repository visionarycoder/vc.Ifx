---
name: wpf-project-setup
title: WPF Project Setup
description: Create .NET WPF applications that start through the generic host, centralize resources, and expose stable seams for screens, services, and configuration.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1155
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dependency-injection-patterns
  - configuration-options-pattern
  - wpf-mvvm-implementation
appliesTo: '**/*.{cs,csproj,xaml,json}'
tags:
  - wpf
  - desktop
  - xaml
  - dotnet
---
# WPF Project Setup

Agent builds a WPF shell that restores, starts through the generic host, resolves the startup window from DI, and exposes clean seams for later feature work.

## When to Use

| Condition | Use |
|---|---|
| Agent creates a new WPF application. | Agent uses this skill. |
| Agent standardizes startup, resources, or dependency injection in an existing WPF application. | Agent uses this skill. |
| Agent prepares a desktop shell for later MVVM, HTTP, or configuration work. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent edits one existing view in a stable WPF application. | Agent uses `wpf-screen-generation`. |
| Agent focuses on view-model behavior instead of shell composition. | Agent uses `wpf-mvvm-implementation`. |
| Agent targets WinUI, MAUI, Blazor Hybrid, or a web client. | Agent uses a stack-specific skill. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Project name | Yes | Project name aligns with repository naming patterns. |
| Startup shell | Yes | Shell identifies the first window or navigation host. |
| Target framework | No | Agent defaults to `net10.0-windows`. |
| Theme choice | No | Theme choice identifies native WPF or one theme package. |
| Configuration sources | No | Sources identify app settings, environment variables, or secure overrides. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Create the project file | Agent defines an SDK-style WPF project with `UseWPF`, nullable annotations, and only required packages. | Project file | Agent restores and builds the project. | Build succeeds and the output is a WPF executable. |
| 2. Define the folder layout | Agent creates predictable folders for views, view models, resources, services, and infrastructure. | Folder map | Agent inspects the folder map. | Each major concern has one obvious home. |
| 3. Bootstrap the host | Agent starts the generic host in `App.xaml.cs` and registers the shell window and startup services in DI. | Startup composition | Agent starts the application. | The shell resolves from `IHost.Services`. |
| 4. Centralize resources | Agent merges shared dictionaries in `App.xaml` and keeps feature-specific resources close to owned views. | Resource plan | Agent opens the application. | Shared brushes and styles resolve without lookup errors. |
| 5. Add configuration seams | Agent binds runtime settings into typed options instead of hard-coded values. | Options contract | Agent reads bound options during startup. | Startup reads external configuration without code edits. |
| 6. Prove a vertical slice | Agent binds one view model to the shell and shows visible data from the view model. | Startup slice | Agent runs the application. | The shell renders bound data instead of code-behind literals. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Build verification | `dotnet build [project].csproj` | Zero compile errors |
| Startup verification | `dotnet run --project [project].csproj` | Shell window opens without startup exceptions |
| Resource verification | Manual startup pass | Zero missing resource errors |

## Verification Checklist

- [ ] Agent targets `net10.0-windows` and enables WPF.
- [ ] Agent resolves the shell from DI.
- [ ] Agent centralizes shared resources in merged dictionaries.
- [ ] Agent binds one real view model into the shell.
- [ ] Agent externalizes runtime settings through configuration.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent constructs `MainWindow` directly in startup code. | Agent resolves the window from the host service provider. |
| Agent scatters colors and styles across feature views. | Agent promotes shared values into merged dictionaries. |
| Agent leaves navigation or dialog logic in click handlers. | Agent moves those concerns behind services. |
| Agent delays configuration and logging work until later. | Agent establishes host-based configuration from the first build. |
