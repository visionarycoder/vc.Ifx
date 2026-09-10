using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace VisionaryCoder.Framework.Secrets.Azure.KeyVault;

internal static class KeyVaultSettings
{
    internal static KeyVaultOptions Snapshot(KeyVaultOptions options) => new()
    {
        VaultUri = options.VaultUri,
        CacheTtl = options.CacheTtl,
        UseLocalSecrets = options.UseLocalSecrets,
        LocalSecretsPrefix = options.LocalSecretsPrefix,
        MaxRetries = options.MaxRetries,
        RetryDelay = options.RetryDelay
    };

    internal static void ValidateRemote(KeyVaultOptions options, bool requireUri)
    {
        if (options.UseLocalSecrets)
        {
            throw new ArgumentException("Use LocalSecretProvider for explicitly selected local mode.", nameof(options));
        }

        if (requireUri || options.VaultUri is not null)
        {
            Uri? uri = options.VaultUri;
            if (uri is null || !uri.IsAbsoluteUri || uri.Scheme != Uri.UriSchemeHttps ||
                uri.UserInfo.Length != 0 || uri.Query.Length != 0 ||
                uri.Fragment.Length != 0 || uri.AbsolutePath != "/")
            {
                throw new ArgumentException("Remote mode requires an absolute HTTPS vault URI without credentials, path, query, or fragment.", nameof(options));
            }
        }

        if (options.CacheTtl < TimeSpan.Zero || options.CacheTtl > TimeSpan.FromDays(1))
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Cache TTL must be between zero and one day.");
        }

        if (options.MaxRetries < 0 || options.MaxRetries > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "SDK retries must be between zero and ten.");
        }

        if (options.RetryDelay < TimeSpan.Zero || options.RetryDelay > TimeSpan.FromSeconds(30))
        {
            throw new ArgumentOutOfRangeException(nameof(options), "SDK retry delay must be between zero and thirty seconds.");
        }
    }

    internal static SecretClientOptions CreateClientOptions(KeyVaultOptions options)
    {
        var clientOptions = new SecretClientOptions();
        clientOptions.Retry.Mode = RetryMode.Exponential;
        clientOptions.Retry.MaxRetries = options.MaxRetries;
        clientOptions.Retry.Delay = options.RetryDelay;
        clientOptions.Retry.MaxDelay = TimeSpan.FromSeconds(30);
        clientOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(30);
        return clientOptions;
    }

    internal static void ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (name.Length > 127 || name.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            throw new ArgumentException("Secret names must contain 1-127 ASCII letters, digits, or hyphens.", nameof(name));
        }
    }

    internal static void ValidateVersion(string? version)
    {
        if (version is not null && (version.Length != 32 || version.Any(character => !char.IsAsciiHexDigit(character))))
        {
            throw new ArgumentException("Secret versions must contain 32 hexadecimal characters.", nameof(version));
        }
    }
}
