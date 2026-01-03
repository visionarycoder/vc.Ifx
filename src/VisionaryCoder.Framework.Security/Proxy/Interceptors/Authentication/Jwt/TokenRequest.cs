// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

/// <summary>
/// Represents a JWT token request containing authentication parameters and configuration options.
/// Supports multiple OAuth 2.0 grant types including client credentials, authorization code, and refresh token flows.
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Gets or sets the OAuth 2.0 grant type for token acquisition.
    /// Common values include "client_credentials", "authorization_code", "refresh_token".
    /// </summary>
    /// <value>The grant type. Defaults to "client_credentials".</value>
    [Required]
    public string GrantType { get; set; } = "client_credentials";

    /// <summary>
    /// Gets or sets the requested scopes for the token.
    /// Scopes define the level of access that the application is requesting.
    /// </summary>
    /// <value>An array of scope strings. Defaults to an empty array.</value>
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the client identifier for the application.
    /// This uniquely identifies the application to the authorization server.
    /// </summary>
    /// <value>The client ID. Defaults to an empty string.</value>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for authentication.
    /// This is a confidential credential used to authenticate the application.
    /// </summary>
    /// <value>The client secret. Defaults to an empty string.</value>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended audience for the token.
    /// This identifies the recipients that the JWT is intended for.
    /// </summary>
    /// <value>The audience identifier. Defaults to an empty string.</value>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to automatically refresh the token if it has expired.
    /// When true, the provider will attempt to refresh expired tokens using refresh tokens.
    /// </summary>
    /// <value>True to refresh expired tokens; otherwise, false. Defaults to true.</value>
    public bool RefreshIfExpired { get; set; } = true;

    /// <summary>
    /// Gets or sets the username for resource owner password credentials grant.
    /// Only used when GrantType is "password".
    /// </summary>
    /// <value>The username. Defaults to null.</value>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for resource owner password credentials grant.
    /// Only used when GrantType is "password".
    /// </summary>
    /// <value>The password. Defaults to null.</value>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the authorization code for authorization code grant.
    /// Only used when GrantType is "authorization_code".
    /// </summary>
    /// <value>The authorization code. Defaults to null.</value>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Gets or sets the redirect URI that must match the one used in the authorization request.
    /// Required for authorization code grant.
    /// </summary>
    /// <value>The redirect URI. Defaults to null.</value>
    public string? RedirectUri { get; set; }

    /// <summary>
    /// Gets or sets additional custom parameters for the token request.
    /// Allows for extension with provider-specific parameters.
    /// </summary>
    /// <value>A dictionary of custom parameters. Defaults to an empty dictionary.</value>
    public Dictionary<string, string> CustomParameters { get; set; } = new();

    /// <summary>
    /// Validates the token request based on the specified grant type.
    /// </summary>
    /// <returns>True if the request is valid for the specified grant type; otherwise, false.</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(GrantType) || string.IsNullOrWhiteSpace(ClientId))
            return false;

        return GrantType.ToLowerInvariant() switch
        {
            "client_credentials" => !string.IsNullOrWhiteSpace(ClientSecret),
            "password" => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password),
            "authorization_code" => !string.IsNullOrWhiteSpace(AuthorizationCode) && !string.IsNullOrWhiteSpace(RedirectUri),
            "refresh_token" => true, // Refresh token is typically handled separately
            _ => true // Allow custom grant types
        };
    }

    /// <summary>
    /// Gets the scope string formatted for the token request.
    /// </summary>
    /// <returns>A space-separated string of scopes, or null if no scopes are specified.</returns>
    public string? GetScopeString() => Scopes.Length > 0 ? string.Join(" ", Scopes) : null;

    /// <summary>
    /// Creates a token request for client credentials flow.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="scopes">The requested scopes.</param>
    /// <param name="audience">The intended audience.</param>
    /// <returns>A configured TokenRequest for client credentials flow.</returns>
    public static TokenRequest CreateClientCredentials(string clientId, string clientSecret, string[]? scopes = null, string? audience = null)
    {
        return new TokenRequest
        {
            GrantType = "client_credentials",
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scopes = scopes ?? [],
            Audience = audience ?? string.Empty
        };
    }

    /// <summary>
    /// Creates a token request for resource owner password credentials flow.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="username">The resource owner username.</param>
    /// <param name="password">The resource owner password.</param>
    /// <param name="scopes">The requested scopes.</param>
    /// <returns>A configured TokenRequest for password flow.</returns>
    public static TokenRequest CreatePasswordCredentials(string clientId, string username, string password, string[]? scopes = null)
    {
        return new TokenRequest
        {
            GrantType = "password",
            ClientId = clientId,
            Username = username,
            Password = password,
            Scopes = scopes ?? []
        };
    }
}
