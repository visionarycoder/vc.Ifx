namespace VisionaryCoder.Framework.Pipeline.Abstractions;

/// <summary>Application-owned response cache; hits can carry null values.</summary>
public interface ICache
{
    /// <summary>Looks up a value using the legacy contract.</summary>
    Task<(bool Hit, T value)> TryGetAsync<T>(string key);
    /// <summary>Stores a value using the legacy contract.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan ttl);

    /// <summary>Checks cancellation before a legacy lookup; override for in-flight cancellation.</summary>
    Task<(bool Hit, T value)> TryGetAsync<T>(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return TryGetAsync<T>(key);
    }

    /// <summary>Checks cancellation before a legacy write; override for in-flight cancellation.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return SetAsync(key, value, ttl);
    }
}
