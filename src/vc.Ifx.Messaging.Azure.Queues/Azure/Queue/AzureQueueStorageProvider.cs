using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace VisionaryCoder.Framework.Messaging.Azure.Queue;

/// <summary>Queue transport over the virtual Azure SDK client, without application-level retries or acknowledgements.</summary>
public sealed class AzureQueueStorageProvider : ServiceBase<AzureQueueStorageProvider>, IQueueStorageProvider
{
    private static readonly Encoding messageEncoding = new UTF8Encoding(false, true);
    private readonly AzureQueueStorageOptions options;
    private readonly QueueClient queueClient;
    private readonly SemaphoreSlim initialization = new(1, 1);
    private bool initialized;

    public AzureQueueStorageProvider(AzureQueueStorageOptions options, ILogger<AzureQueueStorageProvider> logger)
        : this(options, logger, CreateClient(options)) { }

    /// <summary>Uses a borrowed SDK client, configured by the caller with matching encoding and retry settings.</summary>
    public AzureQueueStorageProvider(AzureQueueStorageOptions options, ILogger<AzureQueueStorageProvider> logger, QueueClient queueClient)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        this.options = options;
        this.queueClient = queueClient ?? throw new ArgumentNullException(nameof(queueClient));
    }

    public bool QueueExists()
    {
        ThrowIfDisposed();
        return queueClient.Exists().Value;
    }

    public async Task<bool> QueueExistsAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        var result = await queueClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return result.Value;
    }

    public void SendMessage(string messageText)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(messageText);
        ValidatePayload(messageText);
        Initialize();
        queueClient.SendMessage(messageText, timeToLive: TimeSpan.FromSeconds(options.MessageTimeToLiveSeconds));
    }

    public async Task SendMessageAsync(string messageText, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(messageText);
        ValidatePayload(messageText);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        await queueClient.SendMessageAsync(messageText, timeToLive: TimeSpan.FromSeconds(options.MessageTimeToLiveSeconds), cancellationToken: cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    public void SendMessage<T>(T messageObject) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(messageObject);
        SendMessage(JsonSerializer.Serialize(messageObject));
    }

    public async Task SendMessageAsync<T>(T messageObject, CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(messageObject);
        await SendMessageAsync(JsonSerializer.Serialize(messageObject), cancellationToken).ConfigureAwait(false);
    }

    public QueueMessage[] ReceiveMessages(int? maxMessages = null)
    {
        ThrowIfDisposed();
        int count = MessageCount(maxMessages);
        Initialize();
        return queueClient.ReceiveMessages(count, TimeSpan.FromSeconds(options.VisibilityTimeoutSeconds)).Value;
    }

    public async Task<QueueMessage[]> ReceiveMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        int count = MessageCount(maxMessages);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        var result = await queueClient.ReceiveMessagesAsync(count, TimeSpan.FromSeconds(options.VisibilityTimeoutSeconds), cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return result.Value;
    }

    public PeekedMessage[] PeekMessages(int? maxMessages = null)
    {
        ThrowIfDisposed();
        int count = MessageCount(maxMessages);
        Initialize();
        return queueClient.PeekMessages(count).Value;
    }

    public async Task<PeekedMessage[]> PeekMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        int count = MessageCount(maxMessages);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        var result = await queueClient.PeekMessagesAsync(count, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return result.Value;
    }

    public void DeleteMessage(string messageId, string popReceipt)
    {
        ThrowIfDisposed();
        ValidateReceipt(messageId, popReceipt);
        Initialize();
        queueClient.DeleteMessage(messageId, popReceipt);
    }

    public async Task DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ValidateReceipt(messageId, popReceipt);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        await queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>Legacy update discards the renewed receipt. Prefer UpdateMessageWithReceipt before subsequent acknowledgement.</summary>
    public void UpdateMessage(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null)
        => UpdateMessageWithReceipt(messageId, popReceipt, messageText, visibilityTimeout);

    /// <summary>Legacy update discards the renewed receipt. Prefer UpdateMessageWithReceiptAsync.</summary>
    public async Task UpdateMessageAsync(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
        => await UpdateMessageWithReceiptAsync(messageId, popReceipt, messageText, visibilityTimeout, cancellationToken).ConfigureAwait(false);

    /// <summary>Returns the renewed receipt. Null text preserves the body; empty text replaces it with an empty body.</summary>
    public UpdateReceipt UpdateMessageWithReceipt(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null)
    {
        ThrowIfDisposed();
        var visibility = ValidateUpdate(messageId, popReceipt, messageText, visibilityTimeout);
        Initialize();
        return queueClient.UpdateMessage(messageId, popReceipt, messageText, visibility).Value;
    }

    public async Task<UpdateReceipt> UpdateMessageWithReceiptAsync(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        var visibility = ValidateUpdate(messageId, popReceipt, messageText, visibilityTimeout);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        var result = await queueClient.UpdateMessageAsync(messageId, popReceipt, messageText, visibility, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return result.Value;
    }

    public int GetMessageCount()
    {
        ThrowIfDisposed();
        Initialize();
        return queueClient.GetProperties().Value.ApproximateMessagesCount;
    }

    public async Task<int> GetMessageCountAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        var result = await queueClient.GetPropertiesAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return result.Value.ApproximateMessagesCount;
    }

    public void ClearMessages()
    {
        ThrowIfDisposed();
        Initialize();
        queueClient.ClearMessages();
    }

    public async Task ClearMessagesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        await queueClient.ClearMessagesAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    protected override void Dispose(bool disposing)
    {
        // The sealed provider has no finalizer; callers quiesce operations before disposal.
        initialization.Dispose();
        base.Dispose(disposing);
    }

    private static QueueClient CreateClient(AzureQueueStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var clientOptions = options.CreateClientOptions();
        return options.UseManagedIdentity
            ? new QueueServiceClient(new Uri(options.StorageAccountUri!), new DefaultAzureCredential(), clientOptions).GetQueueClient(options.QueueName)
            : new QueueClient(options.ConnectionString, options.QueueName, clientOptions);
    }

    private void Initialize()
    {
        if (!options.CreateQueueIfNotExists)
            return;
        initialization.Wait();
        try
        {
            if (!initialized)
            {
                queueClient.CreateIfNotExists();
                initialized = true;
            }
        }
        finally { initialization.Release(); }
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (!options.CreateQueueIfNotExists)
            return;
        await initialization.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!initialized)
            {
                await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                initialized = true;
            }
        }
        finally { initialization.Release(); }
    }

    private int MessageCount(int? maxMessages)
    {
        int count = maxMessages ?? options.MaxMessagesToRetrieve;
        if (count is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(maxMessages), "Message count must be between 1 and 32.");
        return count;
    }

    private static void ValidateReceipt(string messageId, string popReceipt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(popReceipt);
    }

    private TimeSpan ValidateUpdate(string messageId, string popReceipt, string? messageText, TimeSpan? visibilityTimeout)
    {
        ValidateReceipt(messageId, popReceipt);
        var visibility = visibilityTimeout ?? TimeSpan.Zero;
        if (visibility < TimeSpan.Zero || visibility > TimeSpan.FromDays(7))
            throw new ArgumentOutOfRangeException(nameof(visibilityTimeout));
        if (messageText is not null)
            ValidatePayload(messageText);
        return visibility;
    }

    private void ValidatePayload(string messageText)
    {
        long bytes = messageEncoding.GetByteCount(messageText);
        long encodedBytes = options.EncodeMessages ? 4 * ((bytes + 2) / 3) : bytes;
        if (encodedBytes > 65536)
            throw new ArgumentException("Encoded message content exceeds the 64 KiB queue limit.", nameof(messageText));
        if (!options.EncodeMessages)
            XmlConvert.VerifyXmlChars(messageText);
    }
}
