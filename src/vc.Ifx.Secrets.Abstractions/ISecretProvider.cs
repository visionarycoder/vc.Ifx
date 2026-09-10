namespace VisionaryCoder.Framework.Secrets;

/// <summary>
/// Defines the contract for secret retrieval from various sources.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Retrieves a secret by its name.
    /// </summary>
    /// <param name="name">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The secret value, or null if not found.</returns>
    Task<string?> GetAsync(string name, CancellationToken cancellationToken = default);

    /// Retrieves multiple secrets by their names.
    /// <param name="names">The names of the secrets to retrieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A dictionary of secret names and their values.</returns>
    async Task<IDictionary<string, string?>> GetMultipleAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, string?>();
        
        foreach (string name in names)
        {
            string? value = await GetAsync(name, cancellationToken);
            results[name] = value;
        }
        return results;
    }
}
