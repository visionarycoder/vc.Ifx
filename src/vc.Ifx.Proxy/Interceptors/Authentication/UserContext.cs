// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

/// <summary>
/// Represents authenticated user context information including identity, roles, and permissions.
/// Contains all necessary data for authorization decisions and user-specific operations.
/// </summary>
public class UserContext
{
    /// <summary>
    /// Gets or sets the unique user identifier.
    /// This should be consistent across authentication sessions.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display name or username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address if available.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the user's assigned roles.
    /// Roles are used for role-based authorization decisions.
    /// </summary>
    public ICollection<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the user's explicit permissions.
    /// Permissions provide fine-grained access control beyond roles.
    /// </summary>
    public ICollection<string> Permissions { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets additional claims associated with the user.
    /// Contains custom attributes and metadata from the authentication system.
    /// </summary>
    public Dictionary<string, object> Claims { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when the user was authenticated.
    /// </summary>
    public DateTimeOffset AuthenticatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the expiration time of the user's authentication session.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user context is valid and not expired.
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(UserId) && 
                          (ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow);

    /// <summary>
    /// Determines if the user has a specific role.
    /// </summary>
    /// <param name="roleName">The role name to check.</param>
    /// <returns>True if the user has the specified role.</returns>
    public bool HasRole(string roleName) => 
        Roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Determines if the user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the specified permission.</returns>
    public bool HasPermission(string permission) => 
        Permissions.Any(p => p.Equals(permission, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Determines if the user has any of the specified roles.
    /// </summary>
    /// <param name="roleNames">The role names to check.</param>
    /// <returns>True if the user has any of the specified roles.</returns>
    public bool HasAnyRole(params string[] roleNames) => 
        roleNames.Any(HasRole);

    /// <summary>
    /// Determines if the user has all of the specified roles.
    /// </summary>
    /// <param name="roleNames">The role names to check.</param>
    /// <returns>True if the user has all of the specified roles.</returns>
    public bool HasAllRoles(params string[] roleNames) => 
        roleNames.All(HasRole);
}