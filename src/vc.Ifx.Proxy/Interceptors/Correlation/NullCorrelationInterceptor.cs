// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Correlation;
/// <summary>
/// Null object pattern implementation of correlation interceptor that performs no operations.
/// </summary>
public sealed class NullCorrelationInterceptor : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 0;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any correlation processing
        return next(context, cancellationToken);
    }
}
