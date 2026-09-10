using Grpc.Net.Client;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

/// <summary>Dispatches through the stable generic gRPC protocol with explicit transport ownership.</summary>
public sealed class GrpcRemoteDispatcher : IRemoteDispatcher
{
    private readonly ISerializer serializer;
    private readonly Func<Uri, (GenericInvoker.GenericInvokerClient Client, IDisposable? Lease)> clientFactory;

    /// <summary>Creates an owned channel per operation with .NET cancellation semantics.</summary>
    public GrpcRemoteDispatcher(ISerializer serializer)
        : this(serializer, new GrpcChannelOptions { ThrowOperationCanceledOnCancellation = true }) { }

    /// <summary>Creates owned per-operation channels with application-selected SDK options.</summary>
    public GrpcRemoteDispatcher(ISerializer serializer, GrpcChannelOptions channelOptions)
    {
        this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        ArgumentNullException.ThrowIfNull(channelOptions);
        clientFactory = uri =>
        {
            var channel = GrpcChannel.ForAddress(uri, channelOptions);
            return (new GenericInvoker.GenericInvokerClient(channel), channel);
        };
    }

    /// <summary>Borrows generated clients/channels managed by an application factory.</summary>
    public GrpcRemoteDispatcher(ISerializer serializer, Func<Uri, GenericInvoker.GenericInvokerClient> clientFactory)
    {
        this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        ArgumentNullException.ThrowIfNull(clientFactory);
        this.clientFactory = uri => (clientFactory(uri), null);
    }

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
        if (endpoint.IsLocal) throw new ArgumentException("A remote endpoint is required.", nameof(endpoint));
        var uri = endpoint.Uri ?? throw new InvalidOperationException("Remote endpoint URI required for gRPC dispatch.");
        if (!uri.IsAbsoluteUri || uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("An absolute HTTP(S) endpoint is required.", nameof(endpoint));

        string payload = serializer.Serialize(request);
        cancellationToken.ThrowIfCancellationRequested();
        var (client, lease) = clientFactory(uri);
        using (lease)
        {
            var response = await new GenericGrpcClient(client).InvokeAsync(payload, typeof(TRequest).Name, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return serializer.Deserialize<TResponse>(response);
        }
    }
}
