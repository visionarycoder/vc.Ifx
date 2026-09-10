namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors;

/// <summary>
/// Defines standard item keys used in the InvocationContext.Items dictionary.
/// These keys are used by interceptors to store and retrieve contextual information.
/// </summary>
public static class InterceptorConstants
{
    /// <summary>
    /// The key for storing the correlation identifier in the invocation context.
    /// </summary>
    public const string CorrelationId = nameof(CorrelationId);

    /// <summary>
    /// The key for storing redacted/safe arguments in the invocation context.
    /// </summary>
    public const string SafeArguments = nameof(SafeArguments);

    /// <summary>
    /// The key for storing the authentication token in the invocation context.
    /// </summary>
    public const string AuthenticationToken = nameof(AuthenticationToken);

    /// <summary>
    /// The key for storing the authenticated ClaimsPrincipal in the invocation context.
    /// </summary>
    public const string Principal = nameof(Principal);
}
