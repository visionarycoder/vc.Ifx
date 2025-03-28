using Ifx.Services.Messaging.Models;

namespace Ifx.Services.Messaging.Contract;

public interface IServiceMessageResponse
{
    public bool IsSuccess { get; }
    public string Message { get; }

    public bool HasErrors { get; }
    public ICollection<ErrorMessage> ErrorMessages { get; }
}