namespace VisionaryCoder.Framework.Proxy.Attributes;

/// <summary>
/// Attribute to specify the execution order of proxy interceptors.
/// </summary>
/// <param name="order">The order value. Lower values execute first.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ProxyInterceptorOrderAttribute(int order) : Attribute
{
    /// <summary>
    /// Gets the order value for the interceptor.
    /// Lower values execute first.
    /// </summary>
    public int Order { get; } = order;
}
