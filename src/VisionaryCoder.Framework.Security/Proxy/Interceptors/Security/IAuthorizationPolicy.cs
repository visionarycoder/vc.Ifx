// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Authorization policy interface.
/// </summary>
public interface IAuthorizationPolicy
{
    /// <summary>
    /// Gets the policy name.
    /// </summary>
    string PolicyName { get; }

    /// <summary>
    /// Evaluates the authorization policy.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <returns>True if authorized; otherwise, false.</returns>
    bool Evaluate(object context);
}