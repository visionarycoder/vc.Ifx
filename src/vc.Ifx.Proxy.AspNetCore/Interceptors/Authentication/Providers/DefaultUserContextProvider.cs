// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Default implementation of <see cref="IUserContextProvider"/> that extracts user information from HTTP context.
/// Provides comprehensive user context extraction from JWT tokens, claims, and HTTP headers.
/// Supports multi-tenant scenarios and comprehensive user attribute management.
/// </summary>
public class DefaultUserContextProvider : IUserContextProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<DefaultUserContextProvider> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultUserContextProvider"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor for accessing request context.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public DefaultUserContextProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<DefaultUserContextProvider> logger)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current user context by user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The user context for the specified user, or null if not found.</returns>
    public async Task<UserContext?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        try
        {
            await Task.CompletedTask; // Placeholder for async user lookup operations

            // In a real implementation, this would load user details from a database or directory service
            // For now, return null to indicate user not found
            logger.LogDebug("User lookup not implemented for user: {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get user by ID: {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Validates if the current user context is still valid.
    /// </summary>
    /// <param name="userContext">The user context to validate.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the user context is valid; otherwise false.</returns>
    public async Task<bool> ValidateUserContextAsync(UserContext userContext, CancellationToken cancellationToken = default)
    {
        if (userContext == null)
            return false;

        try
        {
            await Task.CompletedTask; // Placeholder for async validation operations

            // Use the built-in validation logic
            return userContext.IsValid;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate user context for user: {UserId}", userContext.UserId);
            return false;
        }
    }

    /// <summary>
    /// Gets the current user context from the HTTP request context.
    /// Extracts user information from JWT claims, headers, and authentication context.
    /// </summary>
    /// <returns>A UserContext containing the current user's information, or an anonymous context if no user is authenticated.</returns>
    protected UserContext GetCurrentUser()
    {
        try
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                logger.LogDebug("No authenticated user found in HTTP context");
                return CreateAnonymousUser();
            }

            ClaimsPrincipal principal = httpContext.User;
            UserContext userContext = ExtractUserContextFromPrincipal(principal);

            // Add additional context from HTTP headers if available
            EnrichFromHttpHeaders(userContext, httpContext);

            logger.LogDebug("User context extracted for user: {UserId} ({UserName})",
                userContext.UserId, userContext.UserName);

            return userContext;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract user context from HTTP context");
            return CreateAnonymousUser();
        }
    }

    /// <summary>
    /// Gets the current user context asynchronously with additional processing.
    /// Allows for async operations during user context extraction.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task with the UserContext containing the current user's information.</returns>
    public async Task<UserContext?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            UserContext userContext = GetCurrentUser();

            // Perform any additional async enrichment here
            await EnrichUserContextAsync(userContext, cancellationToken);

            return userContext;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("User context extraction was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract user context asynchronously");
            return CreateAnonymousUser();
        }
    }

    /// <summary>
    /// Validates whether a user has the specified permission.
    /// Checks user roles, claims, and permission mappings.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        try
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return false;

            ClaimsPrincipal principal = httpContext.User;

            // Check for permission claim
            if (principal.HasClaim("permission", permission) ||
                principal.HasClaim("permissions", permission))
            {
                return true;
            }

            // Check for role-based permissions
            return CheckRoleBasedPermissions(principal, permission);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check permission: {Permission}", permission);
            return false;
        }
    }

    /// <summary>
    /// Validates whether a user belongs to the specified role.
    /// Supports both standard roles and custom role claims.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role; otherwise, false.</returns>
    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        try
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return false;

            return httpContext.User.IsInRole(role);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check role: {Role}", role);
            return false;
        }
    }

    /// <summary>
    /// Extracts user context from a ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>A UserContext extracted from the principal's claims.</returns>
    protected virtual UserContext ExtractUserContextFromPrincipal(ClaimsPrincipal principal)
    {
        string userId = GetClaimValue(principal, ClaimTypes.NameIdentifier) ??
                        GetClaimValue(principal, "sub") ??
                        GetClaimValue(principal, "user_id") ??
                        string.Empty;

        string userName = GetClaimValue(principal, ClaimTypes.Name) ??
                          GetClaimValue(principal, "name") ??
                          GetClaimValue(principal, "preferred_username") ??
                          string.Empty;

        string email = GetClaimValue(principal, ClaimTypes.Email) ??
                       GetClaimValue(principal, "email") ??
                       string.Empty;

        string[] roles = GetClaimValues(principal, ClaimTypes.Role)
            .Concat(GetClaimValues(principal, "role"))
            .Concat(GetClaimValues(principal, "roles"))
            .Distinct()
            .ToArray();

        string[] permissions = GetClaimValues(principal, "permission")
            .Concat(GetClaimValues(principal, "permissions"))
            .Distinct()
            .ToArray();

        // Extract additional attributes
        var attributes = new Dictionary<string, object>();

        string? firstName = GetClaimValue(principal, ClaimTypes.GivenName) ?? GetClaimValue(principal, "given_name");
        if (!string.IsNullOrEmpty(firstName))
            attributes["FirstName"] = firstName;

        string? lastName = GetClaimValue(principal, ClaimTypes.Surname) ?? GetClaimValue(principal, "family_name");
        if (!string.IsNullOrEmpty(lastName))
            attributes["LastName"] = lastName;

        string? tenantId = GetClaimValue(principal, "tenant_id") ?? GetClaimValue(principal, "tid");
        if (!string.IsNullOrEmpty(tenantId))
            attributes["TenantId"] = tenantId;

        string? correlationId = GetClaimValue(principal, "correlation_id") ?? GetClaimValue(principal, "cid");
        if (!string.IsNullOrEmpty(correlationId))
            attributes["CorrelationId"] = correlationId;

        var userContext = new UserContext
        {
            UserId = userId,
            UserName = userName,
            Email = email,
            Roles = roles,
            Permissions = permissions,
            Claims = attributes,
            AuthenticatedAt = DateTimeOffset.UtcNow
        };

        // Add authentication metadata to claims
        userContext.Claims["authenticated"] = true;
        userContext.Claims["authentication_type"] = principal.Identity?.AuthenticationType ?? "Unknown";

        return userContext;
    }

    /// <summary>
    /// Enriches user context with information from HTTP headers.
    /// </summary>
    /// <param name="userContext">The user context to enrich.</param>
    /// <param name="httpContext">The HTTP context.</param>
    protected virtual void EnrichFromHttpHeaders(UserContext userContext, HttpContext httpContext)
    {
        IHeaderDictionary headers = httpContext.Request.Headers;

        // Add correlation ID from header if not already present
        if (!userContext.Claims.ContainsKey("CorrelationId") &&
            headers.TryGetValue("X-Correlation-ID", out StringValues correlationId))
        {
            userContext.Claims["CorrelationId"] = correlationId.ToString();
        }

        // Add client information
        if (headers.TryGetValue("User-Agent", out StringValues userAgent))
        {
            userContext.Claims["UserAgent"] = userAgent.ToString();
        }

        if (headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
        {
            userContext.Claims["ClientIP"] = forwardedFor.ToString();
        }
        else
        {
            userContext.Claims["ClientIP"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        // Add custom headers that might contain user context
        if (headers.TryGetValue("X-User-Timezone", out StringValues timezone))
        {
            userContext.Claims["Timezone"] = timezone.ToString();
        }

        if (headers.TryGetValue("X-User-Locale", out StringValues locale))
        {
            userContext.Claims["Locale"] = locale.ToString();
        }
    }

    /// <summary>
    /// Performs additional async enrichment of user context.
    /// Override this method to add custom async processing.
    /// </summary>
    /// <param name="userContext">The user context to enrich.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    protected virtual async Task EnrichUserContextAsync(UserContext userContext, CancellationToken cancellationToken)
    {
        // Default implementation does nothing
        // Override in derived classes to add custom enrichment logic
        await Task.CompletedTask;
    }

    /// <summary>
    /// Checks role-based permissions using a simple mapping.
    /// Override this method to implement custom role-permission logic.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user's roles grant the permission; otherwise, false.</returns>
    protected virtual bool CheckRoleBasedPermissions(ClaimsPrincipal principal, string permission)
    {
        // Simple role-permission mapping
        // In a real implementation, this would likely come from a database or configuration
        var rolePermissionMap = new Dictionary<string, string[]>
        {
            ["Admin"] = ["read", "write", "delete", "manage"],
            ["Editor"] = ["read", "write"],
            ["Viewer"] = ["read"]
        };

        var userRoles = GetClaimValues(principal, ClaimTypes.Role)
            .Concat(GetClaimValues(principal, "role"))
            .ToHashSet();

        return rolePermissionMap
            .Where(kvp => userRoles.Contains(kvp.Key))
            .SelectMany(kvp => kvp.Value)
            .Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates an anonymous user context for unauthenticated scenarios.
    /// </summary>
    /// <returns>An anonymous UserContext.</returns>
    protected virtual UserContext CreateAnonymousUser()
    {
        return new UserContext
        {
            UserId = "anonymous",
            UserName = "Anonymous",
            Email = null,
            Roles = new List<string>(),
            Permissions = new List<string>(),
            Claims = new Dictionary<string, object>
            {
                ["authenticated"] = false,
                ["authentication_type"] = "None"
            }
        };
    }

    /// <summary>
    /// Gets a single claim value from the principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>The claim value, or null if not found.</returns>
    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Gets all claim values for a specific claim type.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>An enumerable of claim values.</returns>
    private static IEnumerable<string> GetClaimValues(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindAll(claimType).Select(c => c.Value);
    }
}
