namespace Ifx.Abstractions;

/// <summary>
/// Marks a strongly typed entity identifier and exposes its boxed primitive value
/// metadata through the shared <see cref="IPrimitiveValue"/> contract.
/// </summary>
public interface IEntityIdentifier : IPrimitiveValue
{
}
