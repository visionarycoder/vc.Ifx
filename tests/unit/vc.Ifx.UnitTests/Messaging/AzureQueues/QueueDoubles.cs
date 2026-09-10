using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Messaging.Azure.Queue;

namespace VisionaryCoder.Framework.Tests.Messaging.AzureQueues;

internal static class QueueFixture
{
    public static Response Raw => Mock.Of<Response>();
    public static AzureQueueStorageOptions Options(bool create = false, bool encode = true, int ttl = -1) => new()
    {
        QueueName = "work-items", ConnectionString = "UseDevelopmentStorage=true", CreateQueueIfNotExists = create,
        EncodeMessages = encode, MessageTimeToLiveSeconds = ttl, VisibilityTimeoutSeconds = 17, MaxMessagesToRetrieve = 7
    };
    public static AzureQueueStorageProvider Provider(QueueClient client, AzureQueueStorageOptions? options = null) =>
        new(options ?? Options(), NullLogger<AzureQueueStorageProvider>.Instance, client);
}

internal sealed record QueueCall(string Operation, CancellationToken Token, string? Text = null, int? Count = null,
    TimeSpan? Visibility = null, TimeSpan? Lifetime = null, string? Id = null, string? Receipt = null);

internal sealed class QueueFake : QueueClient
{
    public List<QueueCall> Calls { get; } = [];
    public Exception? Failure { get; set; }
    public Action<QueueCall>? OnCall { get; set; }
    public Func<CancellationToken, Task>? Initialize { get; set; }
    public bool ExistsResult { get; set; } = true;
    public QueueMessage[] Messages { get; set; } = [];
    public PeekedMessage[] Peeked { get; set; } = [];
    public int Count { get; set; }
    public UpdateReceipt Receipt { get; } = QueuesModelFactory.UpdateReceipt("renewed", DateTimeOffset.UnixEpoch.AddMinutes(1));

    private Response Record(QueueCall call)
    {
        lock (Calls) Calls.Add(call);
        OnCall?.Invoke(call);
        if (Failure is not null) throw Failure;
        return QueueFixture.Raw;
    }
    public override Response CreateIfNotExists(IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
        => Record(new("create", cancellationToken));
    public override async Task<Response> CreateIfNotExistsAsync(IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        var result = Record(new("create", cancellationToken));
        if (Initialize is not null) await Initialize(cancellationToken);
        return result;
    }
    public override Response<bool> Exists(CancellationToken cancellationToken = default)
        => Response.FromValue(ExistsResult, Record(new("exists", cancellationToken)));
    public override Task<Response<bool>> ExistsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Exists(cancellationToken));
    public override Response<SendReceipt> SendMessage(string messageText, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
        => Response.FromValue(QueuesModelFactory.SendReceipt("id", DateTimeOffset.UnixEpoch, DateTimeOffset.MaxValue, "sent", DateTimeOffset.UnixEpoch), Record(new("send", cancellationToken, messageText, Visibility: visibilityTimeout, Lifetime: timeToLive)));
    public override Task<Response<SendReceipt>> SendMessageAsync(string messageText, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
        => Task.FromResult(SendMessage(messageText, visibilityTimeout, timeToLive, cancellationToken));
    public override Response<QueueMessage[]> ReceiveMessages(int? maxMessages = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
        => Response.FromValue(Messages, Record(new("receive", cancellationToken, Count: maxMessages, Visibility: visibilityTimeout)));
    public override Task<Response<QueueMessage[]>> ReceiveMessagesAsync(int? maxMessages = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
        => Task.FromResult(ReceiveMessages(maxMessages, visibilityTimeout, cancellationToken));
    public override Response<PeekedMessage[]> PeekMessages(int? maxMessages = null, CancellationToken cancellationToken = default)
        => Response.FromValue(Peeked, Record(new("peek", cancellationToken, Count: maxMessages)));
    public override Task<Response<PeekedMessage[]>> PeekMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default)
        => Task.FromResult(PeekMessages(maxMessages, cancellationToken));
    public override Response DeleteMessage(string messageId, string popReceipt, CancellationToken cancellationToken = default)
        => Record(new("delete", cancellationToken, Id: messageId, Receipt: popReceipt));
    public override Task<Response> DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken = default)
        => Task.FromResult(DeleteMessage(messageId, popReceipt, cancellationToken));
    public override Response<UpdateReceipt> UpdateMessage(string messageId, string popReceipt, string? messageText = null, TimeSpan visibilityTimeout = default, CancellationToken cancellationToken = default)
        => Response.FromValue(Receipt, Record(new("update", cancellationToken, messageText, Visibility: visibilityTimeout, Id: messageId, Receipt: popReceipt)));
    public override Task<Response<UpdateReceipt>> UpdateMessageAsync(string messageId, string popReceipt, string? messageText = null, TimeSpan visibilityTimeout = default, CancellationToken cancellationToken = default)
        => Task.FromResult(UpdateMessage(messageId, popReceipt, messageText, visibilityTimeout, cancellationToken));
    public override Response<QueueProperties> GetProperties(CancellationToken cancellationToken = default)
        => Response.FromValue(QueuesModelFactory.QueueProperties(new Dictionary<string, string>(), Count), Record(new("count", cancellationToken)));
    public override Task<Response<QueueProperties>> GetPropertiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(GetProperties(cancellationToken));
    public override Response ClearMessages(CancellationToken cancellationToken = default) => Record(new("clear", cancellationToken));
    public override Task<Response> ClearMessagesAsync(CancellationToken cancellationToken = default) => Task.FromResult(ClearMessages(cancellationToken));
}
