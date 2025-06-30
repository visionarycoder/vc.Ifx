using Ifx.Filtering;
using vc.Ifx.Services.Messaging;

namespace Example.Filtering.Alpha.Contact;

public class FindProductsRequest : ServiceMessageRequest
{

    public Filter Filter { get; set; } = new();
    public bool CalculateCost { get; set; }

}