namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface ICache
{
    Task<(bool Hit, T value)> TryGetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan ttl);
}