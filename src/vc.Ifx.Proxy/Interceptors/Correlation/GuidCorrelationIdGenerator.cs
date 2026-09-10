namespace VisionaryCoder.Framework.Proxy.Interceptors.Correlation;
/// <summary>
/// Default correlation ID generator that creates GUIDs.
/// </summary>
public sealed class GuidCorrelationIdGenerator : ICorrelationIdGenerator
{
    /// <inheritdoc />
    public string GenerateId()
    {
        return Guid.NewGuid().ToString("D");
    }
}
