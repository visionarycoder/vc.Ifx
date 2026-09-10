namespace VisionaryCoder.Framework.Filtering.Abstractions;

public interface IFilterExecutionStrategy
{
    IQueryable<T> Apply<T>(IQueryable<T> source, FilterNode? filter);
    IEnumerable<T> Apply<T>(IEnumerable<T> source, FilterNode? filter);
}
