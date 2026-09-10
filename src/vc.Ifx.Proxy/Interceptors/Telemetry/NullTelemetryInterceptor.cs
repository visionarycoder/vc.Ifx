// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Telemetry;
/// <summary>
/// Null object pattern implementation of telemetry interceptor that performs no operations.
/// </summary>
public sealed class NullTelemetryInterceptor : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => -50;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any telemetry processing
        return next(context, cancellationToken);
    }
}
