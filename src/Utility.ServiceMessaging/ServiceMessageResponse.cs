namespace Utility.ServiceMessaging;

public abstract class ServiceMessageResponse : ServiceMessage, IServiceMessageResponse
{
    public MessageError[] Errors { get; internal set; } = [];
    public bool HasErrors => Errors is { } errors && errors.Any(i => true);
}