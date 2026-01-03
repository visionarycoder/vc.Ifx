using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;
/// <summary>
/// JWT interceptor for web-based authentication scenarios.
/// Handles OAuth flows and web-specific JWT token management.
/// </summary>
public class WebJwtInterceptor : IProxyInterceptor
{
    private readonly ITokenProvider tokenProvider;
    private readonly ILogger<WebJwtInterceptor> logger;
    private readonly WebJwtOptions options;
    /// <summary>
    /// Initializes a new instance of the <see cref="WebJwtInterceptor"/> class.
    /// </summary>
    /// <param name="tokenProvider">The token provider for web authentication.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="options">The configuration options for web JWT handling.</param>
    public WebJwtInterceptor(
        ITokenProvider tokenProvider,
        ILogger<WebJwtInterceptor> logger,
        WebJwtOptions options)
    {
        this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }
    /// Intercepts the request and adds web-based JWT authentication.
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="context">The proxy context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Retrieving JWT token for audience: {Audience}", options.Audience);

            TokenResult tokenResult = await tokenProvider.GetTokenAsync(new TokenRequest
            {
                Audience = options.Audience,
                Scopes = options.Scopes,
                RefreshIfExpired = options.RefreshIfExpired
            });
            if (tokenResult.IsSuccess && !string.IsNullOrEmpty(tokenResult.AccessToken))
            {
                context.Headers[options.HeaderName] = $"Bearer {tokenResult.AccessToken}";
                logger.LogDebug("JWT token added to {HeaderName} header", options.HeaderName);

                // Add correlation ID if available
                if (!string.IsNullOrEmpty(tokenResult.CorrelationId))
                {
                    context.Headers["X-Correlation-ID"] = tokenResult.CorrelationId;
                }
            }
            else
            {
                logger.LogWarning("Failed to obtain JWT token: {Error}", tokenResult.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve web JWT token for audience: {Audience}", options.Audience);
        }
        return await next(context, cancellationToken);
    }
}
