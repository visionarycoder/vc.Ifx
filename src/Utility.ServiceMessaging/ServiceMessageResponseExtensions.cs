// ReSharper disable MemberCanBePrivate.Global
namespace Utility.ServiceMessaging;


public static class ServiceMessageResponseExtensions
{

    public static void AddError(this ServiceMessageResponse source, MessageError error)
    {
        source.AddErrorRange([error]);
    }

    public static void AddErrorRange(this ServiceMessageResponse source, ICollection<MessageError> errors)
    {

        var newErrors = errors
            .Where(i => !source.Errors.Contains(i));

        source.Errors = source
            .Errors
            .Concat(newErrors)
            .ToArray();

    }

}