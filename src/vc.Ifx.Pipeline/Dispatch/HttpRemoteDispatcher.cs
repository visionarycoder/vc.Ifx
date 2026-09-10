using System.Diagnostics;
using System.Text;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

/// <summary>Sends JSON requests using an externally owned HttpClient.</summary>
public sealed class HttpRemoteDispatcher(HttpClient http, ISerializer serializer) : IRemoteDispatcher
{
    private readonly HttpClient http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly ISerializer serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

    /// <inheritdoc />
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint)
        where TRequest : IRequest<TResponse> => DispatchAsync<TRequest, TResponse>(request, endpoint, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(endpoint);
        cancellationToken.ThrowIfCancellationRequested();
        if (endpoint.IsLocal)
            throw new ArgumentException("A remote endpoint is required.", nameof(endpoint));
        Uri uri = PipelineGuard.HttpUri(endpoint.Uri!);
        using var message = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(serializer.Serialize(request), Encoding.UTF8, "application/json")
        };
        DistributedContextPropagator.Current.Inject(Activity.Current, message,
            static (carrier, name, value) => ((HttpRequestMessage)carrier!).Headers.TryAddWithoutValidation(name, value));
        using var response = await http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return serializer.Deserialize<TResponse>(json);
    }
}
