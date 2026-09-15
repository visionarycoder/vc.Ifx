using System.Diagnostics.CodeAnalysis;
using Experimental.System.Messaging;
using System.Text;

namespace Util.Messaging.Msmq;

internal interface IMsmqQueueClient
{
    Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken);

    Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken);
}

/// <summary>
/// Wraps direct MSMQ transport operations backed by <c>Experimental.System.Messaging</c>.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MsmqQueueClient : IMsmqQueueClient
{
    public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queuePath);
        ArgumentNullException.ThrowIfNull(message);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using MessageQueue queue = OpenQueue(queuePath);
            using Message msmqMessage = CreateMessage(message);
            queue.Send(msmqMessage);
        }, cancellationToken);
    }

    public Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queuePath);

        if (receiveTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(receiveTimeout), "Receive timeout must be greater than zero.");
        }

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using MessageQueue queue = OpenQueue(queuePath);
            queue.MessageReadPropertyFilter.SetAll();

            try
            {
                using Message msmqMessage = queue.Receive(receiveTimeout);
                return new ReceivedMsmqMessage(
                    ReadBody(msmqMessage),
                    msmqMessage.CorrelationId,
                    msmqMessage.Label);
            }
            catch (MessageQueueException exception) when (exception.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
            {
                return null;
            }
        }, cancellationToken);
    }

    private static MessageQueue OpenQueue(string queuePath)
    {
        if (!MessageQueue.Exists(queuePath))
        {
            throw new InvalidOperationException($"MSMQ queue '{queuePath}' does not exist.");
        }

        return new MessageQueue(queuePath);
    }

    private static Message CreateMessage(SerializedMsmqMessage message)
    {
        Message msmqMessage = new()
        {
            BodyStream = new MemoryStream(Encoding.UTF8.GetBytes(message.Body)),
            Label = message.Label,
            Recoverable = true
        };

        if (!string.IsNullOrWhiteSpace(message.CorrelationId))
        {
            string correlationId = message.CorrelationId.EndsWith("\\0", StringComparison.Ordinal)
                ? message.CorrelationId
                : $"{message.CorrelationId}\\0";
            msmqMessage.CorrelationId = correlationId;
        }

        return msmqMessage;
    }

    private static string ReadBody(Message message)
    {
        Stream bodyStream = message.BodyStream
            ?? throw new InvalidOperationException("MSMQ message body stream was not present.");

        bodyStream.Position = 0;

        using StreamReader reader = new(bodyStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);
        return reader.ReadToEnd();
    }
}
