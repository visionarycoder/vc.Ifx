using Example.Filtering.Alpha.Contact.Models;
using vc.Ifx.Services.Messaging;

namespace Example.Filtering.Alpha.Contact;

public class FindProductsResponse : ServiceMessageResponse
{
    public List<Product> Products { get; set; } = [];
}