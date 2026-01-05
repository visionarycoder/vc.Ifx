// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Abstractions;
using VisionaryCoder.Framework.Proxy.Interceptor.Security.Abstractions;
using VisionaryCoder.Framework.Proxy.Interceptor.Security.Providers;

namespace VisionaryCoder.Framework.Proxy.Interceptor.Security;

/// <summary>
/// Security enricher that adds user information to the proxy context.
/// </summary>
/// <param name="userProvider">The user context provider.</param>
public class UserContextEnricher(IUserContextProvider userProvider) : ISecurityEnricher
{
    private readonly IUserContextProvider userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
    /// <summary>
    /// Gets the execution order for this enricher.
    /// </summary>
    public int Order => 100;
    /// <summary>
    /// Enriches the context with current user information.
    /// </summary>
    /// <param name="context">The proxy context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the enrichment operation.</returns>
    public async Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default)
    {
        // TODO: Add user context enrichment when user provider interface is available
        await Task.CompletedTask.ConfigureAwait(false);

        // For now, add placeholder user information
        context.Metadata["UserId"] = "system";
        context.Metadata["UserName"] = "System User";
        context.Metadata["Roles"] = new[] { "User" };
    }
}
