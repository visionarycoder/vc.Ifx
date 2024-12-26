using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Ifx.Logging;
using Microsoft.Extensions.Logging;

using vc.Ifx.Base;
using vc.Ifx.Data.Contracts;

namespace vc.Ifx.Data;

public class QueueConnector(ILogger<QueueConnector> logger, QueueServiceClient queueServiceClient) : ServiceBase<QueueConnector>(logger), IQueueConnector
{

    // Assign the logger methods to the delegates
    private readonly LogInformation logInformation = logger.LogInformation;
    private readonly LogWarning logWarning = logger.LogWarning;


    public QueueMessage? ReceiveMessage(string queueName)
    {
        var queueClient = queueServiceClient.GetQueueClient(queueName);
        var response = queueClient.ReceiveMessages(maxMessages: 1);
        var queueMessages = response.Value;
        if (queueMessages.Length > 0)
        {
            var queueMessage = queueMessages[0];
            queueClient.DeleteMessage(queueMessage.MessageId, queueMessage.PopReceipt);
            logInformation($"QueueName={queueName};QueueMessage={queueMessage.MessageText}");
            return queueMessage;
        }
        logWarning($"QueueName={queueName};Error=No queue messages received;");
        return null;
    }

    public async Task<QueueMessage?> ReceiveMessageAsync(string queueName)
    {
        var queueClient = queueServiceClient.GetQueueClient(queueName);
        var response = await queueClient.ReceiveMessagesAsync(maxMessages: 1);
        var queueMessages = response.Value;
        if (queueMessages.Length > 0)
        {
            var queueMessage = queueMessages[0];
            await queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt);
            logInformation($"QueueName={queueName};QueueMessage={queueMessage.MessageText}");
            return queueMessage;
        }
        logWarning($"QueueName={queueName};Error=No queue messages received;");
        return null;
    }

    public Response<SendReceipt> SendMessage(string queueName, string message)
    {
        var queueClient = queueServiceClient.GetQueueClient(queueName);
        queueClient.CreateIfNotExists();
        var response = queueClient.SendMessage(message);
        logInformation($"QueueName={queueName};Message={message}");
        return response;
    }

    public async Task<Response<SendReceipt>> SendMessageAsync(string queueName, string message)
    {
        var queueClient = queueServiceClient.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync();
        var response = await queueClient.SendMessageAsync(message);
        logInformation($"QueueName={queueName};Message={message}");
        return response;
    }

}
