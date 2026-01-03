// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.Secrets;

/// <summary>
/// Defines the contract for secret retrieval from various sources such as Azure Key Vault, HashiCorp Vault, or local configuration.
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

    /// <summary>
    /// Retrieves multiple secrets by their names.
    /// </summary>
    /// <param name="names">The names of the secrets to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
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
