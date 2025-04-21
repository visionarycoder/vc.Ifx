using vc.Ifx.Services.Messaging.Models.Base;

namespace vc.Ifx.Services.Messaging.Factory;

public static class ServiceMessageFactory
{
    public static T Create<T>() where T : ServiceMessageBase, new()
    {
        return Create<T>(Guid.NewGuid());
    }

    public static T Create<T>(Guid correlationId) where T : ServiceMessageBase, new()
    {
        var result = new T
        {
            MessageId = Guid.NewGuid(),
            CorrelationId = correlationId,
            TimestampUtc = DateTime.UtcNow
        };
        return result;
    }

    public static T CreateFrom<T>(ServiceMessageBase message) where T : ServiceMessageBase, new()
    {
        return Create<T>(message.CorrelationId);
        ;
    }

    public static async Task<T> CreateAsync<T>() where T : ServiceMessageBase, new()
    {
        return await Task.FromResult(Create<T>());
    }

    public static async Task<T> CreateAsync<T>(Guid correlationId) where T : ServiceMessageBase, new()
    {
        return await Task.FromResult(Create<T>(correlationId));
    }

    public static async Task<T> CreateFromAsync<T>(ServiceMessageBase message) where T : ServiceMessageBase, new()
    {
        return await Task.FromResult(Create<T>(message.CorrelationId));
    }
}