using vc.Ifx.Services.Messaging.Models;

namespace vc.Ifx.Services.Messaging.Factory;

public static class ErrorMessageFactory
{
    public static FaultMessage Create(string message)
    {
        var item = new FaultMessage
        {
            Message = message
        };
        return item;
    }

    public static async Task<FaultMessage> CreateAsync(string message)
    {
        return await Task.FromResult(Create(message));
    }
}