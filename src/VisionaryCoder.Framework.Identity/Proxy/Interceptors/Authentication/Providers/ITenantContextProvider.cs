// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Defines a contract for providing tenant context information in multi-tenant scenarios.
/// Implementations should retrieve current tenant details from the authentication or request context.
/// </summary>
public interface ITenantContextProvider
{
    /// <summary>
    /// Gets the current tenant identifier synchronously.
    /// </summary>
    /// <returns>The tenant ID, or null if not available or not in a multi-tenant context.</returns>
    string? GetTenantId();

    /// <summary>
    /// Gets the current tenant identifier asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the async operation with the tenant ID.</returns>
    Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current tenant context with detailed information.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The tenant context, or null if not available.</returns>
    Task<TenantContext?> GetTenantContextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tenant context by tenant identifier.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The tenant context for the specified tenant, or null if not found.</returns>
    Task<TenantContext?> GetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the current tenant context is still valid.
    /// </summary>
    /// <param name="tenantContext">The tenant context to validate.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the tenant context is valid; otherwise false.</returns>
    Task<bool> ValidateTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken = default);
}
