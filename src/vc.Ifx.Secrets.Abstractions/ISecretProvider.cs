namespace VisionaryCoder.Framework.Secrets;

/// <summary>
/// Defines the contract for secret retrieval from various sources.
/// </summary>
/// <remarks>
/// Names are opaque provider-specific identifiers. This contract does not select or return
/// secret versions: providers resolve their current or configured version. Configuration,
/// credentials, caching, retries, and version selection belong to provider packages.
/// </remarks>
public interface ISecretProvider
{
    /// <summary>
    /// Retrieves a secret by its name.
    /// </summary>
    /// <param name="name">The name of the secret to retrieve; validation is provider-specific.</param>
    /// <param name="cancellationToken">A cancellation request handled by the provider.</param>
    /// <returns>
    /// The secret value, including an empty string when stored, or <see langword="null"/>
    /// when unavailable. A null result is not proof that a secret does not exist: providers
    /// may document a fallback policy that also returns null on retrieval failure.
    /// </returns>
    /// <remarks>
    /// Providers document their cancellation and failure policies. Callers must handle
    /// exceptions from providers that propagate failures instead of applying a fallback.
    /// </remarks>
    Task<string?> GetAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple secrets by their names.
    /// </summary>
    /// <param name="names">The names of the secrets to retrieve.</param>
    /// <param name="cancellationToken">The token forwarded unchanged to each retrieval.</param>
    /// <returns>A dictionary of requested names and their values, including unavailable values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="names"/> is null.</exception>
    /// <remarks>
    /// The default implementation enumerates once and retrieves sequentially in input order.
    /// It uses ordinal, case-sensitive keys, retrieves repeated names again, and retains the
    /// last value for a duplicate key. An empty sequence returns a new empty dictionary.
    /// Names are forwarded unchanged; a null element cannot be stored as a dictionary key.
    /// Cancellation is delegated to <see cref="GetAsync"/>; enumeration and provider failures
    /// propagate without returning partial results. Provider overrides may document different
    /// batching behavior. Invoke through this interface to access the default implementation.
    /// </remarks>
    async Task<IDictionary<string, string?>> GetMultipleAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(names);
        var results = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (string name in names)
        {
            string? value = await GetAsync(name, cancellationToken).ConfigureAwait(false);
            results[name] = value;
        }

        return results;
    }
}
