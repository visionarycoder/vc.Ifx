namespace Utility.ServiceMessaging;

public interface IServiceMessage
{

    Guid MessageId { get; }
    Guid CorrelationId { get; }
    DateTime TimestampUtc { get; }

}