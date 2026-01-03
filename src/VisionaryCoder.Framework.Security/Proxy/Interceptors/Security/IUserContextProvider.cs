namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Interface for providing user context information.
/// </summary>
public interface IUserContextProvider
{
    /// <summary>
    /// Gets the current user context.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The current user context, or null if no user is authenticated.</returns>
    Task<UserContext?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
