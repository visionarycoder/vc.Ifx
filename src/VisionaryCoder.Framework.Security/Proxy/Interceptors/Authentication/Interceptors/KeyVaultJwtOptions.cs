namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;

/// <summary>
/// Configuration options for Key Vault JWT interceptor.
/// Provides comprehensive settings for retrieving and using JWT tokens from Azure Key Vault.
/// </summary>
public class KeyVaultJwtOptions
{
    /// <summary>
    /// Gets or sets the name of the secret in Key Vault containing the JWT token.
    /// </summary>
    /// <value>The secret name. Defaults to an empty string.</value>
    public string SecretName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the secret in Key Vault containing the refresh token.
    /// Used for automatic token refresh when enabled.
    /// </summary>
    /// <value>The refresh token secret name. Defaults to null.</value>
    public string? RefreshSecretName { get; set; }

    /// <summary>
    /// Gets or sets the HTTP header name to add the JWT token to.
    /// </summary>
    /// <value>The header name. Defaults to "Authorization".</value>
    public string HeaderName { get; set; } = "Authorization";

    /// <summary>
    /// Gets or sets whether to validate the JWT token before using it.
    /// When true, tokens are checked for basic format and expiration.
    /// </summary>
    /// <value>True to validate tokens; otherwise, false. Defaults to true.</value>
    public bool ValidateToken { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to automatically refresh expired tokens.
    /// Requires RefreshSecretName to be configured.
    /// </summary>
    /// <value>True to auto-refresh tokens; otherwise, false. Defaults to false.</value>
    public bool AutoRefresh { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout duration for Key Vault operations.
    /// </summary>
    /// <value>The timeout duration. Defaults to 30 seconds.</value>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to fail the request if token acquisition fails.
    /// When false, the request continues without authentication.
    /// </summary>
    /// <value>True to fail on errors; otherwise, false. Defaults to false.</value>
    public bool FailOnError { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to fail the request if token acquisition times out.
    /// </summary>
    /// <value>True to fail on timeout; otherwise, false. Defaults to false.</value>
    public bool FailOnTimeout { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to fail the request if the token is missing from Key Vault.
    /// </summary>
    /// <value>True to fail on missing token; otherwise, false. Defaults to false.</value>
    public bool FailOnMissingToken { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include metadata headers in the request.
    /// </summary>
    /// <value>True to include metadata; otherwise, false. Defaults to false.</value>
    public bool IncludeMetadata { get; set; } = false;

    /// <summary>
    /// Gets or sets the correlation ID for request tracing.
    /// </summary>
    /// <value>The correlation ID. Defaults to null.</value>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Validates the Key Vault JWT options configuration.
    /// </summary>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(SecretName))
            return false;

        if (string.IsNullOrWhiteSpace(HeaderName))
            return false;

        if (RequestTimeout <= TimeSpan.Zero)
            return false;

        return true;
    }
}