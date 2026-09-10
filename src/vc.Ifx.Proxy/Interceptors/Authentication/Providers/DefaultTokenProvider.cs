// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
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

        if (options.RequestTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "Request timeout must be positive.");
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
        cancellationToken.ThrowIfCancellationRequested();

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
            httpRequest.Headers.UserAgent.ParseAdd("VisionaryCoder.Framework.Authentication/1.0");
            httpRequest.Headers.Accept.ParseAdd("application/json");

            using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, timeoutCts.Token);
            string responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (response.IsSuccessStatusCode)
            {
                TokenResponse? tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
                if (tokenResponse != null && !string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                {
                    TokenResult result = MapToTokenResult(tokenResponse);
                    logger.LogDebug("Successfully acquired JWT token. Expires in {ExpiresIn}s", result.ExpiresIn);
                    return result;
                }
            }

            logger.LogWarning("Token request failed with status {StatusCode}", response.StatusCode);

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
    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ValidateToken(token));
    }

    /// <summary>
    /// Validates a JWT token synchronously for scenarios where async is not needed.
    /// Performs signature, issuer, audience, and lifetime validation using configured symmetric keys.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            tokenHandler.ValidateToken(token, GetValidationParameters(), out _);
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
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        logger.LogDebug("Refreshing JWT token");
        var refreshRequest = new TokenRequest
        {
            GrantType = "refresh_token",
            ClientId = options.ClientId,
            ClientSecret = options.ClientSecret,
            CustomParameters = { ["refresh_token"] = refreshToken }
        };
        return await GetTokenAsync(refreshRequest, cancellationToken).ConfigureAwait(false);
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
            claims["iss"] = jsonToken.Issuer;

            logger.LogDebug("Extracted {ClaimCount} claims from JWT token", claims.Count);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to extract claims from JWT token");
        }

        return claims;
    }

    /// <summary>
    /// Gets the token endpoint URL for OAuth 2.0 requests.
    /// </summary>
    /// <returns>The token endpoint URL.</returns>
    private string GetTokenEndpoint()
    {
        string endpoint = string.IsNullOrEmpty(options.TokenEndpoint)
            ? $"{options.Authority.TrimEnd('/')}/token"
            : options.TokenEndpoint;
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && (options.RequireHttpsMetadata || uri.Scheme != Uri.UriSchemeHttp)))
            throw new InvalidOperationException("A valid HTTPS token endpoint is required unless HTTP is explicitly enabled.");
        return endpoint;
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
                // IsValid already requires both fields for this grant.
                data["username"] = request.Username!;
                data["password"] = request.Password!;
                break;

            case "authorization_code":
                data["code"] = request.AuthorizationCode!;
                data["redirect_uri"] = request.RedirectUri!;
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
            response.AccessToken!,
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
    /// <returns>Token validation parameters.</returns>
    private TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = options.ValidateIssuer,
            ValidIssuer = options.Issuer,
            ValidateAudience = options.ValidateAudience,
            ValidAudience = options.Audience,
            ValidateLifetime = options.ValidateLifetime,
            ValidateIssuerSigningKey = options.ValidateIssuerSigningKey,
            RequireSignedTokens = true,
            IssuerSigningKey = string.IsNullOrEmpty(options.SigningKey) ? null : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256, SecurityAlgorithms.HmacSha384, SecurityAlgorithms.HmacSha512],
            ClockSkew = options.ClockSkew
        };
    }

    /// <summary>
    /// Represents a token response from an OAuth 2.0 token endpoint.
    /// </summary>
    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    /// <summary>
    /// Represents an error response from an OAuth 2.0 token endpoint.
    /// </summary>
    private class TokenErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
        [JsonPropertyName("error_uri")]
        public string? ErrorUri { get; set; }
    }
}
