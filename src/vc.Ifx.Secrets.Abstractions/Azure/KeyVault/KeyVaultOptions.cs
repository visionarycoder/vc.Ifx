namespace VisionaryCoder.Framework.Secrets.Azure.KeyVault;

/// <summary>
/// Configuration options for Azure Key Vault secret management.
/// </summary>
public sealed class KeyVaultOptions
{
    /// <summary>
    /// The URI of the Azure Key Vault instance.
    /// </summary>
    /// <example>https://your-keyvault.vault.azure.net/</example>
    public Uri? VaultUri { get; set; }
    /// The time-to-live for cached secrets.
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(15);
    /// Whether to use local secrets instead of Key Vault (for development).
    public bool UseLocalSecrets { get; set; } = false;
    /// The prefix to use when looking up local secrets in configuration.
    public string LocalSecretsPrefix { get; set; } = "Secrets";
    /// Maximum number of retry attempts for Key Vault operations.
    public int MaxRetries { get; set; } = 3;
    /// The delay between retry attempts.
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}
