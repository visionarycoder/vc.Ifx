namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

/// <summary>
/// Represents an argument passed to an invoked method.
/// </summary>
/// <param name="Name">The name of the parameter.</param>
/// <param name="ParameterType">The type of the parameter.</param>
/// <param name="Value">The value passed to the parameter.</param>
public sealed record MethodArgument(string Name, Type ParameterType, object? Value);
