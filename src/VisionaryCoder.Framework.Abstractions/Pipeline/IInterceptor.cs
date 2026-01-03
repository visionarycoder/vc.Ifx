// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.Pipeline;

/// <summary>
/// Defines a pipeline interceptor that can process requests before and after they are handled.
/// </summary>
public interface IInterceptor
{
    /// <summary>
    /// Invokes the interceptor with the given request and next delegate.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to process.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next) 
        where TRequest : IRequest<TResponse>;
}
