// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Defines a contract for providing authenticated user context information.
/// Implementations should retrieve current user details from the authentication system.
/// </summary>
public interface IUserContextProvider
{
    /// <summary>
    /// Gets the current authenticated user context.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The current user context, or null if no user is authenticated.</returns>
    Task<UserContext?> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current user context by user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The user context for the specified user, or null if not found.</returns>
    Task<UserContext?> GetUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the current user context is still valid.
    /// </summary>
    /// <param name="userContext">The user context to validate.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the user context is valid; otherwise false.</returns>
    Task<bool> ValidateUserContextAsync(UserContext userContext, CancellationToken cancellationToken = default);
}