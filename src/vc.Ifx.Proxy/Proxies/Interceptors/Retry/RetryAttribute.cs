namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Retry;

/// <summary>
/// Attribute that specifies retry configuration for method invocation.
/// Can be applied to interface or method levels.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class RetryAttribute(int maxAttempts = 3, int delayMilliseconds = 200) : Attribute
{
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    public int MaxAttempts { get; } = maxAttempts;

    /// <summary>
    /// Gets the delay between retry attempts.
    /// </summary>
    public TimeSpan Delay { get; } = TimeSpan.FromMilliseconds(delayMilliseconds);
}
