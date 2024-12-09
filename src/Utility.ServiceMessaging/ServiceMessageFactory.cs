// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CA1000
namespace Utility.ServiceMessaging;

public static class ServiceMessageFactory<T> where T : ServiceMessage, new()
{

    public static T Create()
    {
        return Create(Guid.Empty);
    }

    public static T Create(Guid correlationId)
    {
        var messageId = Guid.NewGuid();
        var instance = new T
        {
            MessageId = messageId,
            CorrelationId = correlationId == Guid.Empty ? messageId : correlationId,
            TimestampUtc = DateTime.UtcNow,
        };
        return instance;
    }

    public static T CreateFrom(IServiceMessage caller)
    {
        var message = Create(caller.CorrelationId);
        return message;
    }

}