namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Timeout;

/// <summary>
/// Attribute that specifies a timeout duration for method invocation.
/// Can be applied to interface or method levels.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class TimeoutAttribute(int milliseconds) : Attribute
{
    /// <summary>
    /// Gets the timeout duration for the invocation.
    /// </summary>
    public TimeSpan Timeout { get; } = TimeSpan.FromMilliseconds(milliseconds);
}
