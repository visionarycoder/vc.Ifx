using Polly;
using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

public sealed class ResilienceInterceptor(AsyncPolicy policy) : IInterceptor
{

    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse>
    {
        return policy.ExecuteAsync(() => next(request));
    }

    public static AsyncPolicy DefaultPolicy() =>
        Policy.WrapAsync(
            Policy.Handle<Exception>().CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)),
            Policy.Handle<HttpRequestException>().WaitAndRetryAsync([TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1000)])
        );
}
