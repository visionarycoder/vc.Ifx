// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

/// <summary>
/// Represents a token result.
/// </summary>
public class TokenResult
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token type.
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the expires in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the scope.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Gets whether the token is expired.
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiryTime;

    /// <summary>
    /// Gets the expiry time.
    /// </summary>
    public DateTimeOffset ExpiryTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets whether the token request was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the correlation ID.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the error message if the request failed.
    /// </summary>
    public string? Error { get; set; }
}