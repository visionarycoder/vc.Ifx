using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors;

namespace VisionaryCoder.Framework.Pipeline;

/// <summary>Invokes policies in registration order around local or remote dispatch.</summary>
public sealed class PipelineInvoker : IInvoker
{
    private readonly IInterceptor[] interceptors;
    private readonly IEndpointResolver resolver;
    private readonly ILocalDispatcher local;
    private readonly IRemoteDispatcher remote;

    /// <summary>Snapshots policies and validates all collaborators.</summary>
    public PipelineInvoker(IEnumerable<IInterceptor> interceptors, IEndpointResolver resolver,
        ILocalDispatcher local, IRemoteDispatcher remote)
    {
        ArgumentNullException.ThrowIfNull(interceptors);
        this.interceptors = interceptors.ToArray();
        if (this.interceptors.Any(item => item is null))
            throw new ArgumentException("Interceptors cannot contain null.", nameof(interceptors));
        if (this.interceptors.OfType<ResilienceInterceptor>().Skip(1).Any())
            throw new ArgumentException("Register only one resilience interceptor.", nameof(interceptors));
        this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        this.local = local ?? throw new ArgumentNullException(nameof(local));
        this.remote = remote ?? throw new ArgumentNullException(nameof(remote));
    }

    /// <inheritdoc />
    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse> => InvokeAsync<TRequest, TResponse>(request, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        string? previous = Correlation.CurrentId;
        Correlation.CurrentId = previous ?? Guid.NewGuid().ToString("N");
        try
        {
            Func<TRequest, CancellationToken, Task<TResponse>> next = (item, token) =>
            {
                token.ThrowIfCancellationRequested();
                EndpointResolution resolution = resolver.Resolve(typeof(TRequest));
                return resolution.IsLocal
                    ? local.DispatchAsync<TRequest, TResponse>(item, token)
                    : remote.DispatchAsync<TRequest, TResponse>(item, resolution, token);
            };
            for (int i = interceptors.Length - 1; i >= 0; i--)
            {
                var current = next;
                var interceptor = interceptors[i];
                next = (item, token) => interceptor.InvokeAsync(item, current, token);
            }
            return await next(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Correlation.CurrentId = previous;
        }
    }
}
