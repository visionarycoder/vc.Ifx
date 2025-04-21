using Example.Filtering.Alpha.Contact.Models;
using Ifx.Filtering;
using Ifx.Services.Messaging;

namespace Example.Filtering.Alpha.Contact;

public interface IAlphaManager
{

    Task<FindProductsResponse> FindProductsAsync(FindProductsRequest request);

}

public class FindProductsResponse : ServiceMessageResponse
{
    public List<Product> Products { get; set; } = [];
}

public class FindProductsRequest : ServiceMessageRequest
{

    public Filter Filter { get; set; } = new();
    public bool CalculateCost { get; set; }

}