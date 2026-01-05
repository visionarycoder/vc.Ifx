// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Abstractions;
using VisionaryCoder.Framework.Proxy.Interceptor.Security.Abstractions;
using VisionaryCoder.Framework.Proxy.Interceptor.Security.Providers;

namespace VisionaryCoder.Framework.Proxy.Interceptor.Security;

/// <summary>
/// Security enricher that adds tenant information to the proxy context.
/// </summary>
/// <param name="tenantProvider">The tenant context provider.</param>
public class TenantContextEnricher(ITenantContextProvider tenantProvider) : ISecurityEnricher
{
    private readonly ITenantContextProvider tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
    /// <summary>
    /// Gets the execution order for this enricher.
    /// </summary>
    public int Order => 200;
    /// <summary>
    /// Enriches the context with current tenant information.
    /// </summary>
    /// <param name="context">The proxy context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the enrichment operation.</returns>
    public async Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default)
    {
        string? tenantId = await tenantProvider.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId != null)
        {
            context.Metadata["TenantId"] = tenantId;
            context.Headers["X-Tenant-ID"] = tenantId;
        }
    }
}
