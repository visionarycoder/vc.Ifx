namespace Example.Filtering.Beta.Contract.Models;

public class Product
{
    public required int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

}