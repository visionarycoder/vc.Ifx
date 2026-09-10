// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// Null object pattern implementation of caching interceptor that performs no operations.
public sealed class NullCachingInterceptor : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 150;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any caching
        return next(context, cancellationToken);
    }
}
