using Ifx.Messaging.Abstractions;

namespace Util.Messaging.RabbitMQ;

internal sealed record RabbitMqTopology(
    string QueueName,
    string RoutingKey,
    string DeadLetterExchangeName,
    string DeadLetterQueueName,
    string DeadLetterRoutingKey)
{
    public static RabbitMqTopology Create<TMessage>(RabbitMqOptions options, string subscriptionName)
        where TMessage : IMessage
    {
        string messageTypeName = typeof(TMessage).FullName!;
        string queueBaseName = options.QueueName.ValidateNotNullOrWhiteSpace(nameof(options.QueueName));
        string deadLetterExchangeName = string.IsNullOrWhiteSpace(options.DeadLetterExchangeName)
            ? $"{options.ExchangeName}.dead-letter"
            : options.DeadLetterExchangeName;
        string deadLetterQueueBaseName = string.IsNullOrWhiteSpace(options.DeadLetterQueueName)
            ? $"{queueBaseName}.dead-letter"
            : options.DeadLetterQueueName;

        return new RabbitMqTopology(
            $"{queueBaseName}.{subscriptionName}.{typeof(TMessage).Name}",
            messageTypeName,
            deadLetterExchangeName,
            $"{deadLetterQueueBaseName}.{subscriptionName}.{typeof(TMessage).Name}",
            $"{messageTypeName}.dead-letter");
    }
}
