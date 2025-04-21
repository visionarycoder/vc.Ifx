using vc.Ifx.Services.Messaging.Models;

namespace vc.Ifx.Services.Messaging.Extensions;

public static class FaultMessageExtensions
{
    public static void AddFaults(this ICollection<FaultMessage> target, ICollection<(string code, string message)> newFaults)
    {
        foreach (var (_, message) in newFaults) target.Add(new FaultMessage { Message = message });
    }
}