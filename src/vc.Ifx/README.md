# vc.Ifx

## Mission

Provide a compatibility facade for applications intentionally adopting the broad
framework runtime, while keeping each provider and policy in its own package.
This package is an aggregator with retained utility APIs, not the home for new
business-domain objects or provider implementations.

## Boundaries

The explicit project references aggregate service results, primitives, filtering,
database query specifications, pipelines, observation, proxy transports, WebApi,
storage, secrets, Tables and Queues. Analyzers, generators and reporting remain
separate tools. Consumers needing only a small capability should reference its
own package instead of this deliberately broad facade.

Filtering belongs to `vc.Ifx.Filtering`; database access specifications belong to
`vc.Ifx.Querying`. Provider implementations remain in their named packages. The
retained extension methods, pagination, logging, clocks and filesystem convenience
service preserve existing application compatibility.

## Registration And Ownership

Storage registration uses transient providers, owned and disposed by the service
provider. Named registrations have their own options and do not change unnamed
registrations. Repeated registration of the same name keeps the first registration.
Options are configuration inputs; do not mutate them after registration. Hosts
provide logging and external credentials. Registration does not perform network
operations or create remote resources.

`StorageRegistrationBuilder` records named provider configuration and registers
concrete provider types. Supplied options now also configure DI activation, not
just the passive descriptor. Resolve a named builder entry using its concrete
type and name (`GetRequiredKeyedService<FtpStorageProvider>(name)`, for example).
Repeated builder names replace that type/name registration and descriptor;
unkeyed concrete resolution retains the first registration. Unlike the builder,
the `AddNamed*Storage` extension methods retain the first registration per name.
Local builder entries retain their optional ambient `LocalStorageOptions` behavior.
Do not reuse a builder name across provider types; the descriptor identifies only
the most recently recorded type. No automatic factory is registered.

### Portable Object Composition

The existing Local/FTP/Blob extension methods register only legacy
`IStorageProvider`, even though those providers also implement
`IObjectStorageProvider`. This intentionally does not alter legacy filesystem
semantics or add a second service descriptor with ambiguous disposal ownership.
Resolve once at the application's composition boundary and pass the optional
portable interface to the consuming Access component:

```csharp
services.AddNamedLocalStorage("archive", new LocalStorageOptions { RootPath = root });
// The host also supplies standard Microsoft logging services.
IStorageProvider legacy = container.GetRequiredKeyedService<IStorageProvider>("archive");
IObjectStorageProvider objects = (IObjectStorageProvider)legacy;
```

Use the same pattern for the built-in FTP/Blob providers and unkeyed resolution.
Custom legacy implementations need not support the optional interface; validate
that capability during application composition. Do not dispose a DI-owned
provider independently or resolve the transient twice to create two aliases.
Callers own streams returned by reads and retain ownership of input write streams.
Queue/Table registrations use their separate SDK-specific contracts, not this
object-storage interface. Provider READMEs define supported capabilities and I/O.

## Utility Contracts

Collection mutation helpers require external synchronization. `Batch` yields
independent materialized batches, so later enumeration cannot consume a shared
enumerator. Dictionary factories run only for the branch actually taken.
Read-only dictionary wrappers remain live views; immutable conversions are snapshots.
The legacy collection `IsNullOrEmpty` also treats all-default contents as empty;
use LINQ or `HasAny` when only element count matters.

`Wa.Wsdot.Fin.Idl.Ifx.Errors` and `.Generics` are retained legacy public
namespaces for compatibility, not new application-domain contracts. Their mutable
collection classes require external synchronization despite implementing a read-only
enumeration interface. Offset paging checks arithmetic overflow before database
execution; hosts must supply stable ordering and transaction consistency when
required. Token paging delegates own token validation and store-specific ordering.
The synchronous secret-based connection registration is a legacy startup-only
bridge; prefer fetching secrets asynchronously before registering configuration.

Conversion helpers intentionally retain their historical, selective input types;
they are not a general `Convert.ChangeType` replacement. In particular, the
nonnullable `AsFloat`, `AsByte`, and `AsShort` do not accept same-type inputs,
while their nullable counterparts do. Nullable methods may accept fewer numeric
types than nonnullable ones. Parsing is invariant; floating parsing may preserve
IEEE infinity. Integral conversions truncate fractions only within the destination
range and return the supplied fallback/null on overflow or NaN. Numeric-to-decimal,
Unix-millisecond date and double-millisecond duration overflow now also returns
the documented fallback. Enum numeric inputs (int/byte/short) must fit the actual
underlying range and name a defined value; legacy string enum parsing still permits
undefined numeric values. `AsTypeOrNull` retains its class constraint, reference
identity and string conversion, including its legacy swallowed `ToString` failure;
unreachable value-type branches were removed without changing that signature.

`FrameworkInfoProvider.CompiledAt` is file creation time with the legacy local offset, not a reproducible
build timestamp. Single-file hosts and missing assembly files return
`DateTimeOffset.MinValue` to represent unavailable metadata. Storage enumeration
with a cancellation token now rejects prior cancellation even for an empty result.

Logging `Add*Interceptor` methods retain one registration per implementation;
standard logging and timing now coexist instead of silently losing the second
registration. Configured thresholds are captured at registration. `Use*` methods
retain their append behavior, including repeated registrations; they do not remove
existing interceptors. The null logging fallback remains inert. Hosts must register
Microsoft logging independently and choose their own logging/privacy policy.

`NameOfCallingClass` and `TypeOfCallingClass` are best-effort diagnostics of the
physical stack two frames above the inspection method, not logical async callers
or reliable security identities. Their inspection boundary is not inlined, but
callers needing a stable wrapper boundary must also prevent wrapper inlining.
Missing frames return `Unknown`/null; a method without a declaring type uses its
method name. The legacy name lookup skips frames from `mscorlib.dll`.

Target: .NET 10 and stable C# 14. See the repository parallel plan for current
verification status; package coverage and cross-package integration are separate
acceptance gates.

## Verification

Run the aggregator and its legacy tests through the serialized infrastructure
wrapper. `Storage` also selects provider compatibility tests, but coverage below
measures the entire `vc.Ifx` assembly only, with no aggregator source exclusions.
This source selection does not substitute for the unfiltered CI suite.

```powershell
$scope = @(
    'Aggregator', 'Extensions', 'Logging', 'Pagination', 'Providers', 'Storage',
    'ConstantsTests.cs', 'FrameworkConstantsTests.cs', 'CorrelationIdProviderTests.cs',
    'FrameworkInfoProviderTests.cs', 'IFrameworkInfoProviderTests.cs',
    'IRequestIdProviderTests.cs', 'OptionsTests.cs', 'RequestIdProviderTests.cs'
) -join ';'
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope $scope -TestPackage vc.Ifx -CoveragePackage vc.Ifx -Configuration Release -WarningsAsErrors
```

Release verification: 1,095 passed, no skips, 1,137/1,137 lines, 803/803 branches
and 100% methods. Evidence:
`TestResults/coverage/vc.Ifx/68d0069ce1df4c438fd3c10454642b48/summary.json`.
The same strict Debug run passed 1,095 tests, 1,556/1,556 lines and 813/813
branches, also 100% methods:
`TestResults/coverage/vc.Ifx/c4b337fa6436451c991bfde30870efac/summary.json`.
The real-stack tests also run with Release optimization; synthetic missing-frame
tests prove fallback handling, not claims about a logical async caller.
