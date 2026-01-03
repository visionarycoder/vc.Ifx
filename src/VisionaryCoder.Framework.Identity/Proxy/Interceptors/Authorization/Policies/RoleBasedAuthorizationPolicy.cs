// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Results;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;

/// <summary>
/// Role-based authorization policy that validates user access based on required roles.
/// Supports both synchronous and asynchronous evaluation with detailed failure information.
/// </summary>
public class RoleBasedAuthorizationPolicy : IAuthorizationPolicy
{
    private readonly ICollection<string> requiredRoles;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleBasedAuthorizationPolicy"/> class.
    /// </summary>
    /// <param name="requiredRoles">The collection of roles required for authorization.</param>
    /// <exception cref="ArgumentNullException">Thrown when requiredRoles is null.</exception>
    public RoleBasedAuthorizationPolicy(ICollection<string> requiredRoles)
    {
        this.requiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));
    }

    /// <summary>
    /// Gets the unique name of this authorization policy.
    /// </summary>
    public string PolicyName => "RoleBasedAuthorization";

    /// <summary>
    /// Evaluates the authorization policy synchronously.
    /// Checks if the user has any of the required roles for access.
    /// </summary>
    /// <param name="context">The authorization context (expected to be ProxyContext).</param>
    /// <returns>True if the user has at least one required role; otherwise, false.</returns>
    public bool Evaluate(object context)
    {
        if (context is ProxyContext proxyContext)
        {
            return EvaluateRoles(proxyContext).HasRequiredRole;
        }
        return false;
    }

    /// <summary>
    /// Evaluates the authorization policy asynchronously with detailed result information.
    /// Provides comprehensive feedback about authorization decisions and failure reasons.
    /// </summary>
    /// <param name="context">The authorization context (expected to be ProxyContext).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task with detailed authorization result including failure reasons.</returns>
    public Task<AuthorizationResult> EvaluateAsync(object context, CancellationToken cancellationToken = default)
    {
        if (context is not ProxyContext proxyContext)
        {
            return Task.FromResult(AuthorizationResult.Failure("Invalid authorization context type"));
        }

        RoleEvaluationResult evaluation = EvaluateRoles(proxyContext);

        if (evaluation.HasRequiredRole)
        {
            var result = AuthorizationResult.Success();
            result.Context["EvaluatedRoles"] = evaluation.UserRoles;
            result.Context["RequiredRoles"] = requiredRoles;
            result.Context["PolicyName"] = PolicyName;
            return Task.FromResult(result);
        }

        var failureResult = AuthorizationResult.Failure(evaluation.FailureReason ?? "Authorization failed");
        failureResult.Context["UserRoles"] = evaluation.UserRoles;
        failureResult.Context["RequiredRoles"] = requiredRoles;
        failureResult.Context["PolicyName"] = PolicyName;
        return Task.FromResult(failureResult);
    }

    /// <summary>
    /// Internal method to evaluate user roles against required roles.
    /// </summary>
    /// <param name="context">The proxy context containing user role information.</param>
    /// <returns>Role evaluation result with success/failure information.</returns>
    private RoleEvaluationResult EvaluateRoles(ProxyContext context)
    {
        if (!context.Metadata.TryGetValue("Roles", out object? rolesObj) ||
            rolesObj is not ICollection<string> userRoles)
        {
            return new RoleEvaluationResult
            {
                HasRequiredRole = false,
                UserRoles = Array.Empty<string>(),
                FailureReason = "No user roles found in context"
            };
        }

        bool hasRequiredRole = requiredRoles.Any(requiredRole =>
            userRoles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase));

        return new RoleEvaluationResult
        {
            HasRequiredRole = hasRequiredRole,
            UserRoles = userRoles,
            FailureReason = hasRequiredRole ? null :
                $"User roles [{string.Join(", ", userRoles)}] do not include any of required roles [{string.Join(", ", requiredRoles)}]"
        };
    }

    /// <summary>
    /// Internal result structure for role evaluation.
    /// </summary>
    private class RoleEvaluationResult
    {
        public bool HasRequiredRole { get; set; }
        public ICollection<string> UserRoles { get; set; } = Array.Empty<string>();
        public string? FailureReason { get; set; }
    }
}
