using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline;
public sealed class PipelineInvoker(
    IEnumerable<IInterceptor> interceptors,
    IEndpointResolver resolver,
    ILocalDispatcher local,
    IRemoteDispatcher remote)
    : IInvoker
{
    private readonly IReadOnlyList<IInterceptor> interceptors = interceptors.ToList();

    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>
    {
        EndpointResolution resolution = resolver.Resolve(typeof(TRequest));

        Func<TRequest, Task<TResponse>> terminal = resolution.IsLocal
            ? (req) => local.DispatchAsync<TRequest, TResponse>(req)
            : (req) => remote.DispatchAsync<TRequest, TResponse>(req, resolution);

        Func<TRequest, Task<TResponse>> next = terminal;

        // Build chain in reverse so first registered runs first
        for (int i = interceptors.Count - 1; i >= 0; i--)
        {
            Func<TRequest, Task<TResponse>> current = next;
            IInterceptor interceptor = interceptors[i];
            next = (req) => interceptor.InvokeAsync(req, current);
        }

        return next(request);
    }
}
