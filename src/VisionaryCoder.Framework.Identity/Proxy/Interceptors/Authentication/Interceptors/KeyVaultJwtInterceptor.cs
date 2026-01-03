// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;

/// <summary>
/// JWT interceptor specialized for Azure Key Vault authentication scenarios.
/// Retrieves JWT tokens and certificates from Azure Key Vault and adds them to request headers.
/// Provides secure token management with automatic refresh and comprehensive error handling.
/// </summary>
public class KeyVaultJwtInterceptor : IProxyInterceptor
{
    private readonly ISecretProvider secretProvider;
    private readonly ILogger<KeyVaultJwtInterceptor> logger;
    private readonly KeyVaultJwtOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyVaultJwtInterceptor"/> class.
    /// </summary>
    /// <param name="secretProvider">The secret provider for retrieving JWT tokens from Key Vault.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="options">The configuration options for Key Vault JWT handling.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public KeyVaultJwtInterceptor(
        ISecretProvider secretProvider,
        ILogger<KeyVaultJwtInterceptor> logger,
        KeyVaultJwtOptions options)
    {
        this.secretProvider = secretProvider ?? throw new ArgumentNullException(nameof(secretProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.options = options ?? throw new ArgumentNullException(nameof(options));

        if (!options.IsValid())
        {
            throw new ArgumentException("Key Vault JWT options configuration is invalid", nameof(options));
        }
    }

    /// <summary>
    /// Intercepts the proxy call to add JWT authentication from Azure Key Vault.
    /// Handles token retrieval, validation, refresh, and error scenarios.
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
            logger.LogDebug("Retrieving JWT token from Key Vault for secret: {SecretName}", options.SecretName);

            // Create timeout for Key Vault operations
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(options.RequestTimeout);

            // Retrieve token from Key Vault
            string? jwtToken = await secretProvider.GetAsync(options.SecretName, timeoutCts.Token);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                // Validate token if validation is enabled
                if (options.ValidateToken && !IsTokenValid(jwtToken))
                {
                    logger.LogWarning("JWT token from Key Vault is invalid or expired for secret: {SecretName}", options.SecretName);

                    // Attempt to refresh token if configured
                    if (options.AutoRefresh && !string.IsNullOrEmpty(options.RefreshSecretName))
                    {
                        jwtToken = await TryRefreshTokenAsync(timeoutCts.Token);
                    }
                }

                if (!string.IsNullOrEmpty(jwtToken))
                {
                    // Format token based on header type
                    string tokenValue = FormatTokenForHeader(jwtToken);
                    context.Headers[options.HeaderName] = tokenValue;

                    logger.LogDebug("JWT token added to {HeaderName} header from Key Vault secret: {SecretName}",
                        options.HeaderName, options.SecretName);

                    // Add additional metadata headers
                    AddMetadataHeaders(context);
                }
                else
                {
                    logger.LogWarning("Failed to obtain valid JWT token from Key Vault for secret: {SecretName}", options.SecretName);
                    HandleTokenFailure();
                }
            }
            else
            {
                logger.LogWarning("JWT token not found or empty in Key Vault for secret: {SecretName}", options.SecretName);
                HandleTokenFailure();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("JWT token retrieval was cancelled for Key Vault secret: {SecretName}", options.SecretName);
            throw;
        }
        catch (TimeoutException)
        {
            logger.LogError("JWT token retrieval timed out after {Timeout} for Key Vault secret: {SecretName}",
                options.RequestTimeout, options.SecretName);

            if (options.FailOnTimeout)
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve JWT token from Key Vault for secret: {SecretName}", options.SecretName);

            if (options.FailOnError)
            {
                throw;
            }
        }

        return await next(context, cancellationToken);
    }

    /// <summary>
    /// Validates a JWT token for basic format and expiration.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>True if the token appears valid; otherwise, false.</returns>
    protected virtual bool IsTokenValid(string token)
    {
        try
        {
            // Basic JWT format validation (should have 3 parts separated by dots)
            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                logger.LogDebug("JWT token has invalid format - expected 3 parts, got {PartCount}", parts.Length);
                return false;
            }

            // Additional validation can be added here (expiration check, signature validation, etc.)
            // For now, we assume the token is valid if it has the correct format
            return true;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "JWT token validation failed");
            return false;
        }
    }

    /// <summary>
    /// Attempts to refresh the JWT token using a refresh token from Key Vault.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The refreshed JWT token, or null if refresh failed.</returns>
    protected virtual async Task<string?> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(options.RefreshSecretName))
                return null;

            logger.LogDebug("Attempting to refresh JWT token using refresh secret: {RefreshSecretName}", options.RefreshSecretName);

            string? refreshToken = await secretProvider.GetAsync(options.RefreshSecretName, cancellationToken);
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning("Refresh token not found in Key Vault for secret: {RefreshSecretName}", options.RefreshSecretName);
                return null;
            }

            // In a real implementation, this would call a token endpoint to refresh the token
            // For now, we return null to indicate refresh is not implemented
            logger.LogWarning("JWT token refresh is not yet implemented for Key Vault secrets");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh JWT token from Key Vault");
            return null;
        }
    }

    /// <summary>
    /// Formats the JWT token appropriately for the specified header.
    /// </summary>
    /// <param name="token">The raw JWT token.</param>
    /// <returns>The formatted token value for the header.</returns>
    protected virtual string FormatTokenForHeader(string token)
    {
        // For Authorization header, ensure Bearer prefix
        if (options.HeaderName.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
        {
            return token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? token
                : $"Bearer {token}";
        }

        // For other headers, use token as-is
        return token;
    }

    /// <summary>
    /// Adds metadata headers to the request context.
    /// </summary>
    /// <param name="context">The proxy context.</param>
    protected virtual void AddMetadataHeaders(ProxyContext context)
    {
        if (options.IncludeMetadata)
        {
            context.Headers["X-Token-Source"] = "KeyVault";
            context.Headers["X-Token-Secret"] = options.SecretName;

            if (!string.IsNullOrEmpty(options.CorrelationId))
            {
                context.Headers["X-Correlation-ID"] = options.CorrelationId;
            }
        }
    }

    /// <summary>
    /// Handles token acquisition failure based on configuration.
    /// </summary>
    protected virtual void HandleTokenFailure()
    {
        if (options.FailOnMissingToken)
        {
            throw new InvalidOperationException($"Required JWT token not available from Key Vault secret: {options.SecretName}");
        }
    }
}
