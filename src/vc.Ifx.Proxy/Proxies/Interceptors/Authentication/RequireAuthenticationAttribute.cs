namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Authentication;

/// <summary>
/// Attribute that marks a method as requiring authentication via JWT token.
/// Can be applied to interface or method levels.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class RequireAuthenticationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the RequireAuthenticationAttribute.
    /// </summary>
    /// <param name="scheme">The authentication scheme (e.g., "Bearer"). Defaults to "Bearer".</param>
    public RequireAuthenticationAttribute(string scheme = "Bearer")
    {
        Scheme = scheme;
    }

    /// <summary>
    /// Gets the authentication scheme to expect.
    /// </summary>
    public string Scheme { get; }
}
