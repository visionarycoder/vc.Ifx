namespace VisionaryCoder.Framework.Providers;
/// <summary>
/// Default implementation of <see cref="IRequestIdProvider"/>.
/// </summary>
/// <remarks>
/// Provides a per-logical-operation request identifier that flows with async/await.
/// The identifier is stored in an <see cref="System.Threading.AsyncLocal{T}"/>
/// so it does not leak between unrelated logical execution contexts.
/// Generated request IDs are 8-character uppercase strings derived from a GUID.
/// </remarks>
public sealed class RequestIdProvider : IRequestIdProvider
{
    /// <summary>
    /// Stores the current request id in the ambient async context.
    /// </summary>
    private static readonly AsyncLocal<string> currentRequestId = new();

    /// <inheritdoc />
    /// <remarks>
    /// If no request id has been set on the current context, a new id will be generated
    /// and returned by <see cref="GenerateNew"/>.
    /// </remarks>
    public string RequestId => currentRequestId.Value ?? GenerateNew();
    
    /// <summary>
    /// Generates and sets a new request id for the current async context.
    /// </summary>
    /// <returns>The newly generated request id.</returns>
    public string GenerateNew()
    {
        string newId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        currentRequestId.Value = newId;
        return newId;
    }
    
    /// <summary>
    /// Explicitly sets the request id for the current async context.
    /// </summary>
    /// <param name="requestId">The request id to set. Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requestId"/> is null or whitespace.</exception>
    public void SetRequestId(string requestId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        currentRequestId.Value = requestId;
    }
}
