using System.Collections.Concurrent;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VisionaryCoder.Framework.Secrets.Azure.KeyVault;

/// <summary>Retrieves secrets using an injected Azure SDK client and an isolated memory cache.</summary>
public sealed class KeyVaultSecretProvider : ISecretProvider
{
    private readonly SecretClient client;
    private readonly IMemoryCache cache;
    private readonly ILogger<KeyVaultSecretProvider> logger;
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan cacheTtl;
    private readonly object cacheScope = new();
    private readonly ConcurrentDictionary<(string Name, string? Version), Lazy<Task<string?>>> pending = new();

    /// <summary>Creates a provider with the system clock, preserving the legacy constructor.</summary>
    /// <param name="client">The SDK client, which owns retries and credentials.</param>
    /// <param name="options">Remote-mode options; values are validated and snapshotted.</param>
    /// <param name="cache">A caller-owned memory cache.</param>
    /// <param name="logger">A logger that receives no secret values or exception payloads.</param>
    public KeyVaultSecretProvider(SecretClient client, IOptions<KeyVaultOptions> options,
        IMemoryCache cache, ILogger<KeyVaultSecretProvider> logger)
        : this(client, options, cache, logger, TimeProvider.System)
    {
    }

    /// <summary>Creates a provider with an explicit clock for deterministic cache expiry.</summary>
    /// <param name="client">The SDK client, which owns retries and credentials.</param>
    /// <param name="options">Remote-mode options; a vault URI is optional for an injected client.</param>
    /// <param name="cache">A caller-owned memory cache.</param>
    /// <param name="logger">A logger that receives no secret values or exception payloads.</param>
    /// <param name="timeProvider">The clock used to evaluate cache and secret expiry.</param>
    public KeyVaultSecretProvider(SecretClient client, IOptions<KeyVaultOptions> options,
        IMemoryCache cache, ILogger<KeyVaultSecretProvider> logger, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(timeProvider);
        KeyVaultOptions settings = options.Value ?? throw new ArgumentException("Options must contain a value.", nameof(options));
        KeyVaultSettings.ValidateRemote(settings, requireUri: false);
        this.client = client;
        this.cache = cache;
        this.logger = logger;
        this.timeProvider = timeProvider;
        cacheTtl = settings.CacheTtl;
    }

    /// <summary>Retrieves the current version of a secret.</summary>
    /// <param name="name">A Key Vault secret name.</param>
    /// <param name="cancellationToken">The token forwarded to the SDK for cancelable requests.</param>
    /// <returns>The value, including empty strings; null only for an SDK 404 response.</returns>
    public Task<string?> GetAsync(string name, CancellationToken cancellationToken = default) =>
        GetAsync(name, null, cancellationToken);

    /// <summary>Retrieves a selected version, or the current version when version is null.</summary>
    /// <param name="name">A 1-127 character name containing ASCII letters, digits, or hyphens.</param>
    /// <param name="version">A 32-character hexadecimal version, or null for the current version.</param>
    /// <param name="cancellationToken">A token checked before cache access and after SDK completion.</param>
    /// <returns>The value, including empty strings; null only for an SDK 404 response.</returns>
    /// <remarks>
    /// Non-cancelable cache misses for the same key share a request. Cancelable callers use
    /// independent SDK requests so one caller cannot cancel another caller's operation.
    /// Disabled, expired, not-yet-valid, or malformed responses raise InvalidOperationException.
    /// Other SDK failures propagate unchanged. No local fallback or additional retries occur here.
    /// </remarks>
    public async Task<string?> GetAsync(string name, string? version, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        KeyVaultSettings.ValidateName(name);
        KeyVaultSettings.ValidateVersion(version);
        var key = (name, version);
        if (cache.TryGetValue((cacheScope, key), out (string Value, DateTimeOffset Until) cached))
        {
            if (timeProvider.GetUtcNow() < cached.Until)
            {
                return cached.Value;
            }

            cache.Remove((cacheScope, key));
        }

        if (cancellationToken.CanBeCanceled)
        {
            return await RetrieveAsync(name, version, cancellationToken).ConfigureAwait(false);
        }

        Lazy<Task<string?>> operation = pending.GetOrAdd(key,
            entry => new Lazy<Task<string?>>(() => RetrieveAsync(entry.Name, entry.Version, CancellationToken.None)));
        try
        {
            return await operation.Value.ConfigureAwait(false);
        }
        finally
        {
            pending.TryRemove(new KeyValuePair<(string Name, string? Version), Lazy<Task<string?>>>(key, operation));
        }
    }

    /// <summary>Retrieves current versions sequentially, preserving ordinal keys and last duplicate values.</summary>
    /// <param name="names">The names to enumerate once.</param>
    /// <param name="cancellationToken">The token forwarded to each retrieval.</param>
    /// <returns>A dictionary including missing entries as null values.</returns>
    public async Task<IDictionary<string, string?>> GetMultipleAsync(IEnumerable<string> names,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(names);
        var results = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (string name in names)
        {
            results[name] = await GetAsync(name, cancellationToken).ConfigureAwait(false);
        }

        return results;
    }

    private async Task<string?> RetrieveAsync(string name, string? version, CancellationToken cancellationToken)
    {
        KeyVaultSecret secret;
        try
        {
            Response<KeyVaultSecret> response = await client.GetSecretAsync(name, version, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            secret = response.Value ?? throw new InvalidOperationException("Key Vault returned no secret object.");
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug("Key Vault secret was not found.");
            return null;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        if (secret.Properties.Enabled == false || secret.Properties.NotBefore > now || secret.Properties.ExpiresOn <= now)
        {
            throw new InvalidOperationException("The requested secret is disabled or outside its validity period.");
        }

        string value = secret.Value ?? throw new InvalidOperationException("Key Vault returned no secret value.");
        if (cacheTtl > TimeSpan.Zero)
        {
            DateTimeOffset until = now + cacheTtl;
            if (secret.Properties.ExpiresOn < until)
            {
                until = secret.Properties.ExpiresOn.Value;
            }

            cache.Set((cacheScope, (name, version)), (value, until),
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = until - now, Size = 1 });
        }

        return value;
    }
}
