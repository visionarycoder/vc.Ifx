// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;
/// <summary>
/// Helper class for enriching proxy context with JWT Bearer authentication.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="tokenProvider">Function to provide JWT tokens.</param>
public class JwtBearerEnricher(ILogger<JwtBearerEnricher> logger, Func<Task<string?>> tokenProvider) : IProxySecurityEnricher
{
    private readonly ILogger<JwtBearerEnricher> logger = logger;
    private readonly Func<Task<string?>> tokenProvider = tokenProvider;
    /// <inheritdoc />
    public async Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            string? token = await tokenProvider();
            if (!string.IsNullOrWhiteSpace(token))
            {
                context.Headers["Authorization"] = $"Bearer {token}";
                logger.LogDebug("JWT Bearer token added to context");
            }
            else
                logger.LogWarning("JWT token provider returned null or empty token");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve JWT token");
            throw;
        }
    }
}
