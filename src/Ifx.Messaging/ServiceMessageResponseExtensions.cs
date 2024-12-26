// ReSharper disable MemberCanBePrivate.Global

namespace Ifx.Messaging;


public static class ServiceMessageResponseExtensions
{

    public static void AddError(this ServiceMessageResponse source, ErrorMessage error)
    {
        source.Errors.Add(error);
    }

    public static void AddErrorRange(this ServiceMessageResponse source, ICollection<ErrorMessage> errors)
    {

        foreach (var error in errors)
        {
            source.AddError(error);
        }

    }

}