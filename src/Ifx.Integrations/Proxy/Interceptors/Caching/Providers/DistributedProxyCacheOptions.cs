namespace Ifx.Proxy.Interceptor.Caching.Providers;

/// <summary>Configures the key namespace used by a distributed proxy cache.</summary>
public sealed class DistributedProxyCacheOptions
{
    /// <summary>Gets or sets the application-specific cache key prefix.</summary>
    public string KeyPrefix { get; set; } = "default";

    /// <summary>Validates the distributed cache key namespace.</summary>
    public DistributedProxyCacheOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(KeyPrefix);
        if (KeyPrefix.Any(char.IsControl) || KeyPrefix.Contains(':'))
        {
            throw new ArgumentException("The cache prefix cannot contain control characters or colons.", nameof(KeyPrefix));
        }

        return this;
    }
}
