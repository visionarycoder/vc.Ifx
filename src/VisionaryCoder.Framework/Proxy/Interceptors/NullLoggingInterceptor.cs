// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors;

/// <summary>
/// Null object pattern implementation of a logging interceptor that performs no logging operations.
/// Useful for scenarios where logging is disabled or for testing environments where logging overhead should be avoided.
/// </summary>
public sealed class NullLoggingInterceptor : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 100; // Same order as regular logging but performs no operations

    /// <summary>
    /// Passes through the operation without any logging or modification.
    /// Provides minimal overhead while maintaining the interceptor contract.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="context">The proxy context (not used by this implementation).</param>
    /// <param name="next">The next delegate in the pipeline to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The result of the next delegate without any logging side effects.</returns>
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any logging processing for maximum performance
        return next(context, cancellationToken);
    }
}