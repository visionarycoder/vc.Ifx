# vc.Ifx.Proxy.AspNetCore

ASP.NET Core identity and outbound authentication adapters for vc.Ifx proxies.

## Contracts And Trust

Existing registration methods, provider interfaces, constructors and protected
extension hooks remain available. Proxy and Secrets.Abstractions contracts and
dependencies are unchanged. Register the adapters per request; they do not own
authentication schemes, tenant directories, authorization policy or retry policy.
The host must authenticate inbound requests and validate issuer, audience,
signature and lifetime before populating `HttpContext.User`.
ASP.NET Core retains model binding ownership. Do not bind user or tenant authority
from request DTOs; resolve the context providers instead. No custom model binder
or additional public authentication scheme is introduced.

The default user provider accepts exactly one authenticated identity, ignores
unauthenticated identities, and rejects conflicting subject/tenant claims.
Roles and permissions are case-sensitive. Roles do not implicitly grant
permissions; supply host policy through the existing override hook. User lookup
and validation are restricted to the current authenticated subject, not a
fictional directory. Anonymous contexts retain the `anonymous` identifier but
are never validated as authenticated users.

Tenant authority comes only from authenticated `tenant_id`, `tid` or `tenantid`
claims. IDs are 1-128 ASCII letters, digits, hyphens or underscores. Headers,
subdomains and `/tenant/{id}` or `/t/{id}` paths are hints only: the protected
extraction hooks return a tenant only when it matches the authenticated claim.
The default extraction uses claims, not arbitrary route/host values. Cross-tenant
switches and public `HttpContext.Items` overrides are rejected. Replace the tenant
provider for host-authorized multi-tenant switching. The fallback `default`
context is inactive, and tenant ID lookup returns null without authority.
`IsTenantValid` checks syntax only, not membership or subscription status.

Request values are copied before asynchronous enrichment. No request context is
cached, disposed, or used after the enrichment await. Explicit cancellation and
`RequestAborted` propagate; failures are not silently turned into successful
anonymous operations. Client IP uses `Connection.RemoteIpAddress`: configure
trusted forwarded-header middleware in the host, never trust raw forwarded headers.
Optional metadata headers must have one bounded, control-free value.

Outbound bearer adapters reject missing or malformed credentials, replace stale
values for their configured credential header, propagate cancellation and downstream exceptions unchanged,
and never classify authentication failures as transient. Web JWT validation is
delegated to `ITokenProvider`; legacy WebJwtOptions issuer/signing fields do not
configure inbound validation. No retry is introduced by this package.

## Registration And Migration

Registration supplies logging, HTTP context access and HTTP client infrastructure
where required. Ordered interceptors and security enrichers are registered once
per implementation type. Request-dependent providers and secret-backed bearer
interceptors are scoped. Key Vault still requires a host `ISecretProvider`.
Registration does not build a nested service provider.

The aggregate interceptor registration selects the core resilience interceptor
as its sole retry owner. `AddRetryInterceptor` remains available for explicitly
composed pipelines; do not combine it with another retrying policy. Bearer factory
registrations are idempotent and first-registration-wins. The legacy no-token
enricher delegate is checked for cancellation before and after it completes.
Typed JWT registration now requires a valid `JwtOptions` subclass instead of
silently ignoring the supplied type. Setup validation rejects singleton identity
providers; normal DI scope validation remains the host's responsibility.
Core resilience configuration remains owned by the injected Polly pipeline;
`ProxyOptions` does not reconfigure a separately supplied Polly pipeline. Ordered
interceptors are forwarded to both public interceptor interfaces so the core
`DefaultProxyPipeline` executes them. Canonical `IProxyAuthorizationPolicy`
registrations compose with the core's legacy policy bridge; the adapter does not
wrap legacy policies a second time. Authorization remains explicit: a pipeline
without policies is not an authorization boundary.

Hosts relying on unauthenticated tenant headers, arbitrary tenant lookup, implicit
Admin permissions, anonymous validation, swallowed cancellation, or missing-token
requests must migrate to explicit authentication/authorization policy. Existing
aggregator registration calls remain source compatible. Invoke the HTTP context
providers only inside a request; background jobs must carry an explicit snapshot.

References: [ASP.NET Core HttpContext guidance](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/use-http-context),
[trusted forwarded headers](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer),
and [RFC 6750 bearer authorization](https://www.rfc-editor.org/rfc/rfc6750#section-2.1).

See the [framework upgrade plan](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/planning/framework-upgrade-parallel-plan.md) for implementation and verification status.

## Verification

The serialized scoped command below passes 25 tests with strict 561/561 line and
190/190 branch coverage (100% each), with warnings treated as errors. Evidence:
`TestResults/coverage/vc.Ifx.Proxy.AspNetCore/2ac7ef6f49a64ba7b413a49fb68c5739/summary.json`.
This is package-scoped evidence, not a claim that global integration gates passed.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Proxy.AspNetCore -TestSourceScope ProxyAspNetCore -TestPackage vc.Ifx.Proxy.AspNetCore -Filter FullyQualifiedName~Tests.ProxyAspNetCore -WarningsAsErrors
```
