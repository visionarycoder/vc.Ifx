# vc.Ifx.Secrets.Abstractions

Secret retrieval abstractions and passive options contracts for vc.Ifx.

## Contents

- `ISecretProvider` — secret lookup contract with the default sequential batch helper.
- `NullSecretProvider` — no-op singleton implementation.
- `SecretOptions` — legacy passive record retained for compatibility.
- `KeyVaultOptions` — passive options contract for Key Vault registration and the local provider.

`KeyVaultOptions` remains in the historical `Ifx.Secrets.Azure.KeyVault` namespace for
API compatibility, but the type lives in this abstractions assembly so provider packages
share one source of truth without circular references.
