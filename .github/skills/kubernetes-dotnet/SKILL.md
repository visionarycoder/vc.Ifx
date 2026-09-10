---
name: kubernetes-dotnet
title: Kubernetes for .NET
description: Deploy, configure, scale, observe, and update .NET applications on Kubernetes with health probes, secure configuration, and platform integration patterns.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1871
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - docker-dotnet
  - aspire-orchestration
  - health-checks-aspnetcore
  - configuring-opentelemetry-dotnet
  - dotnet-webapi
appliesTo: '**/*.{cs,csproj,sln,slnx,json,yml,yaml,md}'
tags:
  - kubernetes
  - dotnet
  - helm
  - containers
  - observability
---
# Kubernetes for .NET

Agent deploys .NET workloads to Kubernetes with clear workload manifests, probe contracts, configuration surfaces, rollout controls, scaling rules, and cluster-facing observability.

## When to Use

| Condition | Use |
|---|---|
| .NET service deploys to Kubernetes or OpenShift-style clusters | Use this skill |
| Team needs Deployment, Service, Ingress, or Helm guidance for a .NET app | Use this skill |
| Workload needs readiness, liveness, startup probes, HPA, or rollout tuning | Use this skill |
| Request combines .NET Aspire resources with Kubernetes packaging | Use this skill |
| Platform design includes Prometheus, Grafana, or service mesh integration | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Request focuses on image construction only | Use `docker-dotnet` |
| Request focuses on one ASP.NET Core health endpoint implementation | Use `health-checks-aspnetcore` |
| Request focuses on local AppHost orchestration without cluster deployment | Use `aspire-orchestration` |
| Workload targets a single VM, App Service, or Windows Service host | Use the matching deployment skill for that platform |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Workload type | Yes | API, worker, gRPC, scheduled job, or stateful service |
| Image contract | Yes | Registry, tag strategy, ports, and startup command |
| Configuration model | Yes | ConfigMaps, Secrets, mounted files, and external providers |
| Traffic model | Yes | Internal-only, cluster ingress, gateway, or mesh |
| SLO and scaling signals | Yes | CPU, memory, queue depth, request rate, and rollout constraints |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Choose Deployment, StatefulSet, Job, or CronJob from workload and storage needs. | Resource kind matches process lifetime and persistence requirements. |
| 2 | Author manifests or Helm templates for identity, networking, and rollout ownership. | The manifest set covers workload, Service, and ingress-facing resources. |
| 3 | Map .NET configuration into ConfigMaps, Secrets, mounts, or external providers. | Runtime settings stay outside the image. |
| 4 | Wire probes, requests, limits, autoscaling, and rollout policy to the app health contract and SLOs. | Readiness, startup, and scaling behavior stay explicit. |
| 5 | Verify render or apply output, service reachability, and observability. | The workload becomes ready, routes traffic, and exposes required signals. |


## Workload Decision Matrix

| Scenario | Preferred resource | Agent rule |
|---|---|---|
| Stateless ASP.NET Core API or web app | `Deployment` | Use replica-based rollout and probe-driven traffic gating. |
| Queue consumer or background worker | `Deployment` or `Job` | Use `Job` for finite work and `Deployment` for continuous consumers. |
| Scheduled .NET process | `CronJob` | Keep schedule, concurrency policy, and history limits explicit. |
| Stateful .NET app with durable disk identity | `StatefulSet` plus PVC | Agent preserves stable pod identity and storage claims. |
| One-off migration or batch repair | `Job` | Separate migration lifetime from long-running service lifetime. |
| Service with startup-heavy initialization | Resource above plus startup probe | Add startup probe before readiness begins. |

## Traffic, Config, and Platform Matrix

| Concern | Preferred pattern | Avoid |
|---|---|---|
| Service discovery | Cluster `Service` plus DNS name | Pod IP references |
| Public entry | `Ingress` or gateway with TLS termination | NodePort as the default public path |
| App settings | ConfigMap keys or mounted files | Baked environment-specific JSON |
| Secrets | Secret, CSI driver, or external secret operator | Plaintext secrets in committed values |
| Health probes | Separate liveness, readiness, and startup endpoints | One probe path for every lifecycle state |
| Service mesh | Sidecar injection only for stated mTLS, traffic, or telemetry value | Mesh adoption with no outcome |
| Monitoring | Prometheus scrape config plus Grafana and OpenTelemetry alignment | Metrics with no scrape path |
| Aspire integration | Shared resource names and env contracts across AppHost and cluster | Divergent local and cluster naming |


## Resource and Rollout Matrix

| Decision point | Preferred pattern |
|---|---|
| CPU and memory | Requests from steady-state usage and limits from safe burst headroom |
| Horizontal scaling | HPA on CPU, memory, queue depth, or request rate |
| Rolling update | Explicit `maxUnavailable` and `maxSurge` |
| Rollback | Immutable image tags plus revision history |
| Persistent storage | PVC with explicit class and access mode |
| Helm packaging | Values for image, config, probes, resources, ingress, and secrets references |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Manifest fit and config surface | Review workload kind, Service, ingress, ConfigMaps, Secrets, and mounts. | Resources match lifecycle and settings stay externalized. |
| Probe, resource, and rollout semantics | Inspect health endpoints, requests, limits, HPA, and update strategy. | Probes reflect lifecycle intent and scaling or rollback stays explicit. |
| Reachability and observability | Resolve Service DNS, ingress route, and scrape or trace configuration. | Clients reach the service and required logs, metrics, and traces are visible. |
| Stateful behavior | Restart one stateful pod when storage is in scope. | Data and stable identity survive restart. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Readiness and liveness probe point to the same fragile dependency chain | Separate the probe contracts by lifecycle intent. |
| `latest` image tag breaks rollback clarity | Agent moves to immutable version or commit-based tags. |
| ConfigMap stores secrets beside public settings | Agent splits secret values into a Secret or external provider path. |
| HPA scales on CPU while the bottleneck is queue depth or request concurrency | Select a metric that matches the service pressure source. |
| No resource requests leads to unstable scheduling | Add measured requests and safe limits. |
| Ingress path and ASP.NET Core base path drift apart | Align path rewriting, forwarded headers, and app route base settings. |
| Stateful workload writes data into the container filesystem | Agent mounts a PVC and updates the app path contract. |
| Service mesh adds latency and policy surface with no outcome | Record the non-mesh path and its simpler tradeoff. |

## Outputs

- Kubernetes or Helm pattern matched to the .NET workload type
- Externalized config, secret, probe, resource, and rollout model
- Autoscaling, ingress, storage, and monitoring guidance
- Verification checks for readiness, routing, scaling, and observability

