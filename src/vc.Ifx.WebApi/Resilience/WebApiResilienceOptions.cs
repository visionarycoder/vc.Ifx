namespace VisionaryCoder.Framework.WebApi.Resilience;

public sealed class WebApiResilienceOptions
{
    public bool EnableRetry { get; set; } = true;

    public bool EnableTimeout { get; set; } = true;

    public int RetryAttemptCount { get; set; } = 3;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(200);

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
