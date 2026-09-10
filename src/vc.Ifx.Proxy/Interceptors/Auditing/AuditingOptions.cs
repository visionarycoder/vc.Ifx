namespace VisionaryCoder.Framework.Proxy.Interceptors.Auditing;

/// <summary>
/// Configuration options for the auditing interceptor.
/// </summary>
public class AuditingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to include request headers in audit records.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;
    /// Gets or sets a value indicating whether to include error details in audit records.
    public bool IncludeErrorDetails { get; set; } = true;
    /// Gets or sets a value indicating whether to include response data size in audit records.
    public bool IncludeResponseData { get; set; } = false;
}
