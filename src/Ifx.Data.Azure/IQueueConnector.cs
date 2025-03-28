using Azure;
using Azure.Storage.Queues.Models;

namespace vc.Ifx.Data.Contracts;

public interface IQueueConnector
{
    QueueMessage? ReceiveMessage(string queueName);
    Task<QueueMessage?> ReceiveMessageAsync(string queueName);

    Response<SendReceipt> SendMessage(string queueName, string message);
    Task<Response<SendReceipt>> SendMessageAsync(string queueName, string message);
    
}