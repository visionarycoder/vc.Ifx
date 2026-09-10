namespace VisionaryCoder.Framework.Secrets.Azure;

public sealed record SecretOptions
{
    public Uri? KeyVaultUri { get; init; }               // e.g., https://your-vault.vault.azure.net/
    public TimeSpan CacheTtl { get; init; } = TimeSpan.FromMinutes(5);
    public bool UseLocalSecrets { get; init; }           // force local fallback (no Azure calls)
}
