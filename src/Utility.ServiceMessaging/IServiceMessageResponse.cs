namespace Utility.ServiceMessaging;

public interface IServiceMessageResponse : IServiceMessage
{

    MessageError[] Errors { get; }
    bool HasErrors { get; }

}