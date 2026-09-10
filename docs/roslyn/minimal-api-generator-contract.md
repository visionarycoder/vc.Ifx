---
title: Minimal API generator contract v1
doc_type: reference
status: active
last_updated: 2026-09-10
---
# Minimal API generator contract v1

Status: attribute contract frozen and endpoint generator implemented locally on
2026-09-09. Marker constructors, members and defaults remain unchanged. Local
verification does not close the global framework upgrade gates.

Attribute handoff: STABLE. The abstractions implementation passed 31 package-scoped
tests with 43/43 executable lines covered and no branch points, plus a zero-warning
package build in the preceding attribute-only stage. The sequential generator
stage adds endpoint generation, IFX2000-IFX2007 catalog entries, compiler-package
checks and ASP.NET in-memory hosting tests.

## Ownership and compatibility

`vc.Ifx.Generators.Abstractions` targets net10.0/C# 14 and contains passive
attributes with no package, ASP.NET Core, Roslyn, or service-domain dependencies.
`vc.Ifx.Generators` remains a netstandard2.0 compiler component. It reads Roslyn
attribute data by metadata name without loading the abstractions assembly. The
consumer supplies ASP.NET Core 10 and calls generated registration explicitly.
WebApi exception handling is optional consumer composition, never a generator
or marker-package dependency. No shared domain-object library is introduced.

The endpoint marker is the sealed public type
`vc.Ifx.Generators.Abstractions.Attributes.GenerateEndpointAttribute`, with
`AttributeUsage(Method, AllowMultiple=false, Inherited=false)`.

| Member | Type/default | Meaning |
| --- | --- | --- |
| constructor route | string, required | Read-only Route; explicit ASP.NET route template. |
| constructor httpMethod | string, required | Read-only HttpMethod; exactly one method token. |
| Name | string?, null | Optional globally unique endpoint/OpenAPI operation name. |
| Summary | string?, null | Optional OpenAPI summary metadata. |
| Description | string?, null | Optional OpenAPI description metadata. |
| Tags | string[], empty | Ordered OpenAPI tags. |
| RequireAuthorization | bool, false | Require the consumer's default authorization policy. |
| AuthorizationPolicy | string?, null | Require this named policy; also implies authorization. |
| AllowAnonymous | bool, false | Explicit anonymous metadata, including when mapped under an authorized group. |
| ExcludeFromDescription | bool, false | Exclude this endpoint from API description. |

Constructors and setters store supplied data without parsing, normalization,
validation exceptions, registration, reflection, or I/O. Null/invalid declarations
remain representable so the generator can report source diagnostics. Optional
null strings mean unspecified; supplied empty/whitespace strings are invalid.
Tags must be non-null with nonblank, non-null elements. Duplicate tags are emitted
once, preserving the first occurrence using ordinal comparison. The array is
caller-owned, not defensively copied; each default instance exposes an empty array.
Attribute identity/equality follows System.Attribute, not a new record contract.
Future optional metadata is additive; constructor order, names, existing members,
and existing defaults are compatibility commitments.

## Accepted handlers

- Discover only annotated ordinary method declarations in the current compilation,
  using the exact marker metadata name. Do not discover referenced assemblies,
  naming conventions, local functions, lambdas, or inherited methods by scanning.
- Require a static public or internal, non-generic method with a body. Every
  containing type must be public/internal, non-generic, and accessible from the
  generated namespace. Nested accessible types are supported; file-local/private/
  protected containers, interfaces, explicit implementations and partial methods
  are rejected in v1. The containing type need not itself be static.
- Reject overloaded handler names in their declaring type in v1, to ensure a
  natural method-group delegate identifies exactly one original MethodInfo.
  Reject extension methods, ref returns, ref/in/out parameters, pointers,
  function pointers, ref-like types, open/error types, and more than 16 parameters.
- Accept ordinary value/DTO, string, IResult and typed-result returns, including
  Task<T>/ValueTask<T> forms. Reject void, non-generic Task/ValueTask, async void,
  custom awaitables and nested task returns in v1; use an explicit empty IResult
  when no response body is intended. This is a bounded v1 policy, not a claim
  that ASP.NET Core cannot handle every rejected shape.
- Pass the original method group to MapMethods as a Delegate. Do not generate
  lambdas that strip method/parameter attributes or optional-parameter metadata.
  Escape identifiers and string literals with Roslyn APIs, including nested types.

## Routing, binding, and response ownership

Require a nonblank route beginning with `/` and no leading/trailing whitespace.
Preserve its exact text for registration. Route constraints, optional segments,
catch-alls, defaults and escaping remain ASP.NET route-template semantics. The
generator uses the netstandard-compatible ASP.NET Routing 2.3.12 template parser
for syntax validation. A narrow adapter maps unescaped double catch-alls to single
catch-alls for inbound-match validation and rejects three leading stars. Escaped
literal braces and emitted route text remain unchanged. The parser and dependency
closure ship privately beside the compiler extension. The real packaged-compiler
fixture exercises this compatibility boundary, separately from tests under ASP.NET
Core 10. ASP.NET remains the final authority at registration and request matching.
Relative group prefixes are supplied by calling the registry on
an IEndpointRouteBuilder such as a RouteGroupBuilder.

Require one of GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS, TRACE or CONNECT,
case-insensitively, then emit its uppercase invariant spelling. Reject commas,
spaces, wildcard/missing methods and custom methods in v1. GET does not implicitly
declare HEAD in this contract: an additional annotated method is explicit.

Binding comes from ASP.NET Core and the original handler metadata: route/query/
header/body, [FromServices], [AsParameters], custom binding, HttpContext and
CancellationToken. Prefer explicit [FromServices] for dependencies. Do not resolve
services while registering, allocate scopes, deserialize bodies, or synthesize
cancellation tokens. ASP.NET owns request cancellation and binding errors. DI,
JSON options, validation and request-body inference remain consumer configuration;
the generator does not claim to prove service registration or JSON serialization.
ASP.NET owns response execution and serialization; use typed results for explicit
response status/body metadata. Known impossible payload shapes (pointer/ref-like/
delegate values directly used as body/response payloads) receive an error, but
arbitrary DTO serialization and custom converters require consumer tests.

Gate 3 scope acceptance (2026-09-10): this is the accepted v1 interpretation of
the plan's non-serializable-contract diagnostic item. IFX2001 rejects declared
unsupported signature shapes, recursively through arrays/generic type arguments;
IFX2006 rejects declared delegates and delegate arrays on non-service parameters
or the unwrapped response. It does not recursively inspect DTO members or generic
collection payloads such as List<Action>. Those shapes, custom converters and
serializer configuration require consumer tests, not universal static proof.
Arrays of otherwise supported types are not categorically rejected. This bounded
acceptance adds no diagnostic, marker member, or serialization API.

Generated registration does not catch handler exceptions, wrap IResult values,
force status codes, retry handlers, or invent ProblemDetails. Consumers can compose
vc.Ifx.WebApi's exception middleware/catalog without a hard dependency cycle;
standard ASP.NET binding failures are not promised to flow through that middleware.
Request cancellation is not reclassified as an internal server failure.

## Authorization and OpenAPI

Emit RequireAuthorization() only for RequireAuthorization=true without a policy;
emit RequireAuthorization(policy) when a policy is supplied. Emit AllowAnonymous()
only when explicitly requested. Reject AllowAnonymous combined with either
authorization setting, including a method-level AuthorizeAttribute. Reject marker
authorization combined with a method-level AllowAnonymousAttribute. Existing
method-level authorization metadata remains attached through the original delegate;
class-level metadata is not promised to flow to minimal endpoints. Parent-group
authorization and application fallback policies still apply; the default false
flags do not make an endpoint anonymous. Explicit AllowAnonymous can override a
group's policy and must be deliberate in the application.

Apply metadata in this fixed order: WithName, WithSummary, WithDescription,
WithTags, authorization, ExcludeFromDescription. Omit unspecified metadata calls.
Name=null creates no invented operation ID. Apply names case-sensitively for
uniqueness using ordinal comparison; consumers must also avoid conflicts with
manual endpoints and other registries. Use standard .NET 10 metadata extensions;
do not emit deprecated WithOpenApi. Consumers own AddOpenApi/MapOpenApi and security
scheme configuration. Typed-result metadata and ordinary handler attributes supply
response descriptions; no custom response-DTO or HTTP catalog attributes are added.

## Deterministic registry

Emit one UTF-8 file named `IfxEndpointRouteBuilderExtensions.g.cs` with a generated
code marker, nullable enabled, fully qualified references and no timestamps,
absolute paths, environment values, assembly scanning, service lookup or reflection
for endpoint discovery. Use namespace `vc.Ifx.Generated`, internal static class
`IfxEndpointRouteBuilderExtensions`, and public extension method:

```csharp
IEndpointRouteBuilder MapGeneratedIfxEndpoints(this IEndpointRouteBuilder endpoints)
```

The method checks endpoints for null, directly registers every accepted handler,
and returns the same builder. Emit an empty registry for zero candidates when
the consumer exposes the necessary ASP.NET symbols. With zero candidates and no
ASP.NET reference, emit nothing and no missing-host diagnostic. Registration is
explicit and not idempotent: call once per builder. Different assemblies' internal
registries require their own explicit application-facing composition wrappers.
No process-wide cache or mutable static registration state is generated.

Sort registrations by canonical method, route using ordinal comparison, then
fully qualified containing type and method. Normalize only HTTP method spelling
and string-escaping in emitted code; route text remains unchanged. Incremental
inputs retain immutable registration text, identity and diagnostics. Final source
uses ordinal string equality, including all emitted metadata and ordered tags;
unchanged bytes are cached even when semantic validation repeats. Diagnostics
retain current syntax locations independently of source caching. Sorting is ordinal
and source contains no environment-derived text. Honor compiler
cancellation during discovery, validation, grouping and emission.

## Duplicate policy and diagnostics

Within one compilation, duplicate identity is uppercase method plus a parsed route
key compared ordinal-ignore-case. The key removes trailing slashes and parameter
names, retains inline constraints and optional/catch-all markers, and treats single
and double catch-alls as the same inbound match. Literals are length-delimited;
defaults do not make identical match shapes distinct. Report every participant and
emit no registration for that conflict group. Distinct HTTP methods may share a
route. Overlapping different constraints, optional-segment overlaps across different
shapes, group prefixes, manual endpoints and other assemblies are not proven
equivalent by this bounded check. ASP.NET integration tests remain the authority
for those ambiguities. Duplicate explicit Name values similarly invalidate every
participant, even when their routes differ. Never silently keep a winner.

The following implemented IDs use category Generation, default severity Error,
enabled by default, with no code fix. Owned entries are in the shared diagnostic
catalog, with release tracking in the generator project.

| ID | Title | Source location / trigger |
| --- | --- | --- |
| IFX2000 | Invalid endpoint declaration | Attribute argument: missing/blank/invalid route template or metadata, invalid method, null/invalid tags or incompatible marker metadata types. |
| IFX2001 | Unsupported endpoint handler | Method identifier: inaccessible, nonstatic, generic, overloaded or other unsupported signature. |
| IFX2002 | Duplicate endpoint route | Attribute on every duplicate method/route participant; other participants are additional locations. |
| IFX2003 | Duplicate endpoint name | Name argument on every duplicate explicit name participant. |
| IFX2004 | Conflicting endpoint authorization | Endpoint attribute when marker settings conflict with one another or method authorization metadata. |
| IFX2005 | Missing endpoint hosting reference | Endpoint attribute when required ASP.NET types/extensions are unavailable. |
| IFX2006 | Unsupported endpoint payload | Offending parameter's symbol location or method return-type syntax for declared delegate/delegate-array payloads; service-bound parameters are exempt. Other unsupported signature shapes use IFX2001. |
| IFX2007 | Generated registry name collision | Endpoint attribute when the generated registry name conflicts with a user declaration. |

Help links use this document's `#duplicate-policy-and-diagnostics` anchor. Diagnostic
messages identify the validation problem; locations prefer arguments, method
identifiers and payload parameters/returns. Do not duplicate compiler diagnostics
for malformed attribute syntax or duplicate non-repeatable attributes. An invalid
handler is omitted while independent valid handlers may still be emitted; errors
fail compilation. Missing required hosting symbols or a registry collision prevent
the entire registry from being emitted. No report files are written by the generator.

## Example and implementation acceptance

Consumer shape:

```csharp
using vc.Ifx.Generators.Abstractions.Attributes;
using vc.Ifx.Generated;

internal static class HealthEndpoints
{
    [GenerateEndpoint("/health", "GET", Name = "ReadHealth",
        Summary = "Read service health", Tags = new[] { "Health" }, AllowAnonymous = true)]
    internal static string Read(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return "healthy";
    }
}

// In application startup, after normal service and middleware configuration:
// app.MapGeneratedIfxEndpoints();
```

Required generator tests: every diagnostic and no-diagnostic case; all supported
method/return/binding shapes; overload/nested accessibility; null/empty fields;
literal escaping and Unicode; exact duplicate and documented overlap boundaries;
authorization conflicts and parent-group behavior; stable ordering, incremental
changes and cancellation; zero-candidate/missing-host cases; name collisions;
generated source compilation; package-based loading; and in-memory ASP.NET tests
for routes, DI, body/query binding, cancellation, typed results, OpenAPI metadata,
authorization, and optional WebApi exception handling. Attribute tests alone do
not establish these generator/runtime guarantees or Native AOT compatibility.

## Local verification and handoff

Verified on 2026-09-09 using the existing repository mutex runner:

```powershell
pwsh -NoProfile -File src/vc.Ifx.Generators/verification/Test-WebApiIntegration.ps1
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.Generators/verification/PackageSmoke.proj
```

- Strict coverage gate: 134 passed, zero failed/skipped; 1,437/1,437 executable
  lines and 516/516 branches (100% each), measured across the entire generator DLL.
  Artifacts: `TestResults/coverage/vc.Ifx.Generators/8c69b8bfb566437b8adb8a449ea31009`.
- Three ASP.NET in-memory host tests verify route constraints, route/query/header/
  body binding, DI, typed results, default/named authorization, authorized groups
  with explicit anonymous override, OpenAPI endpoint metadata, exception pass-through
  and request cancellation. Source fixtures compile the frozen marker source files;
  package-based marker loading is proved separately below. Six additional integration
  cases use real AddIfxWebApi/middleware/catalog with generated endpoints: catalog-backed
  400/404/500 mappings, application-mapped 422, safe details/type/title/traceId,
  query-free instance paths, HEAD body suppression, explicit catalog results and
  request cancellation. A local targets wrapper adds WebApi only to this scoped test
  build after importing the unchanged shared targets; the generator itself acquires
  no WebApi runtime dependency.
- One test deliberately mocks an unresolved Roslyn attribute class to exercise the
  defensive metadata boundary. Its coverage is not evidence that a compiler emitted
  or accepted that metadata shape. Compiler-invalid declarations are tested without
  claiming successful consumer compilation.
- Release package verification: zero warnings/errors, no NU5104 suppression.
  Artifacts: `TestResults/generator-package/7307d7d14af848a6bb93da33263037b5`.
  The verifier checks extracted compiler-only contents and private dependency
  placement, then invokes the installed C# compiler using the actual packed marker
  DLL, analyzer DLLs and .NET 10 reference assemblies. Positive fixtures include
  double catch-all, optional constrained, and escaped-literal templates.
- Existing generator regressions cover unmanaged constraints, null interceptor
  service arrays, a hidden base property, namespace-qualified hint names, keyword
  enum fields, overflow (`GEN002`), and duplicate partial Crosswalk declarations.
  `GEN002` is documented in the generator README/release file and now in the shared
  catalog's legacy generator inventory. Its existing prefix is a compatibility
  exception, not a new ID allocation or renumbering.

Reconciled 2026-09-10: `Microsoft.AspNetCore.Routing` 2.3.12 is centrally managed;
the generator's private PackageReference has no temporary `VersionOverride`.
TestHost 10.0.0 is supplied by infrastructure. Keep global plan gates In-flight.
The generator has no runtime reference to the net10 abstractions package. The
WebApi-specific integration is verified; final combined release acceptance remains
with Orchestrator. Arbitrary serializer behavior, overlaps beyond the stated route
key, other compiler/IDE hosts, Native AOT and broader legacy unsupported shapes
remain unproven. Passing measured coverage does not certify these behaviors.

## Platform references

- [ASP.NET Core parameter binding](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-10.0)
- [Minimal API responses](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0)
- [WithOpenApi deprecation](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/withopenapi-deprecated?view=aspnetcore-10.0)
