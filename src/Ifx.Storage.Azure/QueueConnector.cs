using Microsoft.Extensions.Logging;

namespace vc.Ifx.Storage.Azure;

public class QueueConnector
{
    private readonly QueueServiceClient queueServiceClient;
    private readonly ILogger<QueueConnector> logger;

    public QueueConnector(string connectionString)
    {
        logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<QueueConnector>();
        queueServiceClient = new QueueServiceClient(connectionString);
    }

    public QueueConnector(ILogger<QueueConnector> logger, string connectionString)
    {
        this.logger = logger;
        queueServiceClient = new QueueServiceClient(connectionString);
    }

    public async Task SendMessageAsync(string queueName, string message)
    {
        var queueClient = queueServiceClient.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync();

        await queueClient.SendMessageAsync(message);
        logger.LogInformation($"Message sent to queue {queueName}: {message}");
    }

    public async Task<string> ReceiveMessageAsync(string queueName)
    {
        var queueClient = queueServiceClient.GetQueueClient(queueName);
        var response = await queueClient.ReceiveMessagesAsync(maxMessages: 1);

        var receivedMessage = response.Value.Length > 0 ? response.Value[0] : null;
        if (receivedMessage != null)
        {
            await queueClient.DeleteMessageAsync(receivedMessage.MessageId, receivedMessage.PopReceipt);
            logger.LogInformation($"Message received from queue {queueName}: {receivedMessage.MessageText}");
            return receivedMessage.MessageText;
        }

        logger.LogInformation($"No messages found in queue {queueName}");
        return null;
    }
}