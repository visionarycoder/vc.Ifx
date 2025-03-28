// ReSharper disable MemberCanBePrivate.Global

using Ifx.Services.Messaging.Models;

namespace Ifx.Services.Messaging.Extensions;

public static class ServiceMessageResponseExtensions
{
    public static void AddError(this ServiceMessageResponse source, ErrorMessage error)
    {
        source.ErrorMessages.Add(error);
    }

    public static void AddErrorRange(this ServiceMessageResponse source, ICollection<ErrorMessage> errors)
    {
        foreach (var error in errors) source.AddError(error);
    }

    public static void AddErrorMessage(this ServiceMessageResponse response, string code, string message)
    {
        response.ErrorMessages.Add(new ErrorMessage { Message = message });
    }

    public static void AddErrorMessageRange(this ServiceMessageResponse response, ICollection<(string code, string message)> errors)
    {
        foreach (var (code, message) in errors) response.ErrorMessages.Add(new ErrorMessage { Message = message });
    }
}