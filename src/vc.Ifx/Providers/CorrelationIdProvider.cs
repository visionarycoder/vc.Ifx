namespace VisionaryCoder.Framework.Providers;

/// <summary>
/// Default implementation of <see cref="ICorrelationIdProvider"/>.
/// </summary>
/// <remarks>
/// Provides a lightweight, per-async-context correlation identifier. The value is stored
/// in an <see cref="System.Threading.AsyncLocal{T}"/> so it flows with async/await and
/// preserves logical operation context without leaking between logical flows.
/// </remarks>
public sealed class CorrelationIdProvider : ICorrelationIdProvider
{
    /// <summary>
    /// Stores the current correlation id in the ambient async context.
    /// </summary>
    private static readonly AsyncLocal<string?> currentCorrelationId = new();

    /// <inheritdoc />
    /// <remarks>
    /// If no correlation id has been set on the current context, a new id will be generated
    /// and returned. Generated ids are 12-character uppercase strings derived from a GUID.
    /// </remarks>
    public string CorrelationId => currentCorrelationId.Value ?? GenerateNew();

    /// <summary>
    /// Generates and sets a new correlation id for the current async context.
    /// </summary>
    /// <returns>The newly generated correlation id.</returns>
    public string GenerateNew()
    {
        string newId = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        currentCorrelationId.Value = newId;
        return newId;
    }

    /// <summary>
    /// Explicitly sets the correlation id for the current async context.
    /// </summary>
    /// <param name="correlationId">The correlation id to set. Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is null or whitespace.</exception>
    public void SetCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation ID cannot be null or whitespace.", nameof(correlationId));
        }
        currentCorrelationId.Value = correlationId;
    }
}
