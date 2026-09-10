using System.Text.Json;

namespace VisionaryCoder.Framework.Proxy.Transports;

/// <summary>HTTP-only limits, JSON policy and core retry classification.</summary>
public sealed class HttpProxyTransportOptions
{
    /// <summary>Whole-attempt timeout; null uses HttpClient.Timeout. Infinite disables this transport deadline.</summary>
    public TimeSpan? RequestTimeout { get; init; }
    /// <summary>Maximum buffered outbound body size in bytes.</summary>
    public int MaxRequestBodyBytes { get; init; } = 4 * 1024 * 1024;
    /// <summary>Maximum buffered successful response body size in bytes.</summary>
    public int MaxResponseBodyBytes { get; init; } = 4 * 1024 * 1024;
    /// <summary>Signal replay-safe transient failures to core; never performs retries itself.</summary>
    public bool ClassifyRetryableFailures { get; init; } = true;
    /// <summary>JSON configuration, copied at construction. Default is System.Text.Json web defaults.</summary>
    public JsonSerializerOptions JsonOptions { get; init; } = new(JsonSerializerDefaults.Web);

    /// <summary>Reject invalid limits, deadlines and serializer options.</summary>
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxRequestBodyBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxResponseBodyBytes);
        ArgumentNullException.ThrowIfNull(JsonOptions);
        if (RequestTimeout is { } timeout && timeout != Timeout.InfiniteTimeSpan &&
            (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > uint.MaxValue - 1))
            throw new ArgumentOutOfRangeException(nameof(RequestTimeout));
    }
}
