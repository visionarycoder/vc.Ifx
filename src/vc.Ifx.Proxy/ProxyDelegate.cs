// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy;
/// <summary>
/// Delegate for proxy operations.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
/// <param name="context">The proxy context.</param>
/// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
/// <returns>A task representing the asynchronous operation with the response.</returns>
public delegate Task<ProxyResponse<T>> ProxyDelegate<T>(ProxyContext context, CancellationToken cancellationToken = default);
