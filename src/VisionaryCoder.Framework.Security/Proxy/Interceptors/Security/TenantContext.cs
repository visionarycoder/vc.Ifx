namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Represents tenant context information.
/// </summary>
public class TenantContext
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    /// Gets or sets the tenant name.
    public string TenantName { get; set; } = string.Empty;
}
