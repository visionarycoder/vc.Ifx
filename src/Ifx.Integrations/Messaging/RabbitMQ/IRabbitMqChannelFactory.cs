namespace vc.Ifx.Messaging.RabbitMQ;

internal interface IRabbitMqChannelFactory
{
    Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}
