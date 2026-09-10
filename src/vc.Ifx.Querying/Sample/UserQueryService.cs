namespace VisionaryCoder.Framework.Querying.Sample;

public sealed class UserQueryService
{
    // Queryable consumer (EF Core)
    public IQueryable<User> ApplyToQueryable(IQueryable<User> source, QueryFilter<User> filter)
    {
        // Extension method in QueryFilterExtensions: source.Apply(filter)
        return source.Apply(filter);
    }

    // Enumerable consumer (POCO in-memory)
    public IEnumerable<User> ApplyToEnumerable(IEnumerable<User> source, QueryFilter<User> filter)
    {
        // Compile the expression and use it for in-memory filtering
        var predicate = filter.Predicate.Compile();
        return source.Where(predicate);
    }
}