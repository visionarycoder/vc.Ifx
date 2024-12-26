namespace vc.Ifx;

public class Filter<T> where T : class
{

    public static Filter<T> EmptyFilter => new();

    // Dictionary to store property names and values
    public Dictionary<string, object?> PropertyValues { get; set; } = new();

    // Pagination properties
    public int? Offset { get; set; } = 0;
    public int? Limit { get; set; } = 50;

    // Ordering properties
    public string? OrderBy { get; set; }
    public bool OrderByDescending { get; set; }
    public string? ThenBy { get; set; }
    public bool ThenByDescending { get; set; }

    // Generic empty constructor
    public Filter() { }

    // Constructor to add properties as params
    public Filter(params (string propertyName, object? value)[] properties)
    {
        foreach (var (propertyName, value) in properties)
        {
            AddProperty(propertyName, value);
        }
    }

    // Method to add a property and its value to the dictionary
    public void AddProperty(string propertyName, object? value)
    {
        PropertyValues[propertyName] = value;
    }

    
}