using Ifx.Services.Messaging.Contract;
using Ifx.Services.Messaging.Models.Base;

namespace Ifx.Services.Messaging;

public class ServiceMessageResponse : ServiceMessageBase, IServiceMessageResponse
{
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = string.Empty;

    public bool HasErrors => ErrorMessages.Count > 0;
    public ICollection<FaultMessage> ErrorMessages { get; } = new List<FaultMessage>();
}