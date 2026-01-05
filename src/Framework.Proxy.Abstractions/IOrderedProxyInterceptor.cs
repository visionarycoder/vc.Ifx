// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Abstractions;

/// <summary>
/// Defines a contract for proxy interceptors that can be ordered in a pipeline.
/// </summary>
public interface IOrderedProxyInterceptor : IProxyInterceptor
{
    /// <summary>
    /// Gets the order in which this interceptor should be executed.
    /// Lower values execute earlier in the pipeline.
    /// </summary>
    int Order { get; }
}
