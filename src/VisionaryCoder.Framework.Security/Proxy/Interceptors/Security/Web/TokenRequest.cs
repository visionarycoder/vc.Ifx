// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

/// <summary>
/// Represents a token request.
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Gets or sets the grant type.
    /// </summary>
    public string GrantType { get; set; } = "client_credentials";

    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to refresh the token if expired.
    /// </summary>
    public bool RefreshIfExpired { get; set; } = true;
}