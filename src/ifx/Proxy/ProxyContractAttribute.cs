namespace Ifx.Proxy;

/// <summary>Marks a typed asynchronous component contract for proxy signature validation.</summary>
[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
public sealed class ProxyContractAttribute : Attribute;
