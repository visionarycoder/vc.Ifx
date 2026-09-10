namespace VisionaryCoder.Framework.Secrets;

/// <summary>
/// A null implementation of ISecretProvider that returns null for all requests.
/// </summary>
public sealed class NullSecretProvider : ISecretProvider
{

    /// <summary>
    /// Gets the singleton instance of the NullSecretProvider.
    /// </summary>
    public static NullSecretProvider Instance { get; } = new();

    private NullSecretProvider() { }

    /// Always returns null.
    public Task<string?> GetAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);

}
