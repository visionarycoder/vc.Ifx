using vc.Ifx.Services.Messaging.Models;

namespace vc.Ifx.Services.Messaging.Contract;

public interface IServiceMessageResponse
{
    public bool Success { get; }
    public string Message { get; }

    public bool HasFaults { get; }
    public ICollection<FaultMessage> Faults { get; }
}