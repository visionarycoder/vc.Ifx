using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

namespace Ifx.Proxy.Interceptor.Caching;

/// Interface for caching interceptors.
public interface ICachingInterceptor : IInterceptor
{
    /// <summary>
    /// Intercepts method calls for caching.
    /// </summary>
    /// <param name="methodName">The name of the method being called.</param>
    /// <param name="parameters">The method parameters.</param>
    /// <param name="next">The next operation in the pipeline.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> InterceptAsync<T>(string methodName, object[] parameters, Func<Task<T>> next);
}