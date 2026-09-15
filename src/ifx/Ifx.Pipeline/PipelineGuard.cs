using Ifx.Pipeline.Abstractions;

namespace Ifx.Pipeline;

public static class PipelineGuard
{
    public static void Call<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
    }

    public static Func<TRequest, CancellationToken, Task<TResponse>> Adapt<TRequest, TResponse>(Func<TRequest, Task<TResponse>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return (request, cancellationToken) => next(request);
    }

    internal static Uri HttpUri(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("An absolute HTTP(S) URI is required.", nameof(uri));
        return uri;
    }
}
