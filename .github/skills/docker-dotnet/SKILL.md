---
name: docker-dotnet
title: Docker for .NET
description: Build, package, run, debug, and secure .NET applications in Docker for local development, CI, and deployment design work.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1840
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - aspire-orchestration
  - health-checks-aspnetcore
  - azure-keyvault-dotnet
  - webapp-frontend-security
appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,yml,yaml,md}'
tags:
  - docker
  - containers
  - dotnet
  - compose
  - security
---
# Docker for .NET

Agent packages .NET applications into Docker images with explicit base-image choices, multi-stage builds, health checks, secure configuration, and local orchestration patterns.

## When to Use

| Condition | Use |
|---|---|
| .NET app needs a repeatable container build | Use this skill |
| Team needs Dockerfile guidance for SDK versus runtime images | Use this skill |
| Local development uses Docker Compose or container-backed dependencies | Use this skill |
| Delivery flow needs image hardening, scanning, or size reduction | Use this skill |
| Request compares container hosting with native deployment | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Kubernetes manifests or cluster operations | `kubernetes-dotnet` |
| Local multi-service orchestration through AppHost | `aspire-orchestration` |
| One ASP.NET Core probe implementation only | `health-checks-aspnetcore` |
| Non-container deployment only | Use the platform deployment skill for that host |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Application type | Yes | Web API, web app, worker, function, console, or Aspire component |
| Build output model | Yes | Framework-dependent, self-contained, or AOT |
| Runtime dependencies | Yes | Native libraries, certs, diagnostics, globalization, ports |
| Deployment target | Yes | Local Docker, Compose, CI registry, App Service, VM, or orchestrator |
| Security posture | Yes | Secret source, scan requirement, user identity, patch cadence |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects SDK, runtime, ASP.NET, or distroless-style image path from the decision matrices. | Read the Dockerfile `FROM` lines. | Base images align with app type and diagnostics needs. |
| 2 | Agent writes a multi-stage Dockerfile with restore, build, publish, and final runtime stages. | Inspect the Dockerfile stages. | Final stage excludes SDK tooling and source files. |
| 3 | Agent configures environment variables, non-secret settings, exposed ports, and health wiring. | Read Dockerfile, compose file, and app configuration. | Runtime settings stay explicit and health endpoints map to the app contract. |
| 4 | Agent places secrets in runtime injection surfaces such as Compose secrets, environment providers, or secret stores. | Search image layers and source files for secret literals. | Secrets stay out of Dockerfile instructions and committed config. |
| 5 | Agent adds local orchestration or debugging support when the workload uses inner-loop containers. | Read compose services, launch settings, or debugger attach notes. | Local workflow covers dependent services and diagnostics access. |
| 6 | Agent verifies build, startup, size, health response, and scan status. | Build the image and run the smallest validation flow. | Image starts, probes respond, and scan output shows no unreviewed critical issue. |

## Base Image Decision Matrix

| Scenario | Preferred image path | Agent rule |
|---|---|---|
| ASP.NET Core web app or API | `mcr.microsoft.com/dotnet/aspnet:10.0` | Agent uses ASP.NET runtime image for HTTP workloads. |
| Background worker or console app | `mcr.microsoft.com/dotnet/runtime:10.0` | Agent uses runtime image when ASP.NET shared framework is absent. |
| Build stage | `mcr.microsoft.com/dotnet/sdk:10.0` | Agent keeps SDK tooling in build stages only. |
| Native AOT or aggressive trim path | Published self-contained output plus minimal runtime base | Agent verifies native dependency coverage before shrinking the base. |
| Debugging-heavy development image | Debian-based SDK or runtime variant | Agent keeps glibc tooling and package availability explicit. |
| Small production image with musl-compatible dependencies | Alpine runtime variant | Agent verifies globalization, native packages, and diagnostics support. |

## Dockerfile Pattern Matrix

| Concern | Preferred pattern | Avoid |
|---|---|---|
| Restore layer reuse | Copy project files first, restore, then copy source | Copy full repo before restore |
| Publish output | `dotnet publish -c Release -o /app/publish` | Run from `bin` or Debug output |
| Final image user | Explicit non-root user when supported | Default root runtime without review |
| Health | HTTP endpoint or process probe aligned to app type | Blind TCP-only checks for readiness |
| Environment | `ASPNETCORE_URLS`, `DOTNET_ENVIRONMENT`, and app settings via runtime injection | Bake environment-specific values into the image |
| Size | Trim caches, exclude build tools, minimize copied content | Ship SDK, tests, and source into final stage |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Stage split | Read the Dockerfile. | Build and final stages are separate. |
| Image build | Run `docker build` for the target image. | Build exits successfully. |
| Runtime contract | Run the container with required ports and environment variables. | Application starts and logs a healthy startup path. |
| Health behavior | Query the container health endpoint or `HEALTHCHECK` status. | Probe reports healthy after startup. |
| Secret handling | Inspect Dockerfile, compose file, and committed config. | No secret literal appears in build instructions or source control. |
| Size discipline | Compare the final image to the base-image choice and publish model. | Final image excludes SDK tooling and unnecessary assets. |
| Security scan | Run the approved image scanner in the delivery flow. | No unreviewed critical issue remains. |
| Debug path | Attach debugger or capture container logs for the local scenario. | Debug workflow reaches the process without changing production defaults. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Using `sdk` as the final runtime image | Agent moves the final stage to `aspnet` or `runtime`. |
| Single-stage Dockerfile copies the whole repo into production image | Agent splits restore, publish, and final runtime stages. |
| Alpine image breaks globalization or native library loading | Agent moves to Debian or adds required packages with verification. |
| Compose file stores connection strings or API keys inline | Agent moves secrets to external injection surfaces. |
| Health check pings a dependency before the app is ready | Agent maps readiness to the application health contract. |
| Container runs as root without a reason | Agent adds an explicit runtime user path or documents the platform constraint. |
| Debug tooling ships in the production image | Agent limits debugger support to development images or attach flows. |
| Docker adds operational weight with no host isolation value | Agent records the native deployment alternative and its tradeoff. |
