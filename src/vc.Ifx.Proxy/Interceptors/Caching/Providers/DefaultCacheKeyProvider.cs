using System.Security.Cryptography;
using System.Text.Json;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

/// <summary>Hashes structured request and identity inputs without delimiter collisions.</summary>
public class DefaultCacheKeyProvider : ICacheKeyProvider
{
    private readonly CachingOptions? options;

    public DefaultCacheKeyProvider() { }

    /// <summary>Uses an explicitly configured custom generator, or the structured default key.</summary>
    public DefaultCacheKeyProvider(CachingOptions options)
        => this.options = options ?? throw new ArgumentNullException(nameof(options));
    private static readonly HashSet<string> RelevantHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Accept", "Accept-Language", "Content-Type", "X-API-Version", "Authorization"
    };

    /// <summary>Includes the declared result type when the context supplies one.</summary>
    public string GenerateKey(ProxyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return CreateKey(context, context.ResultType?.AssemblyQualifiedName);
    }

    /// <summary>Includes the requested result type in the key.</summary>
    public string GenerateKey<T>(ProxyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return CreateKey(context, typeof(T).AssemblyQualifiedName);
    }

    private string CreateKey(ProxyContext context, string? resultType)
    {
        if (options?.KeyGenerator is { } generator)
            return generator(context);
        byte[] content = JsonSerializer.SerializeToUtf8Bytes(new
        {
            context.ServiceName,
            context.MethodName,
            context.OperationName,
            Method = context.Method?.ToUpperInvariant(),
            context.Url,
            ResultType = resultType,
            RequestType = context.Request?.GetType().AssemblyQualifiedName,
            context.Request,
            BodyType = context.Body?.GetType().AssemblyQualifiedName,
            context.Body,
            Tenant = context.Metadata?.GetValueOrDefault("TenantId"),
            User = context.Metadata?.GetValueOrDefault("UserId"),
            Client = context.Metadata?.GetValueOrDefault("ClientId"),
            Headers = (context.Headers ?? [])
                .Where(header => RelevantHeaders.Contains(header.Key))
                .Select(header => new { Name = header.Key.ToUpperInvariant(), header.Value })
                .OrderBy(header => header.Name, StringComparer.Ordinal)
                .ThenBy(header => header.Value, StringComparer.Ordinal)
                .ToArray()
        });
        return Convert.ToBase64String(SHA256.HashData(content));
    }
}
