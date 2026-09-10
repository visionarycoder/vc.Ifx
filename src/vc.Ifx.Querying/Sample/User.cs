namespace VisionaryCoder.Framework.Querying.Sample;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public List<Order> Orders { get; set; } = new();
}