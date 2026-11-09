using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal sealed class AzureServiceBusClientAdapter : IAzureServiceBusClient
{
    private readonly ServiceBusClient client;

    internal AzureServiceBusClientAdapter(ServiceBusClient client)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public IAzureServiceBusSender CreateSender(string entityPath)
        => new AzureServiceBusSenderAdapter(client.CreateSender(entityPath));

    public IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options)
        => new AzureServiceBusProcessorAdapter(client.CreateProcessor(queueName, options));

    public IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options)
        => new AzureServiceBusProcessorAdapter(client.CreateProcessor(topicName, subscriptionName, options));
}
