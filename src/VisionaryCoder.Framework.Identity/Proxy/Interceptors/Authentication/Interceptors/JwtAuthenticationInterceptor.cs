// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;

/// <summary>
/// JWT interceptor for web-based authentication scenarios.
/// Handles OAuth flows, token acquisition, and automatic token refresh for web applications.
/// This interceptor automatically adds JWT Bearer tokens to outgoing requests.
/// </summary>
public class JwtAuthenticationInterceptor : IProxyInterceptor
{
    private readonly ITokenProvider tokenProvider;
    private readonly ILogger<JwtAuthenticationInterceptor> logger;
    private readonly JwtOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtAuthenticationInterceptor"/> class.
    /// </summary>
    /// <param name="tokenProvider">The token provider for JWT authentication.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="options">The configuration options for JWT handling.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public JwtAuthenticationInterceptor(
        ITokenProvider tokenProvider,
        ILogger<JwtAuthenticationInterceptor> logger,
        JwtOptions options)
    {
        this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.options = options ?? throw new ArgumentNullException(nameof(options));

        if (!options.IsValid())
        {
            throw new ArgumentException("JWT options configuration is invalid", nameof(options));
        }
    }

    /// <summary>
    /// Intercepts the request and adds JWT Bearer authentication header.
    /// Automatically handles token acquisition, refresh, and error scenarios.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="context">The proxy context containing request information.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the proxy response.</returns>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Acquiring JWT token for audience: {Audience}", options.Audience);

            // Create token request based on configuration
            var tokenRequest = new TokenRequest
            {
                Audience = options.Audience,
                Scopes = options.Scopes,
                RefreshIfExpired = options.RefreshIfExpired,
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret
            };

            // Validate token request
            if (!tokenRequest.IsValid())
            {
                logger.LogWarning("Invalid token request configuration for audience: {Audience}", options.Audience);
                return await next(context, cancellationToken);
            }

            // Acquire token with timeout handling
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(options.RequestTimeout);

            TokenResult tokenResult = await tokenProvider.GetTokenAsync(tokenRequest, timeoutCts.Token);

            if (tokenResult.IsSuccess && !string.IsNullOrEmpty(tokenResult.AccessToken))
            {
                // Add Authorization header
                context.Headers[options.HeaderName] = $"{tokenResult.TokenType} {tokenResult.AccessToken}";
                logger.LogDebug("JWT token added to {HeaderName} header. Expires in {ExpiresIn}s",
                    options.HeaderName, tokenResult.ExpiresIn);

                // Add correlation ID for request tracing
                if (!string.IsNullOrEmpty(tokenResult.CorrelationId))
                {
                    context.Headers["X-Correlation-ID"] = tokenResult.CorrelationId;
                    logger.LogDebug("Correlation ID added: {CorrelationId}", tokenResult.CorrelationId);
                }

                // Add additional metadata headers if needed
                if (!string.IsNullOrEmpty(tokenResult.Scope))
                {
                    context.Headers["X-Token-Scope"] = tokenResult.Scope;
                }
            }
            else
            {
                logger.LogWarning("Failed to obtain JWT token for audience {Audience}: {Error} - {ErrorDescription}",
                    options.Audience, tokenResult.Error, tokenResult.ErrorDescription);

                // Optionally fail the request based on configuration
                if (options.CustomProperties.TryGetValue("FailOnTokenError", out object? failOnError) &&
                    failOnError is bool fail && fail)
                {
                    throw new InvalidOperationException($"JWT token acquisition failed: {tokenResult.Error}");
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("JWT token acquisition was cancelled for audience: {Audience}", options.Audience);
            throw;
        }
        catch (TimeoutException)
        {
            logger.LogError("JWT token acquisition timed out after {Timeout} for audience: {Audience}",
                options.RequestTimeout, options.Audience);

            // Continue with request without token based on configuration
            if (options.CustomProperties.TryGetValue("FailOnTimeout", out object? failOnTimeout) &&
                failOnTimeout is bool fail && fail)
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during JWT token acquisition for audience: {Audience}", options.Audience);

            // Continue with request without token to avoid breaking the flow
            // unless configured to fail on errors
            if (options.CustomProperties.TryGetValue("FailOnError", out object? failOnError) &&
                failOnError is bool fail && fail)
            {
                throw;
            }
        }

        return await next(context, cancellationToken);
    }

    /// <summary>
    /// Pre-validates the token to ensure it's not expired before making the request.
    /// This can help avoid making requests with expired tokens.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid and not expired; otherwise, false.</returns>
    protected virtual bool IsTokenValid(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            return tokenProvider.ValidateToken(token);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Token validation failed during pre-check");
            return false;
        }
    }

    /// <summary>
    /// Determines if a token refresh should be attempted based on the current token state.
    /// </summary>
    /// <param name="tokenResult">The current token result.</param>
    /// <returns>True if a refresh should be attempted; otherwise, false.</returns>
    protected virtual bool ShouldRefreshToken(TokenResult tokenResult)
    {
        if (!options.RefreshIfExpired || string.IsNullOrEmpty(tokenResult.RefreshToken))
            return false;

        // Refresh if token is expired or close to expiry
        return tokenResult.IsExpired || tokenResult.IsCloseToExpiry(TimeSpan.FromMinutes(2));
    }
}
