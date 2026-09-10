// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Null Object pattern implementation of <see cref="ITokenProvider"/> that provides no token functionality.
/// Used as a safe fallback when no explicit token provider is registered.
/// This ensures that the system gracefully handles scenarios where token functionality is not available.
/// </summary>
public sealed class NullTokenProvider : ITokenProvider
{
    /// <summary>
    /// Gets a token asynchronously (always fails for null provider).
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always throws InvalidOperationException indicating no token provider is available.</returns>
    /// <exception cref="InvalidOperationException">Always thrown to indicate no token provider is configured.</exception>
    public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("No token provider is configured. Register an explicit ITokenProvider implementation.");
    }

    /// <summary>
    /// Gets a token result asynchronously (always fails for null provider).
    /// </summary>
    /// <param name="request">The token request (ignored).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns a failed TokenResult indicating no token provider is available.</returns>
    public Task<TokenResult> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TokenResult.Failure(
            "no_provider", 
            "No token provider is configured. Register an explicit ITokenProvider implementation."));
    }

    /// <summary>
    /// Validates a token asynchronously (always returns false for null provider).
    /// </summary>
    /// <param name="token">The token to validate (ignored).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns false, indicating no validation capability.</returns>
    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Validates a token synchronously (always returns false for null provider).
    /// </summary>
    /// <param name="token">The token to validate (ignored).</param>
    /// <returns>Always returns false, indicating no validation capability.</returns>
    public bool ValidateToken(string token)
    {
        return false;
    }

    /// <summary>
    /// Refreshes a token asynchronously (always returns null for null provider).
    /// </summary>
    /// <param name="refreshToken">The refresh token (ignored).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns null, indicating no refresh capability.</returns>
    public Task<TokenResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TokenResult?>(null);
    }

    /// <summary>
    /// Extracts claims from a token (always returns empty dictionary for null provider).
    /// </summary>
    /// <param name="token">The token to extract claims from (ignored).</param>
    /// <returns>Always returns an empty dictionary, indicating no claim extraction capability.</returns>
    public Dictionary<string, object> ExtractClaims(string token)
    {
        return new Dictionary<string, object>();
    }
}