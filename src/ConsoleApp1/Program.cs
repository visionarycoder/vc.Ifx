using System.ComponentModel.DataAnnotations;
using Ifx.Filtering;
using Ifx.Filtering.Extensions;

Console.WriteLine("Starting...");

var collection = Enumerable.Range(1, 100).Select(i => new Widget { Id = i, Name = $"Item {i}" }).ToList();

var query = collection.AsQueryable();
var filter = new Filter<Widget>();
//filter.AddCriterion("Name", "item 1", Filter.IgnoreCaseOption.Yes, Filter.ComparisonOperatorOption.Equals);
//filter.SetPagination(1,25);

filter.AddCriterion("Fred", "item 1");
var validationResult = filter.ValidateFilter();

if (validationResult != ValidationResult.Success)
{
    Console.WriteLine($"Oops. : {validationResult.ErrorMessage}");
}
else
{
    query = query.ApplyFilter(filter);
    var result = query.ToList();

    foreach (var item in result)
    {
        Console.WriteLine($"Id: {item.Id}, Name: {item.Name}");
    }
}

Console.WriteLine("Done.");
Console.ReadLine();