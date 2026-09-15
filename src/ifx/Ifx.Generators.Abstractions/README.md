# vc.Ifx.Generators.Abstractions

Passive source-generator declarations for .NET 10 and C# 14. This package has no
ASP.NET Core, Roslyn, provider, or service-domain dependencies.

## Endpoint declaration

```csharp
using vc.Ifx.Generators.Abstractions.Attributes;

internal static class HealthEndpoints
{
    [GenerateEndpoint("/health", "GET", Name = "ReadHealth", AllowAnonymous = true)]
    internal static string ReadHealth() => "healthy";
}
```

`GenerateEndpointAttribute` targets one static accessible method. Its required
constructor arguments are `route` and `httpMethod`, exposed as read-only `Route`
and `HttpMethod`. Optional `Name`, `Summary`, `Description`, and
`AuthorizationPolicy` default to null. `Tags` defaults to an empty array.
`RequireAuthorization`, `AllowAnonymous`, and `ExcludeFromDescription` default
to false. A policy implies authorization in the generator contract; false flags
do not override a consumer's group/fallback authorization policy.

Endpoint generation is not implemented in this workstream. The subsequent generator
must follow the [minimal API v1 contract](../../docs/roslyn/minimal-api-generator-contract.md)
for accepted static handlers, diagnostics, deterministic registration, DI/request/
cancellation binding, authorization and OpenAPI metadata. The consumer supplies
ASP.NET Core and invokes the future `MapGeneratedIfxEndpoints` registry explicitly.
The attributes themselves do not register routes or execute service code.

## Existing attributes

`GenerateEnumerationAttribute` retains both constructors `(string name)` and
`(string name, string defaultName)`. `ClassName` is read-only and `Name` remains
its read-only alias. Defaults are empty `Namespace` and `DefaultName`,
`EnumerationNamespace="vc.Ifx.Primitives"`, and `EnumerationTypeName="Enumeration"`.
The four option properties remain settable. Namespace/default-member fallback
belongs to the enumeration generator; these strings alone do not assert that
the requested base type exists in the consumer.

`GenerateInterceptorsAttribute` retains `(string activitySourceName, params Type[]
serviceTypes)`, its read-only `ActivitySourceName` and `ServiceTypes`, and the
`OpenTelemetryInterceptor` default suffix. `DecoratorSuffix` and
`InterceptorSuffix` remain bidirectional aliases. Omitted params yield an empty
array. Supplied arrays retain caller ownership and are not cloned.

## Mapper generation

`GenerateMapperAttribute` declares a class as the source of a generated,
reflection-free property mapper: `[GenerateMapper(typeof(TargetType))]`.
It is repeatable (a class can map to more than one target) and carries an
optional `Bidirectional` flag that also generates the reverse mapper.

`MapFromAttribute` goes on a target property to redirect it to a
differently-named source member: `[MapFrom("SourcePropertyName")]`.

`MapIgnoreAttribute` goes on a target property to exclude it from the
match requirement; an ignored, unmatched property is left at its default
value instead of failing generation.

The mapper generator itself owns matching, diagnostics (`GEN001`), and
code emission; these attributes only declare intent and are not consumed
at runtime.

All attributes are public and sealed. `GenerateEndpointAttribute`, `GenerateEnumerationAttribute`,
and `GenerateInterceptorsAttribute` are non-repeatable; `GenerateMapperAttribute` is repeatable
so one class can declare mappers to more than one target. All are non-inherited. Targets are
method, enum, class, class (repeatable), property, and property respectively for
`GenerateEndpointAttribute`, `GenerateEnumerationAttribute`, `GenerateInterceptorsAttribute`,
`GenerateMapperAttribute`, `MapFromAttribute`, and `MapIgnoreAttribute`. Equality follows
`System.Attribute` with no custom identity or hash contract. Mutable attributes should not be
used as keys.

## Invalid metadata and compatibility

Constructors/setters retain input verbatim, including null passed despite nullable
annotations, empty values, invalid identifiers, unknown methods, conflicting
authorization flags and caller-owned arrays. They do not throw validation errors,
normalize strings, inspect types, or perform I/O. This preserves existing runtime
behavior and lets generators diagnose source declarations without instantiating
attributes. Non-null declarations document intended usage, not a runtime guard.
The endpoint generator must diagnose invalid metadata according to the v1 contract;
legacy generator validation/fallback behavior is unchanged by this package.

## Verification

Tests cover constructor shapes, defaults, setters, aliases, array ownership,
invalid/null metadata, attribute usage, inherited equality, dependency boundaries,
and compiled XML/README-style declarations. Run through the shared mutex:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Generators.Abstractions -TestSourceScope Generators/Abstractions -TestPackage vc.Ifx.Generators.Abstractions -Filter FullyQualifiedName~Generators.Abstractions
```

This is package-scoped evidence, not generator execution or whole-suite coverage.

Verified 2026-09-09: 31/31 tests passed, 43/43 executable lines covered (100%),
0/0 branch points; strict coverage gate passed. Report:
`test-results/coverage/vc.Ifx.Generators.Abstractions/10971deb59764e1bb0c3b3cf0efa21a4/summary.json`.
The mutex-protected package build passed with zero warnings/errors. An initial
test restore observed NU1903 from the centralized test project's SQLite dependency;
the final run did not reproduce it. This package introduces no such dependency.
Final package refresh and global build/test/coverage gates belong to the orchestrator.
