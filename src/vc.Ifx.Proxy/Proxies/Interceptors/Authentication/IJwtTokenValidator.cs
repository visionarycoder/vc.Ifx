using System.Security.Claims;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Authentication;

/// <summary>
/// Defines a contract for validating JWT tokens and extracting claims.
/// </summary>
public interface IJwtTokenValidator
{
    /// <summary>
    /// Validates a JWT token and extracts the claims principal.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>A ClaimsPrincipal representing the token's claims.</returns>
    /// <exception cref="SecurityException">Thrown if the token is invalid or expired.</exception>
    ClaimsPrincipal ValidateToken(string token);
}
