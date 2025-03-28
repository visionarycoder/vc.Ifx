using Ifx.Services.Messaging.Models;

namespace Ifx.Services.Messaging.Factory;

public static class ErrorMessageFactory
{
    public static ErrorMessage Create(string message)
    {
        var item = new ErrorMessage
        {
            Message = message
        };
        return item;
    }

    public static async Task<ErrorMessage> CreateAsync(string message)
    {
        return await Task.FromResult(Create(message));
    }
}