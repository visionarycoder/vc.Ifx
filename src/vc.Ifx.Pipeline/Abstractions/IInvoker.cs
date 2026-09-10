namespace VisionaryCoder.Framework.Pipeline.Abstractions;

/// <summary>Compatible request dispatch boundary with cooperative cancellation support.</summary>
public interface IInvoker
{
    /// <summary>Invokes the legacy operation without an explicit cancellation token.</summary>
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>;

    /// <summary>Checks cancellation before invoking a legacy implementation; override to cancel in-flight work.</summary>
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        cancellationToken.ThrowIfCancellationRequested();
        return InvokeAsync<TRequest, TResponse>(request);
    }
}
