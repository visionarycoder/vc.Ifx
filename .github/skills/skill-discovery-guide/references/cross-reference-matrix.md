---
title: Skill Discovery Cross-Reference Matrix
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Cross-reference lookup matrices for technology dependencies, pattern combinations, problem mapping, and high-usage skill relationships.
tags:
  - skills
  - discovery
  - cross-reference
  - routing
---
# Skill Discovery Cross-Reference Matrix

## Technology Dependency Matrix

| Technology or Stack | Also Consider | Reason |
|---|---|---|
| `graphql-dotnet` | `api-gateway-patterns`, `docker-dotnet`, `kubernetes-dotnet`, `configuring-opentelemetry-dotnet` | Edge routing, packaging, orchestration, telemetry |
| `dotnet-webapi` | `rfc-9457-compliance`, `webapi-input-validation`, `health-checks-aspnetcore`, `rate-limiting-aspnetcore` | Error contract, validation, health, protection |
| `minimal-api-file-upload` | `webapi-input-validation`, `rfc-9457-compliance`, `azure-storage-dotnet`, `cwe-input-validation-fixes` | Safe upload path and error handling |
| `azure-functions-dotnet` | `azure-app-configuration-dotnet`, `azure-keyvault-dotnet`, `application-insights-dotnet`, `azure-storage-dotnet` | Config, secrets, telemetry, bindings |
| `azure-service-bus-patterns` | `message-patterns-dotnet`, `background-services-channels`, `configuring-opentelemetry-dotnet`, `cwe-resource-management-fixes` | Messaging shape, consumers, traces, backpressure |
| `rabbitmq-patterns` | `message-patterns-dotnet`, `background-services-channels`, `docker-dotnet`, `kubernetes-dotnet` | Broker usage, worker hosting, packaging, scale |
| `cqrs-patterns-dotnet` | `event-sourcing-patterns`, `domain-events-dotnet`, `messaging-patterns-bundle`, `result-types-dotnet` | Write segregation, event flow, async transport, command outcomes |
| `event-sourcing-patterns` | `cqrs-patterns-dotnet`, `domain-events-dotnet`, `orleans-patterns`, `microbenchmarking` | Projection flow, events, actor coordination, performance proof |
| `orleans-patterns` | `orleans-virtual-actors`, `message-patterns-dotnet`, `configuring-opentelemetry-dotnet`, `kubernetes-dotnet` | Actor model, messaging, traces, clustered hosting |
| `redis-patterns-dotnet` | `caching-patterns-dotnet`, `message-patterns-dotnet`, `dotnet-resilience-standards`, `kubernetes-dotnet` | Cache strategy, pub-sub, resilience, deployment |
| `wpf-mvvm-implementation` | `communitytoolkit-mvvm`, `dependency-injection-patterns`, `validation-mvvm`, `wpf-testing-patterns` | ViewModels, composition, validation, tests |
| `winui3-patterns` | `communitytoolkit-mvvm`, `windows-ai-apis`, `fluent-design-system`, `ui-accessibility-standards` | MVVM base, local AI, design, accessibility |
| `maui-patterns` | `communitytoolkit-mvvm`, `mvvm-testing-patterns`, `fluent-design-system`, `structured-logging-serilog` | Cross-platform MVVM, tests, UI consistency, diagnostics |
| `angular-modernization` | `angular-security-hardening`, `frontend-module-consolidation`, `openapi-typescript-client-generation`, `vue3-webapp-setup` | Cleanup, security, client generation, migration target |
| `vue3-webapp-setup` | `vue3-pinia-state-management`, `vue3-e2e-testing-playwright`, `vue3-msal-authentication`, `webapp-frontend-security` | State, tests, auth, browser security |
| `openapi-typescript-client-generation` | `dotnet-webapi`, `scalar-openapi-integration`, `api-versioning-aspnetcore`, `rfc-8259-compliance` | Contract source, docs, versioning, JSON stability |
| `fabric-lakehouses` | `fabric-medallion-architecture`, `fabric-onelake-patterns`, `fabric-data-factory-patterns`, `data-quality-reporting` | Storage layering, OneLake layout, ingestion, quality |
| `semantic-layer-fabric` | `semantic-modeling-standards`, `business-glossary-metadata`, `terminology-normalization`, `fabric-powerbi-integration` | Metric consistency, business language, BI delivery |
| `fabric-realtime-analytics` | `fabric-data-activator`, `fabric-kql-diagnostics`, `fabric-workspaces-governance`, `data-lineage` | Activation, diagnostics, governance, traceability |
| `data-lineage` | `data-contract-frontmatter`, `business-glossary-metadata`, `semantic-data-bundle`, `data-quality-reporting` | Contracts, meaning, semantic routing, quality trace |
| `copybook-to-dotnet` | `mainframe-copybook-parsing`, `copybook-data-conversion`, `csv-ingestion-diagnostics`, `legacy-trains-migration` | Parse, convert, validate, migrate |
| `public-sector-accounting` | `accounting-systems-bundle`, `wa-state-systems-bundle`, `audit-trail-compliance`, `financial-reporting-gaap` | Domain model, state systems, controls, reporting |
| `wa-state-fems` | `wa-state-saam`, `wa-state-afrs`, `cgi-advantage-integration`, `public-sector-accounting` | Policy, reporting, integration, domain rules |
| `configuring-opentelemetry-dotnet` | `application-insights-dotnet`, `health-checks-aspnetcore`, `dotnet-logging-standards`, `structured-logging-serilog` | Unified observability, readiness, logs, correlation |
| `azure-keyvault-dotnet` | `azure-app-configuration-dotnet`, `jwt-authentication-dotnet`, `cwe-data-protection-fixes`, `application-insights-dotnet` | Secret flow, token signing, data protection, telemetry |
| `roslyn-analyzer-authoring` | `roslyn-codefix-authoring`, `roslyn-source-generator-authoring`, `sidecar-roslyn-patterns`, `including-generated-files` | Analyzer ecosystem, fixes, generators, build inclusion |

## Pattern Combination Matrix

| Pattern Set | Skills | Outcome |
|---|---|---|
| Event Sourcing + CQRS + Domain Events + Messaging | `event-sourcing-patterns`, `cqrs-patterns-dotnet`, `domain-events-dotnet`, `messaging-patterns-bundle` | Append-only writes, projections, async integration |
| MVVM + Dependency Injection + Validation + Testing | `mvvm-patterns-dotnet`, `dependency-injection-patterns`, `validation-mvvm`, `mvvm-testing-patterns` | Testable desktop presentation stack |
| Web API + OpenTelemetry + Health Checks + Rate Limiting | `dotnet-webapi`, `configuring-opentelemetry-dotnet`, `health-checks-aspnetcore`, `rate-limiting-aspnetcore` | Observable and protected APIs |
| Azure Functions + App Configuration + Key Vault + App Insights | `azure-functions-dotnet`, `azure-app-configuration-dotnet`, `azure-keyvault-dotnet`, `application-insights-dotnet` | Cloud-native serverless baseline |
| GraphQL + API Gateway + Docker + Kubernetes | `graphql-dotnet`, `api-gateway-patterns`, `docker-dotnet`, `kubernetes-dotnet` | Flexible query surface with scalable hosting |
| Fabric Lakehouse + Medallion + Semantic Layer + Power BI | `fabric-lakehouses`, `fabric-medallion-architecture`, `semantic-layer-fabric`, `fabric-powerbi-integration` | Analytics pipeline to reporting |
| Angular Modernization + Vue Migration + OpenAPI Client + E2E | `angular-modernization`, `angular-to-vue3-migration`, `openapi-typescript-client-generation`, `vue3-e2e-testing-playwright` | Controlled SPA migration |
| Background Worker + Channels + Quartz + Structured Logging | `background-services-channels`, `quartz-scheduling-patterns`, `structured-logging-serilog`, `long-running-job-patterns` | Reliable scheduled and continuous jobs |
| WPF + Fluent UI + CommunityToolkit + Accessibility | `wpf-mvvm-implementation`, `wpf-fluent-ui`, `communitytoolkit-mvvm`, `ui-accessibility-standards` | Modern accessible desktop UX |
| Repository + EF Core + Specification + Result Types | `repository-unitofwork-efcore`, `efcore-dbcontext-design`, `specification-pattern-dotnet`, `result-types-dotnet` | Explicit and testable data access |
| Public Sector Accounting + WA State Systems + Audit Trail | `public-sector-accounting`, `wa-state-systems-bundle`, `audit-trail-compliance`, `financial-reporting-gaap` | Government finance compliance flow |
| Analyzer + Code Fix + Source Generator + Generated File Inclusion | `roslyn-analyzer-authoring`, `roslyn-codefix-authoring`, `roslyn-source-generator-authoring`, `including-generated-files` | Complete Roslyn toolchain |

## Problem to Skill Set Matrix

| Problem | Skill Set | Outcome |
|---|---|---|
| Need async communication | `background-services-channels`, `messaging-patterns-bundle`, `signalr-patterns`, `message-patterns-dotnet` | Worker, queue, and push options |
| Performance issues | `analyzing-dotnet-performance`, `microbenchmarking`, `optimizing-ef-core-queries`, `caching-patterns-dotnet` | Diagnosis, proof, and targeted fixes |
| Security hardening | `security-controller`, `webapi-authz-hardening`, `jwt-authentication-dotnet`, `azure-keyvault-dotnet` | Broad security baseline |
| API contract drift | `webapi-hardening-controller`, `rfc-fixes-bundle`, `api-versioning-aspnetcore`, `scalar-openapi-integration` | Stable contract surface |
| Streaming API work | `rfc-8091-compliance`, `rfc-9457-compliance`, `dotnet-webapi`, `configuring-opentelemetry-dotnet` | Framed stream plus safe errors |
| Slow builds | `build-perf-baseline`, `build-perf-diagnostics`, `incremental-build`, `build-parallelism` | Faster build feedback |
| MSBuild failures | `binlog-generation`, `binlog-failure-analysis`, `resolve-project-references`, `check-bin-obj-clash` | Root-cause isolation |
| Need new tests | `code-testing-agent`, `writing-mstest-tests`, `dotnet-unit-testing`, `run-tests` | New tests plus validation |
| Tests are weak | `test-fixes`, `assertion-quality`, `test-gap-analysis`, `test-smell-detection` | Stronger verification |
| Need role-based API auth | `aspnetcore-identity-setup`, `jwt-authentication-dotnet`, `webapi-authz-hardening`, `azure-ad-authentication-dotnet` | Identity and authorization path |
| Need distributed tracing | `configuring-opentelemetry-dotnet`, `application-insights-dotnet`, `structured-logging-serilog`, `dotnet-logging-standards` | Correlated telemetry |
| Need background scheduling | `quartz-scheduling-patterns`, `quartz-calendar-schedules`, `webjobs-authoring`, `long-running-job-patterns` | Timed job architecture |
| Need desktop MVVM foundation | `mvvm-toolkit-bundle`, `wpf-mvvm-implementation`, `communitytoolkit-mvvm`, `validation-mvvm` | Desktop presentation baseline |
| Need SPA modernization | `angular-modernization`, `frontend-module-consolidation`, `webapp-frontend-security`, `vue3-e2e-testing-playwright` | Cleaner and safer frontend |
| Need Angular to Vue migration | `angular-to-vue3-migration`, `angular-vue-migration-orchestrator`, `vue3-webapp-setup`, `openapi-typescript-client-generation` | Migration route and contract reuse |
| Need CQRS architecture | `cqrs-patterns-dotnet`, `domain-events-dotnet`, `event-sourcing-patterns`, `result-types-dotnet` | Command and event flow |
| Need actor model or grains | `orleans-patterns`, `orleans-virtual-actors`, `message-patterns-dotnet`, `kubernetes-dotnet` | Distributed actor runtime |
| Need cloud secret handling | `azure-keyvault-dotnet`, `azure-app-configuration-dotnet`, `cwe-data-protection-fixes`, `jwt-authentication-dotnet` | Secret storage and usage |
| Need Fabric ingestion | `fabric-data-factory-patterns`, `fabric-lakehouse-ingestion`, `fabric-lakehouses`, `data-quality-reporting` | Ingestion to validation |
| Need semantic analytics | `semantic-data-bundle`, `semantic-layer-fabric`, `business-glossary-metadata`, `semantic-modeling-standards` | Business-facing semantic layer |
| Need data lineage | `data-lineage`, `data-contract-frontmatter`, `data-window-validation`, `data-quality-reporting` | Traceable data movement |
| Need mainframe modernization | `mainframe-integration-bundle`, `copybook-to-dotnet`, `copybook-data-conversion`, `legacy-trains-migration` | Legacy extraction and migration |
| Need financial controls | `accounting-systems-bundle`, `audit-trail-compliance`, `reconciliation-patterns`, `closing-cycle-patterns` | Controlled finance workflow |
| Need WA State accounting support | `wa-state-systems-bundle`, `public-sector-accounting`, `wa-state-saam`, `cgi-advantage-integration` | State-specific finance guidance |
| Need Roslyn automation | `roslyn-analyzer-authoring`, `roslyn-codefix-authoring`, `roslyn-source-generator-authoring`, `sidecar-roslyn-patterns` | Analyzer and generator toolchain |
| Need documentation repair | `documentation-fixes`, `documentation-frontmatter`, `documentation-link-repair`, `documentation-governance` | Clean and linked markdown |

## Skill Relationship Graph

| Skill | Prerequisites | Complements | Alternatives |
|---|---|---|---|
| `skill-discovery-guide` | `technology-selection` | `references/cross-reference-matrix.md`, `references/inventory-by-category.md` | Direct specialist selection |
| `technology-selection` | Domain context | `critical-infrastructure-bundle`, `skill-discovery-guide` | Bundle-first routing |
| `dotnet-webapi` | `dependency-injection-patterns`, `dotnet-validation-standards` | `rfc-9457-compliance`, `webapi-input-validation`, `health-checks-aspnetcore` | `graphql-dotnet`, `minimal-api-file-upload` |
| `webapi-hardening-controller` | API scope | `rfc-fixes-bundle`, `security-controller`, `dotnet-webapi` | Direct Web API specialist skills |
| `rfc-fixes-bundle` | API or token contract scope | `dotnet-webapi`, `scalar-openapi-integration` | Direct RFC specialist skills |
| `security-controller` | Security review scope | `webapi-authz-hardening`, `jwt-authentication-dotnet`, `azure-keyvault-dotnet` | Direct CWE family skill |
| `graphql-dotnet` | API schema decision | `api-gateway-patterns`, `docker-dotnet`, `kubernetes-dotnet` | `dotnet-webapi` |
| `cqrs-patterns-dotnet` | Bounded command and query scope | `domain-events-dotnet`, `event-sourcing-patterns`, `messaging-patterns-bundle` | CRUD plus `repository-unitofwork-efcore` |
| `event-sourcing-patterns` | Append-only domain history need | `cqrs-patterns-dotnet`, `domain-events-dotnet`, `microbenchmarking` | CRUD persistence |
| `messaging-patterns-bundle` | Async boundary or queue need | `background-services-channels`, `configuring-opentelemetry-dotnet` | Direct broker specialist skill |
| `azure-functions-dotnet` | Azure hosting context | `azure-app-configuration-dotnet`, `azure-keyvault-dotnet`, `application-insights-dotnet` | `webjobs-authoring` |
| `application-insights-dotnet` | Telemetry need | `configuring-opentelemetry-dotnet`, `structured-logging-serilog` | `dotnet-logging-standards` |
| `fabric-integration-bundle` | Fabric platform scope | `semantic-data-bundle`, `data-lineage`, `data-quality-reporting` | Direct Fabric specialist skill |
| `semantic-data-bundle` | Semantic layer or ontology scope | `business-glossary-metadata`, `semantic-layer-fabric`, `terminology-normalization` | Direct semantic specialist skill |
| `mvvm-toolkit-bundle` | Desktop MVVM scope | `communitytoolkit-mvvm`, `validation-mvvm`, `mvvm-testing-patterns` | `prism-framework-patterns`, `wpf-mvvm-implementation` |
| `angular-modernization` | Existing Angular app | `angular-security-hardening`, `frontend-module-consolidation`, `openapi-typescript-client-generation` | `angular-to-vue3-migration` |
| `vue3-webapp-setup` | Vue 3 target app | `vue3-pinia-state-management`, `vue3-e2e-testing-playwright`, `webapp-frontend-security` | `angular-modernization`, `blazor-patterns` |
| `test-fixes` | Failing or weak tests | `run-tests`, `assertion-quality`, `test-gap-analysis` | `test-modernization-controller`, direct test analysis skill |
| `run-tests` | Existing test project | `code-testing-agent`, `test-fixes` | Build-only validation |
| `documentation-fixes` | Markdown scope | `documentation-frontmatter`, `documentation-link-repair`, `documentation-governance` | Direct documentation specialist skill |
