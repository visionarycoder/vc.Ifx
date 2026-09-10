namespace VisionaryCoder.Framework.Secrets.Azure;

/// <summary>A legacy passive options record retained for source and binary compatibility.</summary>
/// <remarks>Provider registration uses KeyVaultOptions; this record is not automatically bound or consumed.</remarks>
public sealed record SecretOptions
{
    /// <summary>Gets the optional vault URI.</summary>
    public Uri? KeyVaultUri { get; init; }
    /// <summary>Gets the legacy cache TTL, defaulting to five minutes.</summary>
    public TimeSpan CacheTtl { get; init; } = TimeSpan.FromMinutes(5);
    /// <summary>Gets whether local mode was requested in this passive record.</summary>
    public bool UseLocalSecrets { get; init; }
}
