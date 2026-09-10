using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Interceptors;

/// <summary>Authorizes before executing the continuation.</summary>
public sealed class AuthInterceptor(IAuthorizationService auth) : IInterceptor
{
    private readonly IAuthorizationService auth = auth ?? throw new ArgumentNullException(nameof(auth));
    /// <inheritdoc />
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse> => InvokeAsync(request, PipelineGuard.Adapt(next), CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        PipelineGuard.Call(request, next, cancellationToken);
        await auth.AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return await next(request, cancellationToken).ConfigureAwait(false);
    }
}
