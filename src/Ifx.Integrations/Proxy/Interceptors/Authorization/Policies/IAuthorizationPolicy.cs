using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Ifx.Proxy.Interceptor.Authorization.Results;

namespace Ifx.Proxy.Interceptor.Authorization.Policies;

/// <summary>
/// Defines a contract for authorization policies that determine access permissions.
/// Implementations should provide consistent authorization logic based on context and business rules.
/// </summary>
public interface IAuthorizationPolicy
{
    /// <summary>
    /// Gets the unique name of this authorization policy.
    /// Used for identification, logging, and configuration purposes.
    /// </summary>
    string PolicyName { get; }

    /// <summary>
    /// Evaluates the authorization policy against the provided context.
    /// Synchronous evaluation for simple policies that don't require I/O operations.
    /// </summary>
    /// <param name="context">The authorization context containing request and user information.</param>
    /// <returns>True if authorized; otherwise, false.</returns>
    bool Evaluate(object context);

    /// <summary>
    /// Evaluates the authorization policy asynchronously with detailed result information.
    /// Preferred method for complex policies that may require database lookups or external service calls.
    /// </summary>
    /// <param name="context">The authorization context containing request and user information.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the async operation with detailed authorization result.</returns>
    Task<AuthorizationResult> EvaluateAsync(object context, CancellationToken cancellationToken = default);
}