namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Idempotency;

/// <summary>
/// Defines a contract for storing and retrieving idempotent invocation results.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Checks whether a result already exists for the given idempotency key.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <returns>True if a result exists; otherwise false.</returns>
    ValueTask<bool> ExistsAsync(string key);

    /// <summary>
    /// Retrieves the cached result for the given idempotency key.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <returns>The cached result if it exists; otherwise null.</returns>
    ValueTask<object?> GetAsync(string key);

    /// <summary>
    /// Saves the result of an idempotent invocation for future lookups.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="result">The result to cache.</param>
    ValueTask SaveAsync(string key, object? result);
}
