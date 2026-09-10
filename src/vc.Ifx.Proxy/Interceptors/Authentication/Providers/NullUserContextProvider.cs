// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Null Object pattern implementation of <see cref="IUserContextProvider"/> that returns no user context.
/// Used as a safe fallback when no explicit user context provider is registered.
/// This ensures that the system gracefully handles scenarios where user context is not available.
/// </summary>
public sealed class NullUserContextProvider : IUserContextProvider
{
    /// <summary>
    /// Gets a null user context asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns null, indicating no user context is available.</returns>
    public Task<UserContext?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<UserContext?>(null);
    }

    /// <summary>
    /// Gets a null user context by user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (ignored).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns null, indicating no user context is available.</returns>
    public Task<UserContext?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<UserContext?>(null);
    }

    /// <summary>
    /// Validates user context (always returns false for null provider).
    /// </summary>
    /// <param name="userContext">The user context to validate (ignored).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns false, indicating no validation capability.</returns>
    public Task<bool> ValidateUserContextAsync(UserContext userContext, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}