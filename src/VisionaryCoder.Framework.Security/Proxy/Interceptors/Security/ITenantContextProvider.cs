// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Provides tenant context information.
/// </summary>
public interface ITenantContextProvider
{
    /// <summary>
    /// Gets the current tenant ID.
    /// </summary>
    /// <returns>The tenant ID, or null if not available.</returns>
    string? GetTenantId();

    /// <summary>
    /// Gets the current tenant ID asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the tenant ID.</returns>
    Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default);
}