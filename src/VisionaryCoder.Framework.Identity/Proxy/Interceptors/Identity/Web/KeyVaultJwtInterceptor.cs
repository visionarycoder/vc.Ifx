using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;
/// <summary>
/// JWT interceptor specialized for Key Vault authentication scenarios.
/// Retrieves JWT tokens from Azure Key Vault and adds them to request headers.
/// </summary>
public class KeyVaultJwtInterceptor : IProxyInterceptor
{
    private readonly ISecretProvider secretProvider;
    private readonly ILogger<KeyVaultJwtInterceptor> logger;
    private readonly string secretName;
    private readonly string headerName;
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyVaultJwtInterceptor"/> class.
    /// </summary>
    /// <param name="secretProvider">The secret provider for retrieving JWT tokens.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="secretName">The name of the secret in Key Vault containing the JWT token.</param>
    /// <param name="headerName">The header name to add the JWT token to. Defaults to "Authorization".</param>
    public KeyVaultJwtInterceptor(
        ISecretProvider secretProvider,
        ILogger<KeyVaultJwtInterceptor> logger,
        string secretName,
        string headerName = "Authorization")
    {
        this.secretProvider = secretProvider ?? throw new ArgumentNullException(nameof(secretProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.secretName = secretName ?? throw new ArgumentNullException(nameof(secretName));
        this.headerName = headerName ?? throw new ArgumentNullException(nameof(headerName));
    }
    /// Intercepts the proxy call to add JWT authentication from Key Vault.
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="context">The proxy context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Retrieving JWT token from Key Vault for secret: {SecretName}", secretName);

            string? jwtToken = await secretProvider.GetAsync(secretName, cancellationToken);
            if (!string.IsNullOrEmpty(jwtToken))
            {
                // Ensure the token has the Bearer prefix if it's for Authorization header
                string? tokenValue = headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase) &&
                                     !jwtToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? $"Bearer {jwtToken}"
                    : jwtToken;
                context.Headers[headerName] = tokenValue;
                logger.LogDebug("JWT token added to {HeaderName} header", headerName);
            }
            else
            {
                logger.LogWarning("JWT token not found or empty for secret: {SecretName}", secretName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve JWT token from Key Vault for secret: {SecretName}", secretName);
            // Continue without authentication - let downstream decide how to handle
        }
        return await next(context, cancellationToken);
    }
}
