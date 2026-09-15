namespace Ifx.Secrets.Azure.KeyVault;

/// <summary>Represents passive configuration for Key Vault and local secret-provider selection.</summary>
/// <remarks>
/// This type intentionally performs no normalization or validation in its setters.
/// Provider registration and provider constructors validate only the members they consume.
/// </remarks>
public sealed class KeyVaultOptions
{
    /// <summary>Gets or sets the optional Key Vault base URI.</summary>
    public Uri? VaultUri { get; set; }

    /// <summary>Gets or sets the completed-value cache TTL.</summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Gets or sets whether configuration-backed local lookup is explicitly selected.</summary>
    public bool UseLocalSecrets { get; set; }

    /// <summary>Gets or sets the configuration prefix used by the local provider.</summary>
    public string LocalSecretsPrefix { get; set; } = "Secrets";

    /// <summary>Gets or sets the Azure SDK retry count.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Gets or sets the Azure SDK initial retry delay.</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}
