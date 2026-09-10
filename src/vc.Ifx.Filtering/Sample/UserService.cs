using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering.Sample;

public sealed class UserService(IFilterExecutionStrategy strategy)
{
    private readonly IFilterExecutionStrategy strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));

    // Apply to an IQueryable<T> (works for both POCO and EF Core queries)
    public IQueryable<User> Query(IQueryable<User> source, FilterNode? filter) => strategy.Apply(source, filter);

    // Convenience: apply to IEnumerable<T> (in-memory)
    public IEnumerable<User> Query(IEnumerable<User> source, FilterNode? filter) => strategy.Apply(source, filter);
}
