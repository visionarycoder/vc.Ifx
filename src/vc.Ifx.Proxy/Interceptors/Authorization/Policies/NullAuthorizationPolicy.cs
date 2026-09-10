// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Results;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;

/// <summary>
/// Null Object implementation of <see cref="IAuthorizationPolicy"/> that provides safe fallback behavior.
/// Always denies authorization when no explicit policy is registered.
/// Follows SOLID principles by ensuring safe operation without implicit defaults.
/// </summary>
public sealed class NullAuthorizationPolicy : IAuthorizationPolicy
{
    /// <summary>
    /// Gets the policy name for the null authorization policy.
    /// </summary>
    public string PolicyName => "NullAuthorization";

    /// <summary>
    /// Always returns false to deny authorization.
    /// This ensures safe fallback behavior when no explicit policy is configured.
    /// </summary>
    /// <param name="context">The authorization context (ignored in null implementation).</param>
    /// <returns>Always returns false to deny access.</returns>
    public bool Evaluate(object context)
    {
        return false;
    }

    /// <summary>
    /// Always returns a failure result to deny authorization with detailed reason.
    /// This ensures safe fallback behavior when no explicit policy is configured.
    /// </summary>
    /// <param name="context">The authorization context (ignored in null implementation).</param>
    /// <param name="cancellationToken">Cancellation token (ignored in null implementation).</param>
    /// <returns>Always returns a failure result indicating no authorization policy is configured.</returns>
    public Task<AuthorizationResult> EvaluateAsync(object context, CancellationToken cancellationToken = default)
    {
        var result = AuthorizationResult.Failure("No authorization policy configured - using null implementation");
        result.Context["PolicyType"] = "NullAuthorizationPolicy";
        result.Context["Reason"] = "Explicit authorization policy registration required";
        
        return Task.FromResult(result);
    }
}