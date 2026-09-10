using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Idempotency;

/// <summary>
/// Defines a contract for resolving idempotency keys from invocation context.
/// </summary>
public interface IIdempotencyKeyResolver
{
    /// <summary>
    /// Resolves an idempotency key from the given invocation context.
    /// The key uniquely identifies the invocation and is used to detect duplicate calls.
    /// </summary>
    /// <param name="context">The invocation context.</param>
    /// <returns>A unique key identifying the invocation.</returns>
    string GetKey(MethodContext context);
}
