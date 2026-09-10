using Microsoft.Extensions.Configuration;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;

namespace VisionaryCoder.Framework.Secrets.Local;

/// <summary>
/// Retrieves secrets from live configuration with process-environment fallback.
/// </summary>
/// <remarks>
/// The legacy options type is a passive contract in Secrets.Abstractions; this provider has
/// no Key Vault SDK dependency. Configuration and options remain caller-owned. No values
/// are cached, and changes visible through the configuration instance are read on each call.
/// </remarks>
public sealed class LocalSecretProvider : ISecretProvider
{
    private readonly IConfiguration configuration;
    private readonly KeyVaultOptions options;

    /// <summary>Creates a provider using the existing configuration/options contract.</summary>
    /// <param name="configuration">The live configuration instance, owned by the caller.</param>
    /// <param name="options">The options whose local prefix is used for each retrieval.</param>
    /// <exception cref="ArgumentNullException">A constructor argument is null.</exception>
    /// <exception cref="ArgumentException">The local prefix is not a valid configuration path.</exception>
    public LocalSecretProvider(IConfiguration configuration, KeyVaultOptions options)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);
        ValidatePath(options.LocalSecretsPrefix, nameof(options.LocalSecretsPrefix));
        this.configuration = configuration;
        this.options = options;
    }

    /// <summary>
    /// Reads the prefixed configuration key, direct configuration key, then exact environment name.
    /// </summary>
    /// <param name="name">A colon-separated path with nonempty, non-whitespace segments.</param>
    /// <param name="cancellationToken">A token checked before retrieval and after each source read.</param>
    /// <returns>The first non-null value, including empty or whitespace values; otherwise null.</returns>
    /// <exception cref="ArgumentException">The name or current prefix is not a valid path.</exception>
    /// <exception cref="OperationCanceledException">Cancellation has been requested.</exception>
    /// <remarks>
    /// Names and prefixes are not trimmed or rewritten. Configuration controls key comparison;
    /// environment fallback uses the name exactly, without replacing colons with underscores.
    /// Source failures propagate. Synchronous source reads cannot be interrupted in progress.
    /// </remarks>
    public Task<string?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidatePath(name, nameof(name));
        string prefix = options.LocalSecretsPrefix;
        ValidatePath(prefix, nameof(options.LocalSecretsPrefix));

        string? value = configuration[$"{prefix}:{name}"];
        cancellationToken.ThrowIfCancellationRequested();

        if (value is null)
        {
            value = configuration[name];
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (value is null)
        {
            value = Environment.GetEnvironmentVariable(name);
            cancellationToken.ThrowIfCancellationRequested();
        }

        return Task.FromResult(value);
    }

    private static void ValidatePath(string path, string parameterName)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, parameterName);
        if (path.Contains('\0'))
        {
            throw new ArgumentException("Configuration paths cannot contain null characters.", parameterName);
        }

        foreach (string segment in path.Split(':'))
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                throw new ArgumentException("Configuration paths require nonempty, non-whitespace segments.", parameterName);
            }
        }
    }
}
