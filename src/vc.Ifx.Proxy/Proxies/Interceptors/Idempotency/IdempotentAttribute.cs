namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Idempotency;

/// <summary>
/// Attribute that marks a method as idempotent.
/// Idempotent methods can be safely retried without side effects.
/// Can be applied to interface or method levels.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute;
