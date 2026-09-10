# vc.Ifx.Generators

`vc.Ifx.Generators` contains source generators that turn explicit framework annotations into compile-time artifacts. The package keeps generated code deterministic and discoverable while avoiding runtime reflection for repetitive framework plumbing.

The implementation is organized by generator under `Generators/`. Public marker attributes live in `vc.Ifx.Generators.Abstractions` so application projects can reference the annotations without taking a dependency on the generator implementation.

## Minimal endpoints

Reference `vc.Ifx.Generators.Abstractions` normally and `vc.Ifx.Generators` with `PrivateAssets="all"`. The compiler package targets netstandard2.0/C# 14 and reads attribute metadata without loading the net10 marker assembly. In an ASP.NET Core 10 application:

```csharp
using vc.Ifx.Generated;
using vc.Ifx.Generators.Abstractions.Attributes;

var app = WebApplication.CreateBuilder(args).Build();
app.MapGeneratedIfxEndpoints();
app.Run();

internal static class HealthEndpoints
{
    [GenerateEndpoint("/health", "GET", Name = "ReadHealth",
        Summary = "Read service health", Tags = new[] { "Health" })]
    internal static string Read(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return "healthy";
    }
}
```

Handlers must be accessible static, non-generic, non-overloaded methods. Registration passes the original delegate, preserving method and parameter metadata. ASP.NET owns binding, DI, cancellation, responses and exception handling. Configure authorization and OpenAPI services normally; the generator does not invent policies, serializers or middleware.

The internal registry is deterministic and called explicitly once per builder, including route groups. Discovery uses no runtime reflection or assembly scanning. Invalid declarations and every duplicate-group participant are omitted with error diagnostics, never silently selected.

The [v1 contract](../../docs/roslyn/minimal-api-generator-contract.md) defines supported shapes and IFX2000-IFX2007 diagnostics. The compiler-only Routing 2.3.12 parser and dependency closure ship privately beside the generator, with a validation adapter for modern double catch-alls. Emitted route text remains unchanged. Host Roslyn binaries and runtime `lib` assets are not shipped in the compiler package.

## Existing generators

- Enumeration generation preserves both historical marker namespaces and defaults. Values must fit the Int32 base contract; `GEN002` reports an out-of-range member instead of crashing. Keyword fields are escaped and hint names include namespaces.
- Interceptors preserve attribute aliases and defaults. Regression tests cover null service arrays, unmanaged constraints, namespace-qualified hint names and a derived method hiding a base property, alongside ordinary interface members and Task/ValueTask returns.
- Crosswalk mapping de-duplicates partial declarations and orders discovered types. Existing `GEN001` errors remain unchanged. Arbitrary nested/generic Crosswalks, constructor/accessor accessibility, every interface member-hiding combination and unsupported interceptor signatures are not certified by these tests.

## Verification

```powershell
pwsh -NoProfile -File src/vc.Ifx.Generators/verification/Test-WebApiIntegration.ps1
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.Generators/verification/PackageSmoke.proj
```

The strict scoped run passed 134 tests with 1,437/1,437 executable lines and 516/516 branches across the entire generator DLL. In-memory hosts cover binding, DI, typed results, authorization, group overrides, metadata and cancellation. Generated endpoints also exercise the actual vc.Ifx.WebApi middleware and response catalog: safe default/custom ProblemDetails mappings, HEAD body suppression, direct catalog results and cancellation. The verification wrapper adds WebApi only to the scoped test reference set; it does not change generator dependencies or shared targets. One explicitly labeled mocked metadata-reader test covers an unresolved Roslyn attribute class; it is not compiler-output evidence.

The package verifier packs local artifacts with warnings as errors, checks compiler-only contents, and invokes the installed C# compiler against extracted NuGet DLLs and .NET 10 reference assemblies. This is separate compiler-loading evidence, not a Native AOT or full-solution guarantee. Exact artifacts and remaining gates are recorded in the [contract handoff](../../docs/roslyn/minimal-api-generator-contract.md#local-verification-and-handoff).
