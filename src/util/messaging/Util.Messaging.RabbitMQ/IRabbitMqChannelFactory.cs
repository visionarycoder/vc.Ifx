namespace Util.Messaging.RabbitMQ;

internal interface IRabbitMqChannelFactory
{
    Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}
