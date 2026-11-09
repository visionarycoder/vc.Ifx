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

namespace Ifx.Proxy.Interceptor.Authentication.Jwt;

/// <summary>
/// Defines a contract for JWT token providers that handle token acquisition and validation.
/// Implementations should support secure token generation, renewal, and validation.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Gets a JWT token asynchronously using default credentials or configuration.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the async operation with the JWT token string.</returns>
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a JWT token result with additional metadata based on the provided request.
    /// </summary>
    /// <param name="request">The token request containing authentication parameters.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task with the complete token result including expiration and metadata.</returns>
    Task<TokenResult> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JWT token for authenticity and expiration.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task indicating whether the token is valid.</returns>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JWT token synchronously for scenarios where async is not needed.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Refreshes an existing JWT token if supported by the provider.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use for token renewal.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task with the new token result, or null if refresh is not supported.</returns>
    Task<TokenResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts claims from a JWT token without full validation.
    /// </summary>
    /// <param name="token">The JWT token to extract claims from.</param>
    /// <returns>A dictionary of claims found in the token.</returns>
    Dictionary<string, object> ExtractClaims(string token);
}