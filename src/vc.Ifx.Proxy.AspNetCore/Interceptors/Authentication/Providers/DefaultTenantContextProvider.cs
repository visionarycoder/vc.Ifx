using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>Uses authenticated tenant claims as authority, never routing hints as authorization.</summary>
public class DefaultTenantContextProvider : ITenantContextProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public DefaultTenantContextProvider(IHttpContextAccessor httpContextAccessor, ILogger<DefaultTenantContextProvider> logger)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(logger);
    }

    public string? GetTenantId()
    {
        TenantContext tenant = GetCurrentTenant();
        return tenant.IsActive ? tenant.TenantId : null;
    }

    public async Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        TenantContext tenant = await GetCurrentTenantAsync(cancellationToken);
        return tenant.IsActive ? tenant.TenantId : null;
    }

    public async Task<TenantContext?> GetTenantContextAsync(CancellationToken cancellationToken = default) =>
        await GetCurrentTenantAsync(cancellationToken);

    public async Task<TenantContext?> GetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        TenantContext tenant = await GetCurrentTenantAsync(cancellationToken);
        return tenant.IsActive && tenant.TenantId == tenantId ? tenant : null;
    }

    public async Task<bool> ValidateTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken = default)
    {
        TenantContext current = await GetCurrentTenantAsync(cancellationToken);
        return tenantContext is not null && tenantContext.IsActive && current.IsActive &&
            tenantContext.TenantId == current.TenantId;
    }

    protected TenantContext GetCurrentTenant()
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context is null)
            return CreateDefaultTenantContext();
        TenantContext tenant = ExtractTenantFromClaims(context) ?? CreateDefaultTenantContext();
        EnrichTenantContext(tenant, context);
        return tenant;
    }

    public async Task<TenantContext> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        using var cancellation = RequestIdentity.Cancellation(httpContextAccessor, cancellationToken);
        cancellation.Token.ThrowIfCancellationRequested();
        TenantContext tenant = GetCurrentTenant();
        await EnrichTenantContextAsync(tenant, cancellation.Token).ConfigureAwait(false);
        cancellation.Token.ThrowIfCancellationRequested();
        return tenant;
    }

    /// <summary>Checks identifier syntax only, not tenant existence or access.</summary>
    public bool IsTenantValid(string tenantId) =>
        tenantId is not null && Regex.IsMatch(tenantId, @"\A[A-Za-z0-9_-]{1,128}\z", RegexOptions.CultureInvariant);

    public bool SwitchTenant(string tenantId)
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context is null || context.Response.HasStarted || !IsTenantValid(tenantId))
            return false;
        TenantContext? tenant = ExtractTenantFromClaims(context);
        if (tenant is null || tenant.TenantId != tenantId)
            return false;
        context.Response.Headers["X-Current-Tenant"] = tenantId;
        return true;
    }

    protected virtual TenantContext? ExtractTenantFromClaims(HttpContext httpContext)
    {
        ClaimsPrincipal? principal = RequestIdentity.Authenticated(httpContext.User);
        if (principal is null)
            return null;
        string? id = RequestIdentity.Scalar(principal, "tenant_id", "tid", "tenantid");
        if (!IsTenantValid(id!))
            return null;
        return CreateTenantContext(id!, RequestIdentity.Scalar(principal, "tenant_name", "tenant") ?? id!, "Claims");
    }

    protected virtual TenantContext? ExtractTenantFromHeaders(HttpContext httpContext) =>
        MatchHint(httpContext, RequestIdentity.Header(httpContext, "X-Tenant-ID"), "Header");

    protected virtual TenantContext? ExtractTenantFromSubdomain(HttpContext httpContext)
    {
        string[] parts = httpContext.Request.Host.Host.Split('.');
        return parts.Length >= 3 ? MatchHint(httpContext, parts[0], "Subdomain") : null;
    }

    protected virtual TenantContext? ExtractTenantFromPath(HttpContext httpContext)
    {
        string[] parts = httpContext.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 && (parts[0] == "tenant" || parts[0] == "t")
            ? MatchHint(httpContext, parts[1], "Path") : null;
    }

    protected virtual TenantContext CreateTenantContext(string tenantId, string tenantName, string source) => new()
    {
        TenantId = tenantId,
        TenantName = tenantName,
        IsActive = true,
        Settings = new Dictionary<string, object> { ["Source"] = source, ["ExtractedAt"] = DateTimeOffset.UtcNow }
    };

    protected virtual void EnrichTenantContext(TenantContext tenantContext, HttpContext httpContext)
    {
        tenantContext.Settings["RequestPath"] = httpContext.Request.Path.ToString();
        tenantContext.Settings["RequestMethod"] = httpContext.Request.Method;
        string? correlation = RequestIdentity.Header(httpContext, "X-Correlation-ID");
        if (correlation is not null)
            tenantContext.Settings["CorrelationId"] = correlation;
    }

    protected virtual Task EnrichTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    protected virtual TenantContext CreateDefaultTenantContext() => new()
    {
        TenantId = "default",
        TenantName = "Default",
        IsActive = false,
        Settings = new Dictionary<string, object> { ["Source"] = "Default", ["ExtractedAt"] = DateTimeOffset.UtcNow }
    };

    protected virtual bool IsPotentialTenantId(string value) => IsTenantValid(value);

    private TenantContext? MatchHint(HttpContext context, string? hint, string source)
    {
        TenantContext? authority = ExtractTenantFromClaims(context);
        return authority is not null && authority.TenantId == hint
            ? CreateTenantContext(authority.TenantId, authority.TenantName, source) : null;
    }
}
