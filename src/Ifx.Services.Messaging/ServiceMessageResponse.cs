using vc.Ifx.Services.Messaging.Contract;
using vc.Ifx.Services.Messaging.Models;
using vc.Ifx.Services.Messaging.Models.Base;

namespace vc.Ifx.Services.Messaging;

public class ServiceMessageResponse : ServiceMessageBase, IServiceMessageResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;

    public bool HasFaults => Faults.Count > 0;
    public ICollection<FaultMessage> Faults { get; } = new List<FaultMessage>();

}