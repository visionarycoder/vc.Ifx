// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Ifx.Proxy.Interceptor.Resilience;
/// <summary>
/// Null object pattern implementation of resilience interceptor that performs no operations.
/// </summary>
public sealed class NullResilienceInterceptor : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 180;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any resilience processing
        return next(context, cancellationToken);
    }
}
