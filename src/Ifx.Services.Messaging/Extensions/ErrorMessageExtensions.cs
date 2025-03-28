using Ifx.Services.Messaging.Models;

namespace Ifx.Services.Messaging.Extensions;

public static class ErrorMessageExtensions
{
    public static void AddErrorMessageRange(this ICollection<ErrorMessage> errors, ICollection<(string code, string message)> collection)
    {
        foreach (var (code, message) in collection) errors.Add(new ErrorMessage { Message = message });
    }
}