// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Logging;
/// <summary>
/// Null object pattern implementation of logging interceptor that performs no operations.
/// </summary>
public sealed class NullLoggingInterceptor : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 100;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any logging processing
        return next(context, cancellationToken);
    }
}
