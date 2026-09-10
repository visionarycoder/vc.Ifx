using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;
/// <summary>
/// Security enricher that adds tenant information to the proxy context.
/// </summary>
/// <param name="tenantProvider">The tenant context provider.</param>
public class TenantContextEnricher(ITenantContextProvider tenantProvider) : IProxySecurityEnricher
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
        TenantContext? tenantContext = await tenantProvider.GetTenantContextAsync(cancellationToken);
        if (tenantContext != null && !string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            context.Metadata["TenantId"] = tenantContext.TenantId;
            ProxyHeaders.Set(context, "X-Tenant-ID", tenantContext.TenantId);

            if (!string.IsNullOrWhiteSpace(tenantContext.TenantName))
            {
                context.Metadata["TenantName"] = tenantContext.TenantName;
            }
        }
    }
}
