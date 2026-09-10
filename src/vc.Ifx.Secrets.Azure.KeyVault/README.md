# vc.Ifx.Secrets.Azure.KeyVault

Azure Key Vault retrieval for .NET 10 and stable C# 14, using the Azure SDK's
`SecretClient` directly. Its virtual methods provide the unit-test seam; no
additional client wrapper or live vault is required for testing.

## Registration

```csharp
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;

services.AddAzureKeyVaultSecrets(configuration, options =>
{
    options.VaultUri = new Uri("https://example.vault.azure.net/");
    options.CacheTtl = TimeSpan.FromMinutes(5);
    options.MaxRetries = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
});
```

Registration binds the `KeyVault` configuration section, applies the optional
callback, validates the selected mode, and snapshots the options. Later mutations
to callback-captured or resolved options do not silently change the registered
provider. Existing public constructors, registration methods, and passive options
types remain available. `KeyVaultOptions` is still defined in Secrets.Abstractions;
no reverse provider dependency is introduced.

Remote mode requires an absolute HTTPS vault URI with no user information, path
beyond `/`, query, or fragment. It uses `DefaultAzureCredential` with interactive
browser authentication disabled unless the application registers a
`TokenCredential` first. Pre-registered `SecretClient`, `SecretClientOptions`, and
`TimeProvider` instances are also preserved. The injected SDK client owns its
endpoint, authentication, transport, and retry configuration. A caller injecting
a client directly may omit `VaultUri`; an explicitly supplied URI is validated.

Logging is optional: a registered `ILogger<KeyVaultSecretProvider>` is used when
available, otherwise a null logger is used. The provider never logs values,
secret names, SDK exception objects, or response payloads. Cache and clock
dependencies are caller-owned. The provider does not dispose the injected client
or cache.

## Explicit Local Mode

```csharp
services.AddAzureKeyVaultSecrets(configuration, options =>
{
    options.UseLocalSecrets = true;
    options.LocalSecretsPrefix = "Services:Secrets";
});
```

Only `UseLocalSecrets=true` selects the configuration-backed local provider.
Missing remote configuration is an error, and remote 403/404/outage responses
never trigger local lookup. Local mode validates its prefix and uses the supplied
configuration instance without needing a separate `IConfiguration` registration.
Remote-only settings are not applied in local mode. Configuration value reloads
remain visible to LocalSecretProvider.

`AddNullSecrets()` explicitly selects the existing no-op singleton instead.
The legacy `SecretOptions` record remains passive and is not implicitly mapped
to `KeyVaultOptions` or consumed by registration.

## Retrieval And Versions

`ISecretProvider.GetAsync(name, token)` retrieves the current version. The concrete
provider adds `GetAsync(name, version, token)` for explicit version selection.
All three arguments are required for that overload, preserving existing calls
such as `GetAsync(name, default)`. Null selects the current version; a non-null
version must be 32 hexadecimal characters. Names must contain 1-127 ASCII letters,
digits, or hyphens. Names and versions are forwarded without rewriting.

- A Key Vault 404 returns null and is not cached.
- Empty and whitespace secret values are valid, returned unchanged, and cacheable.
- All non-404 SDK failures propagate unchanged, including authentication failures,
  throttling after SDK retries, network errors, and cancellation.
- Disabled, expired, not-yet-valid, and malformed secret objects raise
  `InvalidOperationException`; they are neither cached nor classified as missing.

Batch lookup is sequential, enumerates once, uses ordinal dictionary keys, keeps
the last duplicate value, and retains null entries. Failures stop the batch
without returning partial results. Empty batches perform no SDK operations.
This aligns the provider with the default abstraction batch contract and avoids
unbounded request fan-out.

## Cache And Concurrency

The legacy four-argument provider constructor uses `TimeProvider.System`. An
additional five-argument constructor accepts a `TimeProvider` for deterministic
expiry testing. Each provider instance has its own cache namespace; keys also
include the exact secret name and requested version. Shared memory caches cannot
leak values between provider instances or vault clients.

TTL is validated between zero and one day, inclusive. Zero disables completed
value caching. Cache entries expire at the earlier of TTL or the secret's expiry.
Logical expiry uses the injected clock and is checked on reads; MemoryCache also
receives a matching relative expiry for cleanup. Entries have size one for
compatibility with size-limited application caches. Negative results and failures
are never cached. Rotation or disabling a secret can remain unobserved until the
cached value expires; use zero or a shorter TTL when that delay is unacceptable.

Concurrent non-cancelable misses for the same exact key share one in-flight SDK
operation, and completed/failed operations are removed. Different keys proceed
independently. Cancelable callers use independent requests so their tokens reach
the SDK without canceling another caller's work. Cancellation is checked before
validation/cache access and after SDK completion, including before translating a
404 to null. The provider does not start detached background work.

## Retry Ownership

The Azure SDK is the only retry owner. Registration accepts 0-10 retries and a
0-30 second initial delay, uses exponential retry, and sets SDK maximum delay
and per-network-operation timeout to 30 seconds. There is no Polly policy or
provider retry loop. An injected client retains the application's SDK policy.
These are per-attempt settings, not an overall deadline: SDK handling of
`Retry-After` can override its maximum delay. Supply a cancellation token when an
overall operation deadline is required. See the
[Azure SDK retry options](https://learn.microsoft.com/en-us/dotnet/api/azure.core.retryoptions?view=azure-dotnet).

## Compatibility And Verification

Deliberate corrections to previous behavior are explicit local selection,
propagating non-404 failures, honoring cancellation, validating configuration and
secret validity, and deterministic duplicate handling in sequential batches.
The legacy API identities and passive options defaults are preserved.

Run the centralized wrapper to serialize shared assembly instrumentation:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 `
    -CoveragePackage vc.Ifx.Secrets.Azure.KeyVault `
    -Filter FullyQualifiedName~Secrets.AzureKeyVault
```

Tests under `Secrets/AzureKeyVault` use virtual SDK clients, a manual TimeProvider,
MemoryCache, in-memory configuration, and an in-memory HTTP handler for actual
SDK retry behavior. They require no Azure credentials, live vault, network,
timed sleeps, or secrets. On 2026-09-09 the enforced coverage command passed
75/75 tests with 172/172 lines and 78/78 branches covered (100% line, branch, and
method coverage). The package build passed with zero warnings and errors.
[Final local acceptance](../../docs/planning/local-verification-20260910.md)
subsequently passed the full solution, suite, strict package coverage, reporting,
and package validation. Hosted execution and publication remain external checks;
local tests do not establish live Key Vault deployment behavior.
