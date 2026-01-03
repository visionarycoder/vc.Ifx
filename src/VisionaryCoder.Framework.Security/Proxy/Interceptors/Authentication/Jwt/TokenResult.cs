// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text.Json;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

/// <summary>
/// Represents the result of a JWT token request, containing the token details and metadata.
/// Provides comprehensive information about token acquisition including expiration, refresh capabilities, and error handling.
/// </summary>
public class TokenResult
{
    /// <summary>
    /// Gets or sets the access token string.
    /// This is the JWT token that can be used for API authentication.
    /// </summary>
    /// <value>The access token. Defaults to an empty string.</value>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token type, typically "Bearer" for JWT tokens.
    /// This indicates how the token should be used in authorization headers.
    /// </summary>
    /// <value>The token type. Defaults to "Bearer".</value>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the token lifetime in seconds.
    /// This indicates how long the token remains valid from the time of issuance.
    /// </summary>
    /// <value>The token lifetime in seconds. Defaults to 0.</value>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the refresh token for obtaining new access tokens.
    /// This token can be used to get new access tokens without re-authentication.
    /// </summary>
    /// <value>The refresh token, or null if not provided.</value>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the scope granted to the token.
    /// This may differ from the requested scope if some scopes were not granted.
    /// </summary>
    /// <value>The granted scope, or null if not specified.</value>
    public string? Scope { get; set; }

    /// <summary>
    /// Gets whether the token has expired based on the current time.
    /// </summary>
    /// <value>True if the token is expired; otherwise, false.</value>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiryTime;

    /// <summary>
    /// Gets or sets the absolute expiration time for the token.
    /// This is calculated from the current time plus ExpiresIn seconds.
    /// </summary>
    /// <value>The expiration time. Defaults to the current UTC time.</value>
    public DateTimeOffset ExpiryTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets whether the token request was successful.
    /// When false, check the Error property for details about the failure.
    /// </summary>
    /// <value>True if successful; otherwise, false. Defaults to true.</value>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the correlation ID for tracing the token request.
    /// This can be used to correlate logs and trace the token acquisition process.
    /// </summary>
    /// <value>The correlation ID, or null if not provided.</value>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the error message if the request failed.
    /// This provides details about why the token request was unsuccessful.
    /// </summary>
    /// <value>The error message, or null if the request was successful.</value>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets additional error details or error codes.
    /// Provides more specific information about the nature of the error.
    /// </summary>
    /// <value>Additional error information, or null if not applicable.</value>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Gets or sets the URI for more information about the error.
    /// This can point to documentation or help pages related to the error.
    /// </summary>
    /// <value>The error URI, or null if not provided.</value>
    public string? ErrorUri { get; set; }

    /// <summary>
    /// Gets the time remaining until token expiration.
    /// </summary>
    /// <value>The remaining time, or TimeSpan.Zero if expired.</value>
    public TimeSpan TimeUntilExpiry => IsExpired ? TimeSpan.Zero : ExpiryTime - DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets whether the token is close to expiration (within the specified threshold).
    /// </summary>
    /// <param name="threshold">The time threshold to consider as "close to expiry". Defaults to 5 minutes.</param>
    /// <returns>True if the token expires within the threshold; otherwise, false.</returns>
    public bool IsCloseToExpiry(TimeSpan? threshold = null)
    {
        TimeSpan thresholdTime = threshold ?? TimeSpan.FromMinutes(5);
        return TimeUntilExpiry <= thresholdTime;
    }

    /// <summary>
    /// Sets the expiry time based on the ExpiresIn value and current time.
    /// Call this method after setting ExpiresIn to update the ExpiryTime property.
    /// </summary>
    public void UpdateExpiryTime()
    {
        if (ExpiresIn > 0)
        {
            ExpiryTime = DateTimeOffset.UtcNow.AddSeconds(ExpiresIn);
        }
    }

    /// <summary>
    /// Creates a successful token result with the specified access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="expiresIn">The token lifetime in seconds.</param>
    /// <param name="refreshToken">The optional refresh token.</param>
    /// <param name="scope">The optional granted scope.</param>
    /// <returns>A successful TokenResult instance.</returns>
    public static TokenResult Success(string accessToken, int expiresIn, string? refreshToken = null, string? scope = null)
    {
        var result = new TokenResult
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            Scope = scope,
            IsSuccess = true
        };
        result.UpdateExpiryTime();
        return result;
    }

    /// <summary>
    /// Creates a failed token result with the specified error information.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="errorDescription">Optional detailed error description.</param>
    /// <param name="errorUri">Optional URI for error documentation.</param>
    /// <returns>A failed TokenResult instance.</returns>
    public static TokenResult Failure(string error, string? errorDescription = null, string? errorUri = null)
    {
        return new TokenResult
        {
            IsSuccess = false,
            Error = error,
            ErrorDescription = errorDescription,
            ErrorUri = errorUri
        };
    }

    /// <summary>
    /// Returns a string representation of the token result for logging purposes.
    /// Excludes sensitive information like the actual token value.
    /// </summary>
    /// <returns>A string representation of the token result.</returns>
    public override string ToString()
    {
        if (!IsSuccess)
        {
            return $"TokenResult: Failed - {Error}";
        }

        return $"TokenResult: Success - Type: {TokenType}, ExpiresIn: {ExpiresIn}s, HasRefreshToken: {RefreshToken != null}";
    }

    /// <summary>
    /// Converts the token result to a JSON representation for serialization.
    /// Excludes sensitive information like tokens and secrets.
    /// </summary>
    /// <returns>A JSON string representation of the non-sensitive token result data.</returns>
    public string ToJson()
    {
        var safeData = new
        {
            IsSuccess,
            TokenType,
            ExpiresIn,
            ExpiryTime,
            HasAccessToken = !string.IsNullOrEmpty(AccessToken),
            HasRefreshToken = !string.IsNullOrEmpty(RefreshToken),
            Scope,
            CorrelationId,
            Error,
            ErrorDescription,
            ErrorUri,
            IsExpired,
            TimeUntilExpiry = TimeUntilExpiry.ToString()
        };

        return JsonSerializer.Serialize(safeData, new JsonSerializerOptions { WriteIndented = true });
    }
}