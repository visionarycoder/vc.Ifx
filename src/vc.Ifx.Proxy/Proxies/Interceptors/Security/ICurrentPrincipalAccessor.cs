using System.Security.Claims;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Security;

/// <summary>
/// Defines a contract for accessing the current security principal.
/// </summary>
public interface ICurrentPrincipalAccessor
{
    /// <summary>
    /// Gets the current principal for the invocation.
    /// </summary>
    ClaimsPrincipal Principal { get; }
}
