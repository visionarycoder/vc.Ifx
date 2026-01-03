// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

/// <summary>
/// Configuration options for Web JWT interceptor.
/// </summary>
public class WebJwtOptions
{
    /// <summary>
    /// Gets or sets the JWT authority.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signing key.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether HTTPS metadata is required.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets the scopes for the JWT token.
    /// </summary>
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to refresh the token if it's expired.
    /// </summary>
    public bool RefreshIfExpired { get; set; } = true;

    /// <summary>
    /// Gets or sets the header name for the JWT token.
    /// </summary>
    public string HeaderName { get; set; } = "Authorization";
}