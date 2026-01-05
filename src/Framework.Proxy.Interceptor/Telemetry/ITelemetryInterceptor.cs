// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Abstractions;

namespace VisionaryCoder.Framework.Proxy.Interceptor.Telemetry;

/// <summary>
/// Interface for telemetry interceptors.
/// </summary>
public interface ITelemetryInterceptor : IProxyInterceptor
{
    /// <summary>
    /// Intercepts method calls for telemetry.
    /// </summary>
    /// <param name="methodName">The name of the method being called.</param>
    /// <param name="parameters">The method parameters.</param>
    /// <param name="next">The next operation in the pipeline.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> InterceptAsync<T>(string methodName, object[] parameters, Func<Task<T>> next);
}
