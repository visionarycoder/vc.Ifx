namespace Ifx;

public sealed class Options
{
    public bool EnableCorrelationId { get; set; } = true;
    public bool EnableRequestId { get; set; } = true;
    public bool EnableStructuredLogging { get; set; } = true;
    public int DefaultHttpTimeoutSeconds { get; set; } = IfxConstants.Timeouts.DefaultHttpTimeoutSeconds;
    public int DefaultCacheExpirationMinutes { get; set; } = IfxConstants.Timeouts.DefaultCacheExpirationMinutes;
}
