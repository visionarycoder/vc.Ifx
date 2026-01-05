// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Abstractions;
using VisionaryCoder.Framework.Proxy.Interceptor.Security.Authorization.Abstractions;

namespace VisionaryCoder.Framework.Proxy.Interceptor.Security.Authorization;

/// <summary>
/// Role-based authorization policy.
/// </summary>
/// <param name="requiredRoles">The roles required for authorization.</param>
public class RoleBasedAuthorizationPolicy(ICollection<string> requiredRoles) : IAuthorizationPolicy
{
    
    private readonly ICollection<string> requiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));

    /// <summary>
    /// Gets the name of the authorization policy.
    /// </summary>
    public string Name => "RoleBased";

    /// <summary>
    /// Gets the policy name.
    /// </summary>
    public string PolicyName => Name;

    /// <summary>
    /// Evaluates the authorization policy.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <returns>True if authorized; otherwise, false.</returns>
    public bool Evaluate(object context)
    {
        if (context is ProxyContext proxyContext)
        {
            return IsAuthorizedAsync(proxyContext).GetAwaiter().GetResult();
        }
        return false;
    }

    /// <summary>
    /// Determines whether the operation is authorized based on user roles.
    /// </summary>
    /// <param name="context">The proxy context.</param>
    /// <returns>A task representing the asynchronous operation with the authorization result.</returns>
    public Task<bool> IsAuthorizedAsync(ProxyContext context)
    {
        if (!context.Metadata.TryGetValue("Roles", out object? rolesObj) || 
            rolesObj is not ICollection<string> userRoles)
        {
            return Task.FromResult(false);
        }
        bool hasRequiredRole = requiredRoles.Any(requiredRole => 
            userRoles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase));
        return Task.FromResult(hasRequiredRole);
    }
    
}
