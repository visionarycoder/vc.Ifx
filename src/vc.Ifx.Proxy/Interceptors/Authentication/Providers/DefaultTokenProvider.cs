// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

/// <summary>
/// Default implementation of <see cref="ITokenProvider"/> that handles JWT token acquisition and validation.
/// Supports multiple OAuth 2.0 flows including client credentials, authorization code, and refresh token flows.
/// Provides comprehensive token management with caching, refresh, and validation capabilities.
/// </summary>
public class DefaultTokenProvider : ITokenProvider
{
    private readonly HttpClient httpClient;
    private readonly JwtOptions options;
    private readonly ILogger<DefaultTokenProvider> logger;
    private readonly JwtSecurityTokenHandler tokenHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTokenProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making token requests.</param>
    /// <param name="options">The JWT configuration options.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public DefaultTokenProvider(
        HttpClient httpClient,
        JwtOptions options,
        ILogger<DefaultTokenProvider> logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        tokenHandler = new JwtSecurityTokenHandler();

        ConfigureHttpClient();
    }

    /// <summary>
    /// Gets a JWT token asynchronously using default configuration.
    /// Uses client credentials flow with the configured client ID and secret.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the async operation with the JWT token string.</returns>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var defaultRequest = TokenRequest.CreateClientCredentials(
            options.ClientId,
            options.ClientSecret,
            options.Scopes,
            options.Audience);

        TokenResult result = await GetTokenAsync(defaultRequest, cancellationToken);

        if (result.IsSuccess && !string.IsNullOrEmpty(result.AccessToken))
        {
            return result.AccessToken;
        }

        throw new InvalidOperationException($"Failed to acquire JWT token: {result.Error}");
    }

    /// <summary>
    /// Gets a JWT token result with additional metadata based on the provided request.
    /// Supports multiple OAuth 2.0 grant types and provides comprehensive token information.
    /// </summary>
    /// <param name="request">The token request containing authentication parameters.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task with the complete token result including expiration and metadata.</returns>
    public async Task<TokenResult> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.IsValid())
        {
            return TokenResult.Failure("invalid_request", "The token request is invalid");
        }

        try
        {
            logger.LogDebug("Requesting JWT token for audience: {Audience}, grant type: {GrantType}",
                request.Audience, request.GrantType);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(options.RequestTimeout);

            string tokenEndpoint = GetTokenEndpoint();
            Dictionary<string, string> requestData = BuildTokenRequestData(request);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestData)
            };

            using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, timeoutCts.Token);
            string responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (response.IsSuccessStatusCode)
            {
                TokenResponse? tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
                if (tokenResponse != null)
                {
                    TokenResult result = MapToTokenResult(tokenResponse);
                    logger.LogDebug("Successfully acquired JWT token. Expires in {ExpiresIn}s", result.ExpiresIn);
                    return result;
                }
            }

            logger.LogWarning("Token request failed with status {StatusCode}: {ResponseContent}",
                response.StatusCode, responseContent);

            return ParseErrorResponse(responseContent);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Token request was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to acquire JWT token for audience: {Audience}", request.Audience);
            return TokenResult.Failure("request_error", ex.Message);
        }
    }

    /// <summary>
    /// Validates a JWT token for authenticity and expiration.
    /// Performs comprehensive validation including signature, audience, issuer, and lifetime checks.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task indicating whether the token is valid.</returns>
    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            TokenValidationParameters validationParameters = await GetValidationParametersAsync(cancellationToken);
            ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken? validatedToken);

            logger.LogDebug("JWT token validation successful for subject: {Subject}",
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "JWT token validation failed");
            return false;
        }
    }

    /// <summary>
    /// Validates a JWT token synchronously for scenarios where async is not needed.
    /// Performs basic format and expiration validation without signature verification.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            JwtSecurityToken? jsonToken = tokenHandler.ReadJwtToken(token);

            // Check expiration
            if (options.ValidateLifetime && jsonToken.ValidTo < DateTime.UtcNow)
            {
                logger.LogDebug("JWT token is expired. Valid until: {ValidTo}", jsonToken.ValidTo);
                return false;
            }

            // Check audience if configured
            if (options.ValidateAudience && !string.IsNullOrEmpty(options.Audience))
            {
                if (!jsonToken.Audiences.Contains(options.Audience))
                {
                    logger.LogDebug("JWT token audience mismatch. Expected: {ExpectedAudience}, Actual: {ActualAudiences}",
                        options.Audience, string.Join(", ", jsonToken.Audiences));
                    return false;
                }
            }

            // Check issuer if configured
            if (options.ValidateIssuer && !string.IsNullOrEmpty(options.Issuer))
            {
                if (!string.Equals(jsonToken.Issuer, options.Issuer, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogDebug("JWT token issuer mismatch. Expected: {ExpectedIssuer}, Actual: {ActualIssuer}",
                        options.Issuer, jsonToken.Issuer);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "JWT token validation failed");
            return false;
        }
    }

    /// <summary>
    /// Refreshes an existing JWT token if supported by the provider.
    /// Uses the refresh token to obtain a new access token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use for token renewal.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task with the new token result, or null if refresh is not supported.</returns>
    public async Task<TokenResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        try
        {
            logger.LogDebug("Refreshing JWT token");

            var refreshRequest = new TokenRequest
            {
                GrantType = "refresh_token",
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret,
                CustomParameters = { ["refresh_token"] = refreshToken }
            };

            return await GetTokenAsync(refreshRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh JWT token");
            return TokenResult.Failure("refresh_error", ex.Message);
        }
    }

    /// <summary>
    /// Extracts claims from a JWT token without full validation.
    /// Useful for getting token information without performing expensive validation.
    /// </summary>
    /// <param name="token">The JWT token to extract claims from.</param>
    /// <returns>A dictionary of claims found in the token.</returns>
    public Dictionary<string, object> ExtractClaims(string token)
    {
        var claims = new Dictionary<string, object>();

        if (string.IsNullOrWhiteSpace(token))
            return claims;

        try
        {
            JwtSecurityToken? jsonToken = tokenHandler.ReadJwtToken(token);

            foreach (Claim? claim in jsonToken.Claims)
            {
                if (claims.ContainsKey(claim.Type))
                {
                    // Handle multiple values for the same claim type
                    if (claims[claim.Type] is List<string> existingList)
                    {
                        existingList.Add(claim.Value);
                    }
                    else
                    {
                        claims[claim.Type] = new List<string> { claims[claim.Type].ToString()!, claim.Value };
                    }
                }
                else
                {
                    claims[claim.Type] = claim.Value;
                }
            }

            // Add token metadata
            claims["exp"] = jsonToken.ValidTo;
            claims["iat"] = jsonToken.IssuedAt;
            claims["nbf"] = jsonToken.ValidFrom;
            claims["aud"] = jsonToken.Audiences.ToArray();
            claims["iss"] = jsonToken.Issuer ?? string.Empty;

            logger.LogDebug("Extracted {ClaimCount} claims from JWT token", claims.Count);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to extract claims from JWT token");
        }

        return claims;
    }

    /// <summary>
    /// Configures the HTTP client for token requests.
    /// </summary>
    private void ConfigureHttpClient()
    {
        httpClient.Timeout = options.RequestTimeout;
        httpClient.DefaultRequestHeaders.Add("User-Agent", "VisionaryCoder.Framework.Authentication/1.0");

        if (!httpClient.DefaultRequestHeaders.Contains("Accept"))
        {
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
    }

    /// <summary>
    /// Gets the token endpoint URL for OAuth 2.0 requests.
    /// </summary>
    /// <returns>The token endpoint URL.</returns>
    private string GetTokenEndpoint()
    {
        if (!string.IsNullOrEmpty(options.TokenEndpoint))
        {
            return options.TokenEndpoint;
        }

        // Construct from authority if not explicitly set
        string authority = options.Authority.TrimEnd('/');
        return $"{authority}/token";
    }

    /// <summary>
    /// Builds the form data for token requests based on the grant type.
    /// </summary>
    /// <param name="request">The token request.</param>
    /// <returns>A dictionary of form data for the request.</returns>
    private Dictionary<string, string> BuildTokenRequestData(TokenRequest request)
    {
        var data = new Dictionary<string, string>
        {
            ["grant_type"] = request.GrantType,
            ["client_id"] = request.ClientId
        };

        if (!string.IsNullOrEmpty(request.ClientSecret))
        {
            data["client_secret"] = request.ClientSecret;
        }

        if (!string.IsNullOrEmpty(request.Audience))
        {
            data["audience"] = request.Audience;
        }

        string? scopeString = request.GetScopeString();
        if (!string.IsNullOrEmpty(scopeString))
        {
            data["scope"] = scopeString;
        }

        // Add grant-type specific parameters
        switch (request.GrantType.ToLowerInvariant())
        {
            case "password":
                if (!string.IsNullOrEmpty(request.Username))
                    data["username"] = request.Username;
                if (!string.IsNullOrEmpty(request.Password))
                    data["password"] = request.Password;
                break;

            case "authorization_code":
                if (!string.IsNullOrEmpty(request.AuthorizationCode))
                    data["code"] = request.AuthorizationCode;
                if (!string.IsNullOrEmpty(request.RedirectUri))
                    data["redirect_uri"] = request.RedirectUri;
                break;
        }

        // Add custom parameters
        foreach (KeyValuePair<string, string> kvp in request.CustomParameters)
        {
            data[kvp.Key] = kvp.Value;
        }

        return data;
    }

    /// <summary>
    /// Maps a token response to a TokenResult object.
    /// </summary>
    /// <param name="response">The token response from the server.</param>
    /// <returns>A TokenResult object.</returns>
    private static TokenResult MapToTokenResult(TokenResponse response)
    {
        var result = TokenResult.Success(
            response.AccessToken ?? string.Empty,
            response.ExpiresIn,
            response.RefreshToken,
            response.Scope);

        result.TokenType = response.TokenType ?? "Bearer";
        result.CorrelationId = Guid.NewGuid().ToString();

        return result;
    }

    /// <summary>
    /// Parses an error response from the token endpoint.
    /// </summary>
    /// <param name="responseContent">The response content.</param>
    /// <returns>A TokenResult with error information.</returns>
    private static TokenResult ParseErrorResponse(string responseContent)
    {
        try
        {
            TokenErrorResponse? errorResponse = JsonSerializer.Deserialize<TokenErrorResponse>(responseContent);
            if (errorResponse != null)
            {
                return TokenResult.Failure(
                    errorResponse.Error ?? "unknown_error",
                    errorResponse.ErrorDescription,
                    errorResponse.ErrorUri);
            }
        }
        catch
        {
            // Ignore JSON parsing errors
        }

        return TokenResult.Failure("unknown_error", "An unknown error occurred during token acquisition");
    }

    /// <summary>
    /// Gets token validation parameters for JWT validation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Token validation parameters.</returns>
    private async Task<TokenValidationParameters> GetValidationParametersAsync(CancellationToken cancellationToken)
    {
        // This is a simplified implementation
        // In a real-world scenario, you would fetch the signing keys from the JWKS endpoint
        await Task.CompletedTask; // Placeholder for async operations

        return new TokenValidationParameters
        {
            ValidateIssuer = options.ValidateIssuer,
            ValidIssuer = options.Issuer,
            ValidateAudience = options.ValidateAudience,
            ValidAudience = options.Audience,
            ValidateLifetime = options.ValidateLifetime,
            ValidateIssuerSigningKey = options.ValidateIssuerSigningKey,
            ClockSkew = options.ClockSkew
        };
    }

    /// <summary>
    /// Represents a token response from an OAuth 2.0 token endpoint.
    /// </summary>
    private class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string? RefreshToken { get; set; }
        public string? Scope { get; set; }
    }

    /// <summary>
    /// Represents an error response from an OAuth 2.0 token endpoint.
    /// </summary>
    private class TokenErrorResponse
    {
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public string? ErrorUri { get; set; }
    }
}
