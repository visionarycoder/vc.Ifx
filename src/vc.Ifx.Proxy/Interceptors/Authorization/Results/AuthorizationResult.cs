// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Results;

/// <summary>
/// Represents the result of an authorization check with comprehensive context and failure information.
/// Provides detailed feedback about authorization decisions including success/failure reasons and contextual data.
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationResult"/> class.
    /// Creates an unauthorized result by default with an empty context dictionary.
    /// </summary>
    public AuthorizationResult()
    {
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the authorization was successful.
    /// When true, the operation is authorized; when false, access should be denied.
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Gets or sets the reason for authorization failure.
    /// Provides detailed information about why authorization was denied for logging and debugging purposes.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Gets or sets the context information associated with this authorization result.
    /// Contains additional data about the authorization decision including user info, policies evaluated, etc.
    /// </summary>
    public Dictionary<string, object> Context { get; set; }

    /// <summary>
    /// Creates a successful authorization result.
    /// Returns an authorized result with empty context that can be enriched with additional information.
    /// </summary>
    /// <returns>An authorized <see cref="AuthorizationResult"/> with IsAuthorized set to true.</returns>
    public static AuthorizationResult Success()
    {
        return new AuthorizationResult
        {
            IsAuthorized = true
        };
    }

    /// <summary>
    /// Creates a failed authorization result with the specified reason.
    /// Returns an unauthorized result with the failure reason for auditing and user feedback.
    /// </summary>
    /// <param name="reason">The reason for the authorization failure.</param>
    /// <returns>An unauthorized <see cref="AuthorizationResult"/> with the specified failure reason.</returns>
    public static AuthorizationResult Failure(string reason)
    {
        return new AuthorizationResult
        {
            IsAuthorized = false,
            FailureReason = reason
        };
    }
}
