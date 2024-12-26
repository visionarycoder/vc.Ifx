using Example.Filtering.Alpha.Contact.Models;
using Ifx.Messaging;

namespace Example.Filtering.Alpha.Contact;

public interface IAlphaManager
{

    Task<FindProductsResponse> FindProductsAsync(FindProductsRequest request);

}

public class FindProductsResponse : ServiceMessageResponse
{
    public List<Product> Products { get; set; } = new List<Product>();
}

public class FindProductsRequest : ServiceMessageRequest
{

    public Filter<Product> Filter { get; set; } = new Filter<Product>();
    public bool CalculateCost { get; set; }

}