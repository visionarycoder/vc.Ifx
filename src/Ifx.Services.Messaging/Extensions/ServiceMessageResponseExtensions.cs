// ReSharper disable MemberCanBePrivate.Global

using vc.Ifx.Services.Messaging.Models;

namespace vc.Ifx.Services.Messaging.Extensions;

public static class ServiceMessageResponseExtensions
{
    public static void AddFault(this ServiceMessageResponse source, FaultMessage fault)
    {
        source.Faults.Add(fault);
    }

    public static void AddFaults(this ServiceMessageResponse source, ICollection<FaultMessage> faults)
    {
        foreach (var error in faults) source.AddFault(error);
    }

    public static void AddErrorMessage(this ServiceMessageResponse response, string code, string message)
    {
        response.Faults.Add(new FaultMessage { Message = message });
    }

    public static void AddErrorMessageRange(this ServiceMessageResponse response, ICollection<(string code, string message)> errors)
    {
        foreach (var (_, message) in errors) 
            response.Faults.Add(new FaultMessage { Message = message });
    }
}