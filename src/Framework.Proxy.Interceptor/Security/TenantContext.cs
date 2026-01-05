namespace VisionaryCoder.Framework.Proxy.Interceptor.Security;

/// <summary>
/// Represents tenant context information.
/// </summary>
public class TenantContext
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    public string TenantName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets a value indicating whether the tenant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets additional tenant settings and metadata.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();
}
