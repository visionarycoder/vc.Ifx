using Microsoft.Extensions.Configuration;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;

namespace VisionaryCoder.Framework.Secrets.Local;
/// <summary>
/// Local implementation of ISecretProvider for development scenarios.
/// </summary>
/// <param name="configuration">The configuration instance.</param>
/// <param name="options">The KeyVault options.</param>
public sealed class LocalSecretProvider(IConfiguration configuration, KeyVaultOptions options) : ISecretProvider
{
    private readonly IConfiguration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly KeyVaultOptions options = options ?? throw new ArgumentNullException(nameof(options));
    /// <summary>
    /// Retrieves a secret from local configuration sources.
    /// Searches in order: Secrets:{name}, {name}, Environment Variable {name}
    /// </summary>
    public Task<string?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Secret name cannot be null or empty.", nameof(name));
        }
        // Try configuration with prefix first
        string prefixedKey = $"{options.LocalSecretsPrefix}:{name}";
        string? value = configuration[prefixedKey]
                        ?? configuration[name]
                        ?? Environment.GetEnvironmentVariable(name);
        return Task.FromResult(value);
    }
}
