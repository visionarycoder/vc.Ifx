// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Represents the result of an authorization check.
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationResult"/> class.
    /// </summary>
    public AuthorizationResult()
    {
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the authorization was successful.
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Gets or sets the reason for authorization failure.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Gets or sets the context information associated with this authorization result.
    /// </summary>
    public Dictionary<string, object> Context { get; set; }

    /// <summary>
    /// Creates a successful authorization result.
    /// </summary>
    /// <returns>An authorized <see cref="AuthorizationResult"/>.</returns>
    public static AuthorizationResult Success()
    {
        return new AuthorizationResult
        {
            IsAuthorized = true
        };
    }

    /// <summary>
    /// Creates a failed authorization result with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for the authorization failure.</param>
    /// <returns>An unauthorized <see cref="AuthorizationResult"/>.</returns>
    public static AuthorizationResult Failure(string reason)
    {
        return new AuthorizationResult
        {
            IsAuthorized = false,
            FailureReason = reason
        };
    }
}