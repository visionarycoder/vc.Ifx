using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

internal static class RequestIdentity
{
    internal static ClaimsPrincipal? Authenticated(ClaimsPrincipal principal)
    {
        ClaimsIdentity[] identities = principal.Identities.Where(identity => identity.IsAuthenticated).ToArray();
        return identities.Length == 1 ? new ClaimsPrincipal(identities[0].Clone()) : null;
    }

    internal static string[] Values(ClaimsPrincipal principal, params string[] types) =>
        principal.Claims.Where(claim => types.Contains(claim.Type, StringComparer.Ordinal))
            .Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray();

    internal static string? Scalar(ClaimsPrincipal principal, params string[] types)
    {
        string[] values = Values(principal, types);
        if (values.Length > 1)
            throw new UnauthorizedAccessException("Conflicting identity claims.");
        return values.SingleOrDefault();
    }

    internal static string? Header(HttpContext context, string name)
    {
        var values = context.Request.Headers[name];
        if (values.Count != 1)
            return null;
        string? value = values[0];
        return string.IsNullOrWhiteSpace(value) || value.Length > 512 || value.Any(char.IsControl) ? null : value;
    }

    internal static CancellationTokenSource Cancellation(IHttpContextAccessor accessor, CancellationToken token) =>
        CancellationTokenSource.CreateLinkedTokenSource(token, accessor.HttpContext?.RequestAborted ?? default);
}
