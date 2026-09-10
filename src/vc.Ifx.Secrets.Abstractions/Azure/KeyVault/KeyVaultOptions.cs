namespace VisionaryCoder.Framework.Secrets.Azure.KeyVault;

/// <summary>
/// Configuration options for Azure Key Vault secret management.
/// </summary>
/// <remarks>
/// This passive options class remains in the abstractions assembly for compatibility with
/// existing local and Key Vault consumers. Its historical namespace does not introduce SDK
/// or configuration dependencies. Setters intentionally perform no validation; providers
/// validate the values they consume when configuring their services.
/// </remarks>
public sealed class KeyVaultOptions
{
    /// <summary>
    /// The URI of the Azure Key Vault instance.
    /// </summary>
    /// <example>https://your-keyvault.vault.azure.net/</example>
    public Uri? VaultUri { get; set; }
    /// <summary>The time-to-live for cached secrets. Defaults to fifteen minutes.</summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(15);
    /// <summary>Whether to use local secrets instead of Key Vault. Defaults to false.</summary>
    public bool UseLocalSecrets { get; set; } = false;
    /// <summary>The local configuration prefix. Defaults to <c>Secrets</c>.</summary>
    public string LocalSecretsPrefix { get; set; } = "Secrets";
    /// <summary>The maximum retry count. Defaults to three.</summary>
    public int MaxRetries { get; set; } = 3;
    /// <summary>The delay between retries. Defaults to one second.</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}
