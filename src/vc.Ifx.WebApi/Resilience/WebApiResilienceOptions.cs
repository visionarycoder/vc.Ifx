namespace VisionaryCoder.Framework.WebApi.Resilience;

/// <summary>Controls cooperative per-attempt timeout and explicitly replay-safe retries.</summary>
public sealed class WebApiResilienceOptions
{
    /// <summary>Gets or sets whether retries are enabled; disabled by default.</summary>
    public bool EnableRetry { get; set; }

    /// <summary>Explicitly asserts that every operation using this factory can be replayed safely.</summary>
    public bool OperationsAreReplaySafe { get; set; }

    /// <summary>Gets or sets whether retry delays include random jitter.</summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>Gets or sets whether cooperative per-attempt timeout is enabled.</summary>
    public bool EnableTimeout { get; set; } = true;

    /// <summary>Gets or sets retry attempts after the initial call, from 1 through 100.</summary>
    public int RetryAttemptCount { get; set; } = 3;

    /// <summary>Gets or sets initial exponential delay, from zero through one day.</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>Gets or sets per-attempt timeout, from 10 milliseconds through one day.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    internal void Validate()
    {
        if (EnableRetry && !OperationsAreReplaySafe)
        {
            throw new ArgumentException("Retry requires explicit operation replay safety.", nameof(OperationsAreReplaySafe));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(RetryAttemptCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(RetryAttemptCount, 100);
        ArgumentOutOfRangeException.ThrowIfLessThan(RetryDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(RetryDelay, TimeSpan.FromDays(1));
        ArgumentOutOfRangeException.ThrowIfLessThan(Timeout, TimeSpan.FromMilliseconds(10));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Timeout, TimeSpan.FromDays(1));
    }
}
