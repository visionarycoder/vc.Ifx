namespace VisionaryCoder.Framework.Secrets;

/// <summary>
/// A null implementation of ISecretProvider that returns null for all requests.
/// </summary>
/// <remarks>
/// This stateless singleton performs no I/O, validation, or cancellation checks. It accepts
/// even null or empty names and already-canceled tokens for compatibility with optional
/// secrets consumers. Batch retrieval uses the default interface implementation and its
/// collection validation rules. Register this instance in the application's composition root.
/// </remarks>
public sealed class NullSecretProvider : ISecretProvider
{
    /// <summary>
    /// Gets the singleton instance of the NullSecretProvider.
    /// </summary>
    public static NullSecretProvider Instance { get; } = new();

    private NullSecretProvider() { }

    /// <summary>
    /// Returns a completed task containing <see langword="null"/>.
    /// </summary>
    /// <param name="name">An ignored secret name.</param>
    /// <param name="cancellationToken">An ignored cancellation token.</param>
    /// <returns>A completed task containing no secret value.</returns>
    public Task<string?> GetAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
}
