// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Null Object pattern implementation of <see cref="ITenantContextProvider"/> that returns no tenant context.
/// Used as a safe fallback when no explicit tenant context provider is registered.
/// This ensures that the system gracefully handles scenarios where tenant context is not available.
/// </summary>
public sealed class NullTenantContextProvider : ITenantContextProvider
{
    /// <summary>
    /// Gets a null tenant identifier.
    /// </summary>
    /// <returns>Always returns null, indicating no tenant context is available.</returns>
    public string? GetTenantId()
    {
        return null;
    }

    /// <summary>
    /// Gets a null tenant identifier asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns null, indicating no tenant context is available.</returns>
    public Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Gets a null tenant context.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns null, indicating no tenant context is available.</returns>
    public Task<TenantContext?> GetTenantContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TenantContext?>(null);
    }

    /// <summary>
    /// Gets a null tenant context by identifier.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant (ignored).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns null, indicating no tenant context is available.</returns>
    public Task<TenantContext?> GetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TenantContext?>(null);
    }

    /// <summary>
    /// Validates tenant context (always returns false for null provider).
    /// </summary>
    /// <param name="tenantContext">The tenant context to validate (ignored).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Always returns false, indicating no validation capability.</returns>
    public Task<bool> ValidateTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}