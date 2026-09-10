namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

/// <summary>
/// Represents a delegate that invokes the next handler in the interception pipeline.
/// Returns the result of the invocation as a nullable object wrapped in a ValueTask.
/// </summary>
public delegate ValueTask<object?> HandlerDelegate();
