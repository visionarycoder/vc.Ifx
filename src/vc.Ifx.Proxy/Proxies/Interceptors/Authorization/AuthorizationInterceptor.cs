using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Security;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Authorization;

/// <summary>
/// Enforces authorization policies on method invocation.
/// Uses the RequireAuthorizationAttribute to determine required policies.
/// Throws UnauthorizedAccessException if authorization fails.
/// </summary>
public sealed class AuthorizationInterceptor(ICurrentPrincipalAccessor principalAccessor, IAuthorizationService authorizationService) : IProxyInterceptor
{
    /// <summary>
    /// Executes the validator to enforce authorization on method invocation.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        var attribute = context.GetAttribute<RequireAuthorizationAttribute>();
        if (attribute is null)
            return await next().ConfigureAwait(false);

        // Check authorization with the service
        var authorized = await authorizationService
            .AuthorizeAsync(principalAccessor.Principal, attribute.Policy)
            .ConfigureAwait(false);

        if (!authorized)
            throw new UnauthorizedAccessException($"Authorization policy '{attribute.Policy}' failed.");

        return await next().ConfigureAwait(false);
    }
}
