// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Defines authorization policies for proxy requests.
/// </summary>
public interface IProxyAuthorizationPolicy
{
    /// <summary>
    /// Evaluates whether the request is authorized.
    /// </summary>
    /// <param name="context">The proxy context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with a result indicating authorization status.</returns>
    Task<bool> IsAuthorizedAsync(ProxyContext context, CancellationToken cancellationToken = default);
}