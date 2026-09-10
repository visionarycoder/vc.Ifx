using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>Snapshots one host-authenticated identity; never authenticates incoming tokens.</summary>
public class DefaultUserContextProvider : IUserContextProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public DefaultUserContextProvider(IHttpContextAccessor httpContextAccessor, ILogger<DefaultUserContextProvider> logger)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(logger);
    }

    public async Task<UserContext?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        UserContext current = (await GetCurrentUserAsync(cancellationToken))!;
        return IsAuthenticated(current) && current.UserId == userId ? current : null;
    }

    public async Task<bool> ValidateUserContextAsync(UserContext userContext, CancellationToken cancellationToken = default)
    {
        UserContext current = (await GetCurrentUserAsync(cancellationToken))!;
        return userContext is not null && IsAuthenticated(current) && IsAuthenticated(userContext) &&
            current.UserId == userContext.UserId &&
            Equals(current.Claims.GetValueOrDefault("TenantId"), userContext.Claims.GetValueOrDefault("TenantId")) &&
            current.Roles.ToHashSet(StringComparer.Ordinal).SetEquals(userContext.Roles) &&
            current.Permissions.ToHashSet(StringComparer.Ordinal).SetEquals(userContext.Permissions);
    }

    protected UserContext GetCurrentUser()
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context is null)
            return CreateAnonymousUser();
        ClaimsPrincipal? principal = RequestIdentity.Authenticated(context.User);
        if (principal is null)
            return CreateAnonymousUser();
        UserContext user = ExtractUserContextFromPrincipal(principal);
        if (string.IsNullOrWhiteSpace(user.UserId))
            return CreateAnonymousUser();
        EnrichFromHttpHeaders(user, context);
        return user;
    }

    public async Task<UserContext?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        using var cancellation = RequestIdentity.Cancellation(httpContextAccessor, cancellationToken);
        cancellation.Token.ThrowIfCancellationRequested();
        UserContext user = GetCurrentUser();
        await EnrichUserContextAsync(user, cancellation.Token).ConfigureAwait(false);
        cancellation.Token.ThrowIfCancellationRequested();
        return user;
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;
        UserContext user = GetCurrentUser();
        if (!IsAuthenticated(user))
            return false;
        if (user.Permissions.Contains(permission, StringComparer.Ordinal))
            return true;
        return CheckRoleBasedPermissions(new ClaimsPrincipal(new ClaimsIdentity(
            user.Roles.Select(role => new Claim(ClaimTypes.Role, role)), "Snapshot")), permission);
    }

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;
        UserContext user = GetCurrentUser();
        return IsAuthenticated(user) && user.Roles.Contains(role, StringComparer.Ordinal);
    }

    protected virtual UserContext ExtractUserContextFromPrincipal(ClaimsPrincipal principal)
    {
        var user = new UserContext
        {
            UserId = RequestIdentity.Scalar(principal, ClaimTypes.NameIdentifier, "sub", "user_id") ?? "",
            UserName = principal.Identity!.Name ?? principal.FindFirst("name")?.Value ?? principal.FindFirst("preferred_username")?.Value ?? "",
            Email = RequestIdentity.Scalar(principal, ClaimTypes.Email, "email"),
            Roles = RequestIdentity.Values(principal, ((ClaimsIdentity)principal.Identity).RoleClaimType, ClaimTypes.Role, "role", "roles").ToList(),
            Permissions = RequestIdentity.Values(principal, "permission", "permissions").ToList(),
            AuthenticatedAt = DateTimeOffset.UtcNow,
            Claims = new Dictionary<string, object>
            {
                ["authenticated"] = true,
                ["authentication_type"] = principal.Identity.AuthenticationType!
            }
        };
        AddClaim("FirstName", ClaimTypes.GivenName, "given_name");
        AddClaim("LastName", ClaimTypes.Surname, "family_name");
        AddClaim("TenantId", "tenant_id", "tid", "tenantid");
        AddClaim("CorrelationId", "correlation_id", "cid");
        return user;

        void AddClaim(string key, params string[] types)
        {
            string? value = RequestIdentity.Scalar(principal, types);
            if (value is not null)
                user.Claims[key] = value;
        }
    }

    protected virtual void EnrichFromHttpHeaders(UserContext userContext, HttpContext httpContext)
    {
        foreach (var pair in new[] { ("CorrelationId", "X-Correlation-ID"), ("UserAgent", "User-Agent"),
            ("Timezone", "X-User-Timezone"), ("Locale", "X-User-Locale") })
        {
            string? value = RequestIdentity.Header(httpContext, pair.Item2);
            if (value is not null)
                userContext.Claims.TryAdd(pair.Item1, value);
        }
        userContext.Claims["ClientIP"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    protected virtual Task EnrichUserContextAsync(UserContext userContext, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    protected virtual bool CheckRoleBasedPermissions(ClaimsPrincipal principal, string permission) => false;

    protected virtual UserContext CreateAnonymousUser() => new()
    {
        UserId = "anonymous",
        UserName = "Anonymous",
        Claims = new Dictionary<string, object>
        {
            ["authenticated"] = false,
            ["authentication_type"] = "None"
        }
    };

    private static bool IsAuthenticated(UserContext user) =>
        user.Claims.TryGetValue("authenticated", out object? value) && value is true;
}
