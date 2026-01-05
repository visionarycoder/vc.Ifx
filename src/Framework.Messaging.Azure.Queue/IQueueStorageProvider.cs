using Azure.Storage.Queues.Models;

namespace VisionaryCoder.Framework.Messaging.Azure.Queue;

/// <summary>
/// Defines queue-oriented storage operations (enqueue, dequeue, peek, ack, etc.).
/// This interface separates messaging semantics from file/directory storage concerns.
/// </summary>
public interface IQueueStorageProvider
{
    bool QueueExists();
    Task<bool> QueueExistsAsync(CancellationToken cancellationToken = default);

    void SendMessage(string messageText);
    Task SendMessageAsync(string messageText, CancellationToken cancellationToken = default);
    void SendMessage<T>(T messageObject) where T : class;
    Task SendMessageAsync<T>(T messageObject, CancellationToken cancellationToken = default) where T : class;

    QueueMessage[] ReceiveMessages(int? maxMessages = null);
    Task<QueueMessage[]> ReceiveMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default);

    PeekedMessage[] PeekMessages(int? maxMessages = null);
    Task<PeekedMessage[]> PeekMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default);

    void DeleteMessage(string messageId, string popReceipt);
    Task DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken = default);

    void UpdateMessage(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null);
    Task UpdateMessageAsync(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

    int GetMessageCount();
    Task<int> GetMessageCountAsync(CancellationToken cancellationToken = default);

    void ClearMessages();
    Task ClearMessagesAsync(CancellationToken cancellationToken = default);
}
