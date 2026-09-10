// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Security.Cryptography;
using System.Text;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

/// <summary>
/// Default implementation of <see cref="ICacheKeyProvider"/> that generates cache keys
/// based on operation name, HTTP method, URL, and relevant headers.
/// Uses SHA256 hashing to ensure consistent key length and avoid special characters.
/// </summary>
public class DefaultCacheKeyProvider : ICacheKeyProvider
{
    private static readonly string[] relevantHeaders =
    [
        "Accept",
        "Accept-Language",
        "Content-Type",
        "X-API-Version",
        "Authorization"
    ];

    /// <summary>
    /// Generates a cache key based on the operation name, URL, method, and relevant headers.
    /// The key is hashed using SHA256 to ensure consistent length and format.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>A unique, hashed cache key string.</returns>
    public string GenerateKey(ProxyContext context)
    {
        var keyComponents = new List<string>
        {
            context.OperationName ?? "Unknown",
            context.Method ?? "GET",
            context.Url ?? string.Empty
        };

        // Include relevant headers in the key for context-sensitive caching
        AddRelevantHeaders(context, keyComponents);

        return HashKey(string.Join("|", keyComponents));
    }

    /// <summary>
    /// Generates a cache key with additional type information for generic operations.
    /// </summary>
    /// <typeparam name="T">The type parameter providing additional context.</typeparam>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>A unique, hashed cache key string including type information.</returns>
    public string GenerateKey<T>(ProxyContext context)
    {
        var keyComponents = new List<string>
        {
            context.OperationName ?? "Unknown",
            context.Method ?? "GET",
            context.Url ?? string.Empty,
            typeof(T).FullName ?? typeof(T).Name
        };

        AddRelevantHeaders(context, keyComponents);

        return HashKey(string.Join("|", keyComponents));
    }

    /// <summary>
    /// Adds relevant headers to the key components list for cache differentiation.
    /// </summary>
    /// <param name="context">The proxy context containing headers.</param>
    /// <param name="keyComponents">The list to add header information to.</param>
    private static void AddRelevantHeaders(ProxyContext context, List<string> keyComponents)
    {
        if (context.Headers is { Count: > 0 })
        {
            string headerString = string.Join(";", context.Headers
                .Where(h => IsRelevantHeader(h.Key))
                .OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
                .Select(h => $"{h.Key}={h.Value}"));

            if (!string.IsNullOrEmpty(headerString))
            {
                keyComponents.Add(headerString);
            }
        }
    }

    /// <summary>
    /// Determines if a header should be included in the cache key for differentiation.
    /// </summary>
    /// <param name="headerName">The header name to evaluate.</param>
    /// <returns>True if the header should be included in cache key generation.</returns>
    private static bool IsRelevantHeader(string headerName)
    {
        return relevantHeaders.Any(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Hashes the combined key using SHA256 to ensure consistent length and avoid special characters.
    /// </summary>
    /// <param name="combinedKey">The combined key string to hash.</param>
    /// <returns>A Base64-encoded SHA256 hash of the key.</returns>
    private static string HashKey(string combinedKey)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedKey));
        return Convert.ToBase64String(hashBytes);
    }
}
