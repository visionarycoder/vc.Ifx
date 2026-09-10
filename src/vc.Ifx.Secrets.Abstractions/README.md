# vc.Ifx.Secrets.Abstractions

Provider-neutral secret retrieval for .NET 10 and stable C# 14. The package has
no project or package dependencies. Configuration access, credentials, caching,
retries, and option validation stay in implementation packages behind
`ISecretProvider`. One passive options type is retained for compatibility below.

## Contract

`VisionaryCoder.Framework.Secrets.ISecretProvider` retains its existing methods:

```csharp
Task<string?> GetAsync(string name, CancellationToken cancellationToken = default);
Task<IDictionary<string, string?>> GetMultipleAsync(
    IEnumerable<string> names, CancellationToken cancellationToken = default);
```

- `null` means unavailable, including missing secrets. A provider's documented
  fallback policy may also return null on failure; it is not proof of absence.
- Empty and whitespace values are valid values and are preserved.
- Names are opaque and forwarded unchanged. Providers own validation and naming
  rules, including hierarchical keys and case sensitivity of their backing store.
- There is no version parameter or version metadata. Providers resolve their
  current or configured version. An explicit version request needs a separately
  reviewed contract; it must not be silently encoded or normalized here.
- Provider exceptions may propagate. Callers must account for the selected
  provider's failure and cancellation policy.

The default batch method checks for a null collection, enumerates it once, and
awaits each retrieval sequentially. Its result uses ordinal, case-sensitive keys.
Duplicates are retrieved again, with the last value retained. Missing entries
remain present with null values. An empty input returns a fresh empty dictionary.
The caller's cancellation token passes unchanged to every retrieval; the batch
method does not add cancellation checks of its own. Enumeration and retrieval
failures propagate, and no partial dictionary is returned. Null elements cannot
be dictionary keys. Provider overrides can have separately documented batching
policies; these default implementation guarantees do not describe an override.

## Optional Secrets

```csharp
using VisionaryCoder.Framework.Secrets;

ISecretProvider secrets = NullSecretProvider.Instance;
string? value = await secrets.GetAsync("DatabasePassword");
IDictionary<string, string?> values = await secrets.GetMultipleAsync(
    ["DatabasePassword", "ApiKey"]);
```

`NullSecretProvider.Instance` is a stateless singleton returning a completed task
with null. Its single-secret method deliberately ignores invalid names and
already-canceled tokens to preserve existing optional-secret behavior. Its batch
method follows the default interface rules. Access that default method through
`ISecretProvider`. Applications register the instance at their composition root;
this package intentionally has no dependency-injection dependency or registration
extension.

## Compatibility Options

`VisionaryCoder.Framework.Secrets.Azure.KeyVault.KeyVaultOptions` remains in this
assembly with its existing namespace, public setters, and defaults:

| Property | Default |
| --- | --- |
| `VaultUri` | `null` |
| `CacheTtl` | 15 minutes |
| `UseLocalSecrets` | `false` |
| `LocalSecretsPrefix` | `"Secrets"` |
| `MaxRetries` | `3` |
| `RetryDelay` | 1 second |

This is a documented compatibility quirk. The class contains only passive BCL
values and references no SDK or configuration package. A namespace or folder
named after a provider does not by itself create runtime or domain coupling.
Setters retain their existing behavior, including accepting values a provider
may reject. Validation belongs at provider registration or construction, where
the applicable local or remote mode is known. Instances use reference equality.

Keep this type identity until the provider stage. Moving it to Key Vault and
adding a type forwarder here would introduce an outward assembly dependency
and conflict with Key Vault's existing dependency on Abstractions. If relocation
becomes necessary, review a dependency-neutral destination and a coordinated
compatibility migration first. Separate provider-specific options can later be
introduced through adapters or overloads while preserving existing callers.

## Verification

Package tests live in `tests/unit/vc.Ifx.UnitTests/Secrets/Abstractions`. They cover
batch results, duplicate and case-sensitive names, enumeration, failure and
cancellation propagation, null-provider behavior, and passive options defaults
and assignment semantics. Targeted command:

```powershell
dotnet test tests/unit/vc.Ifx.UnitTests --filter FullyQualifiedName~Secrets.Abstractions -m:1 -p:BuildInParallel=false
```

When other workstreams prevent compilation of the shared test project, this
opt-in command compiles only this package's tests and project reference, using
separate intermediate and output folders. It uses the shared project's existing
test dependencies and coverage thresholds without changing that project:

```powershell
dotnet test tests/unit/vc.Ifx.UnitTests/vc.Ifx.UnitTests.csproj --no-restore `
    --filter FullyQualifiedName~Secrets.Abstractions -m:1 -p:BuildInParallel=false `
    -p:CustomAfterMicrosoftCommonTargets=C:/dev/a/vc.Ifx/tests/unit/vc.Ifx.UnitTests/Secrets/Abstractions/Secrets.Abstractions.Verification.targets `
    -p:IntermediateOutputPath=obj/secrets-abstractions/ `
    -p:OutputPath=bin/secrets-abstractions/ -p:GeneratePackageOnBuild=false `
    -p:CollectCoverage=true -p:IfxCoveragePackage=vc.Ifx.Secrets.Abstractions `
    -p:CoverletOutput=C:/dev/a/vc.Ifx/artifacts/coverage/secrets-abstractions/ -v:minimal
```

Adjust the two absolute paths for a different checkout. On 2026-09-09 this run
passed all 26 tests with 100% line, branch, and method coverage, including options
properties. The scoped run does not prove consumer integration or full-suite
health. Final repository build and suite verification are handed to Orchestrator
in the parallel upgrade plan.
