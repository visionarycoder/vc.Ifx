using Azure;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace VisionaryCoder.Framework.Messaging.Azure.Queue;

/// <summary>
/// Provides Azure Queue Storage-based message queue operations implementation.
/// This service wraps Azure Queue Storage operations with logging, error handling, and async support.
/// Supports both connection string and managed identity authentication.
/// </summary>
public sealed class AzureQueueStorageProvider : ServiceBase<AzureQueueStorageProvider>, IQueueStorageProvider
{
    private static readonly Encoding defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly AzureQueueStorageOptions options;
    private readonly QueueServiceClient queueServiceClient;
    private readonly QueueClient queueClient;

    public AzureQueueStorageProvider(AzureQueueStorageOptions options, ILogger<AzureQueueStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();

        try
        {
            // Create queue service client based on authentication method
            if (options.UseManagedIdentity)
            {
                queueServiceClient = new QueueServiceClient(new Uri(options.StorageAccountUri!), new DefaultAzureCredential());
            }
            else
            {
                queueServiceClient = new QueueServiceClient(options.ConnectionString);
            }

            var clientOptions = new QueueClientOptions
            {
                MessageEncoding = options.EncodeMessages ? QueueMessageEncoding.Base64 : QueueMessageEncoding.None
            };

            queueClient = queueServiceClient.GetQueueClient(options.QueueName);

            // Create queue if it doesn't exist and option is enabled
            if (options.CreateQueueIfNotExists)
            {
                queueClient.CreateIfNotExists();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize Azure Queue Storage client");
            throw;
        }
    }

    /// <summary>
    /// Checks if the queue exists.
    /// </summary>
    public bool QueueExists()
    {
        try
        {
            Response<bool> response = queueClient.Exists();
            Logger.LogTrace("Queue existence check for '{QueueName}': {Exists}", options.QueueName, response.Value);
            return response.Value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking queue existence for '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Checks if the queue exists asynchronously.
    /// </summary>
    public async Task<bool> QueueExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Response<bool> response = await queueClient.ExistsAsync(cancellationToken);
            Logger.LogTrace("Queue existence check async for '{QueueName}': {Exists}", options.QueueName, response.Value);
            return response.Value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking queue existence async for '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Sends a text message to the queue.
    /// </summary>
    public void SendMessage(string messageText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageText);

        try
        {
            Logger.LogDebug("Sending message to queue '{QueueName}'", options.QueueName);

            TimeSpan? timeToLive = options.MessageTimeToLiveSeconds == -1
                ? null
                : TimeSpan.FromSeconds(options.MessageTimeToLiveSeconds);

            queueClient.SendMessage(messageText, timeToLive: timeToLive);
            Logger.LogTrace("Successfully sent message to queue '{QueueName}'", options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending message to queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Sends a text message to the queue asynchronously.
    /// </summary>
    public async Task SendMessageAsync(string messageText, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageText);

        try
        {
            Logger.LogDebug("Sending message async to queue '{QueueName}'", options.QueueName);

            TimeSpan? timeToLive = options.MessageTimeToLiveSeconds == -1
                ? null
                : TimeSpan.FromSeconds(options.MessageTimeToLiveSeconds);

            await queueClient.SendMessageAsync(messageText, timeToLive: timeToLive, cancellationToken: cancellationToken);
            Logger.LogTrace("Successfully sent message async to queue '{QueueName}'", options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending message async to queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Sends an object as a JSON message to the queue.
    /// </summary>
    public void SendMessage<T>(T messageObject) where T : class
    {
        ArgumentNullException.ThrowIfNull(messageObject);

        try
        {
            string messageText = JsonSerializer.Serialize(messageObject);
            SendMessage(messageText);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error serializing and sending message to queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Sends an object as a JSON message to the queue asynchronously.
    /// </summary>
    public async Task SendMessageAsync<T>(T messageObject, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(messageObject);

        try
        {
            string messageText = JsonSerializer.Serialize(messageObject);
            await SendMessageAsync(messageText, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error serializing and sending message async to queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Receives messages from the queue.
    /// </summary>
    public QueueMessage[] ReceiveMessages(int? maxMessages = null)
    {
        try
        {
            int messageCount = maxMessages ?? options.MaxMessagesToRetrieve;
            var visibilityTimeout = TimeSpan.FromSeconds(options.VisibilityTimeoutSeconds);

            Logger.LogDebug("Receiving up to {MaxMessages} messages from queue '{QueueName}'", messageCount, options.QueueName);

            Response<QueueMessage[]> response = queueClient.ReceiveMessages(messageCount, visibilityTimeout);

            Logger.LogTrace("Received {Count} messages from queue '{QueueName}'", response.Value.Length, options.QueueName);
            return response.Value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error receiving messages from queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Receives messages from the queue asynchronously.
    /// </summary>
    public async Task<QueueMessage[]> ReceiveMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default)
    {
        try
        {
            int messageCount = maxMessages ?? options.MaxMessagesToRetrieve;
            var visibilityTimeout = TimeSpan.FromSeconds(options.VisibilityTimeoutSeconds);

            Logger.LogDebug("Receiving up to {MaxMessages} messages async from queue '{QueueName}'", messageCount, options.QueueName);

            Response<QueueMessage[]> response = await queueClient.ReceiveMessagesAsync(messageCount, visibilityTimeout, cancellationToken);

            Logger.LogTrace("Received {Count} messages async from queue '{QueueName}'", response.Value.Length, options.QueueName);
            return response.Value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error receiving messages async from queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Peeks at messages without removing them from the queue.
    /// </summary>
    public PeekedMessage[] PeekMessages(int? maxMessages = null)
    {
        try
        {
            int messageCount = maxMessages ?? options.MaxMessagesToRetrieve;

            Logger.LogDebug("Peeking at up to {MaxMessages} messages from queue '{QueueName}'", messageCount, options.QueueName);

            Response<PeekedMessage[]> response = queueClient.PeekMessages(messageCount);

            Logger.LogTrace("Peeked at {Count} messages from queue '{QueueName}'", response.Value.Length, options.QueueName);
            return response.Value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error peeking at messages from queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Peeks at messages without removing them from the queue asynchronously.
    /// </summary>
    public async Task<PeekedMessage[]> PeekMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default)
    {
        try
        {
            int messageCount = maxMessages ?? options.MaxMessagesToRetrieve;

            Logger.LogDebug("Peeking at up to {MaxMessages} messages async from queue '{QueueName}'", messageCount, options.QueueName);

            Response<PeekedMessage[]> response = await queueClient.PeekMessagesAsync(messageCount, cancellationToken);

            Logger.LogTrace("Peeked at {Count} messages async from queue '{QueueName}'", response.Value.Length, options.QueueName);
            return response.Value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error peeking at messages async from queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a message from the queue.
    /// </summary>
    public void DeleteMessage(string messageId, string popReceipt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(popReceipt);

        try
        {
            Logger.LogDebug("Deleting message '{MessageId}' from queue '{QueueName}'", messageId, options.QueueName);
            queueClient.DeleteMessage(messageId, popReceipt);
            Logger.LogTrace("Successfully deleted message '{MessageId}' from queue '{QueueName}'", messageId, options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting message '{MessageId}' from queue '{QueueName}'", messageId, options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a message from the queue asynchronously.
    /// </summary>
    public async Task DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(popReceipt);

        try
        {
            Logger.LogDebug("Deleting message async '{MessageId}' from queue '{QueueName}'", messageId, options.QueueName);
            await queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken);
            Logger.LogTrace("Successfully deleted message async '{MessageId}' from queue '{QueueName}'", messageId, options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting message async '{MessageId}' from queue '{QueueName}'", messageId, options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Updates the visibility timeout of a message.
    /// </summary>
    public void UpdateMessage(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(popReceipt);

        try
        {
            Logger.LogDebug("Updating message '{MessageId}' in queue '{QueueName}'", messageId, options.QueueName);
            queueClient.UpdateMessage(messageId, popReceipt, messageText, visibilityTimeout ?? TimeSpan.Zero);
            Logger.LogTrace("Successfully updated message '{MessageId}' in queue '{QueueName}'", messageId, options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating message '{MessageId}' in queue '{QueueName}'", messageId, options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Updates the visibility timeout of a message asynchronously.
    /// </summary>
    public async Task UpdateMessageAsync(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(popReceipt);

        try
        {
            Logger.LogDebug("Updating message async '{MessageId}' in queue '{QueueName}'", messageId, options.QueueName);
            await queueClient.UpdateMessageAsync(messageId, popReceipt, messageText, visibilityTimeout ?? TimeSpan.Zero, cancellationToken);
            Logger.LogTrace("Successfully updated message async '{MessageId}' in queue '{QueueName}'", messageId, options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating message async '{MessageId}' in queue '{QueueName}'", messageId, options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Gets the approximate number of messages in the queue.
    /// </summary>
    public int GetMessageCount()
    {
        try
        {
            Logger.LogDebug("Getting message count for queue '{QueueName}'", options.QueueName);

            QueueProperties properties = queueClient.GetProperties();
            int count = properties.ApproximateMessagesCount;

            Logger.LogTrace("Queue '{QueueName}' has approximately {Count} messages", options.QueueName, count);
            return count;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting message count for queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Gets the approximate number of messages in the queue asynchronously.
    /// </summary>
    public async Task<int> GetMessageCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Getting message count async for queue '{QueueName}'", options.QueueName);

            Response<QueueProperties> response = await queueClient.GetPropertiesAsync(cancellationToken);
            int count = response.Value.ApproximateMessagesCount;

            Logger.LogTrace("Queue '{QueueName}' has approximately {Count} messages", options.QueueName, count);
            return count;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting message count async for queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Clears all messages from the queue.
    /// </summary>
    public void ClearMessages()
    {
        try
        {
            Logger.LogDebug("Clearing all messages from queue '{QueueName}'", options.QueueName);
            queueClient.ClearMessages();
            Logger.LogTrace("Successfully cleared all messages from queue '{QueueName}'", options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error clearing messages from queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

    /// <summary>
    /// Clears all messages from the queue asynchronously.
    /// </summary>
    public async Task ClearMessagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Clearing all messages async from queue '{QueueName}'", options.QueueName);
            await queueClient.ClearMessagesAsync(cancellationToken);
            Logger.LogTrace("Successfully cleared all messages async from queue '{QueueName}'", options.QueueName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error clearing messages async from queue '{QueueName}'", options.QueueName);
            throw;
        }
    }

}
