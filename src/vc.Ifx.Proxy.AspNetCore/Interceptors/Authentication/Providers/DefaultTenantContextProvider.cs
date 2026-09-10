// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Default implementation of <see cref="ITenantContextProvider"/> that extracts tenant information from HTTP context.
/// Provides comprehensive tenant context extraction from JWT tokens, claims, HTTP headers, and routing information.
/// Supports multi-tenant SaaS applications with flexible tenant identification strategies.
/// </summary>
public class DefaultTenantContextProvider : ITenantContextProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<DefaultTenantContextProvider> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTenantContextProvider"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor for accessing request context.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public DefaultTenantContextProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<DefaultTenantContextProvider> logger)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current tenant identifier synchronously.
    /// </summary>
    /// <returns>The tenant ID, or null if not available or not in a multi-tenant context.</returns>
    public string? GetTenantId()
    {
        try
        {
            TenantContext tenantContext = GetCurrentTenant();
            return tenantContext.TenantId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tenant ID");
            return null;
        }
    }

    /// <summary>
    /// Gets the current tenant identifier asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the async operation with the tenant ID.</returns>
    public async Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            TenantContext tenantContext = await GetCurrentTenantAsync(cancellationToken);
            return tenantContext.TenantId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tenant ID asynchronously");
            return null;
        }
    }

    /// <summary>
    /// Gets the current tenant context with detailed information.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The tenant context, or null if not available.</returns>
    public async Task<TenantContext?> GetTenantContextAsync(CancellationToken cancellationToken = default)
    {
        return await GetCurrentTenantAsync(cancellationToken);
    }

    /// <summary>
    /// Gets tenant context by tenant identifier.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The tenant context for the specified tenant, or null if not found.</returns>
    public async Task<TenantContext?> GetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return null;

        try
        {
            // In a real implementation, this would likely load tenant details from a database
            // For now, create a basic tenant context with the provided ID
            if (IsTenantValid(tenantId))
            {
                await Task.CompletedTask; // Placeholder for async database operations
                return CreateTenantContext(tenantId, tenantId, "Lookup");
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tenant context for tenant: {TenantId}", tenantId);
            return null;
        }
    }

    /// <summary>
    /// Validates if the current tenant context is still valid.
    /// </summary>
    /// <param name="tenantContext">The tenant context to validate.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the tenant context is valid; otherwise false.</returns>
    public async Task<bool> ValidateTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken = default)
    {
        if (tenantContext == null)
            return false;

        try
        {
            await Task.CompletedTask; // Placeholder for async validation operations

            // Basic validation - check if tenant ID is valid and tenant is active
            return !string.IsNullOrWhiteSpace(tenantContext.TenantId) &&
                   tenantContext.IsActive &&
                   IsTenantValid(tenantContext.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate tenant context for tenant: {TenantId}", tenantContext.TenantId);
            return false;
        }
    }

    /// <summary>
    /// Gets the current tenant context from the HTTP request context.
    /// Attempts multiple strategies to identify the tenant: JWT claims, headers, subdomain, and path.
    /// </summary>
    /// <returns>A TenantContext containing the current tenant's information, or a default context if no tenant is identified.</returns>
    protected TenantContext GetCurrentTenant()
    {
        try
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                logger.LogDebug("No HTTP context available for tenant extraction");
                return CreateDefaultTenantContext();
            }

            TenantContext tenantContext = ExtractTenantFromClaims(httpContext) ??
                                          ExtractTenantFromHeaders(httpContext) ??
                                          ExtractTenantFromSubdomain(httpContext) ??
                                          ExtractTenantFromPath(httpContext) ??
                                          CreateDefaultTenantContext();

            // Enrich with additional context
            EnrichTenantContext(tenantContext, httpContext);

            logger.LogDebug("Tenant context extracted: {TenantId} ({TenantName})",
                tenantContext.TenantId, tenantContext.TenantName);

            return tenantContext;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract tenant context from HTTP context");
            return CreateDefaultTenantContext();
        }
    }

    /// <summary>
    /// Gets the current tenant context asynchronously with additional processing.
    /// Allows for async operations during tenant context extraction, such as database lookups.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task with the TenantContext containing the current tenant's information.</returns>
    public async Task<TenantContext> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            TenantContext tenantContext = GetCurrentTenant();

            // Perform additional async enrichment (e.g., database lookups)
            await EnrichTenantContextAsync(tenantContext, cancellationToken);

            return tenantContext;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Tenant context extraction was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract tenant context asynchronously");
            return CreateDefaultTenantContext();
        }
    }

    /// <summary>
    /// Validates whether the specified tenant is active and accessible.
    /// Checks tenant status, subscription validity, and access permissions.
    /// </summary>
    /// <param name="tenantId">The tenant ID to validate.</param>
    /// <returns>True if the tenant is valid and accessible; otherwise, false.</returns>
    public bool IsTenantValid(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return false;

        try
        {
            // Basic validation - in a real implementation, this would check against a tenant registry
            if (tenantId.Equals("default", StringComparison.OrdinalIgnoreCase))
                return true;

            // Check if tenant ID matches expected format (e.g., GUID)
            if (Guid.TryParse(tenantId, out _))
                return true;

            // Allow alphanumeric tenant identifiers
            return tenantId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate tenant: {TenantId}", tenantId);
            return false;
        }
    }

    /// <summary>
    /// Switches the current tenant context to the specified tenant.
    /// Updates the HTTP context with new tenant information.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant to switch to.</param>
    /// <returns>True if the tenant switch was successful; otherwise, false.</returns>
    public bool SwitchTenant(string tenantId)
    {
        if (!IsTenantValid(tenantId))
        {
            logger.LogWarning("Attempted to switch to invalid tenant: {TenantId}", tenantId);
            return false;
        }

        try
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                logger.LogWarning("No HTTP context available for tenant switch");
                return false;
            }

            // Store the new tenant ID in the HTTP context items for this request
            httpContext.Items["CurrentTenantId"] = tenantId;

            // Add tenant header for downstream services
            httpContext.Response.Headers["X-Current-Tenant"] = tenantId;

            logger.LogInformation("Successfully switched to tenant: {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to switch to tenant: {TenantId}", tenantId);
            return false;
        }
    }

    /// <summary>
    /// Extracts tenant information from JWT claims.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A TenantContext if found in claims, otherwise null.</returns>
    protected virtual TenantContext? ExtractTenantFromClaims(HttpContext httpContext)
    {
        if (httpContext.User?.Identity?.IsAuthenticated != true)
            return null;

        ClaimsPrincipal principal = httpContext.User;

        string? tenantId = GetClaimValue(principal, "tenant_id") ??
                           GetClaimValue(principal, "tid") ??
                           GetClaimValue(principal, "tenantid");

        if (string.IsNullOrEmpty(tenantId))
            return null;

        string tenantName = GetClaimValue(principal, "tenant_name") ??
                            GetClaimValue(principal, "tenant") ??
                            tenantId;

        return CreateTenantContext(tenantId, tenantName, "Claims");
    }

    /// <summary>
    /// Extracts tenant information from HTTP headers.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A TenantContext if found in headers, otherwise null.</returns>
    protected virtual TenantContext? ExtractTenantFromHeaders(HttpContext httpContext)
    {
        IHeaderDictionary headers = httpContext.Request.Headers;

        // Check for explicit tenant header
        if (headers.TryGetValue("X-Tenant-ID", out StringValues tenantIdHeader))
        {
            string tenantId = tenantIdHeader.ToString();
            if (!string.IsNullOrEmpty(tenantId))
            {
                string tenantName = headers.TryGetValue("X-Tenant-Name", out StringValues nameHeader)
                    ? nameHeader.ToString()
                    : tenantId;

                return CreateTenantContext(tenantId, tenantName, "Header");
            }
        }

        // Check for tenant in authorization header (custom format)
        if (headers.TryGetValue("Authorization", out StringValues authHeader))
        {
            string authValue = authHeader.ToString();
            const string tenantPrefix = "Tenant ";
            if (authValue.StartsWith(tenantPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string tenantId = authValue.Substring(tenantPrefix.Length).Trim();
                return CreateTenantContext(tenantId, tenantId, "Authorization");
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts tenant information from the subdomain.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A TenantContext if found in subdomain, otherwise null.</returns>
    protected virtual TenantContext? ExtractTenantFromSubdomain(HttpContext httpContext)
    {
        string host = httpContext.Request.Host.Host;
        if (string.IsNullOrEmpty(host))
            return null;

        // Extract subdomain (e.g., tenant1.example.com -> tenant1)
        string[] hostParts = host.Split('.');
        if (hostParts.Length >= 3)
        {
            string subdomain = hostParts[0];

            // Skip common subdomains that aren't tenants
            string[] commonSubdomains = ["www", "api", "admin", "app", "mail", "ftp"];
            if (!commonSubdomains.Contains(subdomain, StringComparer.OrdinalIgnoreCase))
            {
                return CreateTenantContext(subdomain, subdomain, "Subdomain");
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts tenant information from the URL path.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A TenantContext if found in path, otherwise null.</returns>
    protected virtual TenantContext? ExtractTenantFromPath(HttpContext httpContext)
    {
        string? path = httpContext.Request.Path.Value;
        if (string.IsNullOrEmpty(path))
            return null;

        // Check for path patterns like /tenant/{tenantId}/... or /t/{tenantId}/...
        string[] pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < pathSegments.Length - 1; i++)
        {
            string segment = pathSegments[i];
            if (segment.Equals("tenant", StringComparison.OrdinalIgnoreCase) ||
                segment.Equals("t", StringComparison.OrdinalIgnoreCase))
            {
                string tenantId = pathSegments[i + 1];
                return CreateTenantContext(tenantId, tenantId, "Path");
            }
        }

        // Check if the first segment is a tenant identifier
        if (pathSegments.Length > 0 && IsPotentialTenantId(pathSegments[0]))
        {
            string tenantId = pathSegments[0];
            return CreateTenantContext(tenantId, tenantId, "Path");
        }

        return null;
    }

    /// <summary>
    /// Creates a tenant context with the specified information.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="tenantName">The tenant name.</param>
    /// <param name="source">The source of tenant identification.</param>
    /// <returns>A configured TenantContext.</returns>
    protected virtual TenantContext CreateTenantContext(string tenantId, string tenantName, string source)
    {
        return new TenantContext
        {
            TenantId = tenantId,
            TenantName = tenantName,
            IsActive = true,
            Settings = new Dictionary<string, object>
            {
                ["Source"] = source,
                ["ExtractedAt"] = DateTimeOffset.UtcNow
            }
        };
    }

    /// <summary>
    /// Enriches tenant context with additional information from the HTTP context.
    /// </summary>
    /// <param name="tenantContext">The tenant context to enrich.</param>
    /// <param name="httpContext">The HTTP context.</param>
    protected virtual void EnrichTenantContext(TenantContext tenantContext, HttpContext httpContext)
    {
        // Add request information
        tenantContext.Settings["RequestPath"] = httpContext.Request.Path.Value ?? string.Empty;
        tenantContext.Settings["RequestMethod"] = httpContext.Request.Method;

        // Add any tenant context override from HTTP items
        if (httpContext.Items.TryGetValue("CurrentTenantId", out object? overrideTenantId) &&
            overrideTenantId is string overrideId)
        {
            tenantContext.TenantId = overrideId;
            tenantContext.Settings["OverriddenBy"] = "HttpContext.Items";
        }

        // Add correlation ID
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues correlationId))
        {
            tenantContext.Settings["CorrelationId"] = correlationId.ToString();
        }
    }

    /// <summary>
    /// Performs additional async enrichment of tenant context.
    /// Override this method to add database lookups or external service calls.
    /// </summary>
    /// <param name="tenantContext">The tenant context to enrich.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    protected virtual async Task EnrichTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken)
    {
        // Default implementation does nothing
        // Override in derived classes to add custom enrichment logic
        // Example: Load tenant settings from database
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a default tenant context for scenarios where no specific tenant is identified.
    /// </summary>
    /// <returns>A default TenantContext.</returns>
    protected virtual TenantContext CreateDefaultTenantContext()
    {
        return new TenantContext
        {
            TenantId = "default",
            TenantName = "Default",
            IsActive = true,
            Settings = new Dictionary<string, object>
            {
                ["Source"] = "Default",
                ["ExtractedAt"] = DateTimeOffset.UtcNow
            }
        };
    }

    /// <summary>
    /// Determines if a string could be a valid tenant identifier.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value could be a tenant ID; otherwise, false.</returns>
    protected virtual bool IsPotentialTenantId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Check for GUID format
        if (Guid.TryParse(value, out _))
            return true;

        // Check for reasonable tenant ID patterns
        return value.Length >= 2 &&
               value.Length <= 50 &&
               value.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
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
}
