using Grpc.Core;
using Grpc.Net.Client;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

/// <summary>Maps the stable generic wire contract onto one borrowed generated client.</summary>
public sealed class GenericGrpcClient
{
    private readonly GenericInvoker.GenericInvokerClient client;

    /// <summary>Borrows a channel without taking ownership of its lifetime.</summary>
    public GenericGrpcClient(GrpcChannel channel)
        : this(new GenericInvoker.GenericInvokerClient(channel ?? throw new ArgumentNullException(nameof(channel)))) { }

    /// <summary>Borrows a generated client, supporting replacement in tests and host factories.</summary>
    public GenericGrpcClient(GenericInvoker.GenericInvokerClient client) =>
        this.client = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>Invokes without an explicit cancellation token.</summary>
    public Task<string> InvokeAsync(string payload, string requestType) => InvokeAsync(payload, requestType, CancellationToken.None);

    /// <summary>Forwards cancellation and disposes unary call resources on every completion path.</summary>
    public async Task<string> InvokeAsync(string payload, string requestType, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestType);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var call = client.InvokeAsync(new InvokeRequest { RequestType = requestType, Payload = payload },
                cancellationToken: cancellationToken);
            var response = await call.ResponseAsync.ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return response.Payload;
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Cancelled && cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("The gRPC operation was canceled by its caller.", exception, cancellationToken);
        }
    }
}
