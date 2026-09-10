// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

/// <summary>
/// Configuration options for JWT authentication interceptors and providers.
/// Provides comprehensive settings for JWT token acquisition, validation, and usage in authentication scenarios.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Gets or sets the JWT authority URL for token validation.
    /// This is the URL of the authorization server that issues tokens.
    /// </summary>
    /// <value>The authority URL. Defaults to an empty string.</value>
    [Required]
    [Url]
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended audience for JWT tokens.
    /// This identifies the recipients that the JWT is intended for.
    /// </summary>
    /// <value>The audience identifier. Defaults to an empty string.</value>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected issuer of JWT tokens.
    /// This should match the "iss" claim in received tokens.
    /// </summary>
    /// <value>The issuer identifier. Defaults to an empty string.</value>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signing key for JWT token validation.
    /// This can be a symmetric key or public key for asymmetric validation.
    /// </summary>
    /// <value>The signing key. Defaults to an empty string.</value>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether HTTPS metadata is required for token validation.
    /// When true, only HTTPS endpoints are allowed for metadata discovery.
    /// </summary>
    /// <value>True to require HTTPS metadata; otherwise, false. Defaults to true.</value>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets the scopes to request when acquiring JWT tokens.
    /// Scopes define the level of access that the application is requesting.
    /// </summary>
    /// <value>An array of scope strings. Defaults to an empty array.</value>
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to automatically refresh expired tokens.
    /// When true, the provider will attempt to refresh tokens using refresh tokens.
    /// </summary>
    /// <value>True to refresh expired tokens; otherwise, false. Defaults to true.</value>
    public bool RefreshIfExpired { get; set; } = true;

    /// <summary>
    /// Gets or sets the HTTP header name for JWT token transmission.
    /// Typically "Authorization" for Bearer tokens.
    /// </summary>
    /// <value>The header name. Defaults to "Authorization".</value>
    public string HeaderName { get; set; } = "Authorization";

    /// <summary>
    /// Gets or sets the client ID for OAuth 2.0 flows.
    /// This uniquely identifies the application to the authorization server.
    /// </summary>
    /// <value>The client ID. Defaults to an empty string.</value>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for OAuth 2.0 flows.
    /// This is a confidential credential used to authenticate the application.
    /// </summary>
    /// <value>The client secret. Defaults to an empty string.</value>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token endpoint URL for OAuth 2.0 flows.
    /// This is where token requests are sent.
    /// </summary>
    /// <value>The token endpoint URL. Defaults to an empty string.</value>
    [Url]
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout duration for token requests.
    /// This controls how long to wait for token acquisition operations.
    /// </summary>
    /// <value>The timeout duration. Defaults to 30 seconds.</value>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to validate the token lifetime during validation.
    /// When true, expired tokens will be rejected.
    /// </summary>
    /// <value>True to validate token lifetime; otherwise, false. Defaults to true.</value>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the token issuer during validation.
    /// When true, tokens from unexpected issuers will be rejected.
    /// </summary>
    /// <value>True to validate issuer; otherwise, false. Defaults to true.</value>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the token audience during validation.
    /// When true, tokens for unexpected audiences will be rejected.
    /// </summary>
    /// <value>True to validate audience; otherwise, false. Defaults to true.</value>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the issuer signing key during validation.
    /// When true, tokens with invalid signatures will be rejected.
    /// </summary>
    /// <value>True to validate signing key; otherwise, false. Defaults to true.</value>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Gets or sets the clock skew allowance for token validation.
    /// This provides tolerance for time differences between systems.
    /// </summary>
    /// <value>The clock skew allowance. Defaults to 5 minutes.</value>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets additional custom properties for JWT configuration.
    /// Allows for extension with provider-specific settings.
    /// </summary>
    /// <value>A dictionary of custom properties. Defaults to an empty dictionary.</value>
    public Dictionary<string, object> CustomProperties { get; set; } = new();

    /// <summary>
    /// Validates the JWT options configuration.
    /// </summary>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Authority) || string.IsNullOrWhiteSpace(Audience))
            return false;

        if (!string.IsNullOrWhiteSpace(TokenEndpoint) && !Uri.IsWellFormedUriString(TokenEndpoint, UriKind.Absolute))
            return false;

        if (!string.IsNullOrWhiteSpace(Authority) && !Uri.IsWellFormedUriString(Authority, UriKind.Absolute))
            return false;

        return RequestTimeout > TimeSpan.Zero;
    }

    /// <summary>
    /// Gets the scope string formatted for token requests.
    /// </summary>
    /// <returns>A space-separated string of scopes, or null if no scopes are specified.</returns>
    public string? GetScopeString() => Scopes.Length > 0 ? string.Join(" ", Scopes) : null;

    /// <summary>
    /// Creates JWT options for a typical web application scenario.
    /// </summary>
    /// <param name="authority">The JWT authority.</param>
    /// <param name="audience">The intended audience.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <returns>Configured JwtOptions for web applications.</returns>
    public static JwtOptions CreateForWebApp(string authority, string audience, string clientId, string clientSecret)
    {
        return new JwtOptions
        {
            Authority = authority,
            Audience = audience,
            ClientId = clientId,
            ClientSecret = clientSecret,
            RequireHttpsMetadata = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true
        };
    }

    /// <summary>
    /// Creates JWT options for API-to-API communication.
    /// </summary>
    /// <param name="authority">The JWT authority.</param>
    /// <param name="audience">The intended audience.</param>
    /// <param name="scopes">The required scopes.</param>
    /// <returns>Configured JwtOptions for API communication.</returns>
    public static JwtOptions CreateForApiClient(string authority, string audience, params string[] scopes)
    {
        return new JwtOptions
        {
            Authority = authority,
            Audience = audience,
            Scopes = scopes,
            RefreshIfExpired = true,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true
        };
    }
}
