using System.Security.Claims;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Authorization;

/// <summary>
/// Defines a contract for authorizing method invocations against policies.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Authorizes the given principal against the specified policy.
    /// </summary>
    /// <param name="principal">The claims principal to authorize.</param>
    /// <param name="policy">The authorization policy to enforce.</param>
    /// <returns>True if authorized; otherwise false.</returns>
    ValueTask<bool> AuthorizeAsync(ClaimsPrincipal principal, string policy);
}
