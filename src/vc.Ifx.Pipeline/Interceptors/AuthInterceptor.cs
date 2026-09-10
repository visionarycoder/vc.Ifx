using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

public sealed class AuthInterceptor(IAuthorizationService auth) : IInterceptor
{
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(
        TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse>
    {
        await auth.AuthorizeAsync(request);
        return await next(request);
    }
}