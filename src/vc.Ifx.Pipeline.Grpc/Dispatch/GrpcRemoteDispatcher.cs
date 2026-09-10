using Grpc.Net.Client;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

public sealed class GrpcRemoteDispatcher(ISerializer serializer) : IRemoteDispatcher
{
    private readonly ISerializer serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

    public async Task<TResponse> DispatchAsync<TRequest, TResponse>(
        TRequest request, EndpointResolution endpoint)
        where TRequest : IRequest<TResponse>
    {
        if (endpoint.Uri is null)
            throw new InvalidOperationException("Remote endpoint URI required for gRPC dispatch.");

        // Create channel dynamically based on endpoint
        using var channel = GrpcChannel.ForAddress(endpoint.Uri);

        // Generic gRPC client stub (you’d generate this from .proto in real apps)
        var client = new GenericGrpcClient(channel);

        // Serialize request to JSON (or protobuf if you define contracts)
        var payload = serializer.Serialize(request);

        // Send request over gRPC
        var responseJson = await client.InvokeAsync(payload, typeof(TRequest).Name);

        // Deserialize back into response type
        return serializer.Deserialize<TResponse>(responseJson);
    }
}