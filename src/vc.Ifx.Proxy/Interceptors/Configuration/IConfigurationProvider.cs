namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

/// <summary>
/// Defines the contract for application configuration providers.
/// Supports async operations, caching, refresh capabilities, and type-safe configuration access.
/// </summary>
public interface IConfigurationProvider
{

    string ProviderName { get; }

    T GetValue<T>(string key, T defaultValue) where T : class, new();
    Task<T> GetValueAsync<T>(string key, T defaultValue = default!, CancellationToken cancellationToken = default) where T : class, new();

    bool SetValue<T>(string key, T value) where T : class, new();
    Task<bool> SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class, new();
    
    T GetSection<T>(string sectionName) where T : class, new();
    Task<T> GetSectionAsync<T>(string sectionName, CancellationToken cancellationToken = default) where T : class, new();

    IDictionary<string, object?> GetAllValues();
    Task<IDictionary<string, object?>> GetAllValuesAsync(CancellationToken cancellationToken = default);

    bool Refresh();
    Task<bool> RefreshAsync(CancellationToken cancellationToken = default);

}
