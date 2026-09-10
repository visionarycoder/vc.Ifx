using Grpc.Net.Client;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

public sealed class GenericGrpcClient
{
    private readonly GrpcChannel channel;
    private readonly GenericInvoker.GenericInvokerClient client;

    public GenericGrpcClient(GrpcChannel channel)
    {
        this.channel = channel;
        client = new GenericInvoker.GenericInvokerClient(channel);
    }

    public async Task<string> InvokeAsync(string payload, string requestType)
    {
        var req = new InvokeRequest
        {
            RequestType = requestType,
            Payload = payload
        };

        var resp = await client.InvokeAsync(req);
        return resp.Payload;
    }
}
