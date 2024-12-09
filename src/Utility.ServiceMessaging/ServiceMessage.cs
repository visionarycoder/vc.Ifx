namespace Utility.ServiceMessaging;

public abstract class ServiceMessage : IServiceMessage
{

    public Guid MessageId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

}