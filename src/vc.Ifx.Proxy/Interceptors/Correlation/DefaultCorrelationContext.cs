namespace VisionaryCoder.Framework.Proxy.Interceptors.Correlation;
/// <summary>
/// Default implementation of correlation context using AsyncLocal.
/// </summary>
public sealed class DefaultCorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<string?> correlationId = new();
    private static readonly AsyncLocal<Dictionary<string, string>?> data = new();
    
    /// <inheritdoc />
    public string? CorrelationId 
    { 
        get => correlationId.Value;
        set => correlationId.Value = value;
    }
    
    /// <inheritdoc />
    public Dictionary<string, string> Data 
    { 
        get => data.Value ??= new Dictionary<string, string>();
        set => data.Value = value;
    }
    
    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }
}
