using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal interface IAzureServiceBusClient
{
    IAzureServiceBusSender CreateSender(string entityPath);

    IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options);

    IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options);
}
