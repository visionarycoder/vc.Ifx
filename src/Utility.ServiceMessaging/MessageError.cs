// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Utility.ServiceMessaging;

public sealed class MessageError(string message, string? code = null)
{
    public string Message { get; } = message;

    public string? Code { get; } = code;

    public override string ToString()
    {

        return string.IsNullOrWhiteSpace(Code)
            ? Message
            : $"{Message} ({Code})";
    }

}