using Example.Filtering.Gamma.Contract;
using Example.Filtering.Gamma.Orm;
using Ifx.Filtering;
using Ifx.Filtering.Extensions;

namespace Example.Filtering.Gamma.Service;

public class GammaAccess(ZetaContext ctx) : IGammaAccess
{

    class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public void Thingy()
    {

        var collection = Enumerable.Range(1, 100).Select(i => new Widget { Id = i, Name = $"Item {i}" }).ToList();


        var query = collection.AsQueryable();
        var filter = new Filter<Widget>();
        filter.AddCriterion("Name", "item 1", Filter.IgnoreCaseOption.No);   

        query = query.ApplyFilter(filter);
        var result = query.ToList();




    }

}