namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Authorization;

/// <summary>
/// Attribute that marks a method as requiring authorization.
/// Can be applied to interface or method levels.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class RequireAuthorizationAttribute(string policy) : Attribute
{
    /// <summary>
    /// Gets the authorization policy to enforce.
    /// </summary>
    public string Policy { get; } = policy;
}
