# vc.Ifx.Secrets.Local

Live configuration-backed secret retrieval for .NET 10 and stable C# 14.
`LocalSecretProvider` implements `ISecretProvider` and depends only on
Secrets.Abstractions and Microsoft configuration. It performs no Key Vault calls.

## Usage

```csharp
using Microsoft.Extensions.Configuration;
using VisionaryCoder.Framework.Secrets;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;
using VisionaryCoder.Framework.Secrets.Local;

using var configuration = (ConfigurationRoot)new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Services:Secrets:Database:Password"] = "example-value"
    })
    .Build();

ISecretProvider secrets = new LocalSecretProvider(configuration,
    new KeyVaultOptions { LocalSecretsPrefix = "Services:Secrets" });

string? password = await secrets.GetAsync("Database:Password");
```

The existing public `(IConfiguration, KeyVaultOptions)` constructor is preserved.
`KeyVaultOptions` is a passive type in Secrets.Abstractions despite its historical
namespace. It introduces no Key Vault SDK or provider dependency. Only
`LocalSecretsPrefix` is consumed here; remote URI, retry, cache, and mode settings
do not affect an explicitly constructed local provider. No additional local
options type or dependency-injection package is required. The application's
composition root owns registration and the lifetime of its configuration.

## Lookup Semantics

Each request checks sources in this order, stopping at the first non-null value:

1. Configuration at `{LocalSecretsPrefix}:{name}`; the default prefix is `Secrets`.
2. Configuration at `{name}`.
3. The process environment variable with exactly `{name}`.

Empty and whitespace values are returned unchanged and stop fallback. Null
configuration values continue to the next source. An unavailable secret returns
null. Configuration failures propagate; they are not reported as missing values.

Names and prefixes support colon-separated hierarchical paths. Both must be
non-null and nonempty, with no null characters or empty/whitespace-only segments.
Leading/trailing colons and repeated colons are invalid. Nonempty segments are
not trimmed or case-normalized; key comparison belongs to the configuration
source. For example, `Database:Password` is forwarded as written.

Direct environment fallback does not translate `:` to `__`. Applications that
need environment-variable hierarchy normalization should configure that behavior
through their configuration sources. This preserves the existing exact-name
environment fallback and avoids silently selecting a different secret.

The constructor validates the initial prefix. Each call reads and validates the
current prefix, preserving mutable legacy options behavior. A single call uses
one captured prefix. Validation occurs at this provider boundary rather than in
the passive options setters.

## Cancellation And Reload

Cancellation is checked before validation or source access, and after each
synchronous source read. An already-canceled token raises
`OperationCanceledException` without reading configuration. Cancellation during
a source read prevents subsequent fallback or returning its value. A synchronous
configuration/environment read cannot be interrupted while it is executing.

Values are never cached. Changes exposed through the same configuration instance
are visible on the next retrieval, including updates after `IConfigurationRoot.Reload()`.
No reload subscriptions are retained, and this provider does not dispose the
caller's configuration. File watching and automatic refresh remain configuration
source responsibilities. Environment fallback also reads the current process
value on every request.

Batch calls use the default `ISecretProvider.GetMultipleAsync` implementation:
sequential reads, ordinal result keys, last duplicate value retained, and null
entries preserved. Retrieval failures or cancellation stop the batch without
returning partial results. Empty batches do not perform reads or cancellation
checks, as specified by the abstraction contract.

## Compatibility And Verification

Constructor identity, lookup order, valid values, and exact key spelling remain
compatible. Two deliberate corrections tighten previously permissive behavior:
malformed/whitespace-only paths are rejected, and cancellation tokens are now
honored. Existing tests that asserted ignored cancellation or whitespace-only
names have been updated to verify these rules.

Tests cover the legacy constructor, ordered in-memory lookup, hierarchical keys,
empty/missing values, environment fallback, option mutation, reload/removal,
failure propagation, and cancellation before and during reads. Environment tests
use unique names and clean them up after each run. Run through the shared wrapper
so builds and coverage instrumentation are serialized:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 `
    -CoveragePackage vc.Ifx.Secrets.Local -Filter FullyQualifiedName~Secrets.Local
```

The filter includes both `Secrets.Local` tests and the existing
`Secrets.LocalSecretProviderTests`. On 2026-09-09 the serialized coverage run
passed 54/54 tests with 39/39 lines and 10/10 branches covered (100% line, branch,
and method coverage). Final repository-wide verification and package refresh
are handed to Orchestrator and the metadata owner in
`docs/planning/framework-upgrade-parallel-plan.md`.
